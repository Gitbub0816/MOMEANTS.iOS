using Microsoft.Extensions.Options;
using Momeants.Api.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Momeants.Api.Services;

public class ClerkService : IClerkService
{
    private readonly HttpClient _http;
    private readonly ILogger<ClerkService> _logger;

    public ClerkService(HttpClient http, IOptions<ClerkOptions> opts, ILogger<ClerkService> logger)
    {
        _http = http;
        _logger = logger;
        _http.BaseAddress = new Uri(opts.Value.BaseUrl);
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", opts.Value.SecretKey);
    }

    public async Task<ClerkStartResult> StartEmailVerificationAsync(string email, CancellationToken ct = default)
    {
        try
        {
            // Create a sign-in attempt via Clerk backend API
            var body = JsonSerializer.Serialize(new
            {
                identifier = email.ToLowerInvariant(),
                strategy = "email_code"
            });
            var response = await _http.PostAsync("sign_ins",
                new StringContent(body, Encoding.UTF8, "application/json"), ct);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Clerk start email failed: {Status} {Body}", response.StatusCode, err);
                return new ClerkStartResult(false, "Failed to start verification");
            }

            // After creating sign-in, prepare the first factor
            var content = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(content);
            var signInId = doc.RootElement.GetProperty("id").GetString();

            var prepareBody = JsonSerializer.Serialize(new
            {
                strategy = "email_code",
                email_address_id = GetEmailAddressId(doc.RootElement)
            });
            await _http.PostAsync($"sign_ins/{signInId}/prepare_first_factor",
                new StringContent(prepareBody, Encoding.UTF8, "application/json"), ct);

            return new ClerkStartResult(true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ClerkService.StartEmailVerification error");
            return new ClerkStartResult(false, "Internal error");
        }
    }

    public async Task<ClerkStartResult> StartPhoneVerificationAsync(string phoneE164, CancellationToken ct = default)
    {
        try
        {
            var body = JsonSerializer.Serialize(new
            {
                identifier = phoneE164,
                strategy = "phone_code"
            });
            var response = await _http.PostAsync("sign_ins",
                new StringContent(body, Encoding.UTF8, "application/json"), ct);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Clerk start phone failed: {Status} {Body}", response.StatusCode, err);
                return new ClerkStartResult(false, "Failed to start verification");
            }

            return new ClerkStartResult(true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ClerkService.StartPhoneVerification error");
            return new ClerkStartResult(false, "Internal error");
        }
    }

    public async Task<ClerkVerifyResult> VerifyCodeAsync(string identifier, string code, CancellationToken ct = default)
    {
        try
        {
            // Look up the sign_in by identifier, then attempt the code
            var body = JsonSerializer.Serialize(new
            {
                identifier,
                strategy = identifier.Contains('@') ? "email_code" : "phone_code",
                code
            });
            // Attempt first factor on most recent sign_in
            // In production you'd store the sign_in id from start phase
            var listResp = await _http.GetAsync("sign_ins?limit=1", ct);
            if (!listResp.IsSuccessStatusCode)
                return new ClerkVerifyResult(false, null, null, null, "Could not retrieve sign-in");

            var listContent = await listResp.Content.ReadAsStringAsync(ct);
            using var listDoc = JsonDocument.Parse(listContent);
            var signIns = listDoc.RootElement.GetProperty("data");
            if (signIns.GetArrayLength() == 0)
                return new ClerkVerifyResult(false, null, null, null, "No active sign-in");

            var signInId = signIns[0].GetProperty("id").GetString();

            var attemptBody = JsonSerializer.Serialize(new { strategy = "email_code", code });
            var resp = await _http.PostAsync($"sign_ins/{signInId}/attempt_first_factor",
                new StringContent(attemptBody, Encoding.UTF8, "application/json"), ct);

            if (!resp.IsSuccessStatusCode)
                return new ClerkVerifyResult(false, null, null, null, "Invalid code");

            var content = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(content);
            var clerkUserId = doc.RootElement.TryGetProperty("created_session_id", out var s) ? s.GetString() : null;
            // Extract user id from the sign-in status
            var userId = doc.RootElement.TryGetProperty("user_id", out var uid) ? uid.GetString() : null;

            return new ClerkVerifyResult(true, userId, identifier.Contains('@') ? identifier : null,
                identifier.Contains('@') ? null : identifier, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ClerkService.VerifyCode error");
            return new ClerkVerifyResult(false, null, null, null, "Internal error");
        }
    }

    public async Task<ClerkVerifyResult> GetUserAsync(string clerkUserId, CancellationToken ct = default)
    {
        try
        {
            var resp = await _http.GetAsync($"users/{clerkUserId}", ct);
            if (!resp.IsSuccessStatusCode)
                return new ClerkVerifyResult(false, null, null, null, "User not found");

            var content = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            string? email = null;
            if (root.TryGetProperty("email_addresses", out var emails) && emails.GetArrayLength() > 0)
                email = emails[0].GetProperty("email_address").GetString();

            string? phone = null;
            if (root.TryGetProperty("phone_numbers", out var phones) && phones.GetArrayLength() > 0)
                phone = phones[0].GetProperty("phone_number").GetString();

            return new ClerkVerifyResult(true, clerkUserId, email, phone, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ClerkService.GetUser error");
            return new ClerkVerifyResult(false, null, null, null, "Internal error");
        }
    }

    private static string? GetEmailAddressId(JsonElement root)
    {
        if (root.TryGetProperty("supported_first_factors", out var factors))
        {
            foreach (var f in factors.EnumerateArray())
            {
                if (f.TryGetProperty("strategy", out var strat) && strat.GetString() == "email_code")
                    return f.TryGetProperty("email_address_id", out var id) ? id.GetString() : null;
            }
        }
        return null;
    }
}
