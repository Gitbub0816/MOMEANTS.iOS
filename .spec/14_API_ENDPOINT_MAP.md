# API Endpoint Map

## Auth

```text
POST /api/auth/start
POST /api/auth/verify
POST /api/auth/refresh
POST /api/auth/logout
GET  /api/auth/me
```

## Users and profile

```text
GET  /api/users/me
PATCH /api/users/me
GET  /api/users/{id}
GET  /api/users/by-username/{username}
POST /api/users/check-username
```

## Media

```text
POST /api/media/upload-request
POST /api/media/upload-complete
GET  /api/media/{id}
DELETE /api/media/{id}
```

## Momeants

```text
POST /api/momeants
GET  /api/momeants/{id}
PATCH /api/momeants/{id}
DELETE /api/momeants/{id}
GET  /api/users/{id}/momeants
```

## Feed

```text
GET  /api/feed
POST /api/feed/{feedItemId}/seen
POST /api/feed/{feedItemId}/dismiss
```

## Reactions

```text
POST /api/momeants/{id}/reactions
DELETE /api/momeants/{id}/reactions/{type}
```

## Comments

```text
GET  /api/momeants/{id}/comments
POST /api/momeants/{id}/comments
PATCH /api/comments/{id}
DELETE /api/comments/{id}
```

## Relationships

```text
POST /api/relationships/request
POST /api/relationships/{id}/accept
POST /api/relationships/{id}/decline
DELETE /api/relationships/{id}
GET  /api/relationships
```

## Important People

```text
GET  /api/important-people
POST /api/important-people
PATCH /api/important-people/{id}
DELETE /api/important-people/{id}
```

## Notifications

```text
GET  /api/notifications
POST /api/notifications/{id}/read
POST /api/notifications/read-all
POST /api/devices/register-push-token
DELETE /api/devices/{id}
```

## Reports and safety

```text
POST /api/reports
POST /api/users/{id}/block
POST /api/users/{id}/mute
DELETE /api/users/{id}/block
DELETE /api/users/{id}/mute
```

## Settings and data rights

```text
GET  /api/settings
PATCH /api/settings
POST /api/account/export
POST /api/account/delete-request
POST /api/account/delete-confirm
```
