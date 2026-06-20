using Microsoft.AspNetCore.Mvc;
using Momeants.Api.Data;
using Momeants.Api.Domain;

namespace Momeants.Api.Controllers;

[Route("api/reports")]
public class ReportsController : BaseController
{
    private readonly AppDbContext _db;

    public ReportsController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> CreateReport([FromBody] CreateReportRequest req, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        if (string.IsNullOrWhiteSpace(req.Reason))
            return ApiError("validation_failed", "Reason is required.");

        var report = new Report
        {
            ReporterUserId = cu.UserId,
            TargetType = req.TargetType,
            TargetId = req.TargetId,
            Reason = req.Reason,
            Details = req.Details
        };
        _db.Reports.Add(report);
        await _db.SaveChangesAsync(ct);
        return StatusCode(201, new { id = report.Id });
    }
}

public record CreateReportRequest(string TargetType, Guid TargetId, string Reason, string? Details = null);
