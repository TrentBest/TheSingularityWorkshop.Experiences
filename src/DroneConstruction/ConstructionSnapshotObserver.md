# ConstructionSnapshotObserver

![The Singularity Workshop moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

**Purpose:** maintain the latest accepted committed ConstructionSimulationSnapshot for one caller-identified simulation run, without owning or advancing the simulation.

## Class card

| Member | Contract |
|---|---|
| RunId | Explicit non-empty identity supplied by the run owner |
| LatestSnapshot | Latest accepted immutable snapshot, or null before first publication |
| Sequence | Count of accepted snapshots for this observer |
| TryPublish(snapshot) | Accepts only a tick strictly newer than the current snapshot |

## Invariants

- A new observer is created for each simulation run; run identity is never inferred from tick numbers.
- Snapshot publication is synchronized.
- Duplicate and older ticks are rejected and do not change the sequence or current observation.
- The observer is passive: it does not call Step(), mutate the simulation, interpolate motion, or depend on GUI.SVG.
- This is a latest-value observer, not an event log or a production telemetry stream.

## Example

~~~csharp
var observer = new ConstructionSnapshotObserver(runId: "showcase-run-001");
observer.TryPublish(simulation.Snapshot());

var committed = simulation.Step();
if (observer.TryPublish(committed))
{
    // A downstream view may render observer.LatestSnapshot.
}
~~~

See [class diagram and use cases](ConstructionSnapshotObserverUML.md) and [simulation observability contract](../../experiences/DroneConstruction/design/SIMULATION_OBSERVABILITY.md).
