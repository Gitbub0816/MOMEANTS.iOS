# Native Authentication With Clerk Backend Only

Momeants must feel native. Do not use web redirect login as the primary iOS experience. Clerk can still be the identity provider, but it must be mediated through the backend and native UI.

## Goal

The user sees native MAUI screens:
- Welcome
- Sign in
- Create account
- Verify email/phone
- Profile setup

They should not be thrown into an external browser OAuth-style flow unless Apple/Google provider policy requires a system-auth session.

## Critical rule

The MAUI app never stores or uses Clerk secret keys. The backend owns all Clerk secret operations.

## Recommended flow

### Email/password or phone code flow

1. User enters email or phone in native MAUI screen.
2. MAUI app calls Momeants backend:
   `POST /api/auth/start`
3. Backend calls Clerk server API as needed.
4. Clerk sends OTP or verification email/SMS.
5. User enters code in native MAUI screen.
6. MAUI app calls:
   `POST /api/auth/verify`
7. Backend verifies with Clerk.
8. Backend creates or updates local `users` row in Neon.
9. Backend returns a Momeants API session token or a verified Clerk session token strategy.
10. MAUI stores only the app session token in iOS Keychain.

## Token strategy

Preferred simple version:
- Backend verifies Clerk identity.
- Backend issues its own short-lived JWT access token and long-lived refresh token.
- Access token lifetime: 15 minutes.
- Refresh token lifetime: 30 days with rotation.
- Store refresh token hash in Neon.
- Store tokens in iOS Keychain.

Alternative:
- Use Clerk session JWTs, but validate them on every backend request.
- Still create a local user record in Neon.

## Backend endpoints

```text
POST /api/auth/start
POST /api/auth/verify
POST /api/auth/refresh
POST /api/auth/logout
GET  /api/auth/me
```

## Native UI rules

- All auth screens are MAUI ContentPages.
- No embedded WebView for password entry.
- No raw Clerk UI components in app.
- Apple Sign In must use native Apple authentication APIs where required.
- Google Sign In may require secure system browser depending on provider rules, but result still returns to the app and backend verifies identity.

## User table mapping

Map Clerk user to Neon user:

```text
users.clerk_user_id = Clerk user id
users.email = normalized email if available
users.phone_e164 = normalized phone if available
users.account_status = active/pending/suspended/deleted
```

## Security rules

- Use HTTPS only.
- Store tokens in Keychain only.
- Never store auth tokens in Preferences, SQLite plaintext, logs, analytics, or crash reports.
- Rotate refresh tokens on use.
- Revoke refresh tokens on logout.
- Invalidate tokens after password reset or account compromise.
- Rate limit auth endpoints by IP, device id, and account identifier.
- Log auth events in `audit_events`.

## Clerk setup notes

- Use Clerk backend API secret only in backend environment variables.
- Do not put Clerk secret in MAUI app.
- Public Clerk publishable keys should be avoided unless using a Clerk-supported native SDK flow that does not expose secrets.
- If a native Clerk SDK is used later, it must still route final authorization through backend verification.

## Implementation note for Codex

Build an `IAuthService` in MAUI and an `AuthController` in ASP.NET Core. Keep provider-specific code behind `IIdentityProvider`. The app should not know whether identity is Clerk, Auth0, or custom later.
