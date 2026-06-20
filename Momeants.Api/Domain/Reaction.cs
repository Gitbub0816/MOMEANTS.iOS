using System;

namespace Momeants.Api.Domain;

public class Reaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MomeantId { get; set; }
    public Guid UserId { get; set; }
    public string ReactionType { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Momeant Momeant { get; set; } = null!;
    public User User { get; set; } = null!;
}
