# Drone Construction Gallery

**Purpose:** a concise review index for the 100,000-agent construction showcase. This gallery distinguishes authored design material from live implementation and measured evidence.

## Vision and architecture

![Drone Construction vision storyboard](assets/vision-board.svg)

*Concept storyboard: visual intent only; not a live simulation capture.*

![Drone Construction architecture](assets/drone-construction-architecture.svg)

*Architecture sketch: the simulation remains authoritative; visual adapters observe committed state.*

## Lane execution concept

![Autonomous lane execution concept](assets/autonomous-lane-execution-concept.svg)

*Concept illustration of boundary validation, admitted coordinate-based travel, target arrival, and blocked-move retry. This is authored documentation—not a live simulation capture or evidence that autonomous lane execution is implemented.*

## Snapshot-to-SVG proof

The source contains `ConstructionSnapshotSvgRenderer.Render(snapshot, width, height, spaceId)`, which renders a committed snapshot as standalone SVG. Its labels and cell/agent/emitter marks come from snapshot data; agents from other qualified spaces are not projected onto the selected grid.

*Implementation proof: [renderer source](../../src/DroneConstruction/ConstructionSnapshotSvgRenderer.cs) · [tests](../../tests/DroneConstruction.Tests/ConstructionSnapshotSvgRendererTests.cs) · [contract](../../src/DroneConstruction/ConstructionSnapshotSvgRenderer.md).*

CI run [39851568716](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/actions/runs/39851568716) passed and produced the downloadable `drone-construction-snapshot-svg` artifact (817 bytes; SHA-256 `6a394ea68cc75a83ff77bfee3002624fbe6e0fd2bbb5f7077904198d3d8c34a8`). The absolute workspace path fix is verified. The existing SVGs above remain concept/architecture assets; the artifact is output from the standalone renderer, not a GUI.SVG live view.

## Snapshot-to-GUI.SVG integration test

The optional repository-source test graph now feeds actual initial and completed `ConstructionSimulationSnapshot` values into `GuiBuilder` and `SvgGuiRenderer`. It checks the rendered tick and placement progress, agent identity, placed-block identity, and that the completed view does not retain the initial tick. The simulation itself remains independent of GUI packages.

*Implementation proof: [GUI.SVG integration test](../../tests/DroneConstruction.Tests/ConstructionSnapshotGuiSvgIntegrationTests.cs) · [verified CI run 39853079623](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/actions/runs/39853079623) · [commit db795fec](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/commit/db795fecc043e40496665c898a3a2909064569f5).*

All 25 applicable jobs passed; one was intentionally skipped. This is a source-graph integration test, not yet a maintained continuously updating GUI.SVG view or telemetry feed.

## What is real today

- Deterministic discrete-grid construction loop: task assignment, finite-inventory pickup, one-cell movement, placement, and immutable snapshots.
- Qualified locations and occupancy catalog: an agent's cell is interpreted within a stable space identity.
- Tested skylane direction/speed policy and separately tested hover/slipstream admission/lifecycle contracts.
- Standalone snapshot-to-SVG rendering with a downloadable CI artifact.
- Optional source-graph integration test that renders committed snapshots through GUI.SVG.
- CI builds and tests the Drone Construction project on development.

## What is not yet a live proof

- End-to-end autonomous routing through hover lane → slipstream → skylane inside ConstructionSimulation.Step().
- A maintained, continuously updated GUI.SVG manifestation with run/tick telemetry; the passing source-graph test is a first integration proof, not that finished view.
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
