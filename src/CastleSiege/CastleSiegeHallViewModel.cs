namespace TheSingularityWorkshop.Experiences.CastleSiege;

/// <summary>
/// Renderer-independent presentation data for the Castle Siege Wargaming Hall.
/// </summary>
/// <remarks>
/// This view model is derived only from a captured session report. It is display data, not a rules
/// authority; action choices must still be validated and applied by <see cref="CastleSiegeTabletopSession"/>.
/// </remarks>
public sealed record CastleSiegeHallView(
    string ScenarioKey,
    int ScenarioVersion,
    string RulesVersion,
    int TurnNumber,
    CastleSiegeRole ActiveRole,
    int WallIntegrity,
    int MaximumWallIntegrity,
    IReadOnlyList<bool> WallIntegrityPips,
    int AttackerShotsRemaining,
    int DefenderRepairsRemaining,
    CastleSiegeOutcome Outcome,
    IReadOnlyList<CastleSiegeActionKind> LegalActions,
    IReadOnlyList<CastleSiegeTabletopEvent> Events,
    string EvidenceBoundary);

/// <summary>Projects a stable tabletop report into a presentation-ready Wargaming Hall view.</summary>
public static class CastleSiegeHallViewModel
{
    public const string TabletopEvidenceBoundary =
        "Tabletop rules only; no physical trajectory, intersection, impact, or damage was calculated.";

    /// <summary>Creates a stable view snapshot without mutating the report or underlying session.</summary>
    public static CastleSiegeHallView FromReport(CastleSiegeSessionReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var integrity = Math.Clamp(report.State.WallIntegrity, 0, CastleSiegeTabletopSession.InitialWallIntegrity);
        var pips = Enumerable.Range(0, CastleSiegeTabletopSession.InitialWallIntegrity)
            .Select(index => index < integrity)
            .ToArray();

        return new CastleSiegeHallView(
            report.ScenarioKey,
            report.ScenarioVersion,
            report.RulesVersion,
            report.State.TurnNumber,
            report.State.ActiveRole,
            report.State.WallIntegrity,
            CastleSiegeTabletopSession.InitialWallIntegrity,
            Array.AsReadOnly(pips),
            report.State.AttackerShotsRemaining,
            report.State.DefenderRepairsRemaining,
            report.Outcome,
            Array.AsReadOnly(report.LegalActions.ToArray()),
            Array.AsReadOnly(report.Events.ToArray()),
            TabletopEvidenceBoundary);
    }
}
