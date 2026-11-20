namespace TheSingularityWorkshop.Experiences.Warlords;

public enum WarlordsPhase { Command, Finished }
public enum WarlordsBattleOutcome { NoBattle, AttackerVictory, DefenderVictory }

/// <summary>Current authoritative state of one territory.</summary>
public sealed record WarlordsTerritoryState(string TerritoryId, WarlordsKingdom? Owner, int ArmyCount, bool IsStronghold);

/// <summary>One legal march command from a source territory to an adjacent destination.</summary>
public sealed record WarlordsMarch(string SourceTerritoryId, string DestinationTerritoryId, int ArmyCount);

/// <summary>Canonical, ordered record of one accepted game action.</summary>
public sealed record WarlordsEvent(
    int Sequence,
    int Round,
    WarlordsKingdom Actor,
    string Action,
    string SourceTerritoryId,
    string DestinationTerritoryId,
    int ArmiesCommitted,
    int AttackerRoll,
    int DefenderRoll,
    WarlordsBattleOutcome BattleOutcome,
    WarlordsTerritoryState SourceAfter,
    WarlordsTerritoryState DestinationAfter,
    string RuleId);

/// <summary>Immutable inspection snapshot for a host, replay, or presentation adapter.</summary>
public sealed record WarlordsGameSnapshot(
    string MapKey,
    int MapVersion,
    int Seed,
    int Round,
    WarlordsKingdom ActiveKingdom,
    WarlordsPhase Phase,
    WarlordsKingdom? Winner,
    IReadOnlyList<WarlordsTerritoryState> Territories,
    IReadOnlyList<WarlordsEvent> Events);

/// <summary>
/// Host-independent turn-based kingdom strategy rules. The map, game state, movement validation,
/// recruitment, combat, victory, and event history live here; a host only presents and dispatches choices.
/// </summary>
/// <remarks>
/// Combat uses a seeded pair of six-sided dice. The seed is part of the match setup and accepted
/// events record both rolls, making the outcome reproducible and inspectable. This is an abstract
/// strategy-game rule, not a physical simulation.
/// </remarks>
public sealed class WarlordsGame
{
    public const string RulesVersion = "warlords-four-kingdoms/1";
    public const int VictoryStrongholds = 3;
    public const int MaximumGarrison = 8;

    private readonly WarlordsMap _map;
    private readonly Dictionary<string, WarlordsTerritoryState> _territories;
    private readonly List<WarlordsEvent> _events = [];
    private readonly Random _random;
    private readonly WarlordsKingdom[] _kingdomOrder;
    private int _kingdomIndex;
    private bool _hasMarchedThisTurn;

    public WarlordsGame(WarlordsMap map, int seed)
    {
        ArgumentNullException.ThrowIfNull(map);
        _map = map;
        Seed = seed;
        _random = new Random(seed);
        _kingdomOrder = Enum.GetValues<WarlordsKingdom>();
        _territories = map.Territories.ToDictionary(
            t => t.Id,
            t => new WarlordsTerritoryState(t.Id, t.StartingOwner, t.StartingGarrison, t.IsStronghold),
            StringComparer.Ordinal);

        ActiveKingdom = _kingdomOrder[0];
        RecruitAtStrongholds(ActiveKingdom);
    }

    public int Seed { get; }
    public int Round { get; private set; } = 1;
    public WarlordsKingdom ActiveKingdom { get; private set; }
    public WarlordsPhase Phase { get; private set; } = WarlordsPhase.Command;
    public WarlordsKingdom? Winner { get; private set; }
    public IReadOnlyList<WarlordsEvent> Events => _events.AsReadOnly();

    public IReadOnlyList<WarlordsMarch> GetLegalMarches()
    {
        if (Phase != WarlordsPhase.Command || _hasMarchedThisTurn) return [];
        var legal = new List<WarlordsMarch>();
        foreach (var source in _territories.Values.Where(t => t.Owner == ActiveKingdom && t.ArmyCount > 1))
        {
            var definition = _map.GetTerritory(source.TerritoryId);
            foreach (var destinationId in definition.Neighbors)
            {
                if (_territories[destinationId].Owner == ActiveKingdom) continue;
                for (var armyCount = 1; armyCount < source.ArmyCount; armyCount++)
                    legal.Add(new WarlordsMarch(source.TerritoryId, destinationId, armyCount));
            }
        }
        return legal.AsReadOnly();
    }

    /// <summary>Moves armies across one map edge; invalid marches do not mutate state or consume dice.</summary>
    public WarlordsEvent March(WarlordsMarch march)
    {
        ArgumentNullException.ThrowIfNull(march);
        EnsureActive();
        if (_hasMarchedThisTurn)
            throw new InvalidOperationException("Only one march is allowed per kingdom turn in rules version 1.");

        if (march.ArmyCount <= 0) throw new ArgumentOutOfRangeException(nameof(march), "A march must commit at least one army.");
        if (!_territories.TryGetValue(march.SourceTerritoryId, out var source))
            throw new ArgumentException("Unknown source territory.", nameof(march));
        if (!_territories.TryGetValue(march.DestinationTerritoryId, out var destination))
            throw new ArgumentException("Unknown destination territory.", nameof(march));
        if (source.Owner != ActiveKingdom)
            throw new InvalidOperationException("The active kingdom can only march from its own territory.");
        if (!_map.GetTerritory(source.TerritoryId).Neighbors.Contains(destination.TerritoryId, StringComparer.Ordinal))
            throw new InvalidOperationException("Armies can only march to an adjacent territory.");
        if (march.ArmyCount >= source.ArmyCount)
            throw new InvalidOperationException("A territory must retain at least one defending army.");
        if (destination.Owner == ActiveKingdom)
            throw new InvalidOperationException("Friendly territories are not valid attack destinations in this first rules slice.");

        var sourceAfter = source with { ArmyCount = source.ArmyCount - march.ArmyCount };
        var attackerRoll = _random.Next(1, 7);
        var defenderRoll = _random.Next(1, 7);
        var attackerWins = (march.ArmyCount * 2) + attackerRoll > (destination.ArmyCount * 2) + defenderRoll;
        WarlordsBattleOutcome outcome;
        WarlordsTerritoryState destinationAfter;

        if (attackerWins)
        {
            outcome = WarlordsBattleOutcome.AttackerVictory;
            var survivors = Math.Max(1, march.ArmyCount - Math.Max(1, (destination.ArmyCount + 1) / 2));
            destinationAfter = destination with { Owner = ActiveKingdom, ArmyCount = survivors };
        }
        else
        {
            outcome = WarlordsBattleOutcome.DefenderVictory;
            var survivors = Math.Max(1, destination.ArmyCount - Math.Max(1, (march.ArmyCount + 1) / 2));
            destinationAfter = destination with { ArmyCount = survivors };
        }

        _territories[source.TerritoryId] = sourceAfter;
        _territories[destination.TerritoryId] = destinationAfter;
        _hasMarchedThisTurn = true;

        if (CountStrongholds(ActiveKingdom) >= VictoryStrongholds)
        {
            Winner = ActiveKingdom;
            Phase = WarlordsPhase.Finished;
        }
        var entry = new WarlordsEvent(
            _events.Count + 1, Round, ActiveKingdom, "march",
            source.TerritoryId, destination.TerritoryId, march.ArmyCount,
            attackerRoll, defenderRoll, outcome, sourceAfter, destinationAfter,
            "combat.seeded-d6-strength-comparison");
        _events.Add(entry);
        return entry;
    }

    /// <summary>Ends the current kingdom's command and advances to the next surviving kingdom.</summary>
    public void EndTurn()
    {
        EnsureActive();
        var previous = ActiveKingdom;
        do
        {
            _kingdomIndex = (_kingdomIndex + 1) % _kingdomOrder.Length;
            if (_kingdomIndex == 0) Round++;
            ActiveKingdom = _kingdomOrder[_kingdomIndex];
        }
        while (!HasTerritories(ActiveKingdom) && ActiveKingdom != previous);

        if (ActiveKingdom == previous && !HasTerritories(ActiveKingdom))
        {
            Phase = WarlordsPhase.Finished;
            return;
        }

        _hasMarchedThisTurn = false;
        RecruitAtStrongholds(ActiveKingdom);
    }

    public WarlordsGameSnapshot Capture() => new(
        _map.Key, _map.Version, Seed, Round, ActiveKingdom, Phase, Winner,
        Array.AsReadOnly(_territories.Values.OrderBy(t => t.TerritoryId, StringComparer.Ordinal).ToArray()),
        Array.AsReadOnly(_events.ToArray()));

    private void RecruitAtStrongholds(WarlordsKingdom kingdom)
    {
        foreach (var territory in _territories.Values.Where(t => t.Owner == kingdom && t.IsStronghold).ToArray())
            _territories[territory.TerritoryId] = territory with { ArmyCount = Math.Min(MaximumGarrison, territory.ArmyCount + 1) };
    }

    private int CountStrongholds(WarlordsKingdom kingdom) =>
        _territories.Values.Count(t => t.IsStronghold && t.Owner == kingdom);

    private bool HasTerritories(WarlordsKingdom kingdom) => _territories.Values.Any(t => t.Owner == kingdom);

    private void EnsureActive()
    {
        if (Phase != WarlordsPhase.Command)
            throw new InvalidOperationException("The match is finished; no further actions are accepted.");
    }
}
