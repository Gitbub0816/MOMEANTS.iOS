using System;

namespace Momeants.Api.Domain;

public class FeedItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid MomeantId { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public decimal RankScore { get; set; } = 0;
    public string? ReasonCode { get; set; }
    public DateTimeOffset InsertedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? SeenAt { get; set; }
    public DateTimeOffset? DismissedAt { get; set; }

    public User User { get; set; } = null!;
    public Momeant Momeant { get; set; } = null!;
}
