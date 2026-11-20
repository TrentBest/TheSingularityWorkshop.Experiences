using TheSingularityWorkshop.Experiences.Warlords;

var map = WarlordsMap.CreateStarterMap();
var game = new WarlordsGame(map, seed: 2026);

Console.WriteLine("THE SINGULARITY WORKSHOP — WARLORDS");
Console.WriteLine("Four Kingdoms / first playable rules shell");
Console.WriteLine("Commands: status | march <source-id> <destination-id> <armies> | end | help | quit");
Console.WriteLine("All four kingdoms are human-controlled in this first local proof.");
PrintStatus();

while (true)
{
    Console.Write($"{game.ActiveKingdom} (round {game.Round})> ");
    var input = Console.ReadLine();
    if (input is null) break;

    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    if (parts.Length == 0) continue;

    try
    {
        switch (parts[0].ToLowerInvariant())
        {
            case "quit":
                return;
            case "help":
                Console.WriteLine("status — show the current map and legal marches");
                Console.WriteLine("march <source-id> <destination-id> <armies> — commit armies to an adjacent territory");
                Console.WriteLine("end — end this kingdom's turn and recruit at its strongholds");
                Console.WriteLine("quit — leave the local match");
                break;
            case "status":
                PrintStatus();
                break;
            case "end":
                game.EndTurn();
                PrintStatus();
                break;
            case "march" when parts.Length == 4 && int.TryParse(parts[3], out var armies):
                var entry = game.March(new WarlordsMarch(parts[1], parts[2], armies));
                Console.WriteLine($"{entry.BattleOutcome}: attack roll {entry.AttackerRoll}, defence roll {entry.DefenderRoll}.");
                Console.WriteLine($"{entry.SourceAfter.TerritoryId}: {entry.SourceAfter.ArmyCount} armies.");
                Console.WriteLine($"{entry.DestinationAfter.TerritoryId}: {entry.DestinationAfter.Owner?.ToString() ?? "Neutral"}, {entry.DestinationAfter.ArmyCount} armies.");
                if (game.Winner is { } winner) Console.WriteLine($"{winner} controls three strongholds and wins!");
                break;
            default:
                Console.WriteLine("Command not recognized. Type help.");
                break;
        }
    }
    catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
    {
        Console.WriteLine($"Rejected: {exception.Message}");
    }
}

void PrintStatus()
{
    var snapshot = game.Capture();
    Console.WriteLine($"Round {snapshot.Round} — {snapshot.ActiveKingdom} — {snapshot.Phase}");
    foreach (var territory in snapshot.Territories)
    {
        var definition = map.GetTerritory(territory.TerritoryId);
        var stronghold = territory.IsStronghold ? " [STRONGHOLD]" : string.Empty;
        Console.WriteLine($"  {definition.Name,-18} {territory.Owner?.ToString() ?? "Neutral",-8} {territory.ArmyCount,2} armies{stronghold}  ({territory.TerritoryId})");
    }

    var legal = game.GetLegalMarches();
    Console.WriteLine($"Legal marches: {legal.Count}");
    foreach (var group in legal.GroupBy(move => (move.SourceTerritoryId, move.DestinationTerritoryId)))
    {
        var counts = string.Join(",", group.Select(move => move.ArmyCount));
        Console.WriteLine($"  {group.Key.SourceTerritoryId} -> {group.Key.DestinationTerritoryId}: [{counts}]");
    }
}
