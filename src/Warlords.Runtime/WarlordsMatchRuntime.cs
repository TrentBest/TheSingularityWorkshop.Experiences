using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.Experiences.Warlords.Runtime;

/// <summary>
/// Owns the FSM_API lifecycle for one Warlords match while delegating all game
/// rules and authoritative state to <see cref="WarlordsGame"/>.
/// </summary>
/// <remarks>
/// The adapter does not tick turns, choose moves, or reimplement combat. A host
/// submits domain commands; FSM_API advances the match lifecycle from Command
/// to Finished after the domain reports its terminal state.
/// </remarks>
public sealed class WarlordsMatchRuntime : IStateContext, IDisposable
{
    /// <summary>FSM_API state while the domain accepts match commands.</summary>
    public const string CommandState = "Command";
    /// <summary>FSM_API terminal state after the domain declares a winner.</summary>
    public const string FinishedState = "Finished";

    private readonly WarlordsGame _game;
    private readonly string _processingGroup = $"Warlords:{Guid.NewGuid():N}";
    private readonly string _fsmName = $"WarlordsMatchFSM:{Guid.NewGuid():N}";
    private readonly FSMHandle _handle;
    private bool _disposed;

    /// <summary>Creates an FSM_API-managed session around an already-authored map and seed.</summary>
    public WarlordsMatchRuntime(WarlordsMap map, int seed)
    {
        _game = new WarlordsGame(map, seed);

        FSM_API.FSM_API.Create.CreateProcessingGroup(_processingGroup);
        FSM_API.FSM_API.Create.CreateFiniteStateMachine(_fsmName, -1, _processingGroup)
            .State(CommandState, onEnter: null, onUpdate: null, onExit: null)
            .State(FinishedState, onEnter: null, onUpdate: null, onExit: null)
            .WithInitialState(CommandState)
            .Transition(CommandState, FinishedState,
                context => ((WarlordsMatchRuntime)context)._game.Phase == WarlordsPhase.Finished)
            .BuildDefinition();

        _handle = FSM_API.FSM_API.Create.CreateInstance(_fsmName, this, _processingGroup);
    }

    /// <inheritdoc />
    public string FsmName => _fsmName;

    /// <inheritdoc />
    public string Name { get; set; } = "WarlordsMatchRuntime";

    /// <inheritdoc />
    public bool IsValid { get; set; } = true;

    /// <summary>The lifecycle state owned by FSM_API.</summary>
    public string LifecycleState => _handle.CurrentState;

    /// <summary>The authoritative domain snapshot; presentation code should consume this rather than own game state.</summary>
    public WarlordsGameSnapshot Snapshot => _game.Capture();

    /// <summary>Returns legal commands from the domain while the lifecycle accepts commands.</summary>
    public IReadOnlyList<WarlordsMarch> GetLegalMarches() =>
        CanAcceptCommands ? _game.GetLegalMarches() : [];

    /// <summary>Submits one march to the authoritative Warlords domain.</summary>
    public WarlordsEvent March(WarlordsMarch march)
    {
        EnsureAcceptingCommands();
        var accepted = _game.March(march);
        AdvanceLifecycle();
        return accepted;
    }

    /// <summary>Ends the active kingdom turn in the authoritative Warlords domain.</summary>
    public void EndTurn()
    {
        EnsureAcceptingCommands();
        _game.EndTurn();
        AdvanceLifecycle();
    }

    /// <summary>Advances the FSM_API processing group once.</summary>
    public void Update()
    {
        if (_disposed || !IsValid) return;
        FSM_API.FSM_API.Interaction.Update(_processingGroup);
    }

    private bool CanAcceptCommands =>
        !_disposed && IsValid && _handle.CurrentState == CommandState;

    private void EnsureAcceptingCommands()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!IsValid) throw new InvalidOperationException("The Warlords match context is invalid.");
        if (_handle.CurrentState != CommandState)
            throw new InvalidOperationException("The Warlords match is finished; no further commands are accepted.");
    }

    private void AdvanceLifecycle() => Update();

    /// <summary>Destroys this match's FSM_API instance and invalidates the adapter context.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        IsValid = false;
        FSM_API.FSM_API.Interaction.DestroyFiniteStateMachine(_fsmName, _processingGroup);
        _disposed = true;
    }
}
