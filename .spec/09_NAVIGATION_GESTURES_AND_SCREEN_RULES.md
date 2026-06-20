# Navigation, Gestures, and Screen Rules

## Navigation philosophy

Momeants should feel like moving through memories, not browsing a website. Navigation should be simple, tactile, and emotional.

## Primary navigation

Bottom tab bar with 5 tabs:

1. Feed
2. Create
3. People
4. Memories
5. Profile

The tab bar should be subtle, premium, and not overly rounded. It should feel native but distinctive.

## Feed gestures

On the full-screen Momeant feed:

- Tap right: next Momeant.
- Tap left: previous Momeant.
- Swipe up: like / heart.
- Swipe down: dislike / show less.
- Long press up or press-and-hold heart: Super Like / Gold Heart.
- Long press down: hide or unfollow depending context.
- Tap center: reveal/hide overlay details.
- Double tap: heart, but only if it does not conflict with tap-right/left zones.

## Gesture feedback

- Like: small warm heart pulse.
- Super Like: gold heart glow and gentle upward particles.
- Dislike/show less: quiet fade, not punitive.
- Hide/unfollow: confirmation sheet.
- Navigation tap: subtle haptic tick.

## Screen transitions

- Feed item changes should feel like a horizontal memory slide or soft crossfade.
- Opening a Momeant detail should zoom slightly from the photo.
- Returning should collapse back to the photo.
- Profile transitions can be standard native push.
- Auth/onboarding transitions should be soft and calm.

## Feed overlay rules

Default feed shows:
- Photo.
- Subtle gradient for readability.
- Author avatar/name.
- Date/caption preview.
- Reaction affordance.

On tap center show:
- Full caption.
- Important people tags.
- Location label if shared.
- Comment preview.
- Privacy indicator.

## Create flow

1. Choose photo.
2. Crop/preview.
3. Caption.
4. Tag Important People.
5. Choose audience.
6. Confirm.
7. Upload progress.
8. Success transition into posted Momeant.

## Onboarding navigation

- Welcome.
- Sign up/sign in.
- Verify.
- Name.
- Username.
- Birthday.
- Avatar.
- Location optional.
- Important People seed optional.
- Notification permission later.

## Back behavior

- Back from create with unsaved work asks confirmation.
- Back from auth returns to welcome.
- Back from feed should not exit app accidentally.
- Back from detail returns to exact feed position.

## Empty states

Empty states must be warm and useful:
- No memories: invite user to post first Momeant.
- No people: invite user to add Important People.
- No notifications: “Quiet for now.”
- No resurfaced memories: explain that Momeants will bring back memories over time.
