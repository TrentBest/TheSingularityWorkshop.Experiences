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
    }
    class ConstructionSimulationSnapshot {
        +Tick long
        +PlacedBlocks int
        +IsComplete bool
        +Agents IReadOnlyList
        +Emitters IReadOnlyList
        +CellPlacedBlocks IReadOnlyList
    }
    class SnapshotDrivenView {
        <<consumer>>
        +Refresh(snapshot)
    }
    ConstructionSimulation ..> ConstructionSimulationSnapshot : produces committed state
    ConstructionSnapshotObserver o--> ConstructionSimulationSnapshot : latest accepted
    ConstructionSnapshotObserver ..> SnapshotDrivenView : exposes latest snapshot to
~~~

## Use cases

1. **Maintain a view input:** create an observer with a stable run ID and publish each committed snapshot after a tick.
2. **Ignore delayed work:** if a producer delivers an older tick or repeats a tick, reject it without rolling the view backward.
3. **Separate runs:** instantiate a separate observer for each run so that two runs both at tick zero remain distinct.
4. **Keep the simulation authoritative:** consumers read snapshots; the observer does not advance simulation time or modify domain state.
5. **Feed renderers:** a downstream renderer may refresh from the latest snapshot, but this class alone does not provide scheduling, event delivery, GUI.SVG integration, or telemetry.

## Boundary

The observer is a synchronized latest-value holder, not a queue. Its acceptance sequence counts accepted publications; it is not the simulation tick and should not be treated as a globally unique event ID.
