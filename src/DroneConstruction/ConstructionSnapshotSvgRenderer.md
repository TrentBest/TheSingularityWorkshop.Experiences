# ConstructionSnapshotSvgRenderer

![The Singularity Workshop moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

**Purpose:** turn one immutable `ConstructionSimulationSnapshot` into a standalone, deterministic SVG portrayal. This is the first real snapshot-to-visual bridge in Drone Construction, not yet a GUI.SVG integration.

## Contract

- Input is a committed snapshot plus the source grid dimensions.
- Output includes the snapshot tick, placed/required progress, emitters and inventory, and agent positions.
- Agent styling reflects carrying status from the snapshot.
- Rendering is read-only and does not advance, mutate, or interpolate the simulation.
- Mismatched grid dimensions and invalid canvas settings are rejected.
- The renderer has no WPF, browser, GUI.SVG, or Unity dependency.

## Usage

```csharp
ConstructionSimulationSnapshot snapshot = simulation.Snapshot();
string svg = ConstructionSnapshotSvgRenderer.Render(snapshot, width: 20, height: 12);
File.WriteAllText("construction-snapshot.svg", svg);
```

Use a snapshot captured after a committed tick. For a review artifact, record the tick and the simulation inputs alongside the SVG.

## Scope boundary

This implementation proves that committed snapshot values can be portrayed as SVG. It does **not** prove autonomous skylane routing, a live GUI.SVG manifest, 100,000-agent rendering, or performance at scale. See [snapshot observability](../../../experiences/DroneConstruction/design/SIMULATION_OBSERVABILITY.md) and [the UML/use cases](ConstructionSnapshotSvgRendererUML.md).
