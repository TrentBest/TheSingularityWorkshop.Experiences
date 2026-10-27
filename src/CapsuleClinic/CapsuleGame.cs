using System.Threading;
using TheSingularityWorkshop.FSM_API;
using FsmRuntime = TheSingularityWorkshop.FSM_API.FSM_API;

namespace TheSingularityWorkshop.Experiences.CapsuleClinic;

/// <summary>Player or simulation actions accepted by the deterministic puzzle loop.</summary>
public enum CapsuleAction
{
    /// <summary>Move the active capsule one cell left.</summary>
    MoveLeft,
    /// <summary>Move the active capsule one cell right.</summary>
    MoveRight,
    /// <summary>Toggle between horizontal and vertical orientation.</summary>
    Rotate,
    /// <summary>Advance the active capsule downward by one cell, locking if blocked.</summary>
    SoftDrop,
    /// <summary>Advance one simulation gravity step.</summary>
    Tick,
    /// <summary>Drop the active capsule to its lowest legal position and lock it.</summary>
    HardDrop
}

/// <summary>The current falling capsule, expressed in board coordinates.</summary>
public readonly record struct ActiveCapsule(CapsulePair Pair, int PivotX, int PivotY, CapsuleOrientation Orientation);

/// <summary>The result of applying one action to a Capsule Clinic game.</summary>
public readonly record struct GameStepResult(bool ActionAccepted, bool CapsuleLocked, ResolutionResult Resolution, bool GameOver, int ScoreAwarded = 0);

/// <summary>
/// A deterministic puzzle game whose lifecycle is driven by a named FSM_API definition.
/// The board remains a domain model; FSM_API owns the Falling/GameOver behavior lifecycle.
/// </summary>
/// <remarks>
/// Piece generation uses a specified PRNG rather than framework <see cref="Random"/> so a seed
/// and action sequence are reproducible across supported runtimes. Each game is an
/// <see cref="IStateContext"/> instance attached to the shared CapsuleClinicGameFSM definition.
/// Actions are processed through that instance's manual FSM update, never by ticking every
/// game's processing group as a side effect of one player's input.
/// </remarks>
public sealed class CapsuleGame : IStateContext, IDisposable
{
    private const string FsmDefinitionName = "CapsuleClinicGameFSM";
    private const string ProcessingGroup = "CapsuleClinic";
    private static readonly object DefinitionGate = new();
    private static long _nextGameId;

    private readonly SeededColorGenerator _generator;
    private readonly FSMHandle _fsmHandle;
    private ActiveCapsule? _activeCapsule;
    private CapsuleAction? _pendingAction;
    private GameStepResult _lastStepResult;
    private bool _disposed;

    /// <summary>Creates a game with an empty board and deterministic piece stream.</summary>
    public CapsuleGame(int seed, int width = 8, int height = 16)
    {
        Board = new CapsuleBoard(width, height);
        _generator = new SeededColorGenerator(unchecked((uint)seed));
        Name = $"CapsuleGame_{Interlocked.Increment(ref _nextGameId)}";
        IsValid = true;
        SpawnNext();
        EnsureFsmDefinition();
        _fsmHandle = FsmRuntime.Create.CreateInstance(FsmDefinitionName, this, ProcessingGroup);
    }

    /// <summary>Gets the diagnostic identity of this game context.</summary>
    public string Name { get; set; }

    /// <summary>Gets or sets whether this context is active in FSM_API.</summary>
    public bool IsValid { get; set; }

    /// <summary>Gets the current FSM_API lifecycle state for this game.</summary>
    public string FsmState => _fsmHandle.CurrentState;

    /// <summary>Gets the authoritative local puzzle board.</summary>
    public CapsuleBoard Board { get; }

    /// <summary>Gets the active falling capsule, or null when the game is over.</summary>
    public ActiveCapsule? Active => _activeCapsule;

    /// <summary>Gets the cumulative score for this game.</summary>
    public int Score { get; private set; }

    /// <summary>Gets whether no further capsule can be spawned.</summary>
    public bool GameOver { get; private set; }

    /// <summary>Applies one input or simulation action through this game's FSM_API instance.</summary>
    public GameStepResult Step(CapsuleAction action)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!Enum.IsDefined(action)) throw new ArgumentOutOfRangeException(nameof(action));

        _lastStepResult = new GameStepResult(false, false, default, GameOver);
        if (GameOver || _activeCapsule is null)
        {
            _fsmHandle.Update(ProcessingGroup);
            return _lastStepResult;
        }

        _pendingAction = action;
        _fsmHandle.Update(ProcessingGroup);
        _pendingAction = null;
        return _lastStepResult;
    }

    private static void EnsureFsmDefinition()
    {
        lock (DefinitionGate)
        {
            if (FsmRuntime.Interaction.Exists(FsmDefinitionName, ProcessingGroup))
                return;

            FsmRuntime.Create.CreateProcessingGroup(ProcessingGroup);
            FsmRuntime.Create.CreateFiniteStateMachine(FsmDefinitionName, -1, ProcessingGroup)
                .State("Falling", null, ProcessFallingState, null)
                .State("GameOver", null, null, null)
                .WithInitialState("Falling")
                .Transition("Falling", "GameOver", context => context is CapsuleGame game && game.GameOver)
                .BuildDefinition();
        }
    }

    private static void ProcessFallingState(IStateContext context)
    {
        if (context is CapsuleGame game)
            game.ProcessPendingAction();
    }

    private void ProcessPendingAction()
    {
        if (_pendingAction is not { } action || GameOver || _activeCapsule is null)
            return;

        switch (action)
        {
            case CapsuleAction.MoveLeft:
                _lastStepResult = new GameStepResult(TryShift(-1, 0), false, default, GameOver);
                break;
            case CapsuleAction.MoveRight:
                _lastStepResult = new GameStepResult(TryShift(1, 0), false, default, GameOver);
                break;
            case CapsuleAction.Rotate:
                _lastStepResult = new GameStepResult(TryRotate(), false, default, GameOver);
                break;
            case CapsuleAction.SoftDrop:
            case CapsuleAction.Tick:
                _lastStepResult = TryShift(0, -1)
                    ? new GameStepResult(true, false, default, GameOver)
                    : LockAndSpawn();
                break;
            case CapsuleAction.HardDrop:
                while (TryShift(0, -1)) { }
                _lastStepResult = LockAndSpawn();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private bool TryShift(int dx, int dy)
    {
        var active = _activeCapsule!.Value;
        var moved = active with { PivotX = active.PivotX + dx, PivotY = active.PivotY + dy };
        if (!CanOccupy(moved)) return false;
        _activeCapsule = moved;
        return true;
    }

    private bool TryRotate()
    {
        var active = _activeCapsule!.Value;
        var orientation = active.Orientation == CapsuleOrientation.Horizontal
            ? CapsuleOrientation.Vertical
            : CapsuleOrientation.Horizontal;
        var rotated = active with { Orientation = orientation };
        if (!CanOccupy(rotated)) return false;
        _activeCapsule = rotated;
        return true;
    }

    private bool CanOccupy(ActiveCapsule capsule)
    {
        var partnerX = capsule.PivotX + (capsule.Orientation == CapsuleOrientation.Horizontal ? 1 : 0);
        var partnerY = capsule.PivotY + (capsule.Orientation == CapsuleOrientation.Vertical ? 1 : 0);
        return IsEmpty(capsule.PivotX, capsule.PivotY) && IsEmpty(partnerX, partnerY);
    }

    private bool IsEmpty(int x, int y) =>
        x >= 0 && x < Board.Width && y >= 0 && y < Board.Height && !Board.GetCell(x, y).HasValue;

    private GameStepResult LockAndSpawn()
    {
        var active = _activeCapsule!.Value;
        var locked = Board.TryPlaceCapsule(active.Pair, active.PivotX, active.PivotY, active.Orientation);
        if (!locked)
        {
            GameOver = true;
            _activeCapsule = null;
            return new GameStepResult(false, false, default, GameOver);
        }

        var resolution = Board.ResolveMatchesAndGravity();
        var scoreAwarded = CapsuleScoringRules.Calculate(resolution);
        Score = checked(Score + scoreAwarded);
        _activeCapsule = null;
        SpawnNext();
        return new GameStepResult(true, true, resolution, GameOver, scoreAwarded);
    }

    private void SpawnNext()
    {
        if (Board.Height < 2)
        {
            GameOver = true;
            return;
        }

        var pair = _generator.NextPair();
        var spawn = new ActiveCapsule(pair, Board.Width / 2, Board.Height - 2, CapsuleOrientation.Vertical);
        if (!CanOccupy(spawn))
        {
            GameOver = true;
            _activeCapsule = null;
            return;
        }

        _activeCapsule = spawn;
    }

    /// <summary>Unregisters this game's FSM instance.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        IsValid = false;
        FsmRuntime.Interaction.DestroyInstance(_fsmHandle);
    }

    private sealed class SeededColorGenerator
    {
        private uint _state;

        public SeededColorGenerator(uint seed) => _state = seed == 0 ? 0x9E3779B9u : seed;

        public CapsulePair NextPair() => new(NextColor(), NextColor());

        private CapsuleColor NextColor()
        {
            var x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return (CapsuleColor)(x & 3u);
        }
    }
}
