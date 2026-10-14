# Casino ledger: contracts and design rationale

## Problem

A virtual casino needs to explain every change in player funds, cage inventory, wagers, payouts, and house results. If game outcomes and accounting are entangled, a demo can display a win while balances silently disagree. The first step is a small journal whose invariants can be tested independently of a game, renderer, or host.

## Implemented boundary

```text
game outcome / cashier intent (caller-owned)
                 |
                 v
      CasinoLedger transaction
                 |
       atomic balanced postings
                 |
                 v
       per-currency reconciliation
```

The ledger is deliberately host-neutral. It does not decide whether a game won, whether a wager is legal, how a chip is valued, who owns an account, or whether a cash-out is authorized. Those are caller-owned rules until a separately reviewed service defines them.

## Invariants

1. **Balanced by currency.** Every transaction's signed postings sum to zero for each currency independently. A 100 S$ debit cannot balance a 100 CHIP credit.
2. **Atomic commit.** All identifiers, account references, per-currency totals, and projected account balances are validated before state changes.
3. **No accidental overdraft.** Negative balances are rejected unless the account explicitly permits them.
4. **Idempotency.** An identical transaction replay does not append a second journal entry. Reusing its ID with different content is a hard failure.
5. **Immutable audit record.** Transaction postings are copied at construction; journal and balance snapshots cannot mutate internal state.
6. **Reconciliation.** Summed account balances must be zero for every currency. This is a consistency check, not proof that business policy is correct.
7. **Deterministic inputs.** Transaction IDs, event time, kind, and postings come from the caller. The ledger does not invent state or hide randomness.

## Multi-currency cage example

A simple simulated 1:1 buy-in of 30 S$ for 30 CHIP can be represented as one transaction with four postings:

| Account | Currency | Change |
|---|---|---:|
| Player cash | S$ | -30 |
| Cage cash | S$ | +30 |
| Cage chip inventory | CHIP | -30 |
| Player chips | CHIP | +30 |

The sum is zero for S$ and separately zero for CHIP. This example demonstrates a journal shape; it does not establish an exchange-rate policy, prove that the cage has inventory, or implement a cashier operation. In practice, each account must be registered and funded through balanced transactions before this buy-in can post.

## Wagers and gross payout

The implemented `CasinoLedgerOperations` helpers move a wager from the player account into table escrow and settle it against a caller-supplied gross payout. A payout below the wager credits the difference to house gaming revenue; a payout above the wager debits only the excess from the house reserve. The wager escrow and reserve must have sufficient balances, or the entire settlement is rejected. Game outcome and payout calculation remain outside the ledger. The caller must also guarantee wager lifecycle correctness: these helpers cannot know whether a wager was authorized or already settled under another transaction ID.

## Failure modes and explicit non-goals

- A balanced journal can still encode an incorrect business event. Domain-specific authorization and settlement rules must be tested separately.
- A negative-enabled clearing account can hide issuance if used casually; account configuration and issuance transactions should be explicit and auditable.
- This implementation is in-memory. Process failure loses the journal, and concurrent writers across processes are not coordinated.
- Currency codes are labels, not an FX engine. No automatic conversion, rounding policy, taxes, fees, reserve model, or real-world redemption is provided.
- There is no persistence adapter, database transaction, external payment system, player wallet UI, game-specific settlement adapter, or MicroBundle descriptor in this slice.

## Falsifiable acceptance tests

The contract is inadequate if any of the following can be demonstrated:

- an unbalanced transaction commits;
- an invalid transaction partially changes a balance or journal;
- an identical replay changes balances twice;
- reusing an ID for different content silently succeeds;
- an account overdraws without its negative-balance permission;
- a currency total fails reconciliation after valid transactions;
- a returned snapshot can mutate ledger state;
- concurrent valid postings lose an update.

The test suite exercises these conditions. Future settlement helpers should remain separate and prove their own invariants, especially the relationship between wager escrow, gross payout, house reserve, and operator result.

## Remaining vertical slice

1. Connect deterministic Slots outcomes to settlement only after game payout semantics and account policy agree.
2. Add wager identity/lifecycle tracking so one authorized wager cannot be settled twice under different transaction IDs.
3. Add an operator report that distinguishes gross wagers, gross payouts, net gaming result, outstanding chip obligations, and operating costs.
4. Demonstrate one cage, one table, and one machine, then benchmark increasing instance counts while checking reconciliation at every step.
