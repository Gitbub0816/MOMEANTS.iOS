using System;

namespace Momeants.Api.Domain;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ClerkUserId { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public Guid? AvatarMediaId { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public string? Bio { get; set; }
    public string AccountStatus { get; set; } = "active";
    public string ProfileVisibility { get; set; } = "friends";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<Momeant> Momeants { get; set; } = new List<Momeant>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
