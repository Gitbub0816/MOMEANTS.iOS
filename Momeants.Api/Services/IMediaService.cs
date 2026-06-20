namespace Momeants.Api.Services;

public record UploadRequest(Guid MediaId, string UploadUrl, string ObjectKey, DateTimeOffset ExpiresAt);

public interface IMediaService
{
    Task<UploadRequest> CreateUploadRequestAsync(Guid userId, string mediaType, string mimeType, string extension, Guid? momeantId = null, CancellationToken ct = default);
    Task<bool> FinalizeUploadAsync(Guid mediaId, Guid userId, long byteSize, string? sha256Hex, CancellationToken ct = default);
}
