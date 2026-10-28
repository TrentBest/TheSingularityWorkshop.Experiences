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

## Multiplayer scale and four-pane arena

### Two presentation modes, one player roster

The match model must not impose a four-player cap—or another arbitrary fixed product limit. The four-pane layout is a **local viewing budget**, not a limit on how many players can join a room. Keep the authoritative player roster, match standings, and round-win totals separate from the four visible board panes. Player capacity is intended to scale with the host's actual hardware and measured resource envelope, so better hardware can support more participants without a product redesign.

- **Small room (1–4 players):** use a four-pane 2×2 arena and show every participant at once. The local player's board occupies the upper-left pane; other joined players fill the remaining panes in a stable order. Empty panes are reserved/visually quiet rather than filled with invented opponents.
- **Large room (5+ players):** keep the same four-pane 2×2 composition, with the local player's board fixed in the upper-left pane. The other three panes are selected from the live standings as the most relevant rank-neighbors/contenders, not by allocating one pane to every remote player. Selection updates at a defined standings boundary (preferably round end, or a deliberate low-frequency refresh) so panes do not thrash as rankings change. Every player remains in the authoritative roster and central standings even when not currently visible.
- **Unlimited means hardware-scaled capacity:** do not choose an arbitrary product-level maximum player count. The target is as many concurrent players as the available host hardware and operating conditions can sustain. Capacity should rise naturally when hardware improves, without changing game rules or the four-pane UI. Use runtime resource measurements, load tests, and graceful admission/backpressure to discover and protect the practical operating envelope; any emergency safety ceiling must be an operational safeguard, not a hardcoded gameplay limit. Never advertise a capacity number that has not been measured.

### Large-room pane selection

For a local player ranked first, the three comparison panes show ranks #2, #3, and #4. For a local player lower in the standings, keep the local player in the upper-left and choose the other panes from the leaders and the local player's nearest rank-neighbors, avoiding duplicate identities. The selector must define deterministic tie-breaking (for example, stable join sequence then player ID), handle fewer than four active players, and preserve the selected set for the duration of a round unless the match explicitly requests a refresh. This makes each player's local view useful without pretending every client renders the entire room.

The exact rank-neighbor policy should be a pure, unit-tested selection function, independent of UI and network transport. A spectator/standings view can expose the complete roster without changing the four-pane budget.

### Central standings bar and double score

Reserve a deliberately wider central divider/bar between the four board panes. The puzzle jars can be narrower than a traditional full-screen solo board, so the surrounding pane geometry must preserve a readable playfield while giving the standings enough horizontal room. Treat the center bar as a real layout region, not a thin decorative line.

List every joined competitor in the center standings, with a **double score** per player:

1. **Round rank** — current placement in this round (rank 1, rank 2, and so on; ties follow a documented deterministic ranking policy).
2. **Rounds won** — the number of rounds that player has won in the current match/series.

Show these as two distinct values (for example, `#2  ·  4 wins`), never as one ambiguous combined score. Round rank is recomputed from the current round's authoritative result/progress; rounds won increments only when a round is officially awarded. Do not infer a round win from a momentary lead. Define tie-breaks and what happens when a player disconnects before implementing the scoreboard.

The standings are authoritative match data rendered locally. A client must not be allowed to submit its own rank, score, or round-win total. The central bar must remain navigable/readable for keyboard and assistive technology; narrow boards must not become the only place color or status is conveyed.

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

Current state: **deterministic core and in-process authority contract implemented; presentation integration and multiplayer remain in progress**.

Implemented and described in the current development README:
- `CapsuleBoard` and `CapsuleGame` implement seeded piece generation, falling-capsule actions, placement, matching, gravity, and resolution, with focused tests.
- FSM_API runtime instances own game and match-series lifecycle; disposable room and replay objects release their runtime instances.
- `AuthoritativePuzzleRoom` validates the versioned command envelope, room/player identity, and monotonic per-player command sequences. This is an in-process authority contract—not a socket server, authentication provider, or production security boundary.
- Versioned replay serialization and playback are implemented.
- `ArenaPaneSelector` defines the four-pane selection policy, and `CapsuleArenaSnapshot` captures selected player views and standings from the authoritative room.
- `CapsuleArenaSvgRenderer` can serialize that arena snapshot directly to standalone SVG, including puzzle panes and a central standings rail.

The last item is a useful visual foothold, **not** proof of the desired shared presentation path: this renderer currently writes SVG directly from Experiences and bypasses the GUI semantic tree, Renderer observer/representation contracts, and Visualize integration. The first playable vertical slice must bridge those boundaries and demonstrate a legal semantic command changing authoritative state and updating the manifested view.

Still outstanding: the integrated GUI → Renderer → Visualize path; complete interactive solo loop; authoritative versus pressure/scoring and match progression; real network transport and independent clients; disconnect/reconnect behavior; named benchmarks; and polished tutorial/presentation. CI status must be read from the current workflow run rather than inferred from this design document. Keep issue #22 open until its acceptance criteria have evidence.

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/Avatar.jpg" alt="The Singularity Workshop avatar" width="180" /></p>
