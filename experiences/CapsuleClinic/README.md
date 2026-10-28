![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# Capsule Clinic

**A capsule-matching arcade Experience and a hands-on demonstration of reusable game logic, deterministic simulation, and multiplayer networking.**

Capsule Clinic is an original Workshop project in the falling-capsule puzzle genre. The aim is to combine a quick-to-understand game with an architecture visitors can explore: one rules engine, explicit match rules, replaceable presentation, and a small, inspectable multiplayer boundary.

The deterministic board and game loop now have focused tests; game and match-series lifecycles use FSM_API runtime instances. The project also includes an in-process authority contract, replay serialization/playback, and a pure four-pane player-selection policy. The arena design supports all participants in rooms of up to four and rank-neighbor comparison panes for larger rooms; the actual visual layout and real networked match do not yet exist.

## 01 — What

Players rotate and place two-color capsules to match groups of the same color, clear viruses, and build chains. A versus mode will let two players compete using the same deterministic rules.

The project will use original naming, artwork, sound, interface design, and effects. It will not reuse Dr. Mario branding or protected game assets.

## 02 — Why

A familiar puzzle makes the architecture approachable. Visitors should be able to understand the rules quickly, then discover how the same engine can support:

- a solo puzzle;
- a local versus match;
- a networked room;
- a replay or test harness;
- a custom variant with different rules or presentation.

The showcase should make the simple path simple, while leaving the deeper engineering visible and testable.

## 03 — How

The intended ownership boundaries are:

- **Puzzle engine:** board, capsule movement/rotation, matching, gravity, chain resolution, scoring, and deterministic seeded piece generation.
- **Match rules:** players, turns/ticks, versus pressure, match lifecycle, winner, and rematch.
- **Presentation:** input, animation, sound, visual effects, and responsive UI; it does not decide game outcomes.
- **Networking:** versioned messages, room/session identity, validated player intent, bounded payloads, sequencing, and explicit disconnect behavior.
- **Replay and diagnostics:** record accepted commands/events and reproduce the resulting state transitions.
- **Host:** runs the match authority and presents the Experience. Clients request moves; they do not get to declare authoritative board state or winners.

CreatorNetwork already exists for creator-owned publishing and distribution. It is not presumed to be a real-time game transport. Capsule Clinic will use an explicit networking seam and help determine whether a separate reusable networking package is warranted.

## Multiplayer arena concept

The room can contain more players than the local screen can show. The local view uses four narrow jars in a 2×2 arena, with a **wider central standings bar** so the score/rank information stays readable.

| Upper left | Upper right |
|---|---|
| **You** — always pinned here | Selected contender |
| **Lower left** | **Lower right** |
| Selected contender | Selected contender |

- With **1–4 players**, show every participant at once and leave unused panes empty.
- With **5+ players**, keep the local player upper-left. If the local player is in the top four, show the other top-four contenders; otherwise show the leader and the local player's nearest active rank-neighbors. A full roster remains in the central standings.
- Every standings row has two separate values: **current round rank** and **rounds won in the match series**. Example: #2 · 4 wins. The host owns these values; clients cannot submit their own score or rank.
- **“Unlimited” means hardware-scaled, not mathematically infinite:** we do not impose an arbitrary product-level player cap. The room should support as many concurrent players as the host can sustain; stronger hardware can raise that capacity without changing the game or its four-pane layout. Load tests and runtime resource controls discover the practical envelope, and any emergency safety ceiling is operational protection—not a gameplay limit.

The pure ArenaPaneSelector and its tests define the selection policy. The rendered arena, central bar, authoritative versus pressure rules, and real network transport remain to be implemented.

## 04 — See the intended path

1. Build and test the deterministic rules without a renderer.
2. Make a complete single-player loop.
3. Add versus rules and an in-process transport adapter.
4. Connect two independent clients through an authoritative match host.
5. Record and replay a match.
6. Add diagnostics, named benchmarks, and a tutorial that shows how to create a variant.

Each stage should remain useful and testable. Real sockets should come after the protocol and transport seam are covered by tests.

## 05 — Documentation and progress

- [Experience and implementation contract](design/EXPERIENCE.md)
- [Canonical product acceptance checklist](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/issues/22)
- [First playable presentation integration: GUI → Renderer → Visualize](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/issues/29)

### Current status

**Stage: deterministic core, FSM_API-backed lifecycle, replay, authority contract, and arena-selection policy.** Board placement, four-in-a-row resolution, pair-aware gravity, seeded piece generation, and the falling-capsule loop have focused tests. FSM_API owns the game and match-series lifecycle; disposable room and replay objects release their runtime instances. An in-process room validates protocol version, room/player identity, and monotonically increasing command sequences. Versioned replay serialization and playback are implemented. A pure arena selector specifies a fixed local upper-left pane, complete visibility for rooms of up to four, and leader/neighbor panes for larger rooms. The rendered 2×2 arena, wider central standings bar, authoritative versus pressure rules, real transport, disconnect/reconnect behavior, and named benchmarks remain outstanding. CI validates the current core; the latest workflow run is linked from the repository Actions page.

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/Avatar.jpg" alt="The Singularity Workshop avatar" width="180" /></p>
