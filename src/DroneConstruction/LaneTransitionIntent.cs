namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>
/// An explicit request to move an agent from a qualified source, through one named slipstream entry,
/// toward one qualified destination. An optional transition path opts into autonomous tick-driven
/// traversal; without a path, the caller retains manual control after admission.
/// </summary>
public sealed record LaneTransitionIntent
{
    private readonly GridCell[] _lookahead;
    private readonly GridCell[]? _transitionPath;

    /// <summary>Creates a validated, immutable slipstream transition request.</summary>
    public LaneTransitionIntent(
        QualifiedGridLocation source,
        QualifiedGridLocation transitionEntry,
        QualifiedGridLocation destination,
        IEnumerable<GridCell> lookahead,
        IEnumerable<GridCell>? transitionPath = null)
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

        if (transitionPath is not null)
        {
            _transitionPath = transitionPath.ToArray();
            if (_transitionPath.Length == 0)
                throw new ArgumentException("An explicit transition path cannot be empty.", nameof(transitionPath));
            if (_transitionPath[0] != transitionEntry.Cell)
                throw new ArgumentException("The transition path must begin at the qualified entry cell.", nameof(transitionPath));
            if (_transitionPath.Distinct().Count() != _transitionPath.Length)
                throw new ArgumentException("Transition path cells must be unique.", nameof(transitionPath));
            for (var i = 1; i < _transitionPath.Length; i++)
            {
                var distance = Math.Abs(_transitionPath[i].X - _transitionPath[i - 1].X)
                    + Math.Abs(_transitionPath[i].Y - _transitionPath[i - 1].Y);
                if (distance != 1)
                    throw new ArgumentException("Each transition path step must be orthogonally adjacent.", nameof(transitionPath));
            }
        }

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

    /// <summary>
    /// Gets the optional path through the transition grid, including entry and exit cells.
    /// When absent, traversal remains under explicit caller control.
    /// </summary>
    public IReadOnlyList<GridCell>? TransitionPath =>
        _transitionPath is null ? null : Array.AsReadOnly(_transitionPath);
}
