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
/// A deterministic single-player game loop around the renderer-independent puzzle board.
/// </summary>
/// <remarks>
/// Piece generation uses a small specified PRNG rather than framework <see cref="Random"/> so a seed
/// and action sequence are reproducible across supported runtimes. Wall kicks, scoring, and versus
/// pressure are intentionally not part of this first loop.
/// </remarks>
public sealed class CapsuleGame
{
    private readonly SeededColorGenerator _generator;
    private ActiveCapsule? _activeCapsule;

    /// <summary>Creates a game with an empty board and deterministic piece stream.</summary>
    public CapsuleGame(int seed, int width = 8, int height = 16)
    {
        Board = new CapsuleBoard(width, height);
        _generator = new SeededColorGenerator(unchecked((uint)seed));
        SpawnNext();
    }

    /// <summary>Gets the authoritative local puzzle board.</summary>
    public CapsuleBoard Board { get; }

    /// <summary>Gets the active falling capsule, or null when the game is over.</summary>
    public ActiveCapsule? Active => _activeCapsule;

    /// <summary>Gets whether no further capsule can be spawned.</summary>
    public bool GameOver { get; private set; }

    /// <summary>Applies one input or simulation action.</summary>
    public GameStepResult Step(CapsuleAction action)
    {
        if (!Enum.IsDefined(action)) throw new ArgumentOutOfRangeException(nameof(action));
        if (GameOver || _activeCapsule is null)
            return new GameStepResult(false, false, default, GameOver);

        switch (action)
        {
            case CapsuleAction.MoveLeft:
                return new GameStepResult(TryShift(-1, 0), false, default, GameOver);
            case CapsuleAction.MoveRight:
                return new GameStepResult(TryShift(1, 0), false, default, GameOver);
            case CapsuleAction.Rotate:
                return new GameStepResult(TryRotate(), false, default, GameOver);
            case CapsuleAction.SoftDrop:
            case CapsuleAction.Tick:
                if (TryShift(0, -1))
                    return new GameStepResult(true, false, default, GameOver);
                return LockAndSpawn();
            case CapsuleAction.HardDrop:
                while (TryShift(0, -1)) { }
                return LockAndSpawn();
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
