namespace TheSingularityWorkshop.Experiences.CastleSiege;

/// <summary>
/// A read-only point-in-time view of a tabletop session for hosts, diagnostics, and presentation layers.
/// </summary>
/// <remarks>
/// This report contains abstract rules state and event history only. It does not claim physical simulation
/// evidence, and capturing a report does not advance or otherwise mutate the session.
/// </remarks>
public sealed record CastleSiegeSessionReport(
    string ScenarioKey,
    int ScenarioVersion,
    string RulesVersion,
    CastleSiegeTabletopState State,
    IReadOnlyList<CastleSiegeActionKind> LegalActions,
    IReadOnlyList<CastleSiegeTabletopEvent> Events,
    CastleSiegeOutcome Outcome)
{
    /// <summary>Captures a stable snapshot of the supplied session's current state and accepted events.</summary>
    public static CastleSiegeSessionReport Capture(CastleSiegeTabletopSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        return new CastleSiegeSessionReport(
            CastleSiegeTabletopSession.ScenarioKey,
            CastleSiegeTabletopSession.ScenarioVersion,
            CastleSiegeTabletopSession.RulesVersion,
            session.State,
            Array.AsReadOnly(session.GetLegalActions().ToArray()),
            Array.AsReadOnly(session.Events.ToArray()),
            session.State.Outcome);
    }
}
