# Game Economy MicroBundles

> **A game shop, casino table, wheel, race market, and tournament are separate capabilities—not one monolithic Economy package.**

This is the temporary authoring plan for game-economy MicroBundles while Forge authoring and publication are still being brought online. The source projects belong in this Experiences repository; the future Forge should eventually make authoring, composition, validation, and publication easier without changing what these capabilities mean.

## Boundary

```text
TheSingularityWorkshop.Economy
  Money, items, offers, accounts, transfer intent, fee policy
                     |
                     v
Game-economy MicroBundles
  cards | poker | blackjack | slots | prize wheel
  race markets | competitions | tournaments
                     |
                     v
Experience manifest + configuration
                     |
                     v
FSM_COS composition / runtime
                     |
                     v
WebApp | AnyApp | future VR manifestation
                     |
                     v
MicroBundleRepository (durable artifact delivery)
```

- **Economy** owns provider-neutral economic primitives. It does not own game rules or a game-specific UI.
- **Each game MicroBundle** owns one coherent set of rules and data, with explicit inputs, outputs, configuration, and deterministic test seams where randomness is involved.
- **The Experience** chooses which capabilities are composed and supplies configuration. It does not duplicate the rule engine.
- **The host/renderer** presents the game. No game package depends on GUI, WebApp, AnyApp, or VR.
- **The repository** stores and verifies artifacts; it is not the authoring tool or game runtime.
- **Forge** is the intended long-term authoring and publication experience. This repository is the temporary source home, not a claim that the full Forge workflow already exists.

## Package map

Each row is a distinct package boundary. Names below are proposed package IDs/names, not a claim that those packages or their runtime behavior already exist.

| MicroBundle package | Owns | Does not own |
|---|---|---|
| `TheSingularityWorkshop.Games.Cards` | Standard deck/card identity, deck composition, shuffle/draw/discard operations, hand representation | Poker strategy, blackjack rules, UI, balances |
| `TheSingularityWorkshop.Games.Poker` | Poker variants, hand ranking, round/street state, legal action validation, showdown evaluation | Card/deck implementation, account balances, real-money wagers |
| `TheSingularityWorkshop.Games.Blackjack` | Blackjack rules, hand totals, ace valuation, dealer policy configuration, hit/stand/double/split eligibility | Deck implementation, UI, cash handling |
| `TheSingularityWorkshop.Games.Slots` | Reel/stop definitions, symbol tables, configurable paytables, spin outcome evaluation | Animation, sound, real-money wagering, unreviewed probability claims |
| `TheSingularityWorkshop.Games.PrizeWheel` | Wheel segments, explicit weights, spin result mapping, prize-table evaluation | Wheel animation, hidden odds, real-money payouts |
| `TheSingularityWorkshop.Games.RaceMarkets` | Race/competition entries, market snapshots, odds/price representation, bet-type and result evaluation rules | Race simulation, live sports data, external bookmakers, settlement |
| `TheSingularityWorkshop.Games.Competitions` | Competition definitions, eligibility, scoring rules, standings, result records | Tournament bracket scheduling, UI, payment execution |
| `TheSingularityWorkshop.Games.Tournaments` | Tournament lifecycle, registration, bracket/round progression, advancement, configurable prize allocation rules | Game-specific match rules, balances, actual prize transfer |
| `TheSingularityWorkshop.Games.Wagers` | Shared virtual-wager vocabulary, stake/selection/result references, validation and settlement-intent records | An account ledger, wallet, external financial provider, or proof of settlement |

### Why keep Cards separate?

Poker and blackjack both use cards, but they do not need to own separate, incompatible deck systems. A host can compose the shared Cards capability with either game-specific rules package. Another game can reuse Cards without inheriting poker or blackjack.

### Why keep Competitions and Tournaments separate?

A competition describes how participants are scored and ranked. A tournament describes how matches or rounds are organized and how participants advance. A tournament can use a competition's scoring/results, and a competition need not be a bracket.

## Cross-cutting contract rules

1. **Simulation-first.** These packages describe game mechanics and virtual in-game economies. They do not enable real-money gambling, deposits, withdrawals, payment processing, or bookmaker connectivity.
2. **No assumed settlement.** A wager or prize result is a domain record, not evidence that value moved. Economy's `EconomyTransfer` is an intended movement of value, not a settlement receipt.
3. **Make chance testable.** Random outcomes should be driven by an explicit random source or supplied draw/stop result so tests can verify edge cases without flaky randomness. The configuration and result should preserve enough information to explain an outcome.
4. **Odds are explicit.** Wheel weights, slot paytables, and race-market odds must be inspectable configuration/data. Do not imply that a displayed payout is a guarantee or that a probability is fair without a verified calculation.
5. **Keep rules separate from presentation.** Animation timing, card art, audio, HUD layout, and input gestures belong to presentation capabilities or the host.
6. **Version configuration.** A game-rule package must define how it interprets its configuration and how incompatible configuration versions are rejected or migrated.
7. **Stable identity before publication.** Do not invent Bundle IDs or claim artifacts are publishable before IDs, versions, dependencies, and SHA-256 artifact identities are registered and validated through the repository publication workflow.
8. **Independent testing.** Rule tests should cover boundary conditions and deterministic replay before any package is used by a composed Experience.

## Flagship demonstration: the simulated casino economy

The casino should be a visible, interactive demonstration of the Economy domain—not just a collection of game screens. It gives familiar real-world intuition for exchange, liabilities, inventory, risk, probability, liquidity, operating costs, and accounting. The entire experience remains a virtual simulation; chips and Singularity Dollars have no real-world redemption value.

### Player journey: the cage is part of the model

```text
Player's Singularity Dollar balance
              |
              | buy chips (explicit exchange transaction)
              v
Player's chip balance  <---->  Tables / slots / other games
              |                         |
              | cash out                 | wager / payout
              v                         v
Dollar balance restored          House / game accounting
              \_________________________/
                   auditable ledger
```

- **Buy-in:** the player exchanges Singularity Dollars for a configured amount of digital chips. Record the exchange rate, transaction identity, and both balance changes.
- **Play:** wagers move chip-denominated value between player and game/house accounts according to each game's rules. A game outcome must not silently create or destroy balances.
- **Cash-out:** the player exchanges eligible chips back into Singularity Dollars according to the configured redemption rule. Record the transaction and resulting balances.
- **Reconciliation:** chip supply, player chip balances, chips held by games/house accounts, and chips redeemed must reconcile under the chosen accounting model. Define treatment of rounding, abandoned sessions, and outstanding chips explicitly.
- **No double counting:** selling chips increases the house's received dollars and its outstanding chip-redemption obligation; it is not automatically casino profit. A wager, payout, and redemption are different economic events.

The exact exchange rate and any fees are configuration, not assumptions embedded in a game. A first demonstration can use a fixed 1:1 exchange rate to keep the lesson legible, then expose alternative policies as experiments.

### One economy, many independently composed games

The floor can compose card games (blackjack and poker), slot machines, roulette or other table games, prize wheels, competitions, and tournament experiences. Each machine/table is an instance of a rule capability plus configuration and state; it is not a new copy of the Economy package. Game-specific logic owns outcome rules, while Economy owns only the shared economic concepts and transactions it actually supports.

Keep the floor scalable by separating:
- **Game definition:** rules, odds/paytable, configuration version, and required capabilities.
- **Game instance:** identity, current session/state, limits, and current activity.
- **Presentation:** model, animation, audio, controls, and table/machine visuals.
- **Economic record:** buy-in, wager, payout, fee, redemption, and any adjustment—each with an explicit type and correlation identity.
- **Aggregate view:** derived floor metrics computed from the underlying records, not independently mutated duplicate totals.

The long-term target is a floor with hundreds of table and machine instances, with a smaller set of reusable rule engines and data-driven configuration. Treat that as a scale target to demonstrate and measure, not a claim that the current repository already runs hundreds of games.

### The house: measure the economy, don't fake a win

The operator view should make the economic system inspectable. At minimum, distinguish these metrics:

| Metric | Meaning |
|---|---|
| Chips issued and chips redeemed | Cumulative chip exchange flow; not profit |
| Outstanding chip liability | Chips still eligible for redemption under the configured rules |
| Wagers and gross payouts | Activity and returned value; neither alone equals profit |
| Net gaming result | Wagers less game payouts for the same defined period and accounting scope |
| Operating costs | Explicit configured costs such as staffing or machine upkeep, if modeled |
| Net operating result | Gaming result less modeled costs and other explicitly included flows |
| Player net outcome | Player payouts and remaining value compared with that player's stakes, with buy-in/cash-out separated |
| Per-game and per-instance results | Aggregates sliced by rule type, configuration, instance, and time window |
| Reconciliation difference | Any imbalance between expected and recorded movements; target is zero unless an explained adjustment exists |

Do not call wagers “revenue,” chip sales “profit,” or player losses “house profit” without specifying the accounting definition. Keep gross flow, net gaming result, operating result, and chip liabilities separate. If a metric cannot be derived from authoritative records, label it unavailable rather than estimating it silently.

### Make the demonstration scientific

A configurable simulation should let visitors change one assumption at a time and compare runs:
- slot symbol distributions and paytables;
- table-game rules and payout schedules;
- player strategies and session budgets;
- number of active machines/tables and activity rates;
- chip exchange rate or explicit exchange fees;
- house operating costs and available reserves;
- simulation duration, random seed, and player population.

For each run, preserve the configuration/version, seed or random-source identity, simulation interval, and summary. Explain theoretical expected return separately from observed finite-run outcomes. Tests should cover probability calculations and boundary cases; larger repeatable experiments can show variance and convergence without promising that any individual run must match its expectation.

### Scale is a measured property

“Hundreds of tables and machines” should become a benchmark scenario with declared hardware/runtime, configuration, concurrency model, duration, and output metrics. Measure throughput, latency, memory, allocation/GC pressure, queue depth, and reconciliation correctness. A fast simulation that loses transactions or produces inconsistent balances is a failed simulation. Start with a deterministic small-floor test, then increase instance count and activity while keeping the same accounting invariants.

This is a future demonstration architecture, not a statement that these game types, accounting flows, dashboards, or scale benchmarks are already implemented.

## Suggested composition examples

### RPG shop

```text
Commerce catalog + Economy money/pricing
                  +
       stock / eligibility rules
                  +
      RPG item definitions
                  |
                  v
          Shop Experience
```

Weapon, armor, spell, and potion catalogs can themselves be separate content/rules bundles where their domain justifies it. A shop is a composition of catalog, price, stock, eligibility, and presentation—not a new copy of Economy.

### Casino-style game floor (virtual simulation)

```text
Cards ──> Poker
   └────> Blackjack

Slots ───────┐
Prize Wheel ─┼──> Game floor Experience
Race Markets ┤
Competitions ┤
Tournaments ┘
       |
       v
Virtual wager / reward intent
       |
       v
Economy concepts (where relevant)
```

This diagram shows possible composition, not a currently implemented runtime graph.

## Implementation order

Build the smallest reusable foundation first, then add games as independently testable packages:

1. **Cards** — deck composition, card identity, deterministic shuffle/draw seams.
2. **Wagers** — virtual stake, selection, outcome, and result references without pretending to own a ledger.
3. **Blackjack** — a bounded first rules engine with clear state transitions and edge-case tests.
4. **Poker** — reuse Cards; keep variant-specific rules explicit.
5. **PrizeWheel and Slots** — configurable probability/paytable evaluation with deterministic tests.
6. **Competitions and Tournaments** — results and advancement without game-specific UI.
7. **RaceMarkets** — only after market/odds semantics and simulation-versus-live-data boundaries are explicit.

The order is about validating shared contracts; it does not imply a release schedule. The casino should not wait for every proposed game: a small end-to-end cage + one table + one machine + reconciled ledger is the right first vertical slice, followed by expansion to the wider floor.

## Readiness checklist

A package is not complete merely because a class implements `IMicroBundle`. Before it is treated as a usable artifact, require:

- a documented single responsibility and non-goals;
- stable identity, version, provider/dependency declarations;
- versioned configuration with validation;
- deterministic tests for rules and edge cases;
- no dependency on GUI or a particular host;
- explicit statement that outcomes are simulation results, not financial settlement;
- successful build/test and artifact hash generation;
- a manifest entry that points to the exact immutable artifact identity.

For the casino vertical slice, also require:
- buy-in and cash-out to be recorded as explicit, distinct transactions;
- chip balances and outstanding redemption obligations to reconcile;
- wagers and payouts to be auditable and repeatable from a recorded run configuration;
- game outcomes to be reproducible in tests without depending on timing or animation;
- house/player metrics to state their accounting scope and period;
- a scale benchmark to report correctness alongside performance.

Until those checks pass, the package is **planned or in development**, not published or runtime-ready.
