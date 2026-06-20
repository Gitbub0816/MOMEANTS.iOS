using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Momeants.Api.Domain;
using Momeants.Api.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Momeants.Api.Services;

public class TokenService : ITokenService
{
    private readonly JwtOptions _opts;

    public TokenService(IOptions<JwtOptions> opts)
    {
        _opts = opts.Value;
    }

    public TokenPair GenerateTokens(User user, string? deviceId = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var accessExpiry = DateTimeOffset.UtcNow.AddMinutes(_opts.AccessTokenMinutes);
        var refreshExpiry = DateTimeOffset.UtcNow.AddDays(_opts.RefreshTokenDays);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("clerk_user_id", user.ClerkUserId),
            new("account_status", user.AccountStatus),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        if (deviceId is not null)
            claims.Add(new Claim("device_id", deviceId));

        var token = new JwtSecurityToken(
            issuer: _opts.Issuer,
            audience: _opts.Audience,
            claims: claims,
            expires: accessExpiry.UtcDateTime,
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return new TokenPair(accessToken, refreshToken, accessExpiry, refreshExpiry);
    }

    public bool ValidateAccessToken(string token, out Guid userId, out string clerkUserId)
    {
        userId = Guid.Empty;
        clerkUserId = string.Empty;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.SecretKey));
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _opts.Issuer,
                ValidateAudience = true,
                ValidAudience = _opts.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            // IdentityModel may map "sub" to the URI form; check both
            var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var ckId = principal.FindFirst("clerk_user_id")?.Value;

            if (sub is null || ckId is null) return false;

            userId = Guid.Parse(sub);
            clerkUserId = ckId;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string HashRefreshToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
