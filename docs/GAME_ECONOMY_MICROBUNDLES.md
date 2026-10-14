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

The order is about validating shared contracts; it does not imply a release schedule.

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

Until those checks pass, the package is **planned or in development**, not published or runtime-ready.
