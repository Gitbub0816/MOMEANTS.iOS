namespace Momeants.Api.Services;

public record ClerkStartResult(bool Success, string? Error);
public record ClerkVerifyResult(bool Success, string? ClerkUserId, string? Email, string? PhoneNumber, string? Error);

public interface IClerkService
{
    Task<ClerkStartResult> StartEmailVerificationAsync(string email, CancellationToken ct = default);
    Task<ClerkStartResult> StartPhoneVerificationAsync(string phoneE164, CancellationToken ct = default);
    Task<ClerkVerifyResult> VerifyCodeAsync(string identifier, string code, CancellationToken ct = default);
    Task<ClerkVerifyResult> GetUserAsync(string clerkUserId, CancellationToken ct = default);
}
