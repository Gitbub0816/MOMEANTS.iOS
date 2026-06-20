using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momeants.Api.Data;
using Momeants.Api.Domain;

namespace Momeants.Api.Controllers;

[Route("api/important-people")]
public class ImportantPeopleController : BaseController
{
    private readonly AppDbContext _db;
    private static readonly HashSet<string> ValidCategories = new()
        { "mother", "father", "parent", "sibling", "partner", "child", "close_friend", "family", "mentor", "other" };

    public ImportantPeopleController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var list = await _db.ImportantPeople.AsNoTracking()
            .Where(p => p.UserId == cu.UserId)
            .OrderBy(p => p.Priority)
            .ToListAsync(ct);

        return Ok(list.Select(p => new
        {
            p.Id, p.DisplayName, p.Category, p.CustomLabel, p.PersonUserId, p.Priority, p.CreatedAt
        }));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ImportantPersonRequest req, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        if (!ValidCategories.Contains(req.Category))
            return ApiError("invalid_category", $"Category '{req.Category}' is not valid.");

        var person = new ImportantPerson
        {
            UserId = cu.UserId,
            PersonUserId = req.PersonUserId,
            DisplayName = req.DisplayName,
            Category = req.Category,
            CustomLabel = req.CustomLabel,
            Priority = req.Priority,
            NotesPrivate = req.NotesPrivate
        };
        _db.ImportantPeople.Add(person);
        await _db.SaveChangesAsync(ct);
        return StatusCode(201, new { id = person.Id });
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ImportantPersonRequest req, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var person = await _db.ImportantPeople.FirstOrDefaultAsync(p => p.Id == id && p.UserId == cu.UserId, ct);
        if (person is null) return NotFound();

        person.DisplayName = req.DisplayName;
        person.Category = req.Category;
        person.CustomLabel = req.CustomLabel;
        person.Priority = req.Priority;
        person.NotesPrivate = req.NotesPrivate;
        person.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(new { id = person.Id });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var person = await _db.ImportantPeople.FirstOrDefaultAsync(p => p.Id == id && p.UserId == cu.UserId, ct);
        if (person is null) return NotFound();

        _db.ImportantPeople.Remove(person);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record ImportantPersonRequest(
    string DisplayName,
    string Category,
    Guid? PersonUserId = null,
    string? CustomLabel = null,
    int Priority = 0,
    string? NotesPrivate = null);
