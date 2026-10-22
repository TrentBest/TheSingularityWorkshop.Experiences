namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>
/// Maintains a pull-based SVG view over the latest committed snapshot for one simulation run.
/// The host decides when to call Refresh; intermediate snapshots may be coalesced by the observer.
/// </summary>
public sealed class ConstructionSnapshotSvgView
{
    private readonly ConstructionSnapshotObserver _observer;
    private readonly int _width;
    private readonly int _height;
    private readonly string _spaceId;
    private readonly int _cellSize;
    private long _renderedSequence;

    /// <summary>Creates a view over an observer and the grid dimensions of its simulation run.</summary>
    public ConstructionSnapshotSvgView(
        ConstructionSnapshotObserver observer,
        int width,
        int height,
        string spaceId = "construction-main",
        int cellSize = 48)
    {
        ArgumentNullException.ThrowIfNull(observer);
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (string.IsNullOrWhiteSpace(spaceId)) throw new ArgumentException("A stable simulation-space identity is required.", nameof(spaceId));
        if (cellSize < 16) throw new ArgumentOutOfRangeException(nameof(cellSize));

        _observer = observer;
        _width = width;
        _height = height;
        _spaceId = spaceId;
        _cellSize = cellSize;
    }

    /// <summary>The run identity supplied to the observer.</summary>
    public string RunId => _observer.RunId;

    /// <summary>The observer acceptance sequence used for the last successful refresh.</summary>
    public long RenderedSequence => _renderedSequence;

    /// <summary>The committed simulation tick represented by the last successful refresh, if any.</summary>
    public long? RenderedTick { get; private set; }

    /// <summary>
    /// Renders the newest accepted snapshot only when it has not already been rendered.
    /// The caller owns the refresh cadence and may replace its displayed SVG with the returned output.
    /// </summary>
    /// <param name="svg">New standalone SVG when true is returned; otherwise null.</param>
    /// <returns>True if a newer snapshot was rendered; false if there is no new snapshot.</returns>
    public bool TryRefresh(out string? svg)
    {
        svg = null;
        if (!_observer.TryGetLatest(out var snapshot, out var sequence)
            || snapshot is null
            || sequence <= _renderedSequence)
            return false;

        // Render before advancing the cursor so invalid input does not silently consume an update.
        var rendered = ConstructionSnapshotSvgRenderer.Render(snapshot, _width, _height, _spaceId, _cellSize);
        svg = rendered;
        _renderedSequence = sequence;
        RenderedTick = snapshot.Tick;
        return true;
    }
}
