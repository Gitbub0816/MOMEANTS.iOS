# Codex Implementation Prompt

Build Momeants as a native iOS-first app with a C#/.NET architecture.

## Non-negotiable stack

- Mobile app: .NET MAUI, C#, iOS-first.
- Backend: ASP.NET Core Web API, C#.
- Database: Neon PostgreSQL.
- Storage: Cloudflare R2.
- Image delivery: Cloudflare Images or Image Resizing.
- Auth: Clerk backend-mediated only. No browser redirect as the primary native auth experience.
- Push: APNs through backend.
- Architecture: native app talks only to backend API.

## Absolute rules

- Do not connect MAUI directly to Neon.
- Do not put Clerk secret keys in MAUI.
- Do not put R2 credentials in MAUI.
- Do not put AI provider keys in MAUI.
- Store mobile tokens only in iOS Keychain through SecureStorage.
- Use UTF-8 without BOM for source files.
- Use snake_case DB columns, PascalCase C# properties, camelCase JSON.
- Use UUID IDs.
- Use timestamptz for timestamps.
- Never store image bytes in Postgres.

## Build order

1. Create ASP.NET Core API solution.
2. Create Neon schema migrations.
3. Implement auth abstraction with Clerk backend provider.
4. Implement user/profile endpoints.
5. Implement R2 signed upload flow.
6. Implement create Momeant endpoint.
7. Implement feed endpoint.
8. Implement .NET MAUI shell and auth screens.
9. Implement feed screen with one-photo-at-a-time navigation.
10. Implement create flow.
11. Implement reactions.
12. Implement Important People.
13. Implement notifications.
14. Implement moderation/reporting.

## Native auth instruction

Create native MAUI auth screens. The app sends email/phone/code to backend. Backend talks to Clerk. Backend returns Momeants API tokens. No Clerk-hosted redirect UI for the primary email/phone auth flow.

## Deliverables

- `Momeants.Api` ASP.NET Core project.
- `Momeants.Mobile` .NET MAUI project.
- SQL migrations.
- Typed API contracts.
- Secure token storage service.
- R2 media upload service.
- Feed ViewModel and UI.
- Create Momeant flow.
- Clear README with environment variables and setup.
