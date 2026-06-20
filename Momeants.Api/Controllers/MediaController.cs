using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Momeants.Api.Data;
using Momeants.Api.Services;
using Momeants.Shared.Contracts;

namespace Momeants.Api.Controllers;

[Route("api/media")]
public class MediaController : BaseController
{
    private readonly IMediaService _media;
    private readonly AppDbContext _db;

    public MediaController(IMediaService media, AppDbContext db)
    {
        _media = media;
        _db = db;
    }

    [HttpPost("upload-request")]
    public async Task<IActionResult> RequestUpload([FromBody] UploadRequestDto req, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var allowed = new[] { "image/jpeg", "image/png", "image/heic", "image/heif", "image/webp" };
        if (!allowed.Contains(req.MimeType.ToLowerInvariant()))
            return ApiError("invalid_mime_type", "Unsupported media type.");

        var ext = req.Extension.TrimStart('.').ToLowerInvariant();
        var result = await _media.CreateUploadRequestAsync(cu.UserId, req.MediaType, req.MimeType, ext, req.MomeantId, ct);

        return Ok(new UploadResponseDto(result.MediaId, result.UploadUrl, result.ObjectKey, result.ExpiresAt));
    }

    [HttpPost("upload-complete")]
    public async Task<IActionResult> FinalizeUpload([FromBody] FinalizeUploadRequest req, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var success = await _media.FinalizeUploadAsync(req.MediaId, cu.UserId, req.ByteSize, req.Sha256Hex, ct);
        if (!success) return ApiError("finalize_failed", "Could not finalize upload. Ensure the object was uploaded to R2.");

        return Ok(new { mediaId = req.MediaId, status = "queued" });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMedia(Guid id, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var media = await _db.MediaObjects.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id && m.DeletedAt == null, ct);
        if (media is null) return NotFound();

        return Ok(new
        {
            media.Id,
            media.MediaType,
            media.MimeType,
            media.ByteSize,
            media.Width,
            media.Height,
            media.Blurhash,
            media.ModerationStatus,
            media.CreatedAt
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMedia(Guid id, CancellationToken ct)
    {
        var authResult = RequireAuth(out var cu);
        if (authResult is not null) return authResult;

        var media = await _db.MediaObjects.FirstOrDefaultAsync(m => m.Id == id && m.OwnerUserId == cu.UserId && m.DeletedAt == null, ct);
        if (media is null) return NotFound();

        media.DeletedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
