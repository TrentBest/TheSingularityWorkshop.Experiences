using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheSingularityWorkshop.Experiences.CapsuleClinic;

/// <summary>
/// A bounded, versioned record of the initial conditions and ordered inputs for one deterministic game.
/// </summary>
/// <remarks>
/// Replays are diagnostic artifacts, not authoritative network messages. The format stores actions rather
/// than board snapshots; playback depends on compatible game rules and the same replay format version.
/// </remarks>
public sealed class CapsuleReplay
{
    /// <summary>Gets the current serialized replay format version.</summary>
    public const int CurrentFormatVersion = 1;

    /// <summary>Maximum number of actions accepted in one replay.</summary>
    public const int MaximumActions = 100_000;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    private readonly CapsuleAction[] _actions;

    /// <summary>Creates a replay with the supplied deterministic initial conditions and optional actions.</summary>
    /// <param name="seed">Seed used to generate capsule colors.</param>
    /// <param name="width">Board width in cells.</param>
    /// <param name="height">Board height in cells.</param>
    /// <param name="actions">Optional ordered action stream.</param>
    public CapsuleReplay(int seed, int width = 8, int height = 16, IEnumerable<CapsuleAction>? actions = null)
    {
        if (width < 1) throw new ArgumentOutOfRangeException(nameof(width));
        if (height < 1) throw new ArgumentOutOfRangeException(nameof(height));

        Seed = seed;
        Width = width;
        Height = height;
        _actions = actions?.ToArray() ?? [];
        ValidateActions(_actions);
    }

    /// <summary>Gets the replay format version, independent of the game rules version.</summary>
    public int FormatVersion => CurrentFormatVersion;

    /// <summary>Gets the deterministic capsule-color seed.</summary>
    public int Seed { get; }

    /// <summary>Gets the initial board width in cells.</summary>
    public int Width { get; }

    /// <summary>Gets the initial board height in cells.</summary>
    public int Height { get; }

    /// <summary>Gets a read-only copy of the ordered action stream.</summary>
    public IReadOnlyList<CapsuleAction> Actions => Array.AsReadOnly(_actions);

    /// <summary>Returns a new replay with one additional action.</summary>
    public CapsuleReplay Append(CapsuleAction action)
    {
        if (!Enum.IsDefined(action)) throw new ArgumentOutOfRangeException(nameof(action));
        if (_actions.Length >= MaximumActions)
            throw new InvalidOperationException($"A replay cannot exceed {MaximumActions} actions.");

        var next = new CapsuleAction[_actions.Length + 1];
        Array.Copy(_actions, next, _actions.Length);
        next[^1] = action;
        return new CapsuleReplay(Seed, Width, Height, next);
    }

    /// <summary>Creates a fresh game and applies the recorded actions in order.</summary>
    public CapsuleReplayResult Play()
    {
        var game = new CapsuleGame(Seed, Width, Height);
        var results = new GameStepResult[_actions.Length];
        for (var i = 0; i < _actions.Length; i++)
            results[i] = game.Step(_actions[i]);

        return new CapsuleReplayResult(game, Array.AsReadOnly(results));
    }

    /// <summary>Serializes this replay as versioned JSON.</summary>
    public string ToJson()
    {
        var document = new ReplayDocument
        {
            FormatVersion = CurrentFormatVersion,
            Seed = Seed,
            Width = Width,
            Height = Height,
            Actions = _actions
        };
        return JsonSerializer.Serialize(document, SerializerOptions);
    }

    /// <summary>Parses and validates a versioned JSON replay.</summary>
    public static CapsuleReplay FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        var document = JsonSerializer.Deserialize<ReplayDocument>(json, SerializerOptions)
            ?? throw new FormatException("Replay document cannot be null.");

        if (document.FormatVersion != CurrentFormatVersion)
            throw new FormatException($"Unsupported replay format version: {document.FormatVersion}.");
        if (document.Actions is null)
            throw new FormatException("Replay actions are required.");

        try
        {
            return new CapsuleReplay(document.Seed, document.Width, document.Height, document.Actions);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            throw new FormatException("Replay contains invalid board dimensions or actions.", exception);
        }
    }

    private static void ValidateActions(IReadOnlyList<CapsuleAction> actions)
    {
        if (actions.Count > MaximumActions)
            throw new ArgumentOutOfRangeException(nameof(actions), $"A replay cannot exceed {MaximumActions} actions.");

        for (var i = 0; i < actions.Count; i++)
            if (!Enum.IsDefined(actions[i]))
                throw new ArgumentOutOfRangeException(nameof(actions), $"Action at index {i} is invalid.");
    }

    private sealed class ReplayDocument
    {
        public ReplayDocument() { }

        public int FormatVersion { get; init; }
        public int Seed { get; init; }
        public int Width { get; init; }
        public int Height { get; init; }
        public CapsuleAction[]? Actions { get; init; }
    }
}

/// <summary>The final game state and per-input results produced by replay playback.</summary>
public sealed record CapsuleReplayResult(CapsuleGame Game, IReadOnlyList<GameStepResult> StepResults) : IDisposable
{
    /// <summary>Releases the FSM_API game instance created for playback.</summary>
    public void Dispose() => Game.Dispose();
}
