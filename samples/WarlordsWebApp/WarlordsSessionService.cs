using TheSingularityWorkshop.Experiences.Warlords;
using TheSingularityWorkshop.Experiences.Warlords.Runtime;
using TheSingularityWorkshop.Workshop.Gui;

namespace WarlordsWebApp;

/// <summary>
/// Owns the local sample match and projects its authoritative snapshot into
/// the renderer-neutral GUI tree. Every incoming command is checked against
/// the current domain legal-command set before it is applied.
/// </summary>
public sealed class WarlordsSessionService : IDisposable
{
    private readonly WarlordsMap _map = WarlordsMap.CreateStarterMap();
    private readonly WarlordsMatchRuntime _runtime;

    public WarlordsSessionService()
    {
        _runtime = new WarlordsMatchRuntime(_map, seed: 2026);
        Message = "Match ready. Choose a legal march or end the active kingdom's turn.";
    }

    public event Action? Changed;

    public WarlordsGameSnapshot Snapshot => _runtime.Snapshot;
    public string LifecycleState => _runtime.LifecycleState;
    public string Message { get; private set; }

    public GuiNode BuildTree()
    {
        var snapshot = Snapshot;
        var root = GuiBuilder.Create("Panel", "warlords-root")
            .Property("class", "warlords-board");

        root.Child("Panel", "territory-grid", grid =>
        {
            grid.Property("class", "territory-grid");
            foreach (var territory in snapshot.Territories)
            {
                var definition = _map.GetTerritory(territory.TerritoryId);
                var owner = territory.Owner?.ToString() ?? "Neutral";
                var ownerClass = territory.Owner is null
                    ? "neutral"
                    : territory.Owner.Value.ToString().ToLowerInvariant();

                grid.Child("Panel", $"territory-{territory.TerritoryId}", card =>
                {
                    card.Property("class", $"territory-card {ownerClass}");
                    card.Child("Text", $"territory-{territory.TerritoryId}-name",
                        text => text.Text(definition.Name).Property("class", "territory-name"));
                    card.Child("Text", $"territory-{territory.TerritoryId}-owner",
                        text => text.Text(owner).Property("class", "territory-owner"));
                    card.Child("Text", $"territory-{territory.TerritoryId}-armies",
                        text => text.Text($"{territory.ArmyCount} armies").Property("class", "territory-armies"));
                    if (territory.IsStronghold)
                        card.Child("Text", $"territory-{territory.TerritoryId}-stronghold",
                            text => text.Text("STRONGHOLD").Property("class", "territory-stronghold"));
                });
            }
        });

        root.Child("Panel", "command-panel", panel =>
        {
            panel.Property("class", "command-panel");
            panel.Child("Text", "command-heading",
                text => text.Text("LEGAL MARCHES").Property("class", "command-heading"));

            var legalMarches = _runtime.GetLegalMarches();
            if (legalMarches.Count == 0)
            {
                panel.Child("Text", "no-legal-marches",
                    text => text.Text("No march is available. End the turn to continue.").Property("class", "empty-state"));
            }
            else
            {
                for (var i = 0; i < legalMarches.Count; i++)
                {
                    var move = legalMarches[i];
                    var command = $"march|{move.SourceTerritoryId}|{move.DestinationTerritoryId}|{move.ArmyCount}";
                    panel.Child("Button", $"march-option-{i}",
                        button => button
                            .Text($"{move.SourceTerritoryId} → {move.DestinationTerritoryId} · {move.ArmyCount}")
                            .WithOnClick(command)
                            .Property("class", "march-button"));
                }
            }
        });

        if (snapshot.Phase == WarlordsPhase.Command)
        {
            root.Child("Button", "end-turn",
                button => button
                    .Text($"End {snapshot.ActiveKingdom} turn")
                    .WithOnClick("end-turn")
                    .Property("class", "end-turn-button"));
        }
        else
        {
            root.Child("Text", "match-winner",
                text => text.Text($"{snapshot.Winner} wins the match.").Property("class", "match-winner"));
        }

        return root.Build();
    }

    public void Dispatch(string command)
    {
        try
        {
            if (string.Equals(command, "end-turn", StringComparison.Ordinal))
            {
                _runtime.EndTurn();
                Message = $"Turn advanced to {_runtime.Snapshot.ActiveKingdom}.";
            }
            else
            {
                var parts = command.Split('|');
                if (parts.Length != 4 ||
                    !string.Equals(parts[0], "march", StringComparison.Ordinal) ||
                    !int.TryParse(parts[3], out var armyCount))
                    throw new InvalidOperationException("The requested GUI command is not allow-listed.");

                var move = new WarlordsMarch(parts[1], parts[2], armyCount);
                if (!_runtime.GetLegalMarches().Contains(move))
                    throw new InvalidOperationException("That march is no longer legal in the current match state.");

                var result = _runtime.March(move);
                Message = $"{result.Actor}: {result.BattleOutcome} at {result.DestinationTerritoryId} (attack {result.AttackerRoll}, defence {result.DefenderRoll}).";
                if (_runtime.Snapshot.Winner is { } winner)
                    Message = $"{Message} {winner} wins!";
            }
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            Message = $"Command rejected: {exception.Message}";
        }

        Changed?.Invoke();
    }

    public void Dispose() => _runtime.Dispose();
}
