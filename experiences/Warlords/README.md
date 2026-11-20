# Warlords — First Playable Strategy Experience

**Status:** first host-independent rules slice implemented on this branch; browser/desktop manifestation and MicroBundle composition are not yet connected.

Warlords is the working name for a Workshop-owned kingdom strategy Experience. It uses the familiar territory-control loop as a starting point: expand from a stronghold, commit armies across connected territory, resolve battles, hold castles, and pursue victory. This is an original Workshop scenario and ruleset, not a reproduction of another game's art, maps, data, or proprietary implementation.

## What works in the first slice

The `src/Warlords` project contains a deterministic, renderer-independent game domain:

- A validated territory graph with symmetric adjacency and stable territory IDs.
- A four-kingdom starter map: Azure, Crimson, Verdant, and Amber.
- Strongholds, neutral territories, and starting garrisons.
- One active kingdom at a time, round progression, and automatic recruitment at owned strongholds.
- Legal march discovery and validation; a territory must retain at least one army.
- Seeded six-sided combat, with the match seed and both rolls represented in the event history.
- Territory capture, stronghold ownership, and an explicit three-stronghold victory condition.
- Immutable point-in-time snapshots and ordered, explainable events.
- Focused tests for map invariants, turns, recruitment, movement, combat, deterministic replay, and rejected actions.

The initial combat rule is intentionally simple: each committed army contributes two strength and each side receives one seeded six-sided roll. The higher total wins; ties defend. Survivors are calculated by the versioned rule in `WarlordsGame`. This is a game rule, not a physical simulation.

## Run the domain tests

From the repository root:

```powershell
dotnet test tests/Warlords.Tests/TheSingularityWorkshop.Experiences.Warlords.Tests.csproj
```

The library targets .NET 10 and currently uses only the .NET base class library. It does not add a stable NuGet dependency or publish anything.

## Play a local match

A small console shell lets you exercise the same C# domain without a browser or a second rules implementation:

```powershell
dotnet run --project samples/WarlordsConsole/WarlordsConsole.csproj
```

Use `status` to inspect the board and legal marches, `march azure-keep frostmarch 4` to test a first expansion, and `end` to advance the turn. All four kingdoms are human-controlled in this initial local proof; AI and a graphical board are follow-up work.

## Architecture: use the Workshop, don't bypass it

| Concern | Owner / technology | First-slice status |
|---|---|---|
| Kingdom rules, map identity, turns, combat, victory, event history | Experiences / Warlords domain | Implemented in this branch |
| Core lifecycle and state-machine primitives | FSM_API | Available technology; integration not yet added |
| MicroBundle definitions, providers, dependencies | MicroBundleDomain | Planned adapter boundary; no Warlords bundle identity assigned yet |
| Capability composition and dependency closure | FSM_COS | Intended composition path; not yet wired |
| Host-authored execution lifecycle | FSM_Foundry | Future orchestration boundary; not the game rules owner |
| Semantic interaction and intent | FSM_UserIO | Future host-neutral command vocabulary |
| Semantic controls and board interaction | GUI | Future manifestation surface |
| Map visualization and rendering | Visualize / Renderer | Future manifestation surface; cannot decide ownership or combat |
| Canonical save/load and versioned state transfer | FSM_Serialization | Planned once the domain snapshot contract is stable |
| Browser / desktop / VR host | WebPage / AnyApp / future MyVR | Not connected by this slice |

The intended path is:

```text
Warlords Experience definition
  -> versioned MicroBundle artifact(s)
  -> host resolves requirements and artifact identity
  -> FSM_COS composes the verified capability set
  -> host supplies GUI / UserIO manifestation
  -> FSM_API lifecycle advances the Experience
  -> Warlords domain validates and applies the command
  -> immutable snapshot / event record is presented or serialized
```

The domain does not reference WebPage, AnyApp, GUI, Visualize, FSM_COS, or a game-specific renderer. This keeps the same match rules usable by browser, desktop, and VR manifestations.

## Known gaps — intentionally visible

- No playable browser or desktop board yet.
- No `IMicroBundle` adapter, artifact hash, or published MicroBundle yet. We must allocate stable bundle identity through the Workshop's ID policy before creating one.
- Package policy for future adapters: treat FSM_API and FSM_Serialization as the only stable Workshop package exceptions (verify their current versions); use prerelease package versions for other Workshop dependencies, or repository source references during coordinated development.
- No FSM_COS composition or FSM_Foundry workflow integration yet.
- No FSM_API lifecycle integration yet; the rules currently expose explicit, synchronous transitions.
- No FSM_Serialization save/load adapter yet.
- No AI opponent, diplomacy, economy, supply, fog of war, alliances, multiplayer authority, or network protocol.
- The four-kingdom map is currently authored in the C# starter-map factory; moving it to a versioned content artifact is a follow-up.
- The console match is a local developer shell, not the intended visitor-facing board. CI and a complete host-level playthrough remain to be verified.

Do not present a domain test as proof of a playable WebPage experience. The next milestone is a tiny visible board that consumes this exact domain API—no JavaScript copy of the rules and no UI-owned game state.
