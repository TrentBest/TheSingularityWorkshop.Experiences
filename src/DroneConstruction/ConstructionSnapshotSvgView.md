# ConstructionSnapshotSvgView

![The Singularity Workshop moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

**Purpose:** maintain a pull-based standalone SVG portrayal of the newest accepted committed construction snapshot without coupling the simulation core to GUI.SVG or advancing simulation state.

## Contract

- Uses a ConstructionSnapshotObserver with one explicit run identity.
- TryRefresh returns SVG only when the observer acceptance sequence is newer than the last successfully rendered sequence.
- The SVG is produced by ConstructionSnapshotSvgRenderer and shows tick, placement progress, emitters, and agents on the configured qualified simulation grid.
- RenderedTick and RenderedSequence identify the last successful output.
- The host chooses refresh cadence and where to display or save the SVG. The observer retains only the latest snapshot, so intermediate snapshots may be coalesced.
- Rendering happens before the cursor advances. A render exception leaves the snapshot available for retry.
- The view is a pull component, not a timer, event queue, telemetry recorder, or GUI.SVG-hosted application.

## Usage

~~~csharp
var observer = new ConstructionSnapshotObserver("run-001");
var view = new ConstructionSnapshotSvgView(observer, width: 20, height: 12);

// After each committed simulation tick:
observer.TryPublish(simulation.Snapshot());

// On the host's chosen refresh cadence:
if (view.TryRefresh(out var svg))
    File.WriteAllText("construction-live.svg", svg!);
~~~

## Use cases

1. Poll from a UI or recording loop without allocating an unbounded queue of frames.
2. Replace the displayed SVG only when a new accepted snapshot is available.
3. Associate a saved image with the run ID, tick, and observer sequence.
4. Retry rendering after a transient failure without silently advancing the view cursor.

## Boundary

The component is not thread-safe as a mutable view instance; call TryRefresh from one owning UI/consumer thread. ConstructionSnapshotObserver is synchronized and may accept publications from another thread. The host must decide its scheduling, error presentation, and retention policy. The component does not claim autonomous lane execution, GUI.SVG hosting, telemetry, or population-scale performance.
