using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momeants.Api.Data;
using Momeants.Api.Domain;
using Momeants.Shared.Contracts;

namespace Momeants.Api.Controllers;

[Route("api/momeants")]
public class MomeantsController : BaseController
{
    private readonly AppDbContext _db;

    public MomeantsController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMomeantRequest req, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        // Verify media belongs to user
        var media = await _db.MediaObjects.FirstOrDefaultAsync(m => m.Id == req.PrimaryMediaId && m.OwnerUserId == cu.UserId && m.DeletedAt == null, ct);
        if (media is null)
            return ApiError("media_not_found", "Primary media not found or not owned by you.");

        var momeant = new Momeant
        {
            OwnerUserId = cu.UserId,
            PrimaryMediaId = req.PrimaryMediaId,
            Caption = req.Caption,
            CapturedAt = req.CapturedAt,
            AudienceType = req.AudienceType,
            LocationLabel = req.LocationLabel,
            LocationLat = req.LocationLat,
            LocationLng = req.LocationLng,
            CommentsEnabled = req.CommentsEnabled,
            ResharingAllowed = req.ResharingAllowed
        };

        _db.Momeants.Add(momeant);

        if (req.TaggedPeople is not null)
        {
            foreach (var tp in req.TaggedPeople)
            {
                _db.MomeantPeople.Add(new MomeantPerson
                {
                    MomeantId = momeant.Id,
                    PersonUserId = tp.PersonUserId,
                    DisplayName = tp.DisplayName,
                    RelationshipToOwner = tp.RelationshipToOwner
                });
            }
        }

        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = momeant.Id }, await BuildDto(momeant, cu.UserId, media, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var momeant = await _db.Momeants
            .Include(m => m.Owner)
            .Include(m => m.PrimaryMedia)
            .FirstOrDefaultAsync(m => m.Id == id && m.DeletedAt == null, ct);

        if (momeant is null) return NotFound();

        return Ok(await BuildDto(momeant, cu.UserId, momeant.PrimaryMedia, ct));
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMomeantRequest req, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var momeant = await _db.Momeants.FirstOrDefaultAsync(m => m.Id == id && m.OwnerUserId == cu.UserId && m.DeletedAt == null, ct);
        if (momeant is null) return NotFound();

        if (req.Caption is not null) momeant.Caption = req.Caption;
        if (req.AudienceType is not null) momeant.AudienceType = req.AudienceType;
        if (req.CommentsEnabled.HasValue) momeant.CommentsEnabled = req.CommentsEnabled.Value;
        momeant.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return Ok(new { id = momeant.Id });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var momeant = await _db.Momeants.FirstOrDefaultAsync(m => m.Id == id && m.OwnerUserId == cu.UserId && m.DeletedAt == null, ct);
        if (momeant is null) return NotFound();

        momeant.DeletedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task<MomeantDto> BuildDto(Momeant m, Guid viewerId, Domain.MediaObject? media, CancellationToken ct)
    {
        var reactionCount = await _db.Reactions.CountAsync(r => r.MomeantId == m.Id, ct);
        var commentCount = await _db.Comments.CountAsync(c => c.MomeantId == m.Id && c.DeletedAt == null, ct);
        var viewerReaction = await _db.Reactions
            .Where(r => r.MomeantId == m.Id && r.UserId == viewerId)
            .Select(r => r.ReactionType)
            .FirstOrDefaultAsync(ct);

        var owner = m.Owner ?? await _db.Users.AsNoTracking().FirstAsync(u => u.Id == m.OwnerUserId, ct);
        var ownerDto = new UserDto(owner.Id, owner.Username, owner.DisplayName, owner.FullName,
            null, owner.Bio, owner.AccountStatus, owner.ProfileVisibility, owner.CreatedAt);

        var mediaUrl = media is not null ? $"/api/media/{media.Id}" : string.Empty;

        return new MomeantDto(m.Id, m.OwnerUserId, m.Caption, m.CapturedAt, m.PostedAt,
            m.LocationLabel, m.AudienceType, mediaUrl, media?.Blurhash,
            media?.Width ?? 0, media?.Height ?? 0,
            reactionCount, commentCount, viewerReaction, ownerDto);
    }
}

public record UpdateMomeantRequest(string? Caption, string? AudienceType, bool? CommentsEnabled);
