using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Momeants.Api.Data;
using Momeants.Api.Domain;
using Momeants.Api.Options;

namespace Momeants.Api.Services;

public class MediaService : IMediaService
{
    private readonly AppDbContext _db;
    private readonly IAmazonS3 _s3;
    private readonly R2Options _opts;
    private readonly ILogger<MediaService> _logger;

    public MediaService(AppDbContext db, IAmazonS3 s3, IOptions<R2Options> opts, ILogger<MediaService> logger)
    {
        _db = db;
        _s3 = s3;
        _opts = opts.Value;
        _logger = logger;
    }

    public async Task<UploadRequest> CreateUploadRequestAsync(Guid userId, string mediaType, string mimeType, string extension, Guid? momeantId = null, CancellationToken ct = default)
    {
        var mediaId = Guid.NewGuid();
        string objectKey;

        if (momeantId.HasValue)
            objectKey = $"users/{userId}/momeants/{momeantId}/original/{mediaId}.{extension}";
        else if (mediaType == "avatar")
            objectKey = $"users/{userId}/avatars/{mediaId}.{extension}";
        else
            objectKey = $"users/{userId}/uploads/{mediaId}.{extension}";

        var media = new MediaObject
        {
            Id = mediaId,
            OwnerUserId = userId,
            R2Bucket = _opts.BucketName,
            R2ObjectKey = objectKey,
            MediaType = mediaType,
            MimeType = mimeType,
            ByteSize = 0,
            ModerationStatus = "pending"
        };

        _db.MediaObjects.Add(media);
        await _db.SaveChangesAsync(ct);

        var expiry = DateTimeOffset.UtcNow.AddMinutes(_opts.UploadUrlExpiryMinutes);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _opts.BucketName,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            Expires = expiry.UtcDateTime,
            ContentType = mimeType
        };

        var uploadUrl = _s3.GetPreSignedURL(request);

        return new UploadRequest(mediaId, uploadUrl, objectKey, expiry);
    }

    public async Task<bool> FinalizeUploadAsync(Guid mediaId, Guid userId, long byteSize, string? sha256Hex, CancellationToken ct = default)
    {
        var media = await _db.MediaObjects
            .FirstOrDefaultAsync(m => m.Id == mediaId && m.OwnerUserId == userId && m.DeletedAt == null, ct);

        if (media is null)
        {
            _logger.LogWarning("FinalizeUpload: media {MediaId} not found for user {UserId}", mediaId, userId);
            return false;
        }

        try
        {
            // Verify the object exists in R2
            var metadata = await _s3.GetObjectMetadataAsync(_opts.BucketName, media.R2ObjectKey, ct);
            if (metadata is null) return false;

            media.ByteSize = byteSize > 0 ? byteSize : metadata.ContentLength;
            media.Sha256Hex = sha256Hex;
            media.ModerationStatus = "queued";

            await _db.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FinalizeUpload error for media {MediaId}", mediaId);
            return false;
        }
    }
}
