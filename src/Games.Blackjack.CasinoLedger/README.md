# TheSingularityWorkshop.Games.Blackjack.CasinoLedger

An explicit adapter between the host-neutral Blackjack domain and the simulation-only CasinoLedger.

## Responsibilities

- Place an initial Blackjack wager or an additional double-down stake into a caller-configured table escrow account.
- Use the Blackjack domain's stable logical wager ID to prevent duplicate initial/additional stake placement in one process.
- Settle a registered wager using the final gross virtual return from a Blackjack hand result.
- Route the settlement through `BlackjackWagerBook.SettleOnce` so a second transaction ID cannot settle the same logical wager again.
- Preserve ledger balancing and reconciliation through the existing `CasinoLedgerOperations` contract.

## Example

```csharp
var book = new BlackjackWagerBook();
var adapter = new BlackjackCasinoLedgerAdapter();

// The caller must register/fund the ledger accounts first.
adapter.PlaceWager(
    ledger, book, "round-42:hand-0", "round-42:hand-0:place", occurredAt,
    playerChipAccount: "player:42",
    tableEscrowAccount: "table:blackjack:escrow",
    amount: 10m);

// If that hand doubled down, escrow the additional amount against the same hand ID.
adapter.AddStake(
    ledger, book, "round-42:hand-0", "round-42:hand-0:double", occurredAt,
    playerChipAccount: "player:42",
    tableEscrowAccount: "table:blackjack:escrow",
    amount: 10m);

var handResult = roundResult.Hands[0];
adapter.SettleWager(
    ledger, book, "round-42:hand-0", "round-42:hand-0:settle", occurredAt,
    tableEscrowAccount: "table:blackjack:escrow",
    playerChipAccount: "player:42",
    houseReserveAccount: "house:reserve",
    houseGamingRevenueAccount: "house:gaming-revenue",
    grossReturn: handResult.GrossReturn);
```

## Important limits

- This adapter moves fictional virtual chips only. It does not process payments, cash redemption, or real money.
- The caller owns account registration, funding, authorization, and timestamps.
- The wager book is in-memory and process-local. It is not durable and cannot coordinate multiple hosts.
- Consume the round's incremental wager commitments in sequence order: initial stake creates a hand wager, split creates a new hand wager, and double down adds stake to the existing hand wager.
- The settlement callback and ledger posting are serialized with the book lock inside one process, but this is not a distributed transaction.
- If durable recovery is required, persist wager lifecycle and use a transactional outbox/idempotency contract before claiming exactly-once behavior across restarts.
