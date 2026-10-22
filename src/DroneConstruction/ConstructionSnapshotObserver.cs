namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>
/// Maintains the latest committed snapshot for one explicitly identified simulation run.
/// The observer never advances or mutates the simulation; callers publish snapshots after commits.
/// </summary>
public sealed class ConstructionSnapshotObserver
{
    private readonly object _gate = new();
    private ConstructionSimulationSnapshot? _latestSnapshot;
    private long _sequence;

    /// <summary>Creates an observer bound to one caller-defined simulation-run identity.</summary>
    public ConstructionSnapshotObserver(string runId)
    {
        if (string.IsNullOrWhiteSpace(runId))
            throw new ArgumentException("A non-empty run identity is required.", nameof(runId));

        RunId = runId;
    }

    /// <summary>Stable identity supplied by the owner of this simulation run.</summary>
    public string RunId { get; }

    /// <summary>Number of snapshots accepted by this observer.</summary>
    public long Sequence
    {
        get { lock (_gate) return _sequence; }
    }

    /// <summary>The latest accepted committed snapshot, or null before the first publication.</summary>
    public ConstructionSimulationSnapshot? LatestSnapshot
    {
        get { lock (_gate) return _latestSnapshot; }
    }

    /// <summary>
    /// Publishes a committed snapshot if its tick is strictly newer than the currently observed tick.
    /// Duplicate and out-of-order ticks are ignored, so delayed producers cannot roll the view backward.
    /// The run identity is fixed at construction; create a new observer for a new simulation run.
    /// </summary>
    /// <returns>True when the snapshot becomes the latest observation; otherwise false.</returns>
    public bool TryPublish(ConstructionSimulationSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        lock (_gate)
        {
            if (_latestSnapshot is not null && snapshot.Tick <= _latestSnapshot.Tick)
                return false;

            _latestSnapshot = snapshot;
            _sequence = checked(_sequence + 1);
            return true;
        }
    }
}
