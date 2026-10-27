![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# Capsule Clinic — Experience and implementation contract

## Purpose

Capsule Clinic is an original falling-capsule puzzle Experience. Its purpose is twofold:

1. deliver a genuinely enjoyable arcade puzzle with solo and competitive play;
2. demonstrate that Workshop Experiences can be assembled from small, explicit, testable capabilities, including a real networking boundary.

This document defines the contract and sequence of work. It does not claim that planned behavior has already been implemented.

## Product principles

- **Play first:** users should reach a playable loop quickly.
- **Original expression:** use an original title, art direction, sounds, music, animation, layout, and terminology. The genre may be familiar; do not copy Dr. Mario names, characters, sprites, music, UI, or other protected expression.
- **Rules before rendering:** the core simulation must not depend on a UI framework, graphics engine, socket implementation, or wall-clock timing.
- **Determinism:** a fixed rules version, initial seed, and accepted command sequence must reproduce the same state transitions.
- **Explicit authority:** one match authority validates player commands and determines outcomes.
- **Small contracts:** the tutorial path should show the minimum needed to make a variation, not require visitors to understand every subsystem.
- **Evidence over claims:** tests, replay, and benchmarks must back up correctness and performance statements.

## Domain model

### Puzzle board

The board owns its dimensions, cell occupancy, virus placement, capsule segments, and terminal conditions. It exposes safe state snapshots; callers do not mutate its internal grid directly.

### Capsule and input

A capsule has two colored segments and a defined pivot/orientation. Player intent includes operations such as move left/right, rotate, soft drop, and hard drop where supported. Input validation and legal-move rules belong to the simulation.

The implementation must define rotation near walls/occupied cells explicitly. Do not quietly import another game's proprietary rule set; write and test this project's own clear rules.

### Resolution

The engine resolves placement in explicit phases:

1. lock the capsule when placement ends;
2. identify qualifying same-color runs;
3. remove qualifying cells according to the documented matching rule;
4. split surviving capsule halves where required;
5. apply gravity to unsupported pieces;
6. repeat matching/gravity phases until stable;
7. evaluate cleared targets, score, chain count, and top-out.

The matching threshold, connectivity, split behavior, and scoring table must be constants or rule configuration with focused tests. Avoid hidden behavior in the renderer.

### Deterministic piece generation

Piece generation is seeded and independent of UI timing. The seed and rules version are recorded with a match. If multiplayer uses different random streams, their ownership and initialization must be specified rather than assumed.

### Match lifecycle

A match coordinates player identity, board state, match phase, accepted commands, scoring/pressure, winner, and rematch. The initial networked implementation should use an authoritative host to validate player intent and own the final state.

## Multiplayer boundary

### Client responsibilities

A client sends bounded, versioned intent messages for an authenticated-in-session player. It may render prediction or animation, but any prediction is provisional until reconciled with authority.

A client must not be allowed to submit arbitrary board snapshots, award itself points, select the winner, or act as another player.

### Authority responsibilities

The authoritative match host validates:
- protocol and rules compatibility;
- room membership and player identity;
- command type and legal phase;
- sequence/duplicate policy;
- rate and payload limits;
- command freshness and match/session state.

It applies accepted commands in a defined order and emits authoritative state transitions or snapshots. Rejected commands return explicit, bounded error information that does not leak secrets.

### Message contract

The initial protocol should define a versioned envelope containing only fields needed for reliable interpretation, such as:
- protocol version and message type;
- room/session identifier;
- player identity or session-bound actor;
- sequence number and, where applicable, simulation tick;
- bounded payload;
- correlation/acknowledgement metadata only where the behavior requires it.

The exact wire format and transport are decisions to validate with tests and benchmarks. Do not bind the game rules to WebSocket, TCP, UDP, or a particular hosting provider.

### Reliability and failure behavior

Specify what happens when a command is duplicated, delayed, stale, malformed, out of order, or received after a player disconnects. Define room expiry and reconnect identity before claiming reconnect support. Bound queues and payload sizes. Do not treat transport delivery as proof that a command was valid or applied.

The first milestone should use an in-process transport adapter to test the client/server contract without network nondeterminism. Real transport follows once the contract tests exist.

### Security boundary

A room code is a discovery/join mechanism, not a secret sufficient for authorization by itself. The host must validate membership and allowed actions. Internet exposure, authentication, denial-of-service resistance, deployment hardening, and anti-cheat require explicit threat modeling and tests; a local two-client demo alone does not establish production security.

## Replay and observability

Record the rules/protocol version, seed, accepted commands, and significant authoritative events needed to reproduce a match. Replay should run against the same simulation contract without sockets or animation.

Diagnostics should make it possible to inspect:
- room and match lifecycle;
- command acceptance/rejection reasons;
- sequence and tick progression;
- state/replay divergence;
- disconnect and reconnect transitions;
- message counts, payload bytes, and timing.

Do not log credentials or unnecessary personal data.

## Benchmarks

Use named, reproducible workloads. Keep simulation and transport measurements separate.

At minimum, benchmark:
- puzzle update and resolution under controlled board patterns;
- chain reactions at documented board sizes;
- state snapshot creation;
- message encode/decode;
- in-process command routing;
- two-client and increasing-client message fan-out once a real transport exists.

Report workload, runtime/build settings, operations, allocations, throughput or latency distribution, and participant count. Do not publish a single latency number without describing the measurement boundary. Do not claim scalability from an in-process benchmark.

## Presentation and accessibility

The presentation should have readable color/shape distinctions, clear focus and control hints, adjustable speed where sensible, reduced-motion consideration, keyboard-first play, and responsive layout. Color should not be the only way to distinguish pieces. Animations communicate events but do not own the game rules.

Sound and visual assets must be original or appropriately licensed. Keep asset licensing and attribution with the project.

## Teaching the Workshop

The tutorial should walk through three levels:

1. **Play:** build/run the smallest supported host.
2. **Change:** adjust seed, board size, palette, matching/pressure rules, or presentation through explicit configuration or replaceable contracts.
3. **Extend:** add a new message, bot opponent, replay visualizer, or alternate host without copying the simulation engine.

Each example must identify the files changed and the tests that verify the behavior. Avoid a tutorial that works only because of hidden IDE state or untracked assets.

## Implementation sequence

1. Rules and deterministic simulation contracts.
2. Focused engine tests and seeded piece generation.
3. Playable single-player presentation.
4. Match/versus lifecycle.
5. In-process transport contract and authority tests.
6. Real two-client room, versioned protocol, and bounded validation.
7. Failure/reconnect tests and deterministic replay.
8. Named benchmarks, diagnostics, documentation, and polish.

## Acceptance and honest status

The canonical acceptance checklist is [issue #22](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/issues/22). Keep it open until there is evidence for the required behaviors.

Current state: **definition only**. This document does not imply that the engine, UI, real network transport, replay, or benchmarks are already implemented.

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/Avatar.jpg" alt="The Singularity Workshop avatar" width="180" /></p>
