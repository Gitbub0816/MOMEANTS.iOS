# Notifications and APNs

## Push provider

Use Apple Push Notification service. The backend sends pushes. The mobile app registers device tokens with the backend.

## Device token flow

1. App asks for notification permission after onboarding value is established.
2. iOS returns APNs token.
3. App sends token to backend:
   `POST /api/devices/register-push-token`
4. Backend stores token with user id, platform, environment, device id.
5. Backend sends pushes via APNs.

## Notification types

- New like.
- New super-like.
- New comment.
- New relationship request.
- Memory resurfaced.
- Important date reminder.
- Security alert.
- Account alert.

## Notification style

Copy should be warm:
- “Kristin added a Gold Heart to your Momeant.”
- “A memory from this week last year is waiting.”
- “Someone commented on your Momeant.”

Avoid spammy language:
- “You’re blowing up!”
- “Viral!”
- “Don’t miss out!!!”

## Quiet hours

Add later:
- user-defined quiet hours
- notification category toggles
- close-circle priority

## Badge count

Badge count should represent meaningful unread notifications, not every background event.
