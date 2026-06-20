using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momeants.Api.Data;
using Momeants.Api.Domain;

namespace Momeants.Api.Controllers;

[Route("api/relationships")]
public class RelationshipsController : BaseController
{
    private readonly AppDbContext _db;

    public RelationshipsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetRelationships([FromQuery] string? status, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var query = _db.Relationships.AsNoTracking()
            .Where(r => r.RequesterUserId == cu.UserId || r.AddresseeUserId == cu.UserId);
        if (status is not null) query = query.Where(r => r.Status == status);

        var list = await query.ToListAsync(ct);
        return Ok(list.Select(r => new { r.Id, r.RequesterUserId, r.AddresseeUserId, r.Status, r.RelationshipLabel, r.CreatedAt }));
    }

    [HttpPost("request")]
    public new async Task<IActionResult> Request([FromBody] RelationshipRequestDto req, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        if (cu.UserId == req.AddresseeUserId)
            return ApiError("invalid_request", "Cannot send a relationship request to yourself.");

        var exists = await _db.Relationships.AnyAsync(r =>
            (r.RequesterUserId == cu.UserId && r.AddresseeUserId == req.AddresseeUserId) ||
            (r.RequesterUserId == req.AddresseeUserId && r.AddresseeUserId == cu.UserId), ct);

        if (exists) return ApiError("already_exists", "A relationship with this user already exists.");

        var rel = new Relationship
        {
            RequesterUserId = cu.UserId,
            AddresseeUserId = req.AddresseeUserId,
            Status = "pending"
        };
        _db.Relationships.Add(rel);
        await _db.SaveChangesAsync(ct);
        return StatusCode(201, new { id = rel.Id });
    }

    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var rel = await _db.Relationships.FirstOrDefaultAsync(r => r.Id == id && r.AddresseeUserId == cu.UserId && r.Status == "pending", ct);
        if (rel is null) return NotFound();

        rel.Status = "accepted";
        rel.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(new { id = rel.Id, status = rel.Status });
    }

    [HttpPost("{id:guid}/decline")]
    public async Task<IActionResult> Decline(Guid id, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var rel = await _db.Relationships.FirstOrDefaultAsync(r => r.Id == id && r.AddresseeUserId == cu.UserId && r.Status == "pending", ct);
        if (rel is null) return NotFound();

        rel.Status = "declined";
        rel.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(new { id = rel.Id, status = rel.Status });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remove(Guid id, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var rel = await _db.Relationships.FirstOrDefaultAsync(r => r.Id == id &&
            (r.RequesterUserId == cu.UserId || r.AddresseeUserId == cu.UserId), ct);
        if (rel is null) return NotFound();

        _db.Relationships.Remove(rel);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record RelationshipRequestDto(Guid AddresseeUserId, string? Label = null);
