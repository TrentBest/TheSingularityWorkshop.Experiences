# Warlords — Experience and Rules Contract

**Status:** first rules slice implemented; host manifestation and runtime composition are planned.

## 1. Experience intent

Warlords is a turn-based kingdom strategy game about territorial expansion, command, and the consequences of committing limited forces. The first proof should be immediately understandable: four kingdoms begin at strongholds, neutral lands connect their borders, armies march across a graph, battles change ownership, and the first kingdom to control three strongholds wins.

The initial slice deliberately favors a complete, testable loop over a broad feature list. It is a reusable Workshop Experience, not a standalone game engine.

## 2. Domain ownership

- Experiences owns scenario identity, map semantics, kingdom identity, turn rules, movement legality, recruitment, combat rules, victory conditions, and canonical game events.
- FSM_API supplies lifecycle/state-machine primitives when the Experience needs explicit context lifecycle and coordinated update semantics. It does not replace the Warlords domain rules.
- FSM_COS composes verified MicroBundles from the Experience manifest. It does not own the game loop.
- FSM_Foundry executes host-authored workflows and orchestration. It does not decide who owns a territory.
- MicroBundleDomain defines generic bundle/provider/dependency contracts. It must remain game-neutral.
- FSM_UserIO expresses semantic player intent. The Warlords domain validates every command; an intent is not authorization.
- GUI owns semantic controls and dispatch. WebPage and AnyApp adapt those controls to their respective hosts.
- Visualize / Renderer present the map, armies, borders, and events. A rendered animation is never evidence that a march or battle was accepted.
- FSM_Serialization owns the future canonical save/load representation. A host must not invent a parallel save format.
- Physics is not a dependency of this board-strategy ruleset. Use it only if a later Experience feature genuinely introduces physical simulation.

## 3. Starter scenario

Scenario key: `four-kingdoms`, version `1`.

The map contains nine stable territories:

- Azure Keep, Crimson Hold, Verdant Citadel, and Amber Fort are the four strongholds.
- Frostmarch, Highplain, Sunfield, and Greenmarch are border territories.
- The Heartland is the central contested territory.

Each stronghold begins with four armies; border territories begin with one neutral garrison; the Heartland begins with two. Each kingdom begins with its own stronghold. Azure acts first.

The starter map is a connected, undirected graph. Territory IDs are stable semantic identifiers; names are presentation labels. The current map factory validates unique IDs, valid neighbor references, symmetric adjacency, non-empty neighbor sets, non-negative garrisons, and stronghold ownership.

## 4. Turn and command rules

At the start of a kingdom's turn, it automatically recruits one army at each stronghold it owns, up to the garrison cap of eight.

A kingdom may issue a march from one of its own territories to an adjacent territory owned by another kingdom or by no kingdom. A source territory must retain at least one army. The first slice supports one march per turn; ending the turn advances to the next surviving kingdom. Friendly-to-friendly redistribution is not yet supported.

Rejected commands do not mutate territory state, consume random rolls, or append events.

## 5. Combat rule version 1

Rules key: `warlords-four-kingdoms/1`.

- Each committed attacking army contributes two strength.
- Each defending army contributes two strength.
- Each side receives one six-sided die roll from the match's seeded random stream.
- Higher total wins; a tie is a defender victory.
- A winning attacker captures the territory and retains `max(1, committedArmies - max(1, ceil(defenderArmies / 2)) )` armies.
- A defending winner retains `max(1, defenderArmies - max(1, ceil(committedArmies / 2)) )` armies.
- The source territory loses all committed armies regardless of outcome.
- Every accepted march records the round, active kingdom, source/destination, committed armies, both die rolls, outcome, resulting territory states, sequence, and rule ID.

The seed plus ordered commands make the combat stream reproducible. The event record is suitable for later replay verification, but a durable cross-version replay format has not yet been implemented.

## 6. Victory and elimination

A kingdom wins immediately when it controls three of the four strongholds. Territory ownership is the source of truth for whether a kingdom remains in the match; no separate mutable elimination flag is required. The first implementation does not yet add alternate victory paths, scoring, or tie-break rules.

## 7. Manifestation contract

The domain API exposes:
- `WarlordsMap` and immutable `WarlordsTerritoryDefinition` records;
- `WarlordsGame` as the authoritative transition boundary;
- `GetLegalMarches()` for host presentation of currently valid march choices;
- `March(...)` and `EndTurn()` for validated state transitions;
- `Capture()` for an immutable inspection snapshot;
- `WarlordsEvent` for ordered outcome evidence.

A host may map stable territory IDs to screen/world coordinates and render any style it chooses. It may not maintain a second ownership/army model, decide combat, or apply a capture optimistically. The host sends a command to `WarlordsGame`, then redraws from the resulting snapshot.

## 8. Growth path

1. **First rules slice:** territory graph, turns, recruitment, movement, seeded combat, capture, victory, event history, and tests.
2. **First playable manifestation:** render the starter map; select source/destination; choose army count; display legal moves; end turn; show battle result and ownership from the domain snapshot.
3. **MicroBundle composition:** allocate IDs, define the Experience manifest and semantic requirements, package the Warlords capabilities, and verify immutable artifact identity.
4. **FSM integration:** connect lifecycle through FSM_API and compose through FSM_COS; keep FSM_Foundry as orchestration, not rules.
5. **Serialization and replay:** use FSM_Serialization for versioned canonical save/load and verify that replaying a seeded command sequence reproduces the same state.
6. **Opponents:** add deterministic computer opponents as command producers using the same legal-action API; they receive no privileged state beyond the declared information model.
7. **Strategic depth:** add friendly army movement, recruitment choices, resources, supply, diplomacy, alliances, fog of war, and additional map scenarios as separate tested capabilities.
8. **Competition:** introduce authoritative sessions and multiplayer only after local deterministic matches and replay evidence are sound.

## 9. Acceptance gates

- Domain rules remain independent of host and renderer packages.
- Every command is validated by the domain before mutation.
- Invalid commands leave state, event history, and random sequence unchanged.
- Same map, seed, and command sequence produce the same events and final snapshot.
- A host renders only from domain snapshots and never invents game outcomes.
- Tests and documentation agree with the actual rules version.
- MicroBundle identity is allocated rather than guessed; artifacts are content-pinned before execution.
- CI is green before this increment is described as verified.
