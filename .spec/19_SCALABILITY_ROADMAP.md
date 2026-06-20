# Scalability Roadmap

## Stage 0: Prototype

- Local ASP.NET Core API.
- Neon free/pro tier.
- R2 bucket.
- Manual deployment.
- TestFlight builds.

## Stage 1: MVP

- Hosted API.
- Neon pooled connections.
- R2 signed uploads.
- APNs.
- Basic feed query.
- Basic moderation.
- Basic analytics.

## Stage 2: Early growth

- Add Redis cache.
- Add background workers.
- Precompute feed_items.
- Add image variants.
- Add slow query monitoring.
- Add admin moderation tools.

## Stage 3: Strong traction

- Separate read-heavy feed queries.
- Add queue-based fanout.
- Add PostHog or ClickHouse for event analytics.
- Add search service.
- Add media processing workers.

## Stage 4: Large scale

- Partition high-volume tables.
- Separate analytics database.
- Dedicated recommendation service.
- Multi-region media delivery.
- Dedicated notification service.

## What will break first

Likely bottlenecks:
1. Media delivery if not CDN-backed.
2. Feed generation if done entirely live from joins.
3. Analytics/event volume if stored in Postgres.
4. Notification fanout.
5. Moderation queue.

Postgres/Neon can remain the system of record for a long time if media, logs, analytics, and feed cache are separated.
