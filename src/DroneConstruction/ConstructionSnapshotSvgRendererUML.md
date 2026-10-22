# ConstructionSnapshotSvgRenderer — UML and Use Cases

![The Singularity Workshop moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

## UML class card

```mermaid
classDiagram
    direction TB
    class ConstructionSimulation {
        +Snapshot() ConstructionSimulationSnapshot
        +Step() ConstructionSimulationSnapshot
    }
    class ConstructionSimulationSnapshot {
        +Tick : long
        +RequiredBlocks : int
        +PlacedBlocks : int
        +IsComplete : bool
        +Agents : IReadOnlyList
        +Emitters : IReadOnlyList
        +CellPlacedBlocks : IReadOnlyList
    }
    class ConstructionSnapshotSvgRenderer {
        <<static>>
        +Render(snapshot, width, height, cellSize) string
    }
    class SvgConsumer {
        <<external>>
        +SaveOrDisplay(svg)
    }
    ConstructionSimulation ..> ConstructionSimulationSnapshot : captures
    ConstructionSimulationSnapshot --> ConstructionSnapshotSvgRenderer : input
    ConstructionSnapshotSvgRenderer ..> SvgConsumer : standalone SVG
```

## Use cases

1. **Review a committed tick:** capture `simulation.Snapshot()`, render it, and retain the tick with the artifact.
2. **Inspect progress:** compare emitter inventory, agent location/carrying state, and placed-block counts in one view.
3. **Validate dimensions:** reject an input snapshot when its cell-count does not match the requested grid dimensions.
4. **Integrate later:** adapt the same snapshot into GUI.SVG or Renderer without making the simulation depend on a presentation package.

## Invariants

- Rendering is deterministic for the same snapshot and dimensions.
- Rendering is read-only; it never calls `Step()`.
- SVG labels identify the tick and placement progress.
- This renderer is a snapshot-to-SVG proof, not an autonomous lane-routing or 100k scale claim.
