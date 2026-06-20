using System;

namespace Momeants.Api.Domain;

public class Report
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReporterUserId { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public Guid TargetId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string Status { get; set; } = "open";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ResolvedAt { get; set; }

    public User Reporter { get; set; } = null!;
}
