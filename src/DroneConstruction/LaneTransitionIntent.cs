namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>
/// An explicit request to move an agent from a qualified source, through one named slipstream entry,
/// toward one qualified destination. This is intent data only; it does not reserve cells or execute movement.
/// </summary>
public sealed record LaneTransitionIntent
{
    private readonly GridCell[] _lookahead;

    /// <summary>Creates a validated, immutable slipstream transition request.</summary>
    public LaneTransitionIntent(
        QualifiedGridLocation source,
        QualifiedGridLocation transitionEntry,
        QualifiedGridLocation destination,
        IEnumerable<GridCell> lookahead)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(transitionEntry);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(lookahead);

        if (source.SpaceKind == ConstructionSpaceKind.Slipstream)
            throw new ArgumentException("The source must be outside the transition space.", nameof(source));
        if (transitionEntry.SpaceKind != ConstructionSpaceKind.Slipstream)
            throw new ArgumentException("The transition entry must identify a slipstream space.", nameof(transitionEntry));
        if (destination.SpaceKind == ConstructionSpaceKind.Slipstream)
            throw new ArgumentException("The destination must be outside the transition space.", nameof(destination));

        _lookahead = lookahead.ToArray();
        if (_lookahead.Length == 0)
            throw new ArgumentException("At least one lookahead cell is required.", nameof(lookahead));
        if (_lookahead.Distinct().Count() != _lookahead.Length)
            throw new ArgumentException("Lookahead cells must be unique.", nameof(lookahead));

        Source = source;
        TransitionEntry = transitionEntry;
        Destination = destination;
    }

    /// <summary>Gets the exact qualified source location.</summary>
    public QualifiedGridLocation Source { get; }

    /// <summary>Gets the exact qualified entry location in the named slipstream.</summary>
    public QualifiedGridLocation TransitionEntry { get; }

    /// <summary>Gets the exact qualified destination location.</summary>
    public QualifiedGridLocation Destination { get; }

    /// <summary>Gets a read-only view of the requested lookahead cells.</summary>
    public IReadOnlyList<GridCell> Lookahead => Array.AsReadOnly(_lookahead);
}
