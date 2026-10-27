namespace TheSingularityWorkshop.Experiences.CapsuleClinic;

/// <summary>The lifecycle of a best-of series managed by the authoritative host.</summary>
public enum CapsuleMatchState
{
    WaitingForPlayers,
    InRound,
    RoundComplete,
    MatchComplete
}

/// <summary>The result recorded by the host for one completed round.</summary>
public sealed record CapsuleRoundResult(int RoundNumber, string? WinnerPlayerId, IReadOnlyList<string> RankedPlayerIds, bool IsDraw);

/// <summary>A host-owned series ledger for round wins and stable standings.</summary>
/// <remarks>
/// This class deliberately accepts round outcomes only from the host-side coordinator. Player command
/// envelopes contain actions, never winner declarations. The caller must derive the active-player set
/// from authoritative game state; this ledger is not a transport or security boundary.
/// </remarks>
public sealed class CapsuleMatchSeries
{
    private readonly Dictionary<string, PlayerStandingState> _players = new(StringComparer.Ordinal);
    private readonly List<CapsuleRoundResult> _roundResults = [];
    private readonly Dictionary<string, int> _currentRoundRanks = new(StringComparer.Ordinal);
    private int _nextJoinOrder;

    /// <summary>Creates a series where a player must win this many rounds to win the match.</summary>
    public CapsuleMatchSeries(int roundsToWin = 3)
    {
        if (roundsToWin < 1) throw new ArgumentOutOfRangeException(nameof(roundsToWin));
        RoundsToWin = roundsToWin;
    }

    public int RoundsToWin { get; }
    public CapsuleMatchState State { get; private set; } = CapsuleMatchState.WaitingForPlayers;
    public int RoundNumber { get; private set; }
    public string? MatchWinnerPlayerId { get; private set; }
    public IReadOnlyList<CapsuleRoundResult> RoundResults => _roundResults.AsReadOnly();

    /// <summary>Registers a unique participant before the series begins.</summary>
    public bool JoinPlayer(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId)) throw new ArgumentException("Player ID must not be blank.", nameof(playerId));
        if (State != CapsuleMatchState.WaitingForPlayers || _players.ContainsKey(playerId)) return false;
        _players.Add(playerId, new PlayerStandingState(_nextJoinOrder++));
        return true;
    }

    /// <summary>Starts the first round after at least two participants have joined.</summary>
    public bool StartMatch()
    {
        if (State != CapsuleMatchState.WaitingForPlayers || _players.Count < 2) return false;
        RoundNumber = 1;
        State = CapsuleMatchState.InRound;
        return true;
    }

    /// <summary>
    /// Completes the current round using the active players calculated from authoritative game state.
    /// Pass every player still alive at the host's round-resolution boundary; zero or multiple survivors
    /// produce a draw, while one survivor wins the round.
    /// </summary>
    public CapsuleRoundResult CompleteRound(IEnumerable<string> activePlayerIds)
    {
        ArgumentNullException.ThrowIfNull(activePlayerIds);
        if (State != CapsuleMatchState.InRound) throw new InvalidOperationException("No round is currently in progress.");

        var active = activePlayerIds.ToHashSet(StringComparer.Ordinal);
        if (active.Any(id => !_players.ContainsKey(id)))
            throw new ArgumentException("The active-player set contains a player outside this series.", nameof(activePlayerIds));

        var ranked = _players
            .OrderByDescending(pair => active.Contains(pair.Key))
            .ThenByDescending(pair => pair.Value.RoundsWon)
            .ThenBy(pair => pair.Value.JoinOrder)
            .ThenBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => pair.Key)
            .ToArray();
        _currentRoundRanks.Clear();
        for (var index = 0; index < ranked.Length; index++)
            _currentRoundRanks.Add(ranked[index], index + 1);

        var isDraw = active.Count != 1;
        var winner = isDraw ? null : active.Single();
        if (winner is not null) _players[winner].RoundsWon++;

        var result = new CapsuleRoundResult(RoundNumber, winner, Array.AsReadOnly(ranked), isDraw);
        _roundResults.Add(result);
        MatchWinnerPlayerId = winner is not null && _players[winner].RoundsWon >= RoundsToWin ? winner : null;
        State = MatchWinnerPlayerId is null ? CapsuleMatchState.RoundComplete : CapsuleMatchState.MatchComplete;
        return result;
    }

    /// <summary>Begins another round while preserving series wins and player join order.</summary>
    public bool StartNextRound()
    {
        if (State != CapsuleMatchState.RoundComplete) return false;
        RoundNumber++;
        _currentRoundRanks.Clear();
        State = CapsuleMatchState.InRound;
        return true;
    }

    /// <summary>Returns a standings snapshot with current round rank and cumulative round wins.</summary>
    public IReadOnlyList<ArenaPlayerStanding> GetStandings()
    {
        var ordered = _players
            .OrderBy(pair => _currentRoundRanks.TryGetValue(pair.Key, out var rank) ? rank : int.MaxValue)
            .ThenByDescending(pair => pair.Value.RoundsWon)
            .ThenBy(pair => pair.Value.JoinOrder)
            .ThenBy(pair => pair.Key, StringComparer.Ordinal)
            .ToArray();

        return Array.AsReadOnly(ordered.Select((pair, index) =>
            new ArenaPlayerStanding(pair.Key,
                _currentRoundRanks.TryGetValue(pair.Key, out var rank) ? rank : index + 1,
                pair.Value.RoundsWon, pair.Value.JoinOrder, true)).ToArray());
    }

    private sealed class PlayerStandingState(int joinOrder)
    {
        public int JoinOrder { get; } = joinOrder;
        public int RoundsWon { get; set; }
    }
}
