![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# Drone Construction

**Status:** a deterministic discrete construction loop and tested traffic/lifecycle primitives are present on development. A separate rotor-driven quadrotor physics microbenchmark now measures 1,000, 10,000, and 100,000 physics instances, but autonomous lane routing inside the construction simulation, hosted live GUI.SVG observation, and full construction-agent scale evidence remain open. **100,000 construction agents is the target, not a demonstrated result.**

Drone Construction is a benign large-scale multi-agent construction experience. A source image is converted into a discrete target structure; simulated quadcopters collect blocks from ten emitters and place them in the target. The showcase goal is to demonstrate how a large population can coordinate through explicit state, resource, and traffic contracts.

The original Unity implementation was lost with its former machine. This folder is the canonical reconstruction brief. The new implementation is renderer-independent and does not require Unity.

![Drone Construction vision storyboard](assets/vision-board.svg)

*This storyboard is an authored concept diagram, not a live GUI.SVG rendering of simulation state.*

![Drone Construction architecture](assets/drone-construction-architecture.svg)

*This is an architecture sketch, not a measured performance or runtime capture.*

## The idea in a minute

1. **Image → height field.** Each image sample becomes a target height at a grid location. The initial contract must specify whether channels are interpreted independently or combined into one height; this is a configurable mapping, not an implicit assumption.
2. **Target → block tasks.** The target structure is represented as discrete cells and explicit placement tasks.
3. **Emitters → resource supply.** Ten block emitters expose available inventory and pickup capacity.
4. **Traffic → predictable movement.** Skylanes are 2D cellular grids with one-way movement. Hover lanes are 2D cellular grids for waiting and admission. Slipstreams are compact transition volumes where conflict checks are required.
5. **Agents → observable state.** Each simulated agent has a stable identity, current task, location, movement mode, and lifecycle state.
6. **Renderer + living diagrams.** Renderer presents the world; GUI.SVG documents and visualizes contracts, state transitions, and measured telemetry from actual snapshots.

## Lane execution concept

![Autonomous lane execution concept](assets/autonomous-lane-execution-concept.svg)

*This SVG illustrates the intended hot path: validate and admit at a boundary, move along a known axis toward a precomputed coordinate, and preserve occupancy when blocked. It is a concept image, not a live GUI.SVG rendering or a claim that autonomous lane execution is already integrated.*

## Snapshot-driven SVG view

`ConstructionSnapshotSvgView` now closes the first maintained-view gap: the host publishes committed snapshots to a run-scoped `ConstructionSnapshotObserver`, then calls `TryRefresh` at its chosen cadence. The view renders only when a new accepted snapshot is available, reports its rendered run/tick/sequence, and coalesces intermediate snapshots instead of building an unbounded queue. It never advances the simulation. A failed render does not consume the update.

```csharp
var observer = new ConstructionSnapshotObserver("run-001");
var view = new ConstructionSnapshotSvgView(observer, width: 20, height: 12);
observer.TryPublish(simulation.Snapshot());
if (view.TryRefresh(out var svg))
    File.WriteAllText("construction-live.svg", svg!);
```

This is a maintained pull-based standalone SVG view with focused tests; the host owns refresh cadence and snapshot publication. It is **not yet** a GUI.SVG-hosted application/manifest or telemetry feed, and no autonomous lane routing or scale claim follows from it. See [renderer contract](../../src/DroneConstruction/ConstructionSnapshotSvgRenderer.md), [view source](../../src/DroneConstruction/ConstructionSnapshotSvgView.cs), and [UML/use cases](../../src/DroneConstruction/ConstructionSnapshotSvgRendererUML.md).

## Rotor-driven quadrotor physics benchmark

A separate source-only physics harness now measures the propeller-driven rigid-body step at 1,000, 10,000, and 100,000 quadrotor instances. The recorded hosted-runner baseline completed two million physics steps in about 2.33 seconds. This is evidence for the isolated dynamics kernel only; it is **not** a 100,000-agent construction-simulation run and excludes routing, collision/ground contact, control loops, rendering, and networking. See the [benchmark methodology and results](../../benchmarks/DroneConstruction.Physics/README.md) and [workflow run 40040340639](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/actions/runs/40040340639).

## Phone-friendly review

- **[Gallery — what is real vs. concept](GALLERY.md)**
- **[Readiness ledger and next gates](design/READINESS.md)**
- **[100,000-agent validation protocol](design/100K_SCALE_VALIDATION.md)**
- **[Snapshot and living-view contract](design/SIMULATION_OBSERVABILITY.md)**
- **[Gravity sort, five-cell offsets, and route choreography](design/GRAVITY_SORT_AND_ROUTING.md)**
- [MicroBundle boundaries](design/MICROBUNDLE_BOUNDARIES.md)
- [Lane transition lifecycle](design/LANE_TRANSITION_LIFECYCLE.md)

## Traffic vocabulary

- **Skylane:** a 2D cellular grid. A lane has a declared direction, speed policy, elevation, and cell-occupancy contract. The first model assumes same-direction movement within a lane.
- **Hover lane:** a 2D cellular grid for agents that are stationary or awaiting admission to a transition. It is not an unbounded parking area: capacity and occupancy must be explicit.
- **Slipstream:** a small, bounded transition volume between a hover lane and a skylane. It is a coordination boundary, not a free-form shortcut. Entry requires a reservation/clearance decision and a conflict check.
- **Transition lookahead:** the remembered prototype used a five-cell lookahead. Treat five cells as an initial tunable simulation parameter, not a universal safe distance. Its sufficiency depends on cell units, acceleration model, tick duration, occupancy semantics, and the length of the transition.
- **Block emitter:** one of ten resource sources with inventory, pickup slots, and queue state.
- **Build cell:** a discrete target location whose required height and placed-block count can be compared.
- **Agent:** a simulated construction worker. Its behavior is deterministic when the simulation seed and inputs are fixed.

## MicroBundle boundaries

The initial catalog is defined in [MICROBUNDLE_BOUNDARIES.md](design/MICROBUNDLE_BOUNDARIES.md). The catalog is a proposal for this Experience and its IDs must be checked against the shared registry before executable packages are published. Current provider classes expose typed factories; the dependency column is the intended capability graph, not proof of a resolved runtime service graph.

The initial single-threaded loop assigns tasks, picks up finite-inventory blocks, moves agents one cell at a time under exclusive occupancy, places blocks, and emits immutable snapshots. Agent IDs use a dictionary for average constant-time lookup. Snapshot locations are qualified by space identity, and traffic occupancy has a reverse index for invariant checks.

The separate slipstream lifecycle coordinator reserves downstream capacity before entry, transfers source occupancy into the transition grid, advances orthogonally, retains occupancy/reservation when exit is blocked, and releases transition capacity only after successful exit. Simulation-level tests cover owner registration and blocked-exit retry. **These lane lifecycle primitives are not yet wired into autonomous execution in ConstructionSimulation.Step().** The five-cell lookahead is currently an admission-time check, not a persistent reservation of every lookahead cell.

## Truthful milestones

- **M0 — authored contract:** vocabulary, bundle boundaries, architecture diagrams.
- **M1 — deterministic discrete model:** height mapping, target cells, stable agent IDs, finite emitter inventory, exclusive occupancy, one-cell movement, placement, and immutable snapshots.
- **M2 — lane transition proof:** admission and conflict invariants tested as separate primitives; full autonomous integration remains open.
- **M3 — live rendered vertical slice:** actual snapshots consumed by Renderer/GUI.SVG. Not complete.
- **M4 — scale evidence:** population sweeps, correctness, throughput, memory, allocations, and tick-time reports. Not complete.
- **M5 — optional physical model:** add a tested physics provider only when API, units, stepping, and collision evidence are explicit.

**North star:** show that many simple agents can build a shared target through explicit, testable rules—and make the actual rules visible.
