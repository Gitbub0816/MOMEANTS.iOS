using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momeants.Api.Data;
using Momeants.Api.Domain;
using Momeants.Shared.Contracts;

namespace Momeants.Api.Controllers;

[Route("api/momeants/{momeantId:guid}/reactions")]
public class ReactionsController : BaseController
{
    private readonly AppDbContext _db;
    private static readonly HashSet<string> AllowedTypes = new() { "like", "dislike", "super_like", "heart", "hidden" };

    public ReactionsController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> React(Guid momeantId, [FromBody] ReactionRequest req, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        if (!AllowedTypes.Contains(req.Type))
            return ApiError("invalid_reaction", $"Reaction type '{req.Type}' is not allowed.");

        var momeant = await _db.Momeants.FirstOrDefaultAsync(m => m.Id == momeantId && m.DeletedAt == null, ct);
        if (momeant is null) return NotFound();

        var existing = await _db.Reactions.FirstOrDefaultAsync(r => r.MomeantId == momeantId && r.UserId == cu.UserId && r.ReactionType == req.Type, ct);
        if (existing is not null) return Ok(new { id = existing.Id });

        var reaction = new Reaction
        {
            MomeantId = momeantId,
            UserId = cu.UserId,
            ReactionType = req.Type
        };
        _db.Reactions.Add(reaction);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(React), new { momeantId }, new { id = reaction.Id });
    }

    [HttpDelete("{type}")]
    public async Task<IActionResult> Unreact(Guid momeantId, string type, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var reaction = await _db.Reactions.FirstOrDefaultAsync(r => r.MomeantId == momeantId && r.UserId == cu.UserId && r.ReactionType == type, ct);
        if (reaction is null) return NotFound();

        _db.Reactions.Remove(reaction);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
