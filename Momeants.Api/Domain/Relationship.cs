using System;

namespace Momeants.Api.Domain;

public class Relationship
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RequesterUserId { get; set; }
    public Guid AddresseeUserId { get; set; }
    public string Status { get; set; } = "pending";
    public string? RelationshipLabel { get; set; }
    public decimal ClosenessScore { get; set; } = 0;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User Requester { get; set; } = null!;
    public User Addressee { get; set; } = null!;
}
