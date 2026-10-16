# Playing Cards: Design Rationale and Testable Contracts

**Status:** Implemented capability slice; rationale below describes the current API, not a future MicroBundle adapter.

## Problem

Poker, blackjack, and other card-based experiences need the same basic representation of a card and the same mechanical operations on a draw pile. If each game recreates card identity, deck construction, shuffling, and drawing, the implementations can drift in ordering, edge-case behavior, and test seams. A shared foundation should centralize those mechanics without importing any particular game's rules or presentation stack.

## Design claim

A host-neutral card primitive is reusable when it owns only card identity and pile mechanics, makes randomness an explicit dependency, exposes observable state without handing out mutable storage, and leaves game-specific legality to the consumer.

This claim is deliberately narrower than “a complete card-game system.” The implementation does not decide who owns a card, whether a duplicate is legal in a particular hand, how a hand scores, when a round ends, or what an Ace is worth.

## Current contract

### Card identity

- `PlayingCard` is an immutable value type containing a suit and rank.
- The standard deck uses Clubs, Diamonds, Hearts, and Spades; ranks are ordered Two through Ace.
- Red/black classification recognizes only defined suits. An invalid enum cast is not silently classified as black.
- Rank is not score. Ace valuation and game-specific ordering rules remain with the game capability.

### Deck and pile behavior

- `CreateStandard()` creates 52 unique suit/rank combinations, ordered by suit and then rank.
- The sequence constructor preserves caller order and allows repeated valid cards. This supports multi-deck shoes and specialized decks without forcing one global uniqueness policy.
- `Draw()` removes the final draw-pile item and returns `null` when no card remains.
- `Discard(card)` rejects undefined suit/rank values, but does not verify that the card came from this deck or prevent duplicates. A caller may be holding a card in a separate hand; ownership and legal transitions belong to the game layer.
- `CollectDiscards()` appends the discard pile to the draw pile in discard order, then clears the discard pile. It does not shuffle.
- `Cards` and `Discards` return read-only snapshots. A previously acquired snapshot intentionally does not change when the deck changes.
- A deck instance is mutable and not thread-safe. Sharing it across concurrent callers requires external synchronization.

### Randomness

`Shuffle(Random)` uses the in-place Fisher–Yates algorithm and calls the supplied source for swap indices. A reproducible test must control the starting order, random seed, and runtime implementation. `System.Random` is not a cryptographic random source and must not be used as one for security-sensitive outcomes.

Injecting randomness is a test seam, not a claim that all consumers should share one global generator. The caller decides the random source's lifetime and policy.

## Why this boundary?

| Alternative | Why it is not the current design |
|---|---|
| Put poker and blackjack rules in Cards | Couples a shared primitive to unrelated rule systems and makes each consumer carry rules it does not need. |
| Hide randomness in a global/static generator | Makes deterministic replay and isolated tests harder, and hides a consequential dependency. |
| Return the internal `List<PlayingCard>` | Allows consumers to reorder or mutate a pile without going through deck operations. |
| Reject every repeated card in the constructor | Prevents legitimate multi-deck shoes and specialized decks; uniqueness is a policy for `CreateStandard()`, not every possible deck. |
| Enforce card ownership inside `Discard` | The draw pile does not represent cards currently held in players' hands. Ownership belongs to the consuming game state. |
| Make the deck thread-safe by default | Adds synchronization and semantics that a small deterministic rules primitive does not require. Callers should synchronize at the state-machine/turn boundary if sharing is needed. |

These are scoped trade-offs, not universal claims that the alternatives are always wrong.

## Complexity and observable costs

Let (n) be the number of cards in the draw pile and (d) the number in the discard pile.

- Constructing from a sequence: (O(n)) time and (O(n)) storage.
- Shuffling: (O(n)) time and (O(1)) auxiliary storage.
- Drawing one card from the end of the list: (O(1)) time.
- Discarding one card: amortized (O(1)) time.
- Collecting discards: (O(d)) time and (O(d)) additional draw-pile storage as capacity grows.
- Reading either pile snapshot: (O(n)) or (O(d)) time and copied storage per read.

Snapshots favor a simple safe inspection contract over zero-allocation repeated reads. If profiling shows this to be a bottleneck, a future API can add explicit enumeration or copy-to-buffer options without silently changing snapshot semantics.

## Evidence and falsifiability

The contract is testable, not just descriptive. The test suite should fail if any of these statements stop being true:

1. The standard deck does not contain 52 unique cards with 13 cards per suit.
2. Two standard decks shuffled from the same initial order with the same seed and runtime do not produce the same order.
3. Drawing 52 cards from a standard deck yields each card once, and the next draw returns `null`.
4. Collecting one discarded drawn card restores the count and empties the discard pile.
5. An invalid suit or rank can enter the discard pile.
6. An undefined suit is reported as black.
7. A previously obtained snapshot changes after the live deck mutates.

A test passing is evidence for the tested contract, not a proof of every possible behavior. The workflow result and current source revision should be cited in the pull request when reviewing or releasing changes.

## Integration boundary

Cards remains a plain .NET library in the temporary Experiences source home. It does not yet implement `IMicroBundle`, declare a MicroBundle ID, or claim repository publication readiness. Do not infer an ID from another bundle family. When authoritative identity allocation is available, an adapter should preserve this rules-library boundary rather than make Cards depend upward on composition, hosts, or GUI.

## Open questions before wider adoption

- Should future multi-deck shoe behavior be a separate explicit type, or is the current caller-supplied sequence sufficient?
- Does a consuming game need a first-class hand type, or should hands remain in the game-specific rules package?
- Do measured snapshot allocations justify adding a lower-allocation inspection API?
- Which authoritative bundle identity allocation and artifact metadata are required before Cards can be composed through the MicroBundle runtime?

These questions are intentionally open. They should be resolved by consumer requirements, profiling, and the actual repository contract—not by guessing during this first capability slice.
