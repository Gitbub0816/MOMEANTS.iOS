# Momeants Native Specification Pack

This pack defines Momeants as a native iOS-first memory social app. It intentionally separates the native mobile client from all sensitive services. The app never talks directly to Neon, Cloudflare R2, Clerk secrets, AI providers, moderation vendors, analytics ingestion keys, or admin services.

## Required implementation order

1. Read `01_PRODUCT_VISION_AND_PRINCIPLES.md`.
2. Read `02_TECH_STACK_AND_ARCHITECTURE.md`.
3. Build the backend skeleton in ASP.NET Core before building mobile screens.
4. Create Neon schema from `03_DATABASE_SCHEMA_NEON_POSTGRES.md`.
5. Implement storage from `04_MEDIA_STORAGE_R2_IMAGES_CDN.md`.
6. Implement native authentication from `05_AUTH_NATIVE_CLERK_BACKEND_ONLY.md`.
7. Implement the native app shell from `08_NATIVE_IOS_APP_STRUCTURE_DOTNET_MAUI.md`.
8. Implement navigation and gestures from `09_NAVIGATION_GESTURES_AND_SCREEN_RULES.md`.
9. Implement visual design from `10_VISUAL_STYLE_SYSTEM.md`.
10. Implement upload, feed, and memory intelligence in that order.

## Absolute rules

- Native app language: C#.
- Native app framework: .NET MAUI for iOS.
- Backend language: C#.
- Backend framework: ASP.NET Core Web API.
- Database: Neon PostgreSQL.
- Object storage: Cloudflare R2.
- Image processing/delivery: Cloudflare Images or Cloudflare Image Resizing.
- Auth provider: Clerk, but backend-mediated only. No browser redirect auth in the native app.
- Push: Apple Push Notification service through backend.
- Never put service secrets in the mobile app.
- Never commit `.env`, appsettings secrets, service-account JSON, private keys, or database URLs.
- Treat any exposed service-account JSON as compromised and rotate it immediately.

## What this pack replaces

Older Firebase-first Momeants specs are superseded by this native architecture. Firebase may be used only for legacy migration or temporary experiments, not as the production system of record.
