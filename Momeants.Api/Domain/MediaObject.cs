using System;

namespace Momeants.Api.Domain;

public class MediaObject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerUserId { get; set; }
    public string R2Bucket { get; set; } = string.Empty;
    public string R2ObjectKey { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long ByteSize { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? DurationMs { get; set; }
    public string? Sha256Hex { get; set; }
    public string? Blurhash { get; set; }
    public string ModerationStatus { get; set; } = "pending";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeletedAt { get; set; }

    public User Owner { get; set; } = null!;
}
