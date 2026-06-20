namespace Momeants.Shared.Contracts;

public record CreateMomeantRequest(
    Guid PrimaryMediaId,
    string? Caption,
    DateTimeOffset? CapturedAt,
    string AudienceType = "friends",
    string? LocationLabel = null,
    decimal? LocationLat = null,
    decimal? LocationLng = null,
    bool CommentsEnabled = true,
    bool ResharingAllowed = false,
    List<TaggedPersonDto>? TaggedPeople = null);

public record TaggedPersonDto(Guid? PersonUserId, string? DisplayName, string? RelationshipToOwner);

public record MomeantDto(
    Guid Id,
    Guid OwnerUserId,
    string? Caption,
    DateTimeOffset? CapturedAt,
    DateTimeOffset PostedAt,
    string? LocationLabel,
    string AudienceType,
    string MediaUrl,
    string? Blurhash,
    int Width,
    int Height,
    int ReactionCount,
    int CommentCount,
    string? ViewerReaction,
    UserDto Owner);

public record FeedResponse(List<MomeantDto> Items, string? NextCursor);

public record ReactionRequest(string Type);

public record CreateCommentRequest(string Body);

public record CommentDto(
    Guid Id,
    Guid MomeantId,
    string Body,
    DateTimeOffset CreatedAt,
    UserDto Author);
