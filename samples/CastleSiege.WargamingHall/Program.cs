using TheSingularityWorkshop.Experiences.CastleSiege;

Console.OutputEncoding = System.Text.Encoding.UTF8;
var session = new CastleSiegeTabletopSession();

Console.WriteLine("THE SINGULARITY WORKSHOP  /  WARGAMING HALL");
Console.WriteLine("CASTLE SIEGE — WEST WALL");
Console.WriteLine("A deterministic tabletop demonstration. No physical trajectory or impact is simulated.");
Console.WriteLine();

while (session.State.Outcome == CastleSiegeOutcome.InProgress)
{
    var report = CastleSiegeSessionReport.Capture(session);
    var view = CastleSiegeHallViewModel.FromReport(report);
    PrintState(view);
    var legalActions = view.LegalActions;

    Console.WriteLine($"Active role: {view.ActiveRole}");
    for (var i = 0; i < legalActions.Count; i++)
    {
        Console.WriteLine($"  {i + 1}. {Describe(legalActions[i])}");
    }

    Console.Write("Choose an action by number, or Q to leave the hall: ");
    var input = Console.ReadLine()?.Trim();

    if (string.Equals(input, "q", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine();
        Console.WriteLine("Session paused by the visitor. No further action was applied.");
        break;
    }

    if (!int.TryParse(input, out var choice) || choice < 1 || choice > legalActions.Count)
    {
        Console.WriteLine("Please choose one of the listed legal actions.");
        Console.WriteLine();
        continue;
    }

    var action = new CastleSiegeAction(view.ActiveRole, legalActions[choice - 1]);
    var entry = session.Apply(action);
    Console.WriteLine();
    Console.WriteLine($"EVENT {entry.Sequence:00} · {entry.RuleId}");
    Console.WriteLine(entry.Explanation);
    Console.WriteLine($"Wall integrity: {entry.StateBefore.WallIntegrity} → {entry.StateAfter.WallIntegrity}");
    Console.WriteLine();
}

Console.WriteLine("AFTER-ACTION REPORT");
Console.WriteLine("===================");
Console.WriteLine(CastleSiegeSessionReportText.Format(CastleSiegeSessionReport.Capture(session)));

static void PrintState(CastleSiegeHallView view)
{
    var pips = string.Concat(view.WallIntegrityPips.Select(alive => alive ? "[●]" : "[○]"));
    Console.WriteLine($"Scenario: {view.ScenarioKey} (v{view.ScenarioVersion})  |  Rules: {view.RulesVersion}");
    Console.WriteLine($"Turn {view.TurnNumber}  |  West wall: {pips}  {view.WallIntegrity}/{view.MaximumWallIntegrity}");
    Console.WriteLine($"Attacker shots: {view.AttackerShotsRemaining}  |  Defender repairs: {view.DefenderRepairsRemaining}");
    Console.WriteLine($"Evidence: {view.EvidenceBoundary}");
    Console.WriteLine();
}

static string Describe(CastleSiegeActionKind action) => action switch
{
    CastleSiegeActionKind.FireAtWestWall => "Fire at the west wall (abstract tabletop action)",
    CastleSiegeActionKind.ReinforceWestWall => "Reinforce the west wall (spend one repair token)",
    CastleSiegeActionKind.HoldPosition => "Hold position",
    _ => action.ToString()
};
