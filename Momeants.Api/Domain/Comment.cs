using System;

namespace Momeants.Api.Domain;

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MomeantId { get; set; }
    public Guid UserId { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeletedAt { get; set; }

    public Momeant Momeant { get; set; } = null!;
    public User User { get; set; } = null!;
}
