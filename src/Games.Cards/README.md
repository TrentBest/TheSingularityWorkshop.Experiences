![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# Workshop Games — Cards

A small, host-neutral playing-card capability intended to become the shared card foundation for Poker, Blackjack, and other Experience MicroBundles.

## Included

- Immutable card identity (`CardSuit`, `CardRank`, `PlayingCard`)
- Standard 52-card deck construction
- Fisher–Yates shuffle with caller-supplied `Random`
- Draw, discard, and collect-discard operations
- Read-only snapshots for inspecting pile order

## Example

```csharp
using TheSingularityWorkshop.Games.Cards;

var deck = CardDeck.CreateStandard();
deck.Shuffle(new Random(12345)); // repeatable for the same runtime implementation and seed
var hand = Enumerable.Range(0, 5)
    .Select(_ => deck.Draw())
    .Where(card => card.HasValue)
    .Select(card => card!.Value)
    .ToArray();
```

## Contracts and limitations

- The standard deck has 52 unique cards; the general constructor accepts any ordered sequence of valid cards, including duplicates for multi-deck shoes.
- `Draw()` removes the final card in the current draw-pile order and returns `null` when empty.
- `Cards` and `Discards` are read-only snapshots, not live views. Re-read the property to observe later mutations.
- `Shuffle(Random)` uses the supplied source; repeatability requires the same starting order, seed, and compatible runtime implementation. Do not use `System.Random` as a cryptographic source.
- `Discard(PlayingCard)` rejects undefined enum values but does not track ownership or prevent duplicates. The caller's game rules decide whether a card may be discarded.
- Card rank is identity only. Ace valuation, hand scoring, game legality, and dealing policy belong to a consuming game-rules capability.
- The class is mutable and not thread-safe; synchronize externally if sharing an instance across threads.

## Design and verification

The [design rationale and testable contracts](../../docs/GAMES_CARDS_DESIGN.md) explain the boundary, trade-offs, complexity, determinism assumptions, falsifiable invariants, and unresolved integration questions. The repository-wide [engineering quality standard](../../docs/ENGINEERING_QUALITY_STANDARD.md) defines the evidence and documentation bar for this work.

## Boundaries

This library does not implement Poker or Blackjack rules, betting, account balances, money movement, real-money gambling, payment processing, casino UI, or game settlement. Rules-specific MicroBundles should consume these primitives rather than copy deck logic.

The package project is independently buildable and packing is opt-in. Its `IMicroBundle` adapter and publishable MicroBundle identity are intentionally not declared here until the ecosystem's authoritative ID allocation can be verified.

---

<p align="center">
  <a href="https://github.com/TrentBest/TheSingularityWorkshop" aria-label="The Singularity Workshop living avatar">
    <img src="https://raw.githubusercontent.com/TrentBest/WebPage/development/TheSingularityWorkshop/wwwroot/Images/TheSingularityWorkshopLivingAvatar.svg" alt="The Singularity Workshop living avatar — a digital vortex converging on its central eye" width="112" />
  </a><br />
  <strong>The Singularity Workshop</strong><br />
  <em>If a picture is worth a thousand words, what is a living picture worth?</em><br />
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://www.patreon.com/TheSingularityWorkshop">Support the Workshop</a>
</p>
