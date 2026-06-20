# Momeants Plus and Monetization

## Monetization philosophy

Momeants should not pressure users into paying for basic emotional safety. Paid features should enhance personalization, control, and preservation.

## Momeants Plus features

- Remove ads.
- Custom app icons.
- Gallery themes.
- Advanced memory resurfacing controls.
- Longer captions.
- Deeper memory insights.
- Priority media quality.
- Private vault features later.
- Advanced search later.

## Ads

If ads are used:
- never interrupt a meaningful memory ceremony
- no ads between every Momeant
- no manipulative ad density
- no ads in private memory detail screens

## Subscription stack

For iOS:
- Apple In-App Purchase for consumer subscription.
- Backend validates receipts.
- Store entitlement in Neon.

## Entitlement table

```sql
create table user_entitlements (
  id uuid primary key default gen_random_uuid(),
  user_id uuid not null references users(id),
  entitlement text not null,
  source text not null,
  status text not null,
  starts_at timestamptz not null,
  ends_at timestamptz,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);
```
