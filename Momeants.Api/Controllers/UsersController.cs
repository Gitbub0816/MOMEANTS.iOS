using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momeants.Api.Data;
using Momeants.Shared.Contracts;

namespace Momeants.Api.Controllers;

[Route("api/users")]
public class UsersController : BaseController
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db) => _db = db;

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == cu.UserId && u.DeletedAt == null, ct);
        if (user is null) return NotFound();

        return Ok(new UserDto(user.Id, user.Username, user.DisplayName, user.FullName,
            null, user.Bio, user.AccountStatus, user.ProfileVisibility, user.CreatedAt));
    }

    [HttpPatch("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserRequest req, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == cu.UserId && u.DeletedAt == null, ct);
        if (user is null) return NotFound();

        if (req.DisplayName is not null) user.DisplayName = req.DisplayName;
        if (req.Username is not null)
        {
            var taken = await _db.Users.AnyAsync(u => u.Username == req.Username && u.Id != user.Id, ct);
            if (taken) return ApiError("username_taken", "Username is already taken.");
            user.Username = req.Username;
        }
        if (req.Bio is not null) user.Bio = req.Bio;
        if (req.FullName is not null) user.FullName = req.FullName;
        if (req.City is not null) user.City = req.City;
        if (req.Region is not null) user.Region = req.Region;
        if (req.Country is not null) user.Country = req.Country;
        if (req.ProfileVisibility is not null) user.ProfileVisibility = req.ProfileVisibility;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return Ok(new UserDto(user.Id, user.Username, user.DisplayName, user.FullName,
            null, user.Bio, user.AccountStatus, user.ProfileVisibility, user.CreatedAt));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken ct)
    {
        var authResult = RequireAuth(out _);
        if (authResult is not null) return authResult;

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null, ct);
        if (user is null) return NotFound();

        return Ok(new UserDto(user.Id, user.Username, user.DisplayName, user.FullName,
            null, user.Bio, user.AccountStatus, user.ProfileVisibility, user.CreatedAt));
    }

    [HttpGet("by-username/{username}")]
    public async Task<IActionResult> GetByUsername(string username, CancellationToken ct)
    {
        var authResult = RequireAuth(out _);
        if (authResult is not null) return authResult;

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username && u.DeletedAt == null, ct);
        if (user is null) return NotFound();

        return Ok(new UserDto(user.Id, user.Username, user.DisplayName, user.FullName,
            null, user.Bio, user.AccountStatus, user.ProfileVisibility, user.CreatedAt));
    }

    [HttpPost("check-username")]
    public async Task<IActionResult> CheckUsername([FromBody] CheckUsernameRequest req, CancellationToken ct)
    {
        var taken = await _db.Users.AnyAsync(u => u.Username == req.Username && u.DeletedAt == null, ct);
        return Ok(new { available = !taken });
    }
}

public record UpdateUserRequest(
    string? DisplayName,
    string? Username,
    string? FullName,
    string? Bio,
    string? City,
    string? Region,
    string? Country,
    string? ProfileVisibility);

public record CheckUsernameRequest(string Username);
