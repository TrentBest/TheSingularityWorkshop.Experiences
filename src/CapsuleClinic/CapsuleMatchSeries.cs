using System.Threading;
using TheSingularityWorkshop.FSM_API;
using FsmRuntime = TheSingularityWorkshop.FSM_API.FSM_API;

namespace TheSingularityWorkshop.Experiences.CapsuleClinic;

/// <summary>The lifecycle of a best-of series managed by the authoritative host.</summary>
public enum CapsuleMatchState
{
    /// <summary>The roster is open and the match has not started.</summary>
    WaitingForPlayers,
    /// <summary>A round is accepting authoritative game actions.</summary>
    InRound,
    /// <summary>A round ended and another may begin.</summary>
    RoundComplete,
    /// <summary>A participant has reached the configured series win total.</summary>
    MatchComplete
}

/// <summary>The result recorded by the host for one completed round.</summary>
public sealed record CapsuleRoundResult(int RoundNumber, string? WinnerPlayerId, IReadOnlyList<string> RankedPlayerIds, bool IsDraw);

/// <summary>A host-owned series ledger whose lifecycle transitions are defined and executed by FSM_API.</summary>
/// <remarks>
/// This class deliberately accepts round outcomes only from the host-side coordinator. Player command
/// envelopes contain actions, never winner declarations. The caller must derive the active-player set
/// from authoritative game state; this ledger is not a transport or security boundary.
/// </remarks>
public sealed class CapsuleMatchSeries : IStateContext, IDisposable
{
    private const string FsmDefinitionName = "CapsuleClinicMatchSeriesFSM";
    private const string ProcessingGroup = "CapsuleClinic";
    private static readonly object DefinitionGate = new();
    private static long _nextSeriesId;

    private readonly Dictionary<string, PlayerStandingState> _players = new(StringComparer.Ordinal);
    private readonly List<CapsuleRoundResult> _roundResults = [];
    private readonly Dictionary<string, int> _currentRoundRanks = new(StringComparer.Ordinal);
    private readonly FSMHandle _fsmHandle;
    private int _nextJoinOrder;
    private bool _startRequested;
    private bool _roundCompleted;
    private bool _startNextRoundRequested;
    private bool _disposed;

    /// <summary>Creates a series where a player must win this many rounds to win the match.</summary>
    public CapsuleMatchSeries(int roundsToWin = 3)
    {
        if (roundsToWin < 1) throw new ArgumentOutOfRangeException(nameof(roundsToWin));
        RoundsToWin = roundsToWin;
        Name = $"CapsuleMatchSeries_{Interlocked.Increment(ref _nextSeriesId)}";
        IsValid = true;
        EnsureFsmDefinition();
        _fsmHandle = FsmRuntime.Create.CreateInstance(FsmDefinitionName, this, ProcessingGroup);
    }

    /// <summary>Gets the diagnostic identity of this series context.</summary>
    public string Name { get; set; }

    /// <summary>Gets or sets whether this context is active in FSM_API.</summary>
    public bool IsValid { get; set; }

    /// <summary>Gets the number of round wins required to take the series.</summary>
    public int RoundsToWin { get; }

    /// <summary>Gets the current lifecycle state reported by the FSM_API handle.</summary>
    public CapsuleMatchState State => Enum.Parse<CapsuleMatchState>(_fsmHandle.CurrentState);

    /// <summary>Gets the one-based current round number, or zero before the match starts.</summary>
    public int RoundNumber { get; private set; }

    /// <summary>Gets the match winner after the series completes, otherwise null.</summary>
    public string? MatchWinnerPlayerId { get; private set; }

    /// <summary>Gets the immutable sequence of completed round results.</summary>
    public IReadOnlyList<CapsuleRoundResult> RoundResults => _roundResults.AsReadOnly();

    /// <summary>Registers a unique participant before the series begins.</summary>
    public bool JoinPlayer(string playerId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (string.IsNullOrWhiteSpace(playerId)) throw new ArgumentException("Player ID must not be blank.", nameof(playerId));
        if (State != CapsuleMatchState.WaitingForPlayers || _players.ContainsKey(playerId)) return false;
        _players.Add(playerId, new PlayerStandingState(_nextJoinOrder++));
        return true;
    }

    /// <summary>Starts the first round after at least two participants have joined.</summary>
    public bool StartMatch()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (State != CapsuleMatchState.WaitingForPlayers || _players.Count < 2) return false;
        RoundNumber = 1;
        _startRequested = true;
        _fsmHandle.Update(ProcessingGroup);
        _startRequested = false;
        return State == CapsuleMatchState.InRound;
    }

    /// <summary>
    /// Completes the current round using the active players calculated from authoritative game state.
    /// Pass every player still alive at the host's round-resolution boundary; zero or multiple survivors
    /// produce a draw, while one survivor wins the round.
    /// </summary>
    public CapsuleRoundResult CompleteRound(IEnumerable<string> activePlayerIds)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
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

        _roundCompleted = true;
        _fsmHandle.Update(ProcessingGroup);
        _roundCompleted = false;
        return result;
    }

    /// <summary>Begins another round while preserving series wins and player join order.</summary>
    public bool StartNextRound()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (State != CapsuleMatchState.RoundComplete) return false;
        RoundNumber++;
        _currentRoundRanks.Clear();
        _startNextRoundRequested = true;
        _fsmHandle.Update(ProcessingGroup);
        _startNextRoundRequested = false;
        return State == CapsuleMatchState.InRound;
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

    /// <summary>Unregisters the series FSM instance.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        IsValid = false;
        FsmRuntime.Interaction.DestroyInstance(_fsmHandle);
    }

    private static void EnsureFsmDefinition()
    {
        lock (DefinitionGate)
        {
            if (FsmRuntime.Interaction.Exists(FsmDefinitionName, ProcessingGroup))
                return;

            FsmRuntime.Create.CreateProcessingGroup(ProcessingGroup);
            FsmRuntime.Create.CreateFiniteStateMachine(FsmDefinitionName, -1, ProcessingGroup)
                .State("WaitingForPlayers", null, null, null)
                .State("InRound", null, null, null)
                .State("RoundComplete", null, null, null)
                .State("MatchComplete", null, null, null)
                .WithInitialState("WaitingForPlayers")
                .Transition("WaitingForPlayers", "InRound", context => context is CapsuleMatchSeries series && series._startRequested)
                .Transition("InRound", "MatchComplete", context => context is CapsuleMatchSeries series && series._roundCompleted && series.MatchWinnerPlayerId is not null)
                .Transition("InRound", "RoundComplete", context => context is CapsuleMatchSeries series && series._roundCompleted && series.MatchWinnerPlayerId is null)
                .Transition("RoundComplete", "InRound", context => context is CapsuleMatchSeries series && series._startNextRoundRequested)
                .BuildDefinition();
        }
    }

    private sealed class PlayerStandingState(int joinOrder)
    {
        public int JoinOrder { get; } = joinOrder;
        public int RoundsWon { get; set; }
    }
}
