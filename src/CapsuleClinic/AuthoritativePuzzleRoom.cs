namespace TheSingularityWorkshop.Experiences.CapsuleClinic;

/// <summary>A versioned request to apply one player action to an authoritative puzzle room.</summary>
/// <param name="ProtocolVersion">The protocol version understood by the sender.</param>
/// <param name="RoomId">The intended room identity.</param>
/// <param name="PlayerId">The player identity bound to the sender's authenticated session.</param>
/// <param name="Sequence">The next monotonically increasing command sequence for this player, starting at one.</param>
/// <param name="Action">The requested game action.</param>
public readonly record struct GameCommandEnvelope(
    int ProtocolVersion,
    string RoomId,
    string PlayerId,
    ulong Sequence,
    CapsuleAction Action);

/// <summary>Reasons an authoritative room may reject a command before simulation.</summary>
public enum CommandRejection
{
    /// <summary>The command passed protocol and session validation.</summary>
    None = 0,
    /// <summary>The sender's protocol version is unsupported.</summary>
    ProtocolMismatch,
    /// <summary>The command targets a different room.</summary>
    WrongRoom,
    /// <summary>The message player identity does not match the authenticated session identity.</summary>
    IdentityMismatch,
    /// <summary>The player has not joined this room.</summary>
    PlayerNotJoined,
    /// <summary>The command sequence is duplicate, out of order, or exhausted.</summary>
    InvalidSequence,
    /// <summary>The action value is not defined by this protocol version.</summary>
    InvalidAction
}

/// <summary>The authoritative decision for a received command.</summary>
/// <param name="Accepted">Whether the command was valid and consumed a sequence number.</param>
/// <param name="Rejection">The protocol/session rejection reason, if any.</param>
/// <param name="Sequence">The sequence number supplied by the sender.</param>
/// <param name="Step">The game-loop result when the command was accepted.</param>
public readonly record struct GameCommandDecision(
    bool Accepted,
    CommandRejection Rejection,
    ulong Sequence,
    GameStepResult Step);

/// <summary>A read-only view of one player's authoritative puzzle state.</summary>
/// <param name="PlayerId">The player identity.</param>
/// <param name="Board">An independent bottom-row-first board snapshot.</param>
/// <param name="Active">The active falling capsule, if any.</param>
/// <param name="GameOver">Whether the player's board has topped out.</param>
/// <param name="LastAcceptedSequence">The last command sequence consumed by the room.</param>
public sealed record PuzzlePlayerView(
    string PlayerId,
    IReadOnlyList<PuzzleCell?> Board,
    ActiveCapsule? Active,
    bool GameOver,
    ulong LastAcceptedSequence);

/// <summary>
/// A small authoritative in-process room used to validate the multiplayer command boundary.
/// </summary>
/// <remarks>
/// This is not a socket server, authentication provider, matchmaking service, or production security
/// boundary. The hosting transport must bind <c>authenticatedPlayerId</c> to a real connection/session.
/// Clients submit intent only; the room owns game instances and exposes read-only snapshots.
/// </remarks>
public sealed class AuthoritativePuzzleRoom
{
    private readonly Dictionary<string, PlayerState> _players = new(StringComparer.Ordinal);

    /// <summary>Creates a room with a fixed protocol version and bounded player capacity.</summary>
    public AuthoritativePuzzleRoom(string roomId, int width = 8, int height = 16, int maxPlayers = 2, int protocolVersion = 1)
    {
        if (string.IsNullOrWhiteSpace(roomId)) throw new ArgumentException("Room ID must not be blank.", nameof(roomId));
        if (maxPlayers < 1) throw new ArgumentOutOfRangeException(nameof(maxPlayers));
        if (protocolVersion < 1) throw new ArgumentOutOfRangeException(nameof(protocolVersion));
        if (width < 1) throw new ArgumentOutOfRangeException(nameof(width));
        if (height < 1) throw new ArgumentOutOfRangeException(nameof(height));

        RoomId = roomId;
        Width = width;
        Height = height;
        MaxPlayers = maxPlayers;
        ProtocolVersion = protocolVersion;
    }

    /// <summary>Gets the stable room identity.</summary>
    public string RoomId { get; }

    /// <summary>Gets the configured board width.</summary>
    public int Width { get; }

    /// <summary>Gets the configured board height.</summary>
    public int Height { get; }

    /// <summary>Gets the maximum number of players admitted to this room.</summary>
    public int MaxPlayers { get; }

    /// <summary>Gets the protocol version required by this room.</summary>
    public int ProtocolVersion { get; }

    /// <summary>Gets the current number of joined players.</summary>
    public int PlayerCount => _players.Count;

    /// <summary>Adds a player with an explicit deterministic seed; duplicate IDs and full rooms are rejected.</summary>
    public bool JoinPlayer(string playerId, int seed)
    {
        if (string.IsNullOrWhiteSpace(playerId)) throw new ArgumentException("Player ID must not be blank.", nameof(playerId));
        if (_players.Count >= MaxPlayers || _players.ContainsKey(playerId)) return false;
        _players.Add(playerId, new PlayerState(new CapsuleGame(seed, Width, Height)));
        return true;
    }

    /// <summary>
    /// Validates and applies a command. A valid command consumes its sequence even if the requested
    /// move is blocked by the board; invalid commands do not advance the sequence.
    /// </summary>
    public GameCommandDecision ApplyCommand(string authenticatedPlayerId, GameCommandEnvelope command)
    {
        if (string.IsNullOrWhiteSpace(authenticatedPlayerId))
            return Reject(CommandRejection.IdentityMismatch, command.Sequence);
        if (command.ProtocolVersion != ProtocolVersion)
            return Reject(CommandRejection.ProtocolMismatch, command.Sequence);
        if (!string.Equals(command.RoomId, RoomId, StringComparison.Ordinal))
            return Reject(CommandRejection.WrongRoom, command.Sequence);
        if (!string.Equals(command.PlayerId, authenticatedPlayerId, StringComparison.Ordinal))
            return Reject(CommandRejection.IdentityMismatch, command.Sequence);
        if (!_players.TryGetValue(authenticatedPlayerId, out var player))
            return Reject(CommandRejection.PlayerNotJoined, command.Sequence);
        if (!Enum.IsDefined(command.Action))
            return Reject(CommandRejection.InvalidAction, command.Sequence);
        if (player.LastAcceptedSequence == ulong.MaxValue || command.Sequence != player.LastAcceptedSequence + 1)
            return Reject(CommandRejection.InvalidSequence, command.Sequence);

        var step = player.Game.Step(command.Action);
        player.LastAcceptedSequence = command.Sequence;
        return new GameCommandDecision(true, CommandRejection.None, command.Sequence, step);
    }

    /// <summary>Gets an immutable snapshot for a joined player.</summary>
    public PuzzlePlayerView GetPlayerView(string playerId)
    {
        ArgumentNullException.ThrowIfNull(playerId);
        if (!_players.TryGetValue(playerId, out var player))
            throw new KeyNotFoundException($"Player '{playerId}' has not joined room '{RoomId}'.");
        return new PuzzlePlayerView(
            playerId,
            player.Game.Board.Snapshot(),
            player.Game.Active,
            player.Game.GameOver,
            player.LastAcceptedSequence);
    }

    private static GameCommandDecision Reject(CommandRejection reason, ulong sequence) =>
        new(false, reason, sequence, default);

    private sealed class PlayerState(CapsuleGame game)
    {
        public CapsuleGame Game { get; } = game;
        public ulong LastAcceptedSequence { get; set; }
    }
}
