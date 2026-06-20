using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momeants.Api.Data;

namespace Momeants.Api.Controllers;

[Route("api/notifications")]
public class NotificationsController : BaseController
{
    private readonly AppDbContext _db;

    public NotificationsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int limit = 30, CancellationToken ct = default)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var list = await _db.Notifications.AsNoTracking()
            .Where(n => n.UserId == cu.UserId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

        return Ok(list.Select(n => new
        {
            n.Id, n.Type, n.Title, n.Body, n.DeepLink, n.ReadAt, n.CreatedAt, n.ActorUserId
        }));
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var n = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == cu.UserId, ct);
        if (n is null) return NotFound();

        n.ReadAt ??= DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var unread = await _db.Notifications
            .Where(n => n.UserId == cu.UserId && n.ReadAt == null)
            .ToListAsync(ct);

        var now = DateTimeOffset.UtcNow;
        foreach (var n in unread) n.ReadAt = now;
        await _db.SaveChangesAsync(ct);
        return Ok(new { marked = unread.Count });
    }
}
