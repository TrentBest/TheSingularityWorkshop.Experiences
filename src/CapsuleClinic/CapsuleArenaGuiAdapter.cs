using TheSingularityWorkshop.Workshop.Gui;

namespace TheSingularityWorkshop.Experiences.CapsuleClinic;

/// <summary>
/// Projects an authoritative arena snapshot into the platform-neutral GUI semantic tree.
/// This adapter does not own game state or execute command identifiers.
/// </summary>
public static class CapsuleArenaGuiAdapter
{
    /// <summary>Builds the semantic arena tree from the supplied authoritative snapshot.</summary>
    public static GuiNode Build(CapsuleArenaSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var root = GuiBuilder.Create("Panel", "capsule-clinic-arena")
            .Property("experience", "capsule-clinic")
            .Property("localPlayerId", snapshot.LocalPlayerId)
            .Property("boardWidth", snapshot.BoardWidth.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .Property("boardHeight", snapshot.BoardHeight.ToString(System.Globalization.CultureInfo.InvariantCulture));

        foreach (var pane in snapshot.Panes)
        {
            var paneId = PaneId(pane.Pane);
            var panel = GuiBuilder.Create("Panel", paneId)
                .Property("pane", pane.Pane.ToString());

            if (pane.Player is null || pane.Standing is null)
            {
                panel.Child("Text", $"{paneId}-empty", child => child.Text("No contender"));
            }
            else
            {
                var player = pane.Player;
                var standing = pane.Standing;
                var occupied = player.Board.Count(cell => cell is not null);
                panel.Child("Text", $"{paneId}-player", child => child.Text(player.PlayerId));
                panel.Child("Text", $"{paneId}-rank", child =>
                    child.Text($"Rank {standing.RoundRank} · {standing.RoundsWon} rounds won"));
                panel.Child("Text", $"{paneId}-board-status", child =>
                    child.Text(player.GameOver ? "Game over" : $"{occupied} occupied cells"));
                panel.Property("playerId", player.PlayerId)
                    .Property("rank", standing.RoundRank.ToString(System.Globalization.CultureInfo.InvariantCulture))
                    .Property("roundsWon", standing.RoundsWon.ToString(System.Globalization.CultureInfo.InvariantCulture))
                    .Property("gameOver", player.GameOver ? "true" : "false")
                    .Property("lastAcceptedSequence", player.LastAcceptedSequence.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            root.Child(panel);
        }

        var controls = GuiBuilder.Create("Panel", "capsule-clinic-local-controls")
            .Child("Button", "capsule-move-left", child =>
                child.Text("Move left").WithOnClick(CapsuleClinicIntents.MoveLeft))
            .Child("Button", "capsule-move-right", child =>
                child.Text("Move right").WithOnClick(CapsuleClinicIntents.MoveRight))
            .Child("Button", "capsule-rotate", child =>
                child.Text("Rotate").WithOnClick(CapsuleClinicIntents.Rotate))
            .Child("Button", "capsule-soft-drop", child =>
                child.Text("Soft drop").WithOnClick(CapsuleClinicIntents.SoftDrop))
            .Child("Button", "capsule-hard-drop", child =>
                child.Text("Hard drop").WithOnClick(CapsuleClinicIntents.HardDrop));

        root.Child(controls);
        return root.Build();
    }

    private static string PaneId(ArenaPane pane) => pane switch
    {
        ArenaPane.LocalUpperLeft => "capsule-pane-local",
        ArenaPane.ComparisonUpperRight => "capsule-pane-comparison-1",
        ArenaPane.ComparisonLowerLeft => "capsule-pane-comparison-2",
        ArenaPane.ComparisonLowerRight => "capsule-pane-comparison-3",
        _ => throw new ArgumentOutOfRangeException(nameof(pane), pane, "Unknown arena pane.")
    };
}
