# AI and Memory Features

## AI role

AI in Momeants should enhance memories quietly. It should not generate fake intimacy or turn the app into synthetic content.

## Acceptable AI features

- Image tagging for private organization.
- Caption suggestions.
- Memory resurfacing hints.
- “People likely in this memory” suggestions.
- Content moderation assistance.
- Duplicate/similar photo grouping.
- Accessibility alt text.

## Avoid early

- AI-generated fake memories.
- Public AI filters as a core identity.
- Deepfake or face manipulation.
- Synthetic relationship summaries that feel invasive.

## Provider architecture

Backend interface:

```csharp
public interface IAiProvider
{
    Task<ImageAnalysisResult> AnalyzeImageAsync(MediaObject media, CancellationToken ct);
    Task<CaptionSuggestions> SuggestCaptionsAsync(Guid momeantId, CancellationToken ct);
    Task<ModerationResult> ModerateAsync(MediaObject media, string? caption, CancellationToken ct);
}
```

The app never calls AI providers directly.

## Privacy rules

- AI analysis must be disclosed in privacy policy.
- User-private content must be handled according to provider data policy.
- Do not train public models on user content unless explicit consent exists.
- Store only useful derived metadata.

## Memory intelligence vs AI

Memory intelligence should begin with understandable scoring, not opaque AI:
- relationship weight
- anniversary weight
- important person weight
- rewatch weight
- reaction weight

Add AI only after deterministic scoring works.
