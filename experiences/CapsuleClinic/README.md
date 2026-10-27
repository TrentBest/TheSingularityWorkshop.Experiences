![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# Capsule Clinic

**A capsule-matching arcade Experience and a hands-on demonstration of reusable game logic, deterministic simulation, and multiplayer networking.**

Capsule Clinic is an original Workshop project in the falling-capsule puzzle genre. The aim is to combine a quick-to-understand game with an architecture visitors can explore: one rules engine, explicit match rules, replaceable presentation, and a small, inspectable multiplayer boundary.

This page describes the intended Experience. It is **not yet a claim that a playable game or networked match exists**.

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
- [Tracking issue and acceptance criteria](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/issues/22)

### Current status

**Stage: definition and architecture.** The Experience has an identity, ownership boundaries, and a staged acceptance plan. The playable engine, UI, multiplayer transport, replay, and benchmarks are not yet implemented by this definition alone.

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/Avatar.jpg" alt="The Singularity Workshop avatar" width="180" /></p>
