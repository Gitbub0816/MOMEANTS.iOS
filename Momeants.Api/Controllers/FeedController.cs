using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momeants.Api.Data;
using Momeants.Shared.Contracts;
using System.Text;

namespace Momeants.Api.Controllers;

[Route("api/feed")]
public class FeedController : BaseController
{
    private readonly AppDbContext _db;

    public FeedController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetFeed([FromQuery] string? cursor, [FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        limit = Math.Clamp(limit, 1, 50);

        // Decode cursor (opaque base64 of "rankScore:id")
        decimal? cursorScore = null;
        Guid? cursorId = null;
        if (!string.IsNullOrEmpty(cursor))
        {
            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
                var parts = decoded.Split(':');
                if (parts.Length == 2)
                {
                    cursorScore = decimal.Parse(parts[0]);
                    cursorId = Guid.Parse(parts[1]);
                }
            }
            catch { /* invalid cursor, start from beginning */ }
        }

        var query = _db.FeedItems
            .AsNoTracking()
            .Where(fi => fi.UserId == cu.UserId && fi.DismissedAt == null)
            .Include(fi => fi.Momeant).ThenInclude(m => m.Owner)
            .Include(fi => fi.Momeant).ThenInclude(m => m.PrimaryMedia);

        List<Domain.FeedItem> items;
        if (cursorScore.HasValue && cursorId.HasValue)
        {
            items = await query
                .Where(fi => fi.RankScore < cursorScore || (fi.RankScore == cursorScore && fi.Id.CompareTo(cursorId.Value) < 0))
                .OrderByDescending(fi => fi.RankScore).ThenByDescending(fi => fi.Id)
                .Take(limit + 1)
                .ToListAsync(ct);
        }
        else
        {
            items = await query
                .OrderByDescending(fi => fi.RankScore).ThenByDescending(fi => fi.Id)
                .Take(limit + 1)
                .ToListAsync(ct);
        }

        string? nextCursor = null;
        if (items.Count > limit)
        {
            items = items.Take(limit).ToList();
            var last = items[^1];
            nextCursor = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{last.RankScore}:{last.Id}"));
        }

        var dtos = items.Select(fi =>
        {
            var m = fi.Momeant;
            var owner = m.Owner;
            var ownerDto = new UserDto(owner.Id, owner.Username, owner.DisplayName, owner.FullName,
                null, owner.Bio, owner.AccountStatus, owner.ProfileVisibility, owner.CreatedAt);
            var media = m.PrimaryMedia;
            return new MomeantDto(m.Id, m.OwnerUserId, m.Caption, m.CapturedAt, m.PostedAt,
                m.LocationLabel, m.AudienceType, media is not null ? $"/api/media/{media.Id}" : "",
                media?.Blurhash, media?.Width ?? 0, media?.Height ?? 0,
                0, 0, null, ownerDto);
        }).ToList();

        return Ok(new FeedResponse(dtos, nextCursor));
    }

    [HttpPost("{feedItemId:guid}/seen")]
    public async Task<IActionResult> MarkSeen(Guid feedItemId, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var fi = await _db.FeedItems.FirstOrDefaultAsync(f => f.Id == feedItemId && f.UserId == cu.UserId, ct);
        if (fi is null) return NotFound();

        fi.SeenAt ??= DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpPost("{feedItemId:guid}/dismiss")]
    public async Task<IActionResult> Dismiss(Guid feedItemId, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var fi = await _db.FeedItems.FirstOrDefaultAsync(f => f.Id == feedItemId && f.UserId == cu.UserId, ct);
        if (fi is null) return NotFound();

        fi.DismissedAt ??= DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok();
    }
}
