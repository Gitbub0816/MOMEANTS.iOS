# Native iOS App Structure in .NET MAUI

## App framework

- Use .NET MAUI.
- Target iOS first.
- Use C#.
- Use XAML only where it improves maintainability; C# UI is acceptable for high-custom cinematic screens.
- Use MVVM strictly.

## Project structure

```text
Momeants.Mobile/
  App.xaml
  AppShell.xaml
  MauiProgram.cs
  Platforms/iOS/
  Views/
    Auth/
    Onboarding/
    Feed/
    Capture/
    Memory/
    Profile/
    People/
    Messaging/
    Settings/
  ViewModels/
  Models/
  Services/
    Api/
    Auth/
    Media/
    Navigation/
    Storage/
    Push/
    Haptics/
  Controls/
  Effects/
  Animations/
  Resources/
    Fonts/
    Images/
    Styles/
```

## MVVM rules

- Every screen gets a ViewModel.
- ViewModels do not call raw HttpClient directly; they use typed services.
- Views do not contain business logic.
- Commands must guard against double taps.
- Loading, success, empty, and error states must be explicit.

## Typed services

```text
IAuthService
IApiClient
IMomeantService
IFeedService
IMediaUploadService
IReactionService
IRelationshipService
IImportantPeopleService
INotificationService
ISettingsService
ISecureTokenStore
IHapticsService
INavigationService
```

## Native feel requirements

- Use platform haptics for meaningful gestures.
- Use smooth transitions.
- Avoid webviews for core app interactions.
- Avoid web-styled forms where native controls are expected.
- Use safe areas correctly.
- Support Dynamic Type later, but preserve visual hierarchy.

## Performance rules

- Feed image loading must be lazy.
- Preload one Momeant ahead and one behind.
- Do not preload entire feeds.
- Cache thumbnails locally.
- Avoid blocking UI thread.
- Use cancellation tokens for feed and media calls.

## App lifecycle

On launch:
1. Show splash / warm launch screen.
2. Check secure token store.
3. Refresh session if possible.
4. Route to onboarding or feed.

On resume:
1. Refresh notifications count.
2. Resume feed from last position.
3. Re-check expired media URLs.

## Native permissions

Request permissions only when needed:
- Photos: when user taps create/upload.
- Camera: when user taps camera.
- Notifications: after value is established, not on first launch.
- Location: only when user chooses location tagging.

## Build configurations

Use separate configurations:
- Debug
- Staging
- Production

Each configuration must have separate API base URLs and bundle identifiers.
