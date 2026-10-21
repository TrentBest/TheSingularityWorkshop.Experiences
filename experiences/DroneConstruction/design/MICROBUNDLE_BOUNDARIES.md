# Drone Construction — MicroBundle Boundaries

**Status:** first provider slice implemented for IDs 3410–3416; remaining entries are planned contracts only. These IDs are local to this Experience proposal and must be checked against the repository-wide registry before executable packages are published. Provider implementation is tracked below.

## Ownership rule

The Experience defines the construction problem and its desired outcome. Each MicroBundle owns a narrow, reusable capability contract. FSM_COS composes compatible bundles; MicroBundleRepository stores and retrieves immutable artifacts; the simulation runtime owns authoritative tick execution; Renderer and GUI.SVG observe the resulting state.

Do not create one monolithic “DroneConstruction” bundle that hides image interpretation, resources, traffic, scheduling, and visualization behind a single opaque capability.

## Proposed catalog

| Bundle ID | Provider ID | Responsibility | Depends on | Status |
|---:|---|---|---|
| 3410 | `workshop-drone-construction.height-field` | Versioned image/channel-to-height mapping and validation | none | Implemented
| 3411 | `workshop-drone-construction.build-grid` | Target cells, required/placed block counts, placement tasks | height-field contract | Implemented
| 3412 | `workshop-drone-construction.resource-emitter` | Ten-emitter scene, inventory, pickup capacity, queue state | none | Implemented
| 3413 | `workshop-drone-construction.skylane` | 2D one-way cellular lane, direction/speed/elevation policy, occupancy | traffic-cell contract | Implemented
| 3414 | `workshop-drone-construction.hover-lane` | 2D cellular holding grid and admission queue | traffic-cell contract | Implemented
| 3415 | `workshop-drone-construction.slipstream` | Bounded transition volume, reservations, conflict checks | traffic-cell contract | Implemented
| 3416 | `workshop-drone-construction.traffic-cell` | Shared cell identity, occupancy, reservation and tick semantics | none | Implemented
| 3417 | `workshop-drone-construction.agent` | Stable agent identity and explicit lifecycle/movement/task state | none | Planned |
| 3418 | `workshop-drone-construction.task-scheduler` | Assigns pickup/placement work and commits idempotent task results | build-grid, resource-emitter, agent | Planned |
| 3419 | `workshop-drone-construction.simulation` | Deterministic tick transaction, invariant validation, snapshot/event publication | all required domain contracts | Planned |
| 3420 | `workshop-drone-construction.telemetry` | Run metadata, state counts, invariant failures, performance counters, replay digest | simulation event contract | Planned |
| 3421 | `workshop-drone-construction.renderer-adapter` | Maps snapshots to renderer-facing semantic scene; no authority over simulation | simulation snapshot contract | Planned |
| 3422 | `workshop-drone-construction.svg-observer` | Living diagrams of lanes, transitions, task flow, and actual telemetry | simulation snapshot/event contract | Planned |

## Shared contracts before providers

A provider must publish:
- stable bundle ID, semantic version, provider ID, and immutable artifact identity;
- dependency IDs and compatible contract versions;
- units, coordinate frame, timebase, and capacity semantics where relevant;
- deterministic behavior and failure outcomes;
- tests that establish advertised invariants;
- an explicit distinction between implemented, experimental, and planned behavior.

A provider ID is metadata, not a service locator. Do not claim that composition has resolved a typed runtime service until the host's typed resolution contract exists and is tested.

## Why skylane, hover lane, and slipstream are separate

They have different state and admission rules. Skylane cells encode ordinary directed movement; hover-lane cells encode holding and readiness; slipstream cells encode a scarce transition resource that must be reserved and conflict-checked. Keeping them separate makes the transition boundary small and observable without making the entire airspace a global lock.

The five-cell lookahead belongs in versioned simulation configuration, not as a magic constant embedded in a provider. The scheduler can evaluate a configured lookahead and transition occupancy, while tests establish the assumptions under which admission is valid. No fixed cell count is a universal physical guarantee.

## Dependency direction

```text
HeightField ──> BuildGrid ───────────┐
ResourceEmitter ─────────────────────┤
TrafficCell ──> Skylane ─────────────┤
            ├─> HoverLane ──────────┤──> TaskScheduler ──> Simulation
            └─> Slipstream ─────────┤                       ├─> Telemetry
Agent ───────────────────────────────┘                       ├─> RendererAdapter
                                                            └─> SVGObserver
```

This is a capability graph, not a claim that every dependency must be loaded into every manifestation. For example, a static design preview can use the authored schema and SVG without loading the simulation provider.

## Implementation sequence

1. Implement pure contracts and deterministic invariants in a dependency-light project.
2. Test the traffic-cell, skylane, hover-lane, and slipstream rules independently.
3. Implement image-to-height mapping and block/inventory conservation.
4. Implement the end-to-end agent loop and deterministic replay.
5. Add renderer and SVG observers after snapshots are stable.
6. Only then split independently publishable providers where artifact boundaries and host resolution are real.
