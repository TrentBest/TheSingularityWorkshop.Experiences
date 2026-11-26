# TheSingularityWorkshop.Games.Blackjack

A first deterministic Blackjack rules capability for Workshop Experiences, built on the shared Cards primitives.

## Implemented in this slice

- Hand valuation with ace softening, face-card values, bust detection, and natural Blackjack detection.
- Configurable dealer soft-17 policy.
- Configurable natural payout policy and table limits.
- Legal-action calculation for hit, stand, double down, and split eligibility.
- .NET 10 target; package generation remains disabled.

## Deliberate boundaries

This is a rules kernel, not yet a complete playable round coordinator. It does not draw cards, manage split-hand state, decide settlement, hold balances, provide UI, or integrate with the casino ledger. Callers supply cards and current decision context. Wager lifecycle and accounting remain separate and explicitly authorized. No real-money gambling, payment, or redemption capability is present.

Next: add a deterministic round state machine that owns dealing/action progression and emits immutable outcome records; a separate adapter can then settle a stable wager identity exactly once through CasinoLedger.
