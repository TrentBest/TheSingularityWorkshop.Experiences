# ConstructionSnapshotSvgView — UML and Use Cases

![The Singularity Workshop moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

## Class diagram

~~~mermaid
classDiagram
    class ConstructionSnapshotObserver {
        +RunId string
        +TryPublish(snapshot) bool
        +TryGetLatest(out snapshot, out sequence) bool
    }
    class ConstructionSnapshotSvgView {
        +RunId string
        +RenderedSequence long
        +RenderedTick long?
        +TryRefresh(out svg) bool
    }
    class ConstructionSnapshotSvgRenderer {
        <<static>>
        +Render(snapshot, width, height, spaceId, cellSize) string
    }
    class ConstructionSimulationSnapshot {
        +Tick long
        +PlacedBlocks int
        +RequiredBlocks int
        +Agents IReadOnlyList
        +Emitters IReadOnlyList
    }
    ConstructionSnapshotObserver --> ConstructionSimulationSnapshot : latest committed state
    ConstructionSnapshotSvgView --> ConstructionSnapshotObserver : polls atomically
    ConstructionSnapshotSvgView ..> ConstructionSnapshotSvgRenderer : renders new sequence
    ConstructionSnapshotSvgRenderer ..> ConstructionSimulationSnapshot : reads only
~~~

## Use cases

1. **First frame:** publish the initial committed snapshot and refresh to produce tick zero.
2. **Skip duplicate work:** repeated refresh calls return false until the observer accepts a newer snapshot.
3. **Display current state:** replace the host's displayed SVG only when TryRefresh returns true.
4. **Recover from render failure:** the cursor remains at the last successful sequence, so the same snapshot can be retried.
5. **Control backpressure:** host-selected refresh cadence plus latest-value observation coalesces intermediate snapshots rather than building an unbounded frame queue.

## Boundary

The view does not own a timer, GUI.SVG host, telemetry sink, simulation tick, or event queue. One host thread should own each mutable view instance. It does not change authoritative state.
