namespace TheSingularityWorkshop.Experiences.Warlords;

/// <summary>Whether a match accepts commands or has reached a terminal state.</summary>
public enum WarlordsPhase { Command, Finished }

/// <summary>Outcome of one territory battle.</summary>
public enum WarlordsBattleOutcome { NoBattle, AttackerVictory, DefenderVictory }

/// <summary>Current authoritative ownership and army count for one territory.</summary>
public sealed record WarlordsTerritoryState(string TerritoryId, WarlordsKingdom? Owner, int ArmyCount, bool IsStronghold);

/// <summary>Requested movement of a positive number of armies across one map edge.</summary>
public sealed record WarlordsMarch(string SourceTerritoryId, string DestinationTerritoryId, int ArmyCount);

/// <summary>Immutable evidence for one accepted march and its resolved battle.</summary>
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

/// <summary>Detached point-in-time snapshot of match identity, state, and accepted events.</summary>
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
/// events record both rolls, making the outcome reproducible and inspectable. The explicit
/// xorshift32 algorithm is stable across .NET runtime versions. This is an abstract strategy-game
/// rule, not a physical simulation.
/// </remarks>
public sealed class WarlordsGame
{
    /// <summary>Versioned rules contract for the first four-kingdom scenario.</summary>
    public const string RulesVersion = "warlords-four-kingdoms/1";
    /// <summary>Number of strongholds required to win by conquest.</summary>
    public const int VictoryStrongholds = 3;
    /// <summary>Maximum number of armies permitted in one territory garrison.</summary>
    public const int MaximumGarrison = WarlordsMap.MaximumGarrison;
    /// <summary>Versioned deterministic pseudo-random algorithm used for combat dice.</summary>
    public const string CombatAlgorithm = "combat.xorshift32-d6/v1";

    private readonly WarlordsMap _map;
    private readonly Dictionary<string, WarlordsTerritoryState> _territories;
    private readonly List<WarlordsEvent> _events = [];
    private uint _randomState;
    private readonly WarlordsKingdom[] _kingdomOrder;
    private int _kingdomIndex;
    private bool _hasMarchedThisTurn;

    /// <summary>Starts a new match with the supplied validated map and reproducible combat seed.</summary>
    /// <param name="map">The authored scenario map.</param>
    /// <param name="seed">Seed for the match's deterministic combat roll stream.</param>
    public WarlordsGame(WarlordsMap map, int seed)
    {
        ArgumentNullException.ThrowIfNull(map);
        _map = map;
        Seed = seed;
        _randomState = unchecked((uint)seed) ^ 0x9E3779B9u;
        if (_randomState == 0) _randomState = 0xA341316Cu;
        _kingdomOrder = Enum.GetValues<WarlordsKingdom>();
        _territories = map.Territories.ToDictionary(
            t => t.Id,
            t => new WarlordsTerritoryState(t.Id, t.StartingOwner, t.StartingGarrison, t.IsStronghold),
            StringComparer.Ordinal);

        ActiveKingdom = _kingdomOrder[0];
        RecruitAtStrongholds(ActiveKingdom);
    }

    /// <summary>Gets the match seed used by the combat random stream.</summary>
    public int Seed { get; }
    /// <summary>Gets the current round, starting at one.</summary>
    public int Round { get; private set; } = 1;
    /// <summary>Gets the kingdom currently allowed to issue commands.</summary>
    public WarlordsKingdom ActiveKingdom { get; private set; }
    /// <summary>Gets the current match phase.</summary>
    public WarlordsPhase Phase { get; private set; } = WarlordsPhase.Command;
    /// <summary>Gets the winning kingdom after the victory condition is met.</summary>
    public WarlordsKingdom? Winner { get; private set; }
    /// <summary>Gets accepted events in append order.</summary>
    public IReadOnlyList<WarlordsEvent> Events => _events.AsReadOnly();

    /// <summary>Lists every currently legal one-turn march choice for the active kingdom.</summary>
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
        var attackerRoll = NextDieRoll();
        var defenderRoll = NextDieRoll();
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

        if (CountStrongholds(ActiveKingdom) >= VictoryStrongholds ||
            _kingdomOrder.Where(kingdom => kingdom != ActiveKingdom).All(kingdom => !HasTerritories(kingdom)))
        {
            Winner = ActiveKingdom;
            Phase = WarlordsPhase.Finished;
        }
        var entry = new WarlordsEvent(
            _events.Count + 1, Round, ActiveKingdom, "march",
            source.TerritoryId, destination.TerritoryId, march.ArmyCount,
            attackerRoll, defenderRoll, outcome, sourceAfter, destinationAfter,
            CombatAlgorithm);
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

    /// <summary>Captures a detached snapshot suitable for presentation or later serialization.</summary>
    public WarlordsGameSnapshot Capture() => new(
        _map.Key, _map.Version, Seed, Round, ActiveKingdom, Phase, Winner,
        Array.AsReadOnly(_territories.Values.OrderBy(t => t.TerritoryId, StringComparer.Ordinal).ToArray()),
        Array.AsReadOnly(_events.ToArray()));

    private int NextDieRoll()
    {
        // Rejection sampling avoids modulo bias while preserving the explicit xorshift32 stream.
        const uint bound = 6;
        var threshold = unchecked(0u - bound) % bound;
        while (true)
        {
            var value = _randomState;
            value ^= value << 13;
            value ^= value >> 17;
            value ^= value << 5;
            _randomState = value;
            if (value >= threshold)
                return (int)(value % bound) + 1;
        }
    }

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
