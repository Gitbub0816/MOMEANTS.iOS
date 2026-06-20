export interface Env {
  JWT_SECRET: string;
  WORKER_API_KEY: string;
  JWT_ISSUER: string;
  JWT_AUDIENCE: string;
  ACCESS_TOKEN_TTL_SECONDS: string;
  REFRESH_TOKEN_TTL_SECONDS: string;
}

// ── Helpers ──────────────────────────────────────────────────────────────────

function b64url(buf: ArrayBuffer): string {
  return btoa(String.fromCharCode(...new Uint8Array(buf)))
    .replace(/\+/g, "-")
    .replace(/\//g, "_")
    .replace(/=+$/, "");
}

function b64urlDecode(str: string): Uint8Array {
  const b64 = str.replace(/-/g, "+").replace(/_/g, "/");
  return Uint8Array.from(atob(b64), (c) => c.charCodeAt(0));
}

async function hmacKey(secret: string): Promise<CryptoKey> {
  return crypto.subtle.importKey(
    "raw",
    new TextEncoder().encode(secret),
    { name: "HMAC", hash: "SHA-256" },
    false,
    ["sign", "verify"]
  );
}

async function signJwt(
  payload: Record<string, unknown>,
  secret: string
): Promise<string> {
  const header = { alg: "HS256", typ: "JWT" };
  const enc = new TextEncoder();
  const h = b64url(enc.encode(JSON.stringify(header)));
  const p = b64url(enc.encode(JSON.stringify(payload)));
  const data = `${h}.${p}`;
  const key = await hmacKey(secret);
  const sig = await crypto.subtle.sign("HMAC", key, enc.encode(data));
  return `${data}.${b64url(sig)}`;
}

async function verifyJwt(
  token: string,
  secret: string
): Promise<Record<string, unknown> | null> {
  const parts = token.split(".");
  if (parts.length !== 3) return null;

  const [h, p, s] = parts;
  const key = await hmacKey(secret);
  const valid = await crypto.subtle.verify(
    "HMAC",
    key,
    b64urlDecode(s),
    new TextEncoder().encode(`${h}.${p}`)
  );
  if (!valid) return null;

  const payload = JSON.parse(new TextDecoder().decode(b64urlDecode(p)));
  if (payload.exp && payload.exp < Math.floor(Date.now() / 1000)) return null;

  return payload;
}

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

function errorResponse(code: string, message: string, status: number): Response {
  return jsonResponse({ error: { code, message } }, status);
}

function requireApiKey(request: Request, env: Env): Response | null {
  const key = request.headers.get("x-api-key");
  if (!key || key !== env.WORKER_API_KEY) {
    return errorResponse("unauthorized", "Invalid or missing API key.", 401);
  }
  return null;
}

// ── Route handlers ────────────────────────────────────────────────────────────

async function handleIssue(request: Request, env: Env): Promise<Response> {
  const authError = requireApiKey(request, env);
  if (authError) return authError;

  let body: {
    userId?: string;
    clerkUserId?: string;
    username?: string;
    accountStatus?: string;
  };

  try {
    body = await request.json();
  } catch {
    return errorResponse("invalid_body", "Request body must be JSON.", 400);
  }

  const { userId, clerkUserId, username, accountStatus = "active" } = body;
  if (!userId || !clerkUserId) {
    return errorResponse("missing_fields", "userId and clerkUserId are required.", 400);
  }

  const now = Math.floor(Date.now() / 1000);
  const accessTtl = parseInt(env.ACCESS_TOKEN_TTL_SECONDS, 10) || 900;
  const refreshTtl = parseInt(env.REFRESH_TOKEN_TTL_SECONDS, 10) || 2592000;

  const accessToken = await signJwt(
    {
      iss: env.JWT_ISSUER,
      aud: env.JWT_AUDIENCE,
      sub: userId,
      clerk_id: clerkUserId,
      username: username ?? null,
      status: accountStatus,
      token_type: "access",
      iat: now,
      exp: now + accessTtl,
    },
    env.JWT_SECRET
  );

  // Refresh token carries a random jti so the backend can store and rotate it
  const jti = b64url(crypto.getRandomValues(new Uint8Array(32)));
  const refreshToken = await signJwt(
    {
      iss: env.JWT_ISSUER,
      aud: env.JWT_AUDIENCE,
      sub: userId,
      token_type: "refresh",
      jti,
      iat: now,
      exp: now + refreshTtl,
    },
    env.JWT_SECRET
  );

  return jsonResponse({
    accessToken,
    refreshToken,
    expiresIn: accessTtl,
    refreshExpiresIn: refreshTtl,
    tokenType: "Bearer",
  });
}

async function handleVerify(request: Request, env: Env): Promise<Response> {
  const authError = requireApiKey(request, env);
  if (authError) return authError;

  let body: { token?: string };
  try {
    body = await request.json();
  } catch {
    return errorResponse("invalid_body", "Request body must be JSON.", 400);
  }

  if (!body.token) {
    return errorResponse("missing_fields", "token is required.", 400);
  }

  const payload = await verifyJwt(body.token, env.JWT_SECRET);
  if (!payload) {
    return errorResponse("invalid_token", "Token is invalid or expired.", 401);
  }

  return jsonResponse({ valid: true, payload });
}

async function handleRefresh(request: Request, env: Env): Promise<Response> {
  const authError = requireApiKey(request, env);
  if (authError) return authError;

  let body: { refreshToken?: string; userId?: string; clerkUserId?: string; username?: string; accountStatus?: string };
  try {
    body = await request.json();
  } catch {
    return errorResponse("invalid_body", "Request body must be JSON.", 400);
  }

  if (!body.refreshToken) {
    return errorResponse("missing_fields", "refreshToken is required.", 400);
  }

  const payload = await verifyJwt(body.refreshToken, env.JWT_SECRET);
  if (!payload || payload["token_type"] !== "refresh") {
    return errorResponse("invalid_token", "Refresh token is invalid or expired.", 401);
  }

  const userId = (body.userId ?? payload["sub"]) as string;
  const clerkUserId = body.clerkUserId as string | undefined;
  const now = Math.floor(Date.now() / 1000);
  const accessTtl = parseInt(env.ACCESS_TOKEN_TTL_SECONDS, 10) || 900;
  const refreshTtl = parseInt(env.REFRESH_TOKEN_TTL_SECONDS, 10) || 2592000;

  const accessToken = await signJwt(
    {
      iss: env.JWT_ISSUER,
      aud: env.JWT_AUDIENCE,
      sub: userId,
      clerk_id: clerkUserId ?? payload["clerk_id"] ?? null,
      username: body.username ?? payload["username"] ?? null,
      status: body.accountStatus ?? "active",
      token_type: "access",
      iat: now,
      exp: now + accessTtl,
    },
    env.JWT_SECRET
  );

  // Rotate refresh token (new jti so backend can invalidate the old one)
  const jti = b64url(crypto.getRandomValues(new Uint8Array(32)));
  const newRefreshToken = await signJwt(
    {
      iss: env.JWT_ISSUER,
      aud: env.JWT_AUDIENCE,
      sub: userId,
      token_type: "refresh",
      jti,
      iat: now,
      exp: now + refreshTtl,
    },
    env.JWT_SECRET
  );

  return jsonResponse({
    accessToken,
    refreshToken: newRefreshToken,
    previousJti: payload["jti"],
    expiresIn: accessTtl,
    refreshExpiresIn: refreshTtl,
    tokenType: "Bearer",
  });
}

// ── Router ────────────────────────────────────────────────────────────────────

export default {
  async fetch(request: Request, env: Env): Promise<Response> {
    const url = new URL(request.url);
    const method = request.method.toUpperCase();

    if (method === "GET" && url.pathname === "/health") {
      return jsonResponse({ status: "ok", service: "momeants-jwt-api" });
    }

    if (method === "POST" && url.pathname === "/issue") {
      return handleIssue(request, env);
    }

    if (method === "POST" && url.pathname === "/verify") {
      return handleVerify(request, env);
    }

    if (method === "POST" && url.pathname === "/refresh") {
      return handleRefresh(request, env);
    }

    return errorResponse("not_found", "Route not found.", 404);
  },
};
