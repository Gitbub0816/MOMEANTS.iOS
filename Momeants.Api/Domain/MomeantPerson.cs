using System;

namespace Momeants.Api.Domain;

public class MomeantPerson
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MomeantId { get; set; }
    public Guid? PersonUserId { get; set; }
    public string? DisplayName { get; set; }
    public string? RelationshipToOwner { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Momeant Momeant { get; set; } = null!;
    public User? PersonUser { get; set; }
}
