# Drone Construction Gallery

**Purpose:** a concise review index for the 100,000-agent construction showcase. This gallery distinguishes authored design material from live implementation and measured evidence.

## Vision and architecture

![Drone Construction vision storyboard](assets/vision-board.svg)

*Concept storyboard: visual intent only; not a live simulation capture.*

![Drone Construction architecture](assets/drone-construction-architecture.svg)

*Architecture sketch: the simulation remains authoritative; visual adapters observe committed state.*

## What is real today

- Deterministic discrete-grid construction loop: task assignment, finite-inventory pickup, one-cell movement, placement, and immutable snapshots.
- Qualified locations and occupancy catalog: an agent's cell is interpreted within a stable space identity.
- Tested skylane direction/speed policy and separately tested hover/slipstream admission/lifecycle contracts.
- CI builds and tests the Drone Construction project on development.

## What is not yet a live proof

- End-to-end autonomous routing through hover lane → slipstream → skylane inside ConstructionSimulation.Step().
- GUI.SVG manifestation driven from actual snapshots.
- A measured 100,000-agent run with repeatable throughput, memory, and correctness results.

## Reviewer entry points

- [Experience brief and vocabulary](README.md)
- [MicroBundle boundaries](design/MICROBUNDLE_BOUNDARIES.md)
- [Lane transition lifecycle contract](design/LANE_TRANSITION_LIFECYCLE.md)
- [Snapshot and living-view contract](design/SIMULATION_OBSERVABILITY.md)
- [100K scale validation protocol](design/100K_SCALE_VALIDATION.md)
- [Readiness ledger](design/READINESS.md)

## Definition of “living”

A gallery item earns the label *live* only when its visual output is derived from a real committed snapshot or telemetry artifact and identifies the corresponding tick/run. Static SVGs remain useful, but are labeled as concept or architecture images. Performance claims must link to reproducible run metadata and raw results.
