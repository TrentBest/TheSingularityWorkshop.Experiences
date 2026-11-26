# TheSingularityWorkshop.Games.Blackjack

A deterministic, host-neutral Blackjack rules and single-round coordination capability built on the shared Cards primitives.

## Implemented

- Hand valuation with ace softening, face-card values, bust detection, and natural Blackjack detection.
- Configurable dealer soft-17 policy, natural payout, double-after-split policy, and maximum hand count.
- Legal-action calculation for hit, stand, double down, and split eligibility.
- A caller-deck-driven round coordinator with alternating initial deal, dealer hole-card concealment, player turns, multiple split hands, split-ace one-card completion, dealer progression, and immutable result records.
- Gross virtual return calculation, including returned stake, kept separate from actual ledger posting.
- An in-memory, thread-safe logical wager book with stable wager IDs, one accepted settlement per wager, and protection against reusing a settlement transaction ID across wagers.

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
- The wager book is process-local and not durable; it serializes settlement callbacks only within one process. Durable recovery and cross-process concurrency remain future work.
- Insurance, surrender, side bets, table/seat management, persistence, UI, and background NPC play are not implemented.
- The game does not process real money, payments, or cash redemption.
- The library is host-neutral and has no invented MicroBundle ID or publication claim.

Next: add a narrow CasinoLedger settlement adapter that uses the wager book's SettleOnce guard, then compose the game into an authored Casino Experience.
