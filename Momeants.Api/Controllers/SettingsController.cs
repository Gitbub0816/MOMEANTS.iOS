using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momeants.Api.Data;

namespace Momeants.Api.Controllers;

[Route("api/settings")]
public class SettingsController : BaseController
{
    private readonly AppDbContext _db;

    public SettingsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == cu.UserId, ct);
        if (user is null) return NotFound();

        return Ok(new
        {
            profileVisibility = user.ProfileVisibility,
            accountStatus = user.AccountStatus
        });
    }

    [HttpPatch]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest req, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == cu.UserId, ct);
        if (user is null) return NotFound();

        if (req.ProfileVisibility is not null)
        {
            var allowed = new[] { "public", "friends", "close_friends", "private" };
            if (!allowed.Contains(req.ProfileVisibility))
                return ApiError("invalid_value", "Invalid profile visibility setting.");
            user.ProfileVisibility = req.ProfileVisibility;
        }
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(new { profileVisibility = user.ProfileVisibility });
    }
}

[Route("api/account")]
public class AccountController : BaseController
{
    private readonly AppDbContext _db;

    public AccountController(AppDbContext db) => _db = db;

    [HttpPost("delete-request")]
    public async Task<IActionResult> DeleteRequest(CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == cu.UserId, ct);
        if (user is null) return NotFound();

        user.AccountStatus = "pending_deletion";
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Account deletion requested. Your account will be deleted within 30 days." });
    }
}

public record UpdateSettingsRequest(string? ProfileVisibility);
