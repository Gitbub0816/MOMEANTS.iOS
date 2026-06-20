using Momeants.Mobile.Services.Api;
using Momeants.Shared.Contracts;

namespace Momeants.Mobile.Services.Media;

public class MediaUploadService : IMediaUploadService
{
    private readonly IApiClient _api;
    private readonly HttpClient _http;

    public MediaUploadService(IApiClient api, HttpClient http)
    {
        _api = api;
        _http = http;
    }

    public async Task<MediaUploadResult> UploadImageAsync(Stream imageStream, string mimeType, string extension, Guid? momeantId = null, CancellationToken ct = default)
    {
        try
        {
            // Step 1: Request upload URL from backend
            var uploadResp = await _api.PostAsync<UploadResponseDto>("api/media/upload-request", new UploadRequestDto("image", mimeType, extension, momeantId), ct);
            if (uploadResp is null) return new MediaUploadResult(false, null, "Failed to get upload URL");

            // Step 2: Read stream into bytes to get size
            using var ms = new MemoryStream();
            await imageStream.CopyToAsync(ms, ct);
            var bytes = ms.ToArray();

            // Step 3: PUT directly to R2
            using var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
            var putResp = await _http.PutAsync(uploadResp.UploadUrl, content, ct);
            if (!putResp.IsSuccessStatusCode)
                return new MediaUploadResult(false, null, $"Upload to R2 failed: {putResp.StatusCode}");

            // Step 4: Finalize
            var finalizeResp = await _api.PostAsync<object>("api/media/upload-complete",
                new FinalizeUploadRequest(uploadResp.MediaId, bytes.Length), ct);

            return new MediaUploadResult(true, uploadResp.MediaId, null);
        }
        catch (Exception ex)
        {
            return new MediaUploadResult(false, null, ex.Message);
        }
    }
}
