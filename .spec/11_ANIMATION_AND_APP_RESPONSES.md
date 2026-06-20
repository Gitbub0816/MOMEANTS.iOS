# Animation and App Responses

## Animation philosophy

Animations should make the app feel emotionally alive. They should not make it feel like a game, a toy, or a noisy social feed.

## Required interaction responses

### Opening a Momeant
- Photo gently resolves from blurhash to full image.
- Slight scale-in from 0.985 to 1.0.
- Caption overlay fades after image is ready.

### Like
- Small heart emerges near gesture direction.
- Haptic light impact.
- Heart fades within 600ms.

### Super Like / Gold Heart
- Gold heart blooms from chest/center area of UI.
- Warm radial glow.
- Optional small particles.
- Haptic medium impact.
- Should feel special and not spammy.

### Dislike / Show Less
- Photo subtly dims and moves away.
- No shame language.
- Copy: “We’ll show fewer like this.”

### Memory resurfacing
- Use a warmer, slower transition.
- Display label: “A Momeant from this day” or “Worth remembering.”

### Upload success
- Photo locks into frame.
- Small glow runs around the frame.
- Copy: “Saved as a Momeant.”

## Timing rules

- Micro animations: 120–250ms.
- Screen transitions: 250–450ms.
- Emotional/resurfacing animations: 500–900ms.
- Avoid animations longer than 1 second unless ceremonial.

## Accessibility

Respect reduced motion:
- Disable particles.
- Use fades instead of scale/movement.
- Keep haptics optional.

## Loading states

- Use blurhash/skeleton image placeholders.
- Avoid generic spinners where possible.
- Feed should never show a blank white loading screen.

## Error states

Errors should be calm:
- “Couldn’t load this Momeant.”
- “Upload paused. Try again.”
- “That memory is no longer available.”

Avoid:
- raw exception messages
- backend error codes in UI
- blame language
