namespace TheSingularityWorkshop.Experiences.CastleSiege;

/// <summary>
/// Produces a plain-text inspection view of a captured Castle Siege session report.
/// </summary>
/// <remarks>
/// This is a host-facing presentation helper, not part of the rules engine. It consumes a report snapshot,
/// performs no session mutations, and makes no claims about physical simulation evidence.
/// </remarks>
public static class CastleSiegeSessionReportText
{
    /// <summary>Formats scenario identity, state, legal actions, and ordered event evidence for inspection.</summary>
    public static string Format(CastleSiegeSessionReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var lines = new List<string>
        {
            "Castle Siege — West Wall",
            $"Scenario: {report.ScenarioKey} (v{report.ScenarioVersion})",
            $"Rules: {report.RulesVersion}",
            $"Outcome: {report.Outcome}",
            $"Turn: {report.State.TurnNumber}",
            $"Active role: {report.State.ActiveRole}",
            $"Wall integrity (abstract): {report.State.WallIntegrity}/{CastleSiegeTabletopSession.InitialWallIntegrity}",
            $"Attacker shots remaining: {report.State.AttackerShotsRemaining}",
            $"Defender repairs remaining: {report.State.DefenderRepairsRemaining}",
            "Legal actions:"
        };

        if (report.LegalActions.Count == 0)
        {
            lines.Add("  (none)");
        }
        else
        {
            lines.AddRange(report.LegalActions.Select(action => $"  - {action}"));
        }

        lines.Add("Event history (abstract-tabletop):");

        if (report.Events.Count == 0)
        {
            lines.Add("  (no accepted actions)");
        }
        else
        {
            foreach (var entry in report.Events)
            {
                lines.Add($"  #{entry.Sequence} | turn {entry.TurnNumber} | {entry.Actor} | {entry.Action}");
                lines.Add($"    Rule: {entry.RuleId}");
                lines.Add($"    State: wall {entry.StateBefore.WallIntegrity} -> {entry.StateAfter.WallIntegrity}; " +
                           $"shots {entry.StateBefore.AttackerShotsRemaining} -> {entry.StateAfter.AttackerShotsRemaining}; " +
                           $"repairs {entry.StateBefore.DefenderRepairsRemaining} -> {entry.StateAfter.DefenderRepairsRemaining}");
                lines.Add($"    Outcome: {entry.StateAfter.Outcome}");
                lines.Add($"    {entry.Explanation}");
            }
        }

        lines.Add("Evidence boundary: tabletop rules only; no physical trajectory, intersection, impact, or damage was calculated.");
        return string.Join(Environment.NewLine, lines);
    }
}
