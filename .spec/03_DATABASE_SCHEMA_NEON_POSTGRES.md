# Neon PostgreSQL Database Schema

## Global database rules

- Use UUID primary keys.
- Use `created_at timestamptz not null default now()`.
- Use `updated_at timestamptz not null default now()` where records mutate.
- Use soft deletes for user-facing content: `deleted_at timestamptz null`.
- Store normalized text columns where search/filtering is needed.
- Store phone numbers in E.164 format only.
- Store emails lowercase.
- Never store raw auth provider secrets.
- Never store media bytes in Postgres.

## Core tables

### users
```sql
create table users (
  id uuid primary key default gen_random_uuid(),
  clerk_user_id text unique not null,
  username text unique,
  display_name text not null,
  full_name text,
  avatar_media_id uuid,
  birth_date date,
  city text,
  region text,
  country text,
  bio text,
  account_status text not null default 'active',
  profile_visibility text not null default 'friends',
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  deleted_at timestamptz
);
```

### momeants
```sql
create table momeants (
  id uuid primary key default gen_random_uuid(),
  owner_user_id uuid not null references users(id),
  primary_media_id uuid not null,
  caption text,
  captured_at timestamptz,
  posted_at timestamptz not null default now(),
  location_label text,
  location_lat numeric(9,6),
  location_lng numeric(9,6),
  audience_type text not null default 'friends',
  significance_score numeric(8,4) not null default 0,
  memory_score numeric(8,4) not null default 0,
  comments_enabled boolean not null default true,
  resharing_allowed boolean not null default false,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  deleted_at timestamptz
);
```

### media_objects
```sql
create table media_objects (
  id uuid primary key default gen_random_uuid(),
  owner_user_id uuid not null references users(id),
  r2_bucket text not null,
  r2_object_key text not null unique,
  media_type text not null,
  mime_type text not null,
  byte_size bigint not null,
  width int,
  height int,
  duration_ms int,
  sha256_hex text,
  blurhash text,
  moderation_status text not null default 'pending',
  created_at timestamptz not null default now(),
  deleted_at timestamptz
);
```

### relationships
```sql
create table relationships (
  id uuid primary key default gen_random_uuid(),
  requester_user_id uuid not null references users(id),
  addressee_user_id uuid not null references users(id),
  status text not null default 'pending',
  relationship_label text,
  closeness_score numeric(8,4) not null default 0,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  unique(requester_user_id, addressee_user_id)
);
```

### important_people
```sql
create table important_people (
  id uuid primary key default gen_random_uuid(),
  user_id uuid not null references users(id),
  person_user_id uuid references users(id),
  display_name text not null,
  category text not null,
  custom_label text,
  priority int not null default 0,
  notes_private text,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);
```

Categories include: mother, father, parent, sibling, partner, child, close_friend, family, mentor, other.

### momeant_people
```sql
create table momeant_people (
  id uuid primary key default gen_random_uuid(),
  momeant_id uuid not null references momeants(id),
  person_user_id uuid references users(id),
  display_name text,
  relationship_to_owner text,
  created_at timestamptz not null default now()
);
```

### reactions
```sql
create table reactions (
  id uuid primary key default gen_random_uuid(),
  momeant_id uuid not null references momeants(id),
  user_id uuid not null references users(id),
  reaction_type text not null,
  created_at timestamptz not null default now(),
  unique(momeant_id, user_id, reaction_type)
);
```

Allowed reaction_type values:
- like
- dislike
- super_like
- heart
- hidden

### comments
```sql
create table comments (
  id uuid primary key default gen_random_uuid(),
  momeant_id uuid not null references momeants(id),
  user_id uuid not null references users(id),
  body text not null,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  deleted_at timestamptz
);
```

### feed_items
```sql
create table feed_items (
  id uuid primary key default gen_random_uuid(),
  user_id uuid not null references users(id),
  momeant_id uuid not null references momeants(id),
  source_type text not null,
  rank_score numeric(12,6) not null default 0,
  reason_code text,
  inserted_at timestamptz not null default now(),
  seen_at timestamptz,
  dismissed_at timestamptz,
  unique(user_id, momeant_id)
);
```

### notifications
```sql
create table notifications (
  id uuid primary key default gen_random_uuid(),
  user_id uuid not null references users(id),
  actor_user_id uuid references users(id),
  type text not null,
  title text not null,
  body text,
  deep_link text,
  read_at timestamptz,
  created_at timestamptz not null default now()
);
```

### reports
```sql
create table reports (
  id uuid primary key default gen_random_uuid(),
  reporter_user_id uuid not null references users(id),
  target_type text not null,
  target_id uuid not null,
  reason text not null,
  details text,
  status text not null default 'open',
  created_at timestamptz not null default now(),
  resolved_at timestamptz
);
```

### audit_events
```sql
create table audit_events (
  id uuid primary key default gen_random_uuid(),
  user_id uuid references users(id),
  event_type text not null,
  entity_type text,
  entity_id uuid,
  ip_address inet,
  user_agent text,
  metadata jsonb not null default '{}',
  created_at timestamptz not null default now()
);
```

## Indexing rules

Create indexes for:
- `users(clerk_user_id)`.
- `users(username)`.
- `momeants(owner_user_id, posted_at desc)`.
- `momeants(audience_type, posted_at desc)`.
- `relationships(requester_user_id, status)`.
- `relationships(addressee_user_id, status)`.
- `feed_items(user_id, rank_score desc)`.
- `feed_items(user_id, seen_at)`.
- `reactions(momeant_id)`.
- `comments(momeant_id, created_at)`.
- `notifications(user_id, read_at, created_at desc)`.

## What not to store in Neon

- Original photos.
- Videos.
- Raw image EXIF beyond allowed metadata.
- Massive clickstream/event streams.
- CDN access logs.
- AI embeddings at large scale unless using pgvector intentionally.
