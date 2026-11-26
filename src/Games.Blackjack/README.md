# TheSingularityWorkshop.Games.Blackjack

A deterministic, host-neutral Blackjack rules and single-round coordination capability built on the shared Cards primitives.

## Implemented

- Hand valuation with ace softening, face-card values, bust detection, and natural Blackjack detection.
- Configurable dealer soft-17 policy, natural payout, double-after-split policy, and maximum hand count.
- Legal-action calculation for hit, stand, double down, and split eligibility.
- A caller-deck-driven round coordinator with alternating initial deal, dealer hole-card concealment, player turns, multiple split hands, split-ace one-card completion, dealer progression, and immutable result records.
- Gross virtual return calculation, including returned stake, kept separate from actual ledger posting.

## Deterministic round example

The caller owns the deck and random source. Shuffle before starting; the same deck order, rule policy, and player actions reproduce the same round.

```csharp
using TheSingularityWorkshop.Games.Blackjack;
using TheSingularityWorkshop.Games.Cards;

var deck = CardDeck.CreateStandard();
deck.Shuffle(new Random(12345));

var round = BlackjackRound.Start(
    deck,
    new BlackjackRules(dealerHitsSoft17: true),
    wager: 10m,
    availableBalance: 100m);

while (round.Phase == BlackjackRoundPhase.PlayerTurn)
{
    // A host asks the player/agent to choose from round.LegalActions.
    // Apply the selected action to advance this hand.
}

var result = round.PlayDealerAndComplete();
// result.Hands contains final outcomes and gross virtual returns.
// A separate, explicitly authorized adapter may settle them through CasinoLedger.
```

## Boundaries and remaining work

- The round is an in-memory, single-session coordinator. It is not thread-safe.
- The caller owns the deck and its shuffle/randomness policy.
- The coordinator tracks a supplied virtual balance for action eligibility; it does not own an account or post accounting entries.
- No wager identity/lifecycle or duplicate-settlement protection exists yet.
- Insurance, surrender, side bets, table/seat management, persistence, UI, and background NPC play are not implemented.
- The game does not process real money, payments, or cash redemption.
- The library is host-neutral and has no invented MicroBundle ID or publication claim.

Next: add a stable wager lifecycle and an explicit settlement adapter to CasinoLedger, then compose the game into an authored Casino Experience.
