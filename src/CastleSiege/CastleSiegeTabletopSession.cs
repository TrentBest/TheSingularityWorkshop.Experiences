namespace TheSingularityWorkshop.Experiences.CastleSiege;

/// <summary>Roles in the renderer-independent West Wall tabletop demonstration.</summary>
public enum CastleSiegeRole
{
    Attacker,
    Defender
}

/// <summary>Actions accepted by the abstract tabletop rules; these are not physical commands.</summary>
public enum CastleSiegeActionKind
{
    FireAtWestWall,
    ReinforceWestWall,
    HoldPosition
}

/// <summary>Classifies a tabletop session's terminal outcome.</summary>
public enum CastleSiegeOutcome
{
    InProgress,
    AttackerVictory,
    DefenderVictory
}

/// <summary>One validated player choice.</summary>
public sealed record CastleSiegeAction(CastleSiegeRole Actor, CastleSiegeActionKind Kind);

/// <summary>The immutable state needed to resolve and replay the abstract tabletop scenario.</summary>
public sealed record CastleSiegeTabletopState(
    int TurnNumber,
    CastleSiegeRole ActiveRole,
    int WallIntegrity,
    int AttackerShotsRemaining,
    int DefenderRepairsRemaining,
    CastleSiegeOutcome Outcome);

/// <summary>An ordered, explainable abstract rules result. It is not Physics evidence.</summary>
public sealed record CastleSiegeTabletopEvent(
    int Sequence,
    int TurnNumber,
    CastleSiegeRole Actor,
    CastleSiegeActionKind Action,
    CastleSiegeTabletopState StateBefore,
    CastleSiegeTabletopState StateAfter,
    string RuleId,
    string Explanation,
    string Classification = "abstract-tabletop");

/// <summary>A stable, versioned, deterministic tabletop rules exercise for the Castle Siege Experience.</summary>
/// <remarks>
/// This rules slice intentionally has no dependency on Physics, rendering, host frame rate, random seeds,
/// or MicroBundle identifiers. It resolves game tokens only; it cannot report a physical trajectory,
/// intersection, impact, or damage result.
/// </remarks>
public sealed class CastleSiegeTabletopSession
{
    public const string ScenarioKey = "castle.west-wall";
    public const int ScenarioVersion = 1;
    public const string RulesVersion = "castle-siege-tabletop/1";

    public const int InitialWallIntegrity = 3;
    public const int InitialAttackerShots = 3;
    public const int InitialDefenderRepairs = 2;

    private readonly List<CastleSiegeTabletopEvent> _events = [];

    public CastleSiegeTabletopSession()
    {
        State = InitialState();
    }

    public CastleSiegeTabletopState State { get; private set; }

    /// <summary>Accepted events in canonical sequence order.</summary>
    public IReadOnlyList<CastleSiegeTabletopEvent> Events => _events.AsReadOnly();

    public static CastleSiegeTabletopState InitialState() =>
        new(1, CastleSiegeRole.Attacker, InitialWallIntegrity, InitialAttackerShots,
            InitialDefenderRepairs, CastleSiegeOutcome.InProgress);

    /// <summary>Returns the legal actions for the active role in the current state.</summary>
    public IReadOnlyList<CastleSiegeActionKind> GetLegalActions()
    {
        if (State.Outcome != CastleSiegeOutcome.InProgress)
        {
            return [];
        }

        return State.ActiveRole switch
        {
            CastleSiegeRole.Attacker when State.AttackerShotsRemaining > 0 =>
                [CastleSiegeActionKind.FireAtWestWall],
            CastleSiegeRole.Defender =>
                State.DefenderRepairsRemaining > 0
                    ? [CastleSiegeActionKind.ReinforceWestWall, CastleSiegeActionKind.HoldPosition]
                    : [CastleSiegeActionKind.HoldPosition],
            _ => []
        };
    }

    /// <summary>
    /// Validates and resolves one action. Invalid actions leave state and event history unchanged.
    /// </summary>
    public CastleSiegeTabletopEvent Apply(CastleSiegeAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (State.Outcome != CastleSiegeOutcome.InProgress)
        {
            throw new InvalidOperationException("The session is terminal; no further actions are accepted.");
        }

        if (action.Actor != State.ActiveRole)
        {
            throw new InvalidOperationException($"Out-of-turn action: {action.Actor} cannot act while {State.ActiveRole} is active.");
        }

        if (!GetLegalActions().Contains(action.Kind))
        {
            throw new InvalidOperationException($"Action {action.Kind} is not legal for {State.ActiveRole} in the current state.");
        }

        var before = State;
        var wallIntegrity = before.WallIntegrity;
        var shots = before.AttackerShotsRemaining;
        var repairs = before.DefenderRepairsRemaining;
        var ruleId = string.Empty;
        var explanation = string.Empty;

        switch (action.Kind)
        {
            case CastleSiegeActionKind.FireAtWestWall:
                wallIntegrity--;
                shots--;
                ruleId = "wall.abstract-breach-point";
                explanation = $"The tabletop attack spends one shot and reduces abstract wall integrity to {wallIntegrity}. This is not a simulated impact.";
                break;

            case CastleSiegeActionKind.ReinforceWestWall:
                wallIntegrity = Math.Min(InitialWallIntegrity, wallIntegrity + 1);
                repairs--;
                ruleId = "wall.abstract-reinforcement";
                explanation = $"The defender spends one repair token; abstract wall integrity is {wallIntegrity}.";
                break;

            case CastleSiegeActionKind.HoldPosition:
                ruleId = "defender.abstract-hold";
                explanation = "The defender holds position without changing wall integrity or spending a repair token.";
                break;

            default:
                throw new InvalidOperationException($"Unsupported action: {action.Kind}.");
        }

        var outcome = wallIntegrity <= 0
            ? CastleSiegeOutcome.AttackerVictory
            : shots == 0
                ? CastleSiegeOutcome.DefenderVictory
                : CastleSiegeOutcome.InProgress;

        var nextRole = before.ActiveRole == CastleSiegeRole.Attacker
            ? CastleSiegeRole.Defender
            : CastleSiegeRole.Attacker;

        State = new CastleSiegeTabletopState(
            before.TurnNumber + 1,
            nextRole,
            wallIntegrity,
            shots,
            repairs,
            outcome);

        var entry = new CastleSiegeTabletopEvent(
            _events.Count + 1,
            before.TurnNumber,
            action.Actor,
            action.Kind,
            before,
            State,
            ruleId,
            explanation);

        _events.Add(entry);
        return entry;
    }
}
