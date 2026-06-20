# ASP.NET Core Backend API Specification

## Framework

- Language: C#.
- Runtime: .NET 9 or current LTS .NET if team requires stability.
- Framework: ASP.NET Core Web API.
- API style: REST JSON.
- Serialization: System.Text.Json.
- Naming: camelCase JSON, PascalCase C# properties, snake_case database columns.

## Project structure

```text
Momeants.Api/
  Controllers/
  Middleware/
  Auth/
  Domain/
  Infrastructure/
  Data/
  Services/
  Workers/
  Contracts/
  Options/
  Program.cs
Momeants.Shared/
  Contracts/
  Validation/
Momeants.Tests/
```

## Core controllers

```text
AuthController
UsersController
ProfilesController
MomeantsController
MediaController
FeedController
ReactionsController
CommentsController
RelationshipsController
ImportantPeopleController
NotificationsController
ReportsController
SettingsController
```

## API conventions

- All endpoints return JSON.
- All errors use a consistent envelope.
- Every mutating request must be authenticated unless explicitly public.
- Use idempotency keys for upload finalization and payment/subscription operations later.
- Use cursor pagination, not offset pagination, for feeds.

## Error envelope

```json
{
  "error": {
    "code": "validation_failed",
    "message": "One or more fields are invalid.",
    "details": []
  }
}
```

## Authentication middleware

Every authenticated request must resolve:

```text
UserId
ClerkUserId
SessionId
DeviceId optional
AccountStatus
```

If account is suspended/deleted, reject non-recovery endpoints.

## Example endpoints

### Create Momeant
```text
POST /api/momeants
```

Request:
```json
{
  "primaryMediaId": "uuid",
  "caption": "A warm evening by the water.",
  "capturedAt": "2026-06-20T04:00:00Z",
  "audienceType": "friends",
  "taggedPeople": []
}
```

### Feed
```text
GET /api/feed?cursor=...&limit=20
```

Response:
```json
{
  "items": [],
  "nextCursor": "opaque-token"
}
```

### React
```text
POST /api/momeants/{id}/reactions
```

Request:
```json
{ "type": "super_like" }
```

## Observability

- Structured logs only.
- No secrets in logs.
- Request correlation id required.
- Log slow queries.
- Log auth failures without leaking credentials.

## Background job candidates

- Media moderation.
- Image variant generation.
- Feed insertion.
- Notification dispatch.
- Memory score recalculation.
- Anniversary resurfacing.
- Abuse detection.

## API security

- HTTPS only.
- CORS locked to official web/admin domains.
- Native app uses Authorization header.
- Rate limit auth, uploads, comments, reports, search.
- Validate file sizes before upload and after upload.
- Validate ownership of every entity.
