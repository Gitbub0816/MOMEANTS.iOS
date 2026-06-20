# Momeants

Native iOS memory social platform built with .NET MAUI (mobile) and ASP.NET Core (backend).

## Projects

- `Momeants.Api` — ASP.NET Core Web API (.NET 8), PostgreSQL via EF Core
- `Momeants.Mobile` — .NET MAUI iOS-first app
- `Momeants.Shared` — Shared DTOs and contracts
- `Momeants.Tests` — xUnit tests

## Required Environment Variables

Set these in `appsettings.*.local.json` (not committed) or as real environment variables:

| Variable | Description |
|---|---|
| `ConnectionStrings__NeonPostgres` | Neon PostgreSQL connection string |
| `Clerk__SecretKey` | Clerk backend API secret key |
| `R2__AccountId` | Cloudflare R2 account ID |
| `R2__AccessKeyId` | Cloudflare R2 access key ID |
| `R2__SecretAccessKey` | Cloudflare R2 secret access key |
| `R2__BucketName` | R2 bucket name (e.g. `momeants-prod-originals`) |
| `R2__PublicBaseUrl` | Public CDN base URL for R2 |
| `Jwt__SecretKey` | JWT signing secret (min 32 chars) |
| `Jwt__Issuer` | JWT issuer string |
| `Jwt__Audience` | JWT audience string |
| `Apns__KeyId` | APNs key ID |
| `Apns__TeamId` | Apple Team ID |
| `Apns__BundleId` | iOS app bundle ID |

## Running the API

```bash
cd Momeants.Api
dotnet ef database update        # Apply migrations to Neon
dotnet run
```

## Auth Flow

1. Mobile calls `POST /api/auth/start` with email or phone
2. Clerk sends OTP via email/SMS
3. Mobile calls `POST /api/auth/verify` with code
4. Backend returns JWT access token (15 min) + refresh token (30 days)
5. Tokens stored in iOS Keychain via SecureStorage
6. Auto-refresh via `POST /api/auth/refresh` on 401

## Media Upload Flow

1. Mobile calls `POST /api/media/upload-request`
2. Backend creates pending `media_objects` row, returns signed R2 PUT URL
3. Mobile PUTs image bytes directly to R2
4. Mobile calls `POST /api/media/upload-complete`
5. Backend verifies object exists, marks as queued for moderation
