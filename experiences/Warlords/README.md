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
- Seeded six-sided combat using the explicit `combat.xorshift32-d6/v1` algorithm, with the match seed and both rolls represented in the event history. The PRNG is implemented in the domain rather than delegated to runtime-specific `System.Random` behavior.
- Territory capture, stronghold ownership, and an explicit three-stronghold victory condition.
- Immutable point-in-time snapshots and ordered, explainable events.
- An authoritative opening campaign vector at `experiences/Warlords/vectors/four-kingdoms-opening-seed-17.json`, replayed in tests against exact accepted events and the final snapshot; this is the source fixture intended for the Gauntlet adapter.
- Focused tests for map invariants, turns, recruitment, movement, combat, deterministic replay, and rejected actions.

The initial combat rule is intentionally simple: each committed army contributes two strength and each side receives one seeded six-sided roll. The higher total wins; ties defend. Survivors are calculated by the versioned rule in `WarlordsGame`. This is a game rule, not a physical simulation.

## Run the domain tests

From the repository root:

```powershell
dotnet test tests/Warlords.Tests/TheSingularityWorkshop.Experiences.Warlords.Tests.csproj
```

The library targets .NET 10 and currently uses only the .NET base class library. It does not add a stable NuGet dependency or publish anything.

## Play a local match

A small console shell exercises the same C# domain through the `Warlords.Runtime` adapter. The adapter uses FSM_API for the match lifecycle; the shell does not own game state or rules:

```powershell
dotnet run --project samples/WarlordsConsole/WarlordsConsole.csproj
```

Use `status` to inspect the board and legal marches, `march azure-keep frostmarch 4` to test a first expansion, and `end` to advance the turn. All four kingdoms are human-controlled in this initial local proof.

A first browser manifestation is also available:

```powershell
dotnet run --project samples/WarlordsWebApp/WarlordsWebApp.csproj
```

The local Blazor Server sample renders the authoritative match snapshot through the neutral `GuiBuilder` tree and `BlazorGuiRenderer`. Its legal-march buttons dispatch stable command IDs back to a host allow-list, which rechecks each request against the current domain legal-march set before applying it. The server rejects non-loopback clients. This is a proving sample, not yet the WebPage/FSM_COS MicroBundle installation path.

## Architecture: use the Workshop, don't bypass it

| Concern | Owner / technology | First-slice status |
|---|---|---|
| Kingdom rules, map identity, turns, combat, victory, event history | Experiences / Warlords domain | Implemented in this branch |
| Core lifecycle and state-machine primitives | FSM_API 1.0.13 | Match lifecycle adapter implemented in `src/Warlords.Runtime`; domain rules remain authoritative |
| Semantic GUI and browser manifestation | GUI.Core + GUI.Blazor 0.1.0-alpha.8 | Local `WarlordsWebApp` sample renders the semantic tree; domain remains authoritative |
| MicroBundle definitions, providers, dependencies | MicroBundleDomain | Planned adapter boundary; no Warlords bundle identity assigned yet |
| Capability composition and dependency closure | FSM_COS | Intended composition path; not yet wired |
| Host-authored execution lifecycle | FSM_Execution | Future orchestration boundary; not the game rules owner |
| Semantic interaction and intent | FSM_UserIO | Future host-neutral command vocabulary |
| Semantic controls and board interaction | GUI | Future manifestation surface |
| Map visualization and rendering | Visualize / Renderer | Future manifestation surface; cannot decide ownership or combat |
| Campaign vectors and failure evidence | Gauntlet | Consumes Experiences-authored vectors; does not own Warlords rules |
| Canonical save/load and versioned state transfer | FSM_Serialization | Planned once the domain snapshot contract is stable |
| Browser / desktop / VR host | WebPage / AnyApp / future MyVR | Not connected by this slice |

The intended path is:

```text
Warlords Experience definition
  -> versioned MicroBundle artifact(s)
  -> host resolves requirements and artifact identity
  -> FSM_COS composes the verified capability set
  -> host supplies GUI / UserIO manifestation
  -> FSM_API lifecycle advances the match session
  -> Warlords domain validates and applies the command
  -> immutable snapshot / event record is presented or serialized
```

The domain does not reference WebPage, AnyApp, GUI, Visualize, FSM_API, FSM_COS, or a game-specific renderer. The separate `Warlords.Runtime` adapter is the only FSM_API boundary. This keeps the same match rules usable by browser, desktop, and VR manifestations.

## Known gaps — intentionally visible

- No playable browser or desktop board yet.
- No `IMicroBundle` adapter, artifact hash, or published MicroBundle yet. We must allocate stable bundle identity through the Workshop's ID policy before creating one.
- Package policy for future adapters: treat FSM_API and FSM_Serialization as the only stable Workshop package exceptions (verify their current versions); use prerelease package versions for other Workshop dependencies, or repository source references during coordinated development.
- No FSM_COS composition or FSM_Execution workflow integration yet.
- The `Warlords.Runtime` adapter now uses FSM_API 1.0.13 to own the Command → Finished match lifecycle; it does not move turn, combat, or ownership rules out of the Warlords domain.
- No FSM_Serialization save/load adapter yet.
- No AI opponent, diplomacy, economy, supply, fog of war, alliances, multiplayer authority, or network protocol.
- The four-kingdom map is currently authored in the C# starter-map factory; moving it to a versioned content artifact is a follow-up.
- The console match is a local developer shell, not the intended visitor-facing board. CI and a complete host-level playthrough remain to be verified.

Do not present a domain test as proof of a playable WebPage experience. The next milestone is a tiny visible board that consumes this exact domain API—no JavaScript copy of the rules and no UI-owned game state.
