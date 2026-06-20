using System;

namespace Momeants.Api.Domain;

public class Momeant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerUserId { get; set; }
    public Guid PrimaryMediaId { get; set; }
    public string? Caption { get; set; }
    public DateTimeOffset? CapturedAt { get; set; }
    public DateTimeOffset PostedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? LocationLabel { get; set; }
    public decimal? LocationLat { get; set; }
    public decimal? LocationLng { get; set; }
    public string AudienceType { get; set; } = "friends";
    public decimal SignificanceScore { get; set; } = 0;
    public decimal MemoryScore { get; set; } = 0;
    public bool CommentsEnabled { get; set; } = true;
    public bool ResharingAllowed { get; set; } = false;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeletedAt { get; set; }

    public User Owner { get; set; } = null!;
    public MediaObject? PrimaryMedia { get; set; }
    public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<MomeantPerson> TaggedPeople { get; set; } = new List<MomeantPerson>();
}
