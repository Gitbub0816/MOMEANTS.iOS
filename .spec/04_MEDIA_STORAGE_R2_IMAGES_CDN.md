# Media Storage, R2, Images, and CDN

Momeants is media-heavy. The database must never store photo or video bytes. Postgres stores only metadata and object references.

## Object storage provider

Use Cloudflare R2 for original media objects.

Recommended buckets:
- `momeants-prod-originals`
- `momeants-prod-derived`
- `momeants-staging-originals`
- `momeants-staging-derived`

## Object key format

Use stable, non-guessable keys:

```text
users/{userId}/momeants/{momeantId}/original/{mediaId}.{ext}
users/{userId}/avatars/{mediaId}.{ext}
derived/{mediaId}/w{width}_q{quality}.{ext}
```

Do not use user-provided filenames in object keys.

## Upload flow

1. Mobile app requests upload from backend.
2. Backend authenticates user.
3. Backend creates pending `media_objects` row.
4. Backend returns a short-lived signed upload URL.
5. Mobile app uploads directly to R2 using that URL.
6. Mobile app calls backend to finalize upload.
7. Backend verifies object existence, size, content type, checksum if available.
8. Backend queues moderation and derivative generation.

## Encoding rules

### Images
- Accept: JPEG, HEIC, PNG.
- Convert delivery variants to WebP/AVIF where supported.
- Preserve original only if needed.
- Strip GPS EXIF by default unless the user explicitly chooses location sharing.
- Generate blurhash or low-quality placeholder.
- Store width, height, byte size, SHA-256.

### Video later
- Accept only after MVP.
- Store original in R2.
- Transcode via background media pipeline.
- Generate thumbnail and poster image.

## Cloudflare Images or Image Resizing

Use Cloudflare Images/Image Resizing for delivery variants:
- thumbnail
- feed full-screen
- profile grid
- blurred placeholder

The native app should request display-appropriate URLs from backend or derive from a signed delivery template. Do not expose private object keys when unnecessary.

## Access control

Media URLs should respect the Momeant audience:
- Public Momeants may use public CDN URLs.
- Friends/close-circle/private Momeants should use signed URLs or tokenized delivery.
- Signed URLs must expire.
- Backend must verify viewer access before issuing private media URLs.

## Caching rules

- Public images: aggressive CDN cache.
- Private images: short-lived signed URL, moderate cache TTL.
- Avatar images: cache aggressively but version on update.

## Privacy rules

- Strip sensitive EXIF by default.
- Never expose storage credentials to the native app.
- Never allow arbitrary object-key reads from the client.
- Never trust MIME type from the client alone.
- Scan/validate uploaded media before broad distribution.
