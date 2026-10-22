namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>
/// Resolves committed agent locations across explicitly registered traffic spaces.
/// The grids remain the occupancy authority; this catalog does not duplicate occupancy state.
/// </summary>
public sealed class ConstructionSpaceCatalog
{
    private readonly Dictionary<(ConstructionSpaceKind Kind, string Id), TrafficCellGrid> _spaces = [];
    private readonly HashSet<TrafficCellGrid> _registeredGrids = [];

    /// <summary>Gets the number of registered spaces.</summary>
    public int Count => _spaces.Count;

    /// <summary>
    /// Registers a named grid once. A grid instance may have only one qualified identity in this catalog.
    /// </summary>
    public bool TryRegister(ConstructionSpaceKind kind, string spaceId, TrafficCellGrid grid)
    {
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        if (string.IsNullOrWhiteSpace(spaceId)) throw new ArgumentException("A stable space identity is required.", nameof(spaceId));
        ArgumentNullException.ThrowIfNull(grid);

        var key = (kind, spaceId);
        if (_spaces.ContainsKey(key) || _registeredGrids.Contains(grid)) return false;
        _spaces.Add(key, grid);
        _registeredGrids.Add(grid);
        return true;
    }

    /// <summary>Resolves a qualified location to its registered grid and verifies the local cell is in bounds.</summary>
    public bool TryResolve(QualifiedGridLocation location, out TrafficCellGrid? grid)
    {
        ArgumentNullException.ThrowIfNull(location);
        if (_spaces.TryGetValue((location.SpaceKind, location.SpaceId), out var found) && found.Contains(location.Cell))
        {
            grid = found;
            return true;
        }

        grid = null;
        return false;
    }

    /// <summary>
    /// Finds the one registered space whose occupancy index currently contains an agent.
    /// Returns false if the agent is absent or appears in more than one registered space.
    /// </summary>
    public bool TryGetAgentLocation(ulong agentId, out QualifiedGridLocation? location)
    {
        if (agentId == 0) throw new ArgumentOutOfRangeException(nameof(agentId), "Agent ID zero is reserved.");

        location = null;
        foreach (var entry in _spaces)
        {
            if (!entry.Value.TryGetAgentCell(agentId, out var cell)) continue;
            if (location is not null)
            {
                // Ambiguous occupancy is a broken cross-space invariant; never guess which location is true.
                location = null;
                return false;
            }

            location = new QualifiedGridLocation(entry.Key.Kind, entry.Key.Id, cell);
        }

        return location is not null;
    }

    /// <summary>Gets an agent's unique registered location or throws when it is absent or ambiguous.</summary>
    public QualifiedGridLocation GetAgentLocation(ulong agentId)
    {
        if (TryGetAgentLocation(agentId, out var location)) return location!;
        throw new KeyNotFoundException($"Agent {agentId} is absent from registered spaces or has ambiguous occupancy.");
    }
}
