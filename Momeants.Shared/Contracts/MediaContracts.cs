namespace Momeants.Shared.Contracts;

public record UploadRequestDto(string MediaType, string MimeType, string Extension, Guid? MomeantId = null);

public record UploadResponseDto(Guid MediaId, string UploadUrl, string ObjectKey, DateTimeOffset ExpiresAt);

public record FinalizeUploadRequest(Guid MediaId, long ByteSize, string? Sha256Hex = null);
