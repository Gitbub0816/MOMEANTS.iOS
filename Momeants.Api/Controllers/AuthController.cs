using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momeants.Api.Data;
using Momeants.Api.Domain;
using Momeants.Api.Services;
using Momeants.Shared.Contracts;

namespace Momeants.Api.Controllers;

[Route("api/auth")]
public class AuthController : BaseController
{
    private readonly IClerkService _clerk;
    private readonly ITokenService _tokens;
    private readonly AppDbContext _db;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IClerkService clerk, ITokenService tokens, AppDbContext db, ILogger<AuthController> logger)
    {
        _clerk = clerk;
        _tokens = tokens;
        _db = db;
        _logger = logger;
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartAuthRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Identifier))
            return ApiError("validation_failed", "Identifier is required.");

        ClerkStartResult result;
        if (req.Identifier.Contains('@'))
            result = await _clerk.StartEmailVerificationAsync(req.Identifier.Trim().ToLowerInvariant(), ct);
        else
            result = await _clerk.StartPhoneVerificationAsync(req.Identifier.Trim(), ct);

        if (!result.Success)
            return ApiError("clerk_error", result.Error ?? "Failed to start verification.");

        return Ok(new { message = "Verification code sent." });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyAuthRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Identifier) || string.IsNullOrWhiteSpace(req.Code))
            return ApiError("validation_failed", "Identifier and code are required.");

        var result = await _clerk.VerifyCodeAsync(req.Identifier, req.Code, ct);
        if (!result.Success || result.ClerkUserId is null)
            return ApiError("invalid_code", result.Error ?? "Invalid or expired verification code.", 401);

        // Upsert user
        var user = await _db.Users.FirstOrDefaultAsync(u => u.ClerkUserId == result.ClerkUserId, ct);
        bool isNew = false;
        if (user is null)
        {
            isNew = true;
            user = new User
            {
                ClerkUserId = result.ClerkUserId,
                DisplayName = result.Email?.Split('@')[0] ?? "User",
                AccountStatus = "active"
            };
            _db.Users.Add(user);
        }
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Auth verify success for user {UserId} (new={IsNew})", user.Id, isNew);

        var tokenPair = _tokens.GenerateTokens(user, req.DeviceId);

        // Store refresh token hash
        var rt = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokens.HashRefreshToken(tokenPair.RefreshToken),
            DeviceId = req.DeviceId,
            ExpiresAt = tokenPair.RefreshTokenExpiry
        };
        _db.RefreshTokens.Add(rt);
        await _db.SaveChangesAsync(ct);

        var userDto = ToUserDto(user);
        return Ok(new AuthResponse(tokenPair.AccessToken, tokenPair.RefreshToken,
            tokenPair.AccessTokenExpiry, tokenPair.RefreshTokenExpiry, userDto));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.RefreshToken))
            return ApiError("validation_failed", "Refresh token is required.");

        var hash = _tokens.HashRefreshToken(req.RefreshToken);
        var existing = await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == hash && r.RevokedAt == null && r.ExpiresAt > DateTimeOffset.UtcNow, ct);

        if (existing is null)
            return ApiError("invalid_token", "Invalid or expired refresh token.", 401);

        // Rotate: revoke old, issue new
        existing.RevokedAt = DateTimeOffset.UtcNow;
        var newPair = _tokens.GenerateTokens(existing.User, req.DeviceId);
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = existing.UserId,
            TokenHash = _tokens.HashRefreshToken(newPair.RefreshToken),
            DeviceId = req.DeviceId,
            ExpiresAt = newPair.RefreshTokenExpiry
        });
        await _db.SaveChangesAsync(ct);

        return Ok(new AuthResponse(newPair.AccessToken, newPair.RefreshToken,
            newPair.AccessTokenExpiry, newPair.RefreshTokenExpiry, ToUserDto(existing.User)));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest req, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(req.RefreshToken))
        {
            var hash = _tokens.HashRefreshToken(req.RefreshToken);
            var rt = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash, ct);
            if (rt is not null)
            {
                rt.RevokedAt = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync(ct);
            }
        }
        return Ok(new { message = "Logged out." });
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var authResult = RequireAuth(out var currentUser);
        if (authResult is not null) return authResult;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == currentUser.UserId, ct);
        if (user is null) return NotFound();

        return Ok(ToUserDto(user));
    }

    private static UserDto ToUserDto(User u) => new(
        u.Id, u.Username, u.DisplayName, u.FullName,
        null, u.Bio, u.AccountStatus, u.ProfileVisibility, u.CreatedAt);
}
