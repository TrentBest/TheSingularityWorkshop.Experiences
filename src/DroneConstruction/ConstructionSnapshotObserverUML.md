# ConstructionSnapshotObserver — UML and Use Cases

![The Singularity Workshop moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

## Class diagram

~~~mermaid
classDiagram
    class ConstructionSimulation {
        +Snapshot() ConstructionSimulationSnapshot
        +Step() ConstructionSimulationSnapshot
    }
    class ConstructionSnapshotObserver {
        +RunId string
        +Sequence long
        +LatestSnapshot ConstructionSimulationSnapshot
        +TryPublish(snapshot) bool
        +TryGetLatest(out snapshot, out sequence) bool
    }
    class ConstructionSimulationSnapshot {
        +Tick long
        +PlacedBlocks int
        +IsComplete bool
        +Agents IReadOnlyList
        +Emitters IReadOnlyList
        +CellPlacedBlocks IReadOnlyList
    }
    class ConstructionSnapshotSvgView {
        +RunId string
        +RenderedSequence long
        +RenderedTick long?
        +TryRefresh(out svg) bool
    }
    ConstructionSimulation ..> ConstructionSimulationSnapshot : produces committed state
    ConstructionSnapshotObserver o--> ConstructionSimulationSnapshot : latest accepted
    ConstructionSnapshotObserver ..> ConstructionSnapshotSvgView : supplies latest accepted snapshot
~~~

## Use cases

1. **Maintain a view input:** create an observer with a stable run ID and publish each committed snapshot after a tick.
2. **Ignore delayed work:** if a producer delivers an older tick or repeats a tick, reject it without rolling the view backward.
3. **Separate runs:** instantiate a separate observer for each run so that two runs both at tick zero remain distinct.
4. **Keep the simulation authoritative:** consumers read snapshots; the observer does not advance simulation time or modify domain state.
5. **Feed a maintained view:** `ConstructionSnapshotSvgView` polls the observer on demand, emits SVG only for a new accepted sequence, and exposes the rendered tick and sequence. The host owns refresh cadence and presentation; no timer, event queue, GUI.SVG host, or telemetry service is implied.

## Boundary

The observer is a synchronized latest-value holder, not a queue. Its acceptance sequence counts accepted publications; it is not the simulation tick and should not be treated as a globally unique event ID.
