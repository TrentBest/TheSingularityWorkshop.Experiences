namespace TheSingularityWorkshop.Experiences.Warlords;

/// <summary>Playable kingdoms in the four-kingdom starter scenario.</summary>
public enum WarlordsKingdom { Azure, Crimson, Verdant, Amber }

/// <summary>Immutable authored definition of one territory.</summary>
/// <summary>Stable, immutable territory identity and authored map adjacency.</summary>
public sealed record WarlordsTerritoryDefinition(
    string Id,
    string Name,
    IReadOnlyList<string> Neighbors,
    WarlordsKingdom? StartingOwner = null,
    bool IsStronghold = false,
    int StartingGarrison = 1);

/// <summary>Validated, renderer-independent territory graph for a Warlords scenario.</summary>
public sealed class WarlordsMap
{
    private readonly IReadOnlyDictionary<string, WarlordsTerritoryDefinition> _territories;

    /// <summary>Creates and validates an immutable connected territory graph.</summary>
    /// <param name="key">Stable scenario key.</param>
    /// <param name="version">Positive scenario version.</param>
    /// <param name="territories">Territory definitions with symmetric adjacency.</param>
    public WarlordsMap(string key, int version, IEnumerable<WarlordsTerritoryDefinition> territories)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (version <= 0) throw new ArgumentOutOfRangeException(nameof(version));
        ArgumentNullException.ThrowIfNull(territories);

        var supplied = territories.ToArray();
        if (supplied.Any(t => t is null || t.Neighbors is null))
            throw new ArgumentException("Territory definitions and neighbor lists are required.", nameof(territories));
        var entries = supplied.Select(t => t with { Neighbors = Array.AsReadOnly(t.Neighbors.ToArray()) }).ToArray();
        if (entries.Length < 5) throw new ArgumentException("A map needs at least five territories.", nameof(territories));
        if (entries.Any(t => string.IsNullOrWhiteSpace(t.Id) || string.IsNullOrWhiteSpace(t.Name)))
            throw new ArgumentException("Territory IDs and names are required.", nameof(territories));
        if (entries.Select(t => t.Id).Distinct(StringComparer.Ordinal).Count() != entries.Length)
            throw new ArgumentException("Territory IDs must be unique.", nameof(territories));
        if (entries.Any(t => t.StartingGarrison < 0))
            throw new ArgumentException("Starting garrisons cannot be negative.", nameof(territories));

        var byId = entries.ToDictionary(t => t.Id, StringComparer.Ordinal);
        foreach (var territory in entries)
        {
            if (territory.Neighbors.Count == 0)
                throw new ArgumentException($"Territory '{territory.Id}' has no neighbors.", nameof(territories));
            if (territory.Neighbors.Contains(territory.Id, StringComparer.Ordinal))
                throw new ArgumentException($"Territory '{territory.Id}' cannot neighbor itself.", nameof(territories));
            if (territory.Neighbors.Distinct(StringComparer.Ordinal).Count() != territory.Neighbors.Count)
                throw new ArgumentException($"Territory '{territory.Id}' contains duplicate neighbors.", nameof(territories));
            foreach (var neighborId in territory.Neighbors)
            {
                if (!byId.TryGetValue(neighborId, out var neighbor))
                    throw new ArgumentException($"Territory '{territory.Id}' references missing neighbor '{neighborId}'.", nameof(territories));
                if (!neighbor.Neighbors.Contains(territory.Id, StringComparer.Ordinal))
                    throw new ArgumentException($"Map adjacency must be symmetric: '{territory.Id}' and '{neighborId}'.", nameof(territories));
            }
            if (territory.IsStronghold && territory.StartingOwner is null)
                throw new ArgumentException($"Stronghold '{territory.Id}' needs a starting kingdom.", nameof(territories));
        }

        var strongholdOwners = entries.Where(t => t.IsStronghold).Select(t => t.StartingOwner).ToHashSet();
        if (Enum.GetValues<WarlordsKingdom>().Any(kingdom => !strongholdOwners.Contains(kingdom)))
            throw new ArgumentException("Every playable kingdom needs a starting stronghold.", nameof(territories));

        var visited = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>();
        pending.Push(entries[0].Id);
        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!visited.Add(current)) continue;
            foreach (var neighbor in byId[current].Neighbors)
                if (!visited.Contains(neighbor)) pending.Push(neighbor);
        }
        if (visited.Count != entries.Length)
            throw new ArgumentException("All territories must belong to one connected map.", nameof(territories));

        Key = key;
        Version = version;
        _territories = new System.Collections.ObjectModel.ReadOnlyDictionary<string, WarlordsTerritoryDefinition>(byId);
    }

    /// <summary>Gets the stable scenario key.</summary>
    public string Key { get; }
    /// <summary>Gets the authored scenario version.</summary>
    public int Version { get; }
    /// <summary>Gets the immutable territory definitions.</summary>
    public IReadOnlyCollection<WarlordsTerritoryDefinition> Territories => _territories.Values.ToArray();
    /// <summary>Gets one territory definition by stable ID.</summary>
    /// <param name="id">Stable territory ID.</param>
    public WarlordsTerritoryDefinition GetTerritory(string id) =>
        _territories.TryGetValue(id, out var territory)
            ? territory
            : throw new ArgumentException($"Unknown territory '{id}'.", nameof(id));

    /// <summary>Small four-kingdom map designed for a fast first playable match.</summary>
    public static WarlordsMap CreateStarterMap() => new("four-kingdoms", 1,
    [
        new("azure-keep", "Azure Keep", ["frostmarch", "highplain", "heartland"], WarlordsKingdom.Azure, true, 4),
        new("crimson-hold", "Crimson Hold", ["highplain", "sunfield", "heartland"], WarlordsKingdom.Crimson, true, 4),
        new("verdant-citadel", "Verdant Citadel", ["sunfield", "greenmarch", "heartland"], WarlordsKingdom.Verdant, true, 4),
        new("amber-fort", "Amber Fort", ["greenmarch", "frostmarch", "heartland"], WarlordsKingdom.Amber, true, 4),
        new("frostmarch", "Frostmarch", ["azure-keep", "amber-fort", "heartland"], null, false, 1),
        new("highplain", "Highplain", ["azure-keep", "crimson-hold", "heartland"], null, false, 1),
        new("sunfield", "Sunfield", ["crimson-hold", "verdant-citadel", "heartland"], null, false, 1),
        new("greenmarch", "Greenmarch", ["verdant-citadel", "amber-fort", "heartland"], null, false, 1),
        new("heartland", "The Heartland", ["azure-keep", "crimson-hold", "verdant-citadel", "amber-fort", "frostmarch", "highplain", "sunfield", "greenmarch"], null, false, 2)
    ]);
}
