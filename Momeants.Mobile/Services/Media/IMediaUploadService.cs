namespace Momeants.Mobile.Services.Media;

public record MediaUploadResult(bool Success, Guid? MediaId, string? Error);

public interface IMediaUploadService
{
    Task<MediaUploadResult> UploadImageAsync(Stream imageStream, string mimeType, string extension, Guid? momeantId = null, CancellationToken ct = default);
}
