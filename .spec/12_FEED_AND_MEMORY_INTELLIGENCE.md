# Feed and Memory Intelligence

## Feed goal

The feed should prioritize emotionally relevant memories, not viral popularity.

## Feed sources

- Friends' recent Momeants.
- Close-circle Momeants.
- Important People Momeants.
- Resurfaced memories.
- Memories with shared people.
- Memories from meaningful dates.
- Limited discovery later.

## Initial feed ranking formula

```text
rank_score =
  recency_weight
+ relationship_weight
+ important_person_weight
+ interaction_weight
+ memory_significance_weight
+ anniversary_weight
+ super_like_weight
- negative_feedback_weight
- repetition_penalty
```

## Relationship weight

Increase when:
- user marked person as important
- frequent mutual interactions
- direct relationship category is close
- shared Momeants include that person
- recent meaningful engagement

## Memory significance score

Inputs:
- Important People tagged.
- Date significance.
- Location recurrence.
- Super Likes.
- Rewatches.
- Comments from close people.
- Caption emotional keywords.
- Anniversary proximity.

## Negative signals

Decrease when:
- user swipes down/show less
- user hides person
- user skips similar content repeatedly
- user unfollows/mutes
- report/moderation risk exists

## Feed implementation phases

### Phase 1: Query-time ranking
Use SQL queries and simple scoring.

### Phase 2: Precomputed feed_items
Background job inserts feed items per user.

### Phase 3: Hybrid cache
Use Redis sorted sets for hot feed windows.

### Phase 4: ML/recommendation service
Only after meaningful data exists.

## One-photo-at-a-time rule

The feed should deliver items in a sequence. The app preloads only:
- current Momeant
- previous Momeant
- next Momeant

## Resurfacing rules

Resurface a memory when:
- same date anniversary
- important person birthday/anniversary
- user revisits related memories
- seasonal or holiday relevance
- long-unseen high-significance Momeant

Avoid resurfacing:
- ex-relationship content after user suppresses it
- reported/hidden memories
- painful categories if user opts out
- memories involving blocked users

## Explainability

The app can show subtle reasons:
- “Because she’s one of your Important People.”
- “From this week last year.”
- “A memory with your close circle.”

Do not expose raw scores to users.
