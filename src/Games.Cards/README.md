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

## Boundaries

This library does not implement Poker or Blackjack rules, betting, account balances, money movement, real-money gambling, payment processing, casino UI, or game settlement. Rules-specific MicroBundles should consume these primitives rather than copy deck logic.

The package project is independently buildable and packing is opt-in. Its `IMicroBundle` adapter and publishable MicroBundle identity are intentionally not declared here until the ecosystem's authoritative ID allocation can be verified.
