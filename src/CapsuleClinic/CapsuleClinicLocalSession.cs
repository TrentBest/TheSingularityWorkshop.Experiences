namespace TheSingularityWorkshop.Experiences.CapsuleClinic;

/// <summary>Stable semantic intent identifiers exposed by the Capsule Clinic keyboard-first host.</summary>
/// <remarks>
/// These identifiers describe player intent, not direct board mutations. A host may bind keyboard,
/// GUI, or other UserIO events to them; every recognized intent still passes through the room's
/// versioned authoritative command validation.
/// </remarks>
public static class CapsuleClinicIntents
{
    public const string MoveLeft = "capsule.move-left";
    public const string MoveRight = "capsule.move-right";
    public const string Rotate = "capsule.rotate";
    public const string SoftDrop = "capsule.soft-drop";
    public const string HardDrop = "capsule.hard-drop";

    internal static bool TryResolve(string intentId, out CapsuleAction action)
    {
        action = intentId switch
        {
            MoveLeft => CapsuleAction.MoveLeft,
            MoveRight => CapsuleAction.MoveRight,
            Rotate => CapsuleAction.Rotate,
            SoftDrop => CapsuleAction.SoftDrop,
            HardDrop => CapsuleAction.HardDrop,
            _ => (CapsuleAction)(-1)
        };
        return Enum.IsDefined(action);
    }
}

/// <summary>The result of a semantic intent submission and the resulting authoritative presentation snapshot.</summary>
public sealed record CapsuleClinicSessionResult(
    bool IntentRecognized,
    GameCommandDecision Decision,
    CapsuleArenaSnapshot Snapshot);

/// <summary>
/// A small deterministic local play session that connects semantic intents to the existing authority boundary.
/// </summary>
/// <remarks>
/// The session creates a two-player in-process room so the existing match/standing/snapshot pipeline is active.
/// It does not provide sockets, authentication, a GUI toolkit, or a browser runtime. A host binds its controls
/// to <see cref="CapsuleClinicIntents"/> and renders the returned snapshot through its selected presentation stack.
/// </remarks>
public sealed class CapsuleClinicLocalSession : IDisposable
{
    private readonly AuthoritativePuzzleRoom _room;
    private readonly string _localPlayerId;
    private ulong _nextSequence = 1;
    private bool _disposed;

    /// <summary>Creates and starts a seeded two-player local session.</summary>
    public CapsuleClinicLocalSession(
        string roomId,
        string localPlayerId,
        int localSeed,
        string opponentPlayerId,
        int opponentSeed,
        int width = 8,
        int height = 16,
        int roundsToWin = 3)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localPlayerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(opponentPlayerId);
        if (string.Equals(localPlayerId, opponentPlayerId, StringComparison.Ordinal))
            throw new ArgumentException("Local player and opponent must have different identities.", nameof(opponentPlayerId));

        _localPlayerId = localPlayerId;
        _room = new AuthoritativePuzzleRoom(roomId, width, height);
        if (!_room.JoinPlayer(localPlayerId, localSeed) || !_room.JoinPlayer(opponentPlayerId, opponentSeed))
        {
            _room.Dispose();
            throw new InvalidOperationException("The seeded local session could not create its player roster.");
        }

        if (!_room.StartMatch(roundsToWin))
        {
            _room.Dispose();
            throw new InvalidOperationException("The seeded local session could not start its match.");
        }
    }

    /// <summary>Gets the latest authoritative arena snapshot.</summary>
    public CapsuleArenaSnapshot CaptureSnapshot()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return CapsuleArenaSnapshot.Capture(_room, _localPlayerId);
    }

    /// <summary>Submits one stable semantic intent through authoritative validation.</summary>
    public CapsuleClinicSessionResult SubmitIntent(string intentId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(intentId);

        if (!CapsuleClinicIntents.TryResolve(intentId, out var action))
        {
            var rejected = new GameCommandDecision(
                false,
                CommandRejection.InvalidAction,
                _nextSequence,
                default);
            return new CapsuleClinicSessionResult(false, rejected, CaptureSnapshot());
        }

        var command = new GameCommandEnvelope(
            _room.ProtocolVersion,
            _room.RoomId,
            _localPlayerId,
            _nextSequence,
            action);
        var decision = _room.ApplyCommand(_localPlayerId, command);
        if (decision.Accepted && _nextSequence < ulong.MaxValue)
            _nextSequence++;

        return new CapsuleClinicSessionResult(true, decision, CaptureSnapshot());
    }

    /// <summary>Releases the in-process authoritative room and its FSM_API game instances.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _room.Dispose();
    }
}
