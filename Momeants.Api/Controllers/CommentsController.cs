using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momeants.Api.Data;
using Momeants.Api.Domain;
using Momeants.Shared.Contracts;

namespace Momeants.Api.Controllers;

[Route("api")]
public class CommentsController : BaseController
{
    private readonly AppDbContext _db;

    public CommentsController(AppDbContext db) => _db = db;

    [HttpGet("momeants/{momeantId:guid}/comments")]
    public async Task<IActionResult> GetComments(Guid momeantId, [FromQuery] string? cursor, [FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var authResult = RequireAuth(out _);
        if (authResult is not null) return authResult;

        limit = Math.Clamp(limit, 1, 50);

        var comments = await _db.Comments
            .AsNoTracking()
            .Include(c => c.User)
            .Where(c => c.MomeantId == momeantId && c.DeletedAt == null)
            .OrderByDescending(c => c.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

        var dtos = comments.Select(c => new CommentDto(
            c.Id, c.MomeantId, c.Body, c.CreatedAt,
            new UserDto(c.User.Id, c.User.Username, c.User.DisplayName, c.User.FullName,
                null, c.User.Bio, c.User.AccountStatus, c.User.ProfileVisibility, c.User.CreatedAt)
        )).ToList();

        return Ok(dtos);
    }

    [HttpPost("momeants/{momeantId:guid}/comments")]
    public async Task<IActionResult> CreateComment(Guid momeantId, [FromBody] CreateCommentRequest req, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        if (string.IsNullOrWhiteSpace(req.Body))
            return ApiError("validation_failed", "Comment body is required.");

        var momeant = await _db.Momeants.FirstOrDefaultAsync(m => m.Id == momeantId && m.DeletedAt == null && m.CommentsEnabled, ct);
        if (momeant is null) return NotFound();

        var comment = new Comment
        {
            MomeantId = momeantId,
            UserId = cu.UserId,
            Body = req.Body.Trim()
        };
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync(ct);

        return StatusCode(201, new { id = comment.Id });
    }

    [HttpPatch("comments/{id:guid}")]
    public async Task<IActionResult> UpdateComment(Guid id, [FromBody] CreateCommentRequest req, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == id && c.UserId == cu.UserId && c.DeletedAt == null, ct);
        if (comment is null) return NotFound();

        comment.Body = req.Body.Trim();
        comment.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(new { id = comment.Id });
    }

    [HttpDelete("comments/{id:guid}")]
    public async Task<IActionResult> DeleteComment(Guid id, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == id && c.UserId == cu.UserId && c.DeletedAt == null, ct);
        if (comment is null) return NotFound();

        comment.DeletedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
