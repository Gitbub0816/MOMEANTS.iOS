namespace Momeants.Shared.Contracts;

public record StartAuthRequest(string Identifier, string Method = "email_code");

public record VerifyAuthRequest(string Identifier, string Code, string? DeviceId = null);

public record RefreshTokenRequest(string RefreshToken, string? DeviceId = null);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiry,
    DateTimeOffset RefreshTokenExpiry,
    UserDto User);

public record UserDto(
    Guid Id,
    string? Username,
    string DisplayName,
    string? FullName,
    string? AvatarUrl,
    string? Bio,
    string AccountStatus,
    string ProfileVisibility,
    DateTimeOffset CreatedAt);
