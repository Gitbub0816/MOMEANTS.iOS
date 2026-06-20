# Moderation, Safety, and Privacy

## Safety requirements

Momeants must support:
- report content
- report user
- block user
- mute user
- hide Momeant
- remove comments
- moderation status on media
- review queue

## Moderation pipeline

1. Upload media.
2. Mark media `pending`.
3. Queue automated moderation.
4. If safe, mark `approved`.
5. If uncertain, mark `needs_review`.
6. If unsafe, restrict distribution.

## User controls

Every Momeant menu should include:
- Hide
- Report
- Block user
- Copy link if allowed

## Privacy model

Audience options:
- Only me
- Close circle
- Friends
- Selected people
- Public later

Backend must enforce audience. UI must never be trusted as the security boundary.

## Account deletion

Requirements:
- request deletion
- confirmation window
- export option before delete
- soft delete account
- remove/ anonymize content according to policy

## Data export

Export should include:
- profile data
- Momeant metadata
- comments
- relationships
- Important People
- media download references if allowed

## Child safety

If minors are allowed, add explicit policies and stronger safeguards before launch.

## AI moderation

AI moderation can assist but must not be the sole final authority for severe cases. Store moderation decisions and reasons.
