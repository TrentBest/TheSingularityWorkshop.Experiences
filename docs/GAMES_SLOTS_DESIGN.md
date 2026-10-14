# Slot simulation: contracts, probability, and the next vertical slice

> Status: source-aligned design note for the current `Games.Slots` library. This is a deliberately small rules kernel, not a complete slot experience.

## 1. The rule boundary

The current implementation has three responsibilities:

- **Definition:** validate and retain independent reel strips and a paytable.
- **Evaluation:** validate caller-supplied stops and calculate the gross return for a single-line, all-reels-match rule.
- **Analysis:** calculate exact expected gross return and return-to-player (RTP) for that same rule set.

The library does not select stops, own a wallet, move chips, render reels, or settle wagers. Keeping stop selection outside the evaluator makes the same rule kernel useful for deterministic tests, scripted demonstrations, and later seeded simulations.

## 2. Repeated symbols are probability weights

A reel is a finite strip of stops. If a symbol appears three times on a 12-stop reel, its probability on that reel is (3/12), assuming each stop is selected uniformly and reels are independent.

For a paying symbol (s) across (n) reels:

[
P(s	ext{ matches}) = prod_{i=1}^{n}rac{c_{i,s}}{N_i}
]

where (c_{i,s}) is the number of occurrences of (s) on reel (i), and (N_i) is that reel's stop count.

The exact expected gross return per spin is:

[
E[	ext{gross payout}] = 	ext{stake}sum_s P(s	ext{ matches})m_s
]

where (m_s) is the configured total-return multiplier. The stake is included in gross return: a 5x multiplier returns five units for a one-unit stake, not six. A symbol omitted from the paytable loses even if it appears on every reel.

RTP is expected gross return divided by stake. An RTP of 0.95 means an expected gross return of 0.95 units per unit staked across the defined probability model. It is not a promise about any finite run and does not describe variance or maximum drawdown.

## 3. Why exact analysis comes before a large simulation

Monte Carlo runs are valuable for studying finite-session variation, player strategies, and concurrency. They should not be the first source of truth for a simple probability model that can be calculated exactly.

Use the analytical result as a baseline, then compare seeded runs against it. A mismatch can reveal a defect in the random-stop generator, the simulation loop, or the evaluator. Record the definition/version, reel strips, paytable, stake, seed, spin count, observed payout, and exact expected payout for reproducibility.

The current class calculates the exact expectation for the simple matching rule; it does not yet implement a seeded random generator or an experiment runner.

## 4. Accounting is a different boundary

A slot result is not a debit, credit, wager settlement, chip redemption, or proof that an account balance changed. Those are separate economic events.

The casino's eventual end-to-end path should remain explicit:

1. Buy virtual chips at the cage under configured rules.
2. Reserve or record a virtual stake according to the future game's accounting contract.
3. Produce a deterministic or reproducibly seeded game outcome.
4. Record the result and gross payout.
5. Apply the outcome to the virtual chip account through a separately defined accounting operation.
6. Reconcile player balances, outstanding chip obligations, and the house ledger.

Until that accounting contract is implemented and tested, this library must not be presented as a complete wager/settlement system. It has no real-money, deposit, withdrawal, or payment-provider behavior.

## 5. Scope and next steps

Current support is one payline and one win shape: all reels show the same configured paying symbol. The next increments should be separate, testable changes:

- seeded stop selection with a documented reproducibility contract;
- a simulation runner that records configuration and seed with each run;
- finite-session statistics and variance analysis, distinct from exact RTP;
- explicit paylines and a payout evaluator with independent test vectors;
- an Experience-level adapter that composes the rule kernel without leaking UI types into it;
- a separate chip-accounting contract, with idempotency and reconciliation tests, before any integrated casino claim.

Do not add wilds, scatter wins, bonus games, paylines, or settlement as undocumented special cases. Extend the rule model and its test corpus together.
