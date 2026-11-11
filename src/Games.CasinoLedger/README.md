# TheSingularityWorkshop.Games.CasinoLedger

A host-neutral, simulation-only accounting kernel for virtual casino experiments.

**Implemented:** atomic balanced postings, per-currency reconciliation, immutable journal snapshots, optional overdraft rules, idempotent transaction IDs, and tested buy-in/cash-out/wager/settlement builders. This package does not implement a cashier UI, game rules, persistence, payment processing, real-money gambling, or a complete casino settlement workflow.

## First step

Register accounts, then post a balanced transaction. Negative balances must be explicitly enabled for accounts such as a simulation clearing account.

```csharp
using TheSingularityWorkshop.Games.CasinoLedger;

var ledger = new CasinoLedger();
ledger.RegisterAccount("player:001");
ledger.RegisterAccount("cage:cash");
ledger.RegisterAccount("system:cash-issuance", allowNegativeBalance: true);

ledger.Post(new LedgerTransaction(
    "fund-player-001",
    "simulation-funding",
    DateTimeOffset.Parse("2026-10-14T12:00:00Z"),
    [
        new LedgerPosting("system:cash-issuance", "S$", -100m),
        new LedgerPosting("player:001", "S$", 100m)
    ]));

var report = ledger.Reconcile();
if (!report.IsBalanced)
    throw new InvalidOperationException("Ledger reconciliation failed.");
```

The example funds a simulation account from an explicitly named issuance/clearing account. It does not represent real money or a payment.

## Contracts

- Each transaction must contain at least two nonzero postings.
- For every currency represented in a transaction, signed posting amounts must sum to exactly zero. Different currencies are balanced separately, not converted or netted together.
- Accounts must be registered before use. Balances cannot go below zero unless the account was explicitly registered to permit it.
- Posting is atomic: invalid transactions do not partially change balances or the journal.
- A transaction ID is an idempotency key. Reposting identical content returns `false`; using the same ID for different content throws.
- Amounts use `decimal`; currency codes are trimmed and normalized to uppercase invariant. Account and transaction IDs are case-sensitive and caller-defined.
- Event timestamps and transaction kinds are supplied by the caller. The ledger does not silently generate randomness, IDs, timestamps, exchange rates, game outcomes, or accounting policy.
- `CasinoLedgerOperations.BuyIn` and `CashOut` post a paired cash/chip exchange using independently supplied amounts; no fixed exchange rate is assumed.
- `PlaceWager` moves player chips to table escrow. `SettleWager` returns the caller-supplied gross payout, credits house gaming revenue when payout is below the wager, or debits house reserve for the excess when payout is above the wager.
- Settlement helpers require a valid existing escrow balance and available reserve when needed, but they do not prove that a wager was authorized or prevent a caller from settling the same wager again under a different transaction ID. Callers must own wager identity and lifecycle.
- The ledger is in-memory and thread-safe within one process. It is not durable across restarts or distributed across hosts.

## Common transaction builders

After registering and funding the referenced accounts, the operations API can post the standard simulated casino movements:

- `BuyIn(...)` and `CashOut(...)` for cash/chip exchange.
- `PlaceWager(...)` to move player chips into table escrow.
- `SettleWager(...)` to settle escrow against a caller-provided gross payout and, when required, the house reserve.

The caller supplies transaction IDs and timestamps. Game rules remain responsible for determining the wager and payout; these helpers only validate and post the accounting movements.

## Verification

Run the dedicated contract suite:

```sh
dotnet test tests/Games.CasinoLedger.Tests/TheSingularityWorkshop.Games.CasinoLedger.Tests.csproj --configuration Release
```

The broader engineering rationale and planned casino vertical slice are documented in [Casino ledger design](../../docs/GAMES_CASINO_LEDGER_DESIGN.md).

## Package status

Version `0.1.0-alpha.1`. Package-on-build and publication remain disabled. This library is not yet an `IMicroBundle` artifact; no Bundle ID has been assigned or inferred.
