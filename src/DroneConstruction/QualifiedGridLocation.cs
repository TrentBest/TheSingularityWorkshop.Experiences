namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>Identifies the kind of coordinate space that owns a construction agent's position.</summary>
public enum ConstructionSpaceKind
{
    /// <summary>The ordinary construction simulation traffic grid.</summary>
    SimulationGrid,
    /// <summary>A hover lane used for staging and holding.</summary>
    HoverLane,
    /// <summary>A bounded transition resource between traffic spaces.</summary>
    Slipstream,
    /// <summary>A directed skylane.</summary>
    Skylane
}

/// <summary>
/// A grid coordinate qualified by the identity and kind of the space that owns it.
/// Coordinates from different spaces are never interchangeable, even when their X/Y values match.
/// </summary>
public sealed record QualifiedGridLocation
{
    /// <summary>Creates a qualified location in a named traffic space.</summary>
    public QualifiedGridLocation(ConstructionSpaceKind spaceKind, string spaceId, GridCell cell)
    {
        if (!Enum.IsDefined(spaceKind))
            throw new ArgumentOutOfRangeException(nameof(spaceKind));
        if (string.IsNullOrWhiteSpace(spaceId))
            throw new ArgumentException("A stable space identity is required.", nameof(spaceId));

        SpaceKind = spaceKind;
        SpaceId = spaceId;
        Cell = cell;
    }

    /// <summary>Gets the kind of space that owns this coordinate.</summary>
    public ConstructionSpaceKind SpaceKind { get; }

    /// <summary>Gets the stable identity of the particular grid or lane.</summary>
    public string SpaceId { get; }

    /// <summary>Gets the coordinate local to the named space.</summary>
    public GridCell Cell { get; }
}
