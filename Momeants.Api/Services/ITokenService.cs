using Momeants.Api.Domain;

namespace Momeants.Api.Services;

public record TokenPair(string AccessToken, string RefreshToken, DateTimeOffset AccessTokenExpiry, DateTimeOffset RefreshTokenExpiry);

public interface ITokenService
{
    TokenPair GenerateTokens(User user, string? deviceId = null);
    bool ValidateAccessToken(string token, out Guid userId, out string clerkUserId);
    string HashRefreshToken(string rawToken);
}
