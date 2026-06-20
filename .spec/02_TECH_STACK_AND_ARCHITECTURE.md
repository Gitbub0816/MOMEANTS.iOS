# Tech Stack and Architecture

## Recommended production architecture

Momeants should start inexpensive but be able to scale quickly by separating core data, media, feed caching, analytics, and background jobs.

```text
.NET MAUI iOS App
        |
        | HTTPS JSON API only
        v
ASP.NET Core Web API
        |
        +--> Clerk backend integration for identity verification
        +--> Neon PostgreSQL for system of record
        +--> Cloudflare R2 for original media objects
        +--> Cloudflare Images / Image Resizing for optimized delivery
        +--> APNs for push notifications
        +--> Upstash Redis for feed/session/cache later
        +--> Cloudflare Queues or Hangfire/worker service for async jobs
        +--> PostHog for product analytics later
        +--> OpenAI/Gemini provider abstraction for AI features later
```

## Coding languages

### Mobile app
- Language: C#.
- Framework: .NET MAUI.
- UI pattern: MVVM.
- State style: ViewModel per screen, service interfaces injected.
- HTTP: `HttpClient` with typed API clients.
- Local secure storage: iOS Keychain via MAUI SecureStorage.

### Backend
- Language: C#.
- Framework: ASP.NET Core Web API.
- Database access: EF Core for core domain models, Dapper for high-performance feed queries if needed.
- Validation: FluentValidation or explicit request validators.
- Background jobs: queue-first design.
- Secrets: environment variables / managed secret store only.

### Database
- Neon PostgreSQL.
- Extensions: `pgcrypto`, `uuid-ossp` or native `gen_random_uuid()`.
- Use `timestamptz` for all timestamps.
- Use `uuid` primary keys.
- Use snake_case table and column names.

### Storage
- Cloudflare R2 for original media.
- Cloudflare Images or Image Resizing for variants.
- Store only metadata, object keys, hashes, dimensions, and delivery URLs in Postgres.

## Inexpensive starting point

Start with:
- One ASP.NET Core API app.
- One Neon project.
- One R2 bucket.
- One Cloudflare CDN zone.
- Clerk for identity, mediated through backend.
- APNs only for iOS push.

Add later:
- Redis for feed cache.
- Separate worker service.
- PostHog/ClickHouse for analytics.
- Meilisearch/Typesense for search.
- Dedicated moderation pipeline.

## Boundary rules

The iOS app may call only your backend API. It must never directly call:
- Neon.
- Cloudflare R2 with permanent credentials.
- Clerk secret-key endpoints.
- OpenAI/Gemini with API keys.
- Admin APIs.

The backend issues short-lived upload URLs when direct-to-R2 upload is needed. The backend validates every user action with the authenticated user context.

## Deployment options

### Cheapest solid start
- API: Fly.io, Render, Railway, Azure App Service, or a small VPS.
- DB: Neon.
- Storage/CDN: Cloudflare.

### Microsoft-aligned C# path
- API: Azure App Service or Azure Container Apps.
- DB: Neon remains fine, or Azure PostgreSQL later.
- Secrets: Azure Key Vault.
- Observability: Application Insights.

## Architecture goal

Keep the first production version boring, typed, and explicit. Social scale fails when media, feed generation, analytics, and relational state are tangled together. Momeants must split those concerns from day one without overengineering the MVP.
