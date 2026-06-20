# Security, Encoding, Encryption, and Secret Rules

## Secret handling

Never commit:
- Firebase service account JSON.
- Clerk secret keys.
- Neon database URLs.
- Cloudflare R2 access keys.
- APNs private keys.
- OpenAI/Gemini API keys.
- JWT signing keys.
- `.env` files.

If a private key or service-account JSON has been shared or uploaded to an external tool, treat it as compromised. Rotate/revoke it immediately.

## Mobile local storage

Use iOS Keychain through MAUI SecureStorage for:
- Access token.
- Refresh token.
- Device identifier.

Do not store tokens in:
- Preferences.
- SQLite plaintext.
- App logs.
- Crash reports.
- Analytics events.

## Database encryption

- Rely on Neon encryption at rest for baseline storage.
- Use TLS for all database connections.
- For highly sensitive fields, use application-level encryption before writing to DB.
- Candidate fields for application-level encryption: private notes about Important People, recovery secrets, sensitive moderation notes.

## Passwords

Do not store passwords in Momeants if Clerk is the identity provider. Clerk handles credential storage.

## Tokens

- Access token: short-lived, signed JWT.
- Refresh token: long-lived random secret stored only hashed server-side.
- Refresh token rotation required.
- On refresh, invalidate old token.
- On logout, revoke current refresh token.

## Encoding rules

- All source files: UTF-8 without BOM.
- API JSON: UTF-8.
- Database text: UTF-8.
- Normalize user input where appropriate.
- Store phone numbers in E.164.
- Store emails lowercase.
- Store usernames lowercase for uniqueness, display separately if needed.

## Media safety

- Strip EXIF GPS by default.
- Do not trust client MIME type.
- Validate file signature server-side or in worker.
- Generate SHA-256 for uploaded media.
- Enforce max file size.
- Enforce allowed extensions.

## Network security

- HTTPS required.
- HSTS on web/admin domains.
- Certificate pinning optional later, not MVP.
- Do not send tokens in query strings.
- Use Authorization header.

## Privacy controls

Every Momeant must have an audience:
- private
- close_circle
- friends
- selected_people
- public later

Backend must enforce audience checks. UI privacy labels are not security.

## Logging rules

Never log:
- auth tokens
- passwords
- OTP codes
- private keys
- full database URLs
- signed upload URLs
- private message bodies if avoidable

## Abuse prevention

Rate limit:
- login starts
- OTP verification
- uploads
- comments
- follows/friend requests
- reports
- search
- AI requests

## Compliance posture

Prepare for:
- Account deletion.
- Data export.
- Content report review.
- Child safety policies if minors are allowed.
- Terms, Privacy, SMS, and AI disclosure pages.
