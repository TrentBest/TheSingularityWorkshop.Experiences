using System.Globalization;
using System.Security;

namespace TheSingularityWorkshop.Experiences.CapsuleClinic;

/// <summary>Renders an authoritative four-pane Capsule Clinic arena snapshot as standalone SVG.</summary>
/// <remarks>
/// The renderer is a pure presentation adapter: it does not advance the simulation, accept input,
/// establish network connections, or mutate the supplied snapshot. The center standings rail is
/// deliberately wider than a puzzle jar and remains visually distinct from the playfields.
/// </remarks>
public static class CapsuleArenaSvgRenderer
{
    private const int Width = 1000;
    private const int Height = 820;
    private const int PaneWidth = 450;
    private const int PaneHeight = 380;
    private const int LeftX = 24;
    private const int RightX = 526;
    private const int TopY = 78;
    private const int BottomY = 430;

    /// <summary>Renders the current local player's selected panes and authoritative standings.</summary>
    public static string Render(CapsuleArenaSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (snapshot.Panes.Count != 4)
            throw new ArgumentException("An arena snapshot must contain exactly four pane assignments.", nameof(snapshot));
        if (snapshot.BoardWidth < 1 || snapshot.BoardHeight < 1)
            throw new ArgumentException("Arena board dimensions must be positive.", nameof(snapshot));

        var output = new System.Text.StringBuilder();
        output.AppendLine($"""<svg xmlns="http://www.w3.org/2000/svg" role="img" aria-labelledby="arena-title arena-description" viewBox="0 0 {Width} {Height}" width="{Width}" height="{Height}">""");
        output.AppendLine("""<title id="arena-title">Capsule Clinic arena</title>""");
        output.AppendLine($"""<desc id="arena-description">Local player {Escape(snapshot.LocalPlayerId)}. Four selected player panes and current round standings.</desc>""");
        output.AppendLine("""<rect width="100%" height="100%" rx="18" fill="#101923"/>""");
        output.AppendLine("""<text x="24" y="34" fill="#f4f7fb" font-family="sans-serif" font-size="24" font-weight="800" letter-spacing="2">CAPSULE CLINIC</text>""");
        output.AppendLine("""<text x="24" y="56" fill="#9fb3c8" font-family="sans-serif" font-size="12">PUZZLE ARENA · AUTHORITATIVE SNAPSHOT</text>""");

        foreach (var pane in snapshot.Panes)
        {
            var (x, y) = pane.Pane switch
            {
                ArenaPane.LocalUpperLeft => (LeftX, TopY),
                ArenaPane.ComparisonUpperRight => (RightX, TopY),
                ArenaPane.ComparisonLowerLeft => (LeftX, BottomY),
                ArenaPane.ComparisonLowerRight => (RightX, BottomY),
                _ => throw new ArgumentOutOfRangeException(nameof(snapshot), "Unknown arena pane position.")
            };
            RenderPane(output, pane, x, y, snapshot.BoardWidth, snapshot.BoardHeight);
        }

        // A dedicated central rail separates the two columns and is wider than either jar.
        output.AppendLine("""<rect x="474" y="72" width="52" height="742" rx="16" fill="#263748" stroke="#3b5267" stroke-width="1"/>""");
        output.AppendLine("""<text x="500" y="100" fill="#f2b84b" font-family="sans-serif" font-size="10" font-weight="800" text-anchor="middle" transform="rotate(90 500 100)">STANDINGS</text>""");
        output.AppendLine("</svg>");
        return output.ToString();
    }

    private static void RenderPane(System.Text.StringBuilder output, CapsuleArenaPaneView pane, int x, int y, int boardWidth, int boardHeight)
    {
        output.AppendLine($"""<rect x="{x}" y="{y}" width="{PaneWidth}" height="{PaneHeight}" rx="14" fill="#172532" stroke="{(pane.Pane == ArenaPane.LocalUpperLeft ? "#f2b84b" : "#34485a")}" stroke-width="{(pane.Pane == ArenaPane.LocalUpperLeft ? 2 : 1)}"/>""");
        if (pane.Standing is null || pane.Player is null)
        {
            output.AppendLine($"""<text x="{x + 24}" y="{y + 36}" fill="#8ca0b4" font-family="sans-serif" font-size="14" font-weight="700">WAITING FOR CONTENDER</text>""");
            output.AppendLine($"""<text x="{x + 24}" y="{y + 60}" fill="#64798d" font-family="sans-serif" font-size="11">This pane is available for a player.</text>""");
            return;
        }

        var standing = pane.Standing;
        var player = pane.Player;
        var label = pane.Pane == ArenaPane.LocalUpperLeft ? "YOU" : $"RANK #{standing.RoundRank}";
        output.AppendLine($"""<text x="{x + 18}" y="{y + 28}" fill="#f4f7fb" font-family="sans-serif" font-size="15" font-weight="800">{Escape(player.PlayerId)}</text>""");
        output.AppendLine($"""<text x="{x + PaneWidth - 18}" y="{y + 27}" fill="{(player.GameOver ? "#ff8c69" : "#75b9ff")}" font-family="sans-serif" font-size="10" font-weight="800" text-anchor="end">{(player.GameOver ? "TOP OUT" : Escape(label))}</text>""");
        output.AppendLine($"""<text x="{x + 18}" y="{y + 47}" fill="#9fb3c8" font-family="sans-serif" font-size="11">{standing.RoundsWon} ROUND{(standing.RoundsWon == 1 ? "" : "S")} WON</text>""");

        var jarX = (double)x + 24;
        var jarY = (double)y + 62;
        var cellSize = Math.Min(15d, Math.Min(120d / boardWidth, 240d / boardHeight));
        var jarWidth = cellSize * boardWidth;
        var jarHeight = cellSize * boardHeight;
        output.AppendLine($"""<rect x="{N(jarX - 4)}" y="{N(jarY - 4)}" width="{N(jarWidth + 8)}" height="{N(jarHeight + 8)}" rx="5" fill="#0b121a" stroke="#52697e" stroke-width="2"/>""");
        for (var boardY = 0; boardY < boardHeight; boardY++)
        for (var boardX = 0; boardX < boardWidth; boardX++)
        {
            var px = jarX + boardX * cellSize;
            var py = jarY + (boardHeight - 1 - boardY) * cellSize;
            var cell = player.Board[boardY * boardWidth + boardX];
            output.AppendLine($"""<rect x="{N(px)}" y="{N(py)}" width="{N(cellSize)}" height="{N(cellSize)}" fill="{(cell.HasValue ? Color(cell.Value.Color) : "#152330")}" stroke="#273949" stroke-width="0.7"/>""");
            if (cell is { IsVirus: true } virus)
            {
                output.AppendLine($"""<circle cx="{N(px + cellSize / 2)}" cy="{N(py + cellSize / 2)}" r="{N(cellSize * 0.28)}" fill="{Color(virus.Color)}" stroke="#f4f7fb" stroke-width="0.8"/>""");
                output.AppendLine($"""<circle cx="{N(px + cellSize * 0.27)}" cy="{N(py + cellSize * 0.33)}" r="{N(cellSize * 0.07)}" fill="#101923"/><circle cx="{N(px + cellSize * 0.67)}" cy="{N(py + cellSize * 0.33)}" r="{N(cellSize * 0.07)}" fill="#101923"/>""");
            }
        }

        if (player.Active is { } active)
        {
            DrawActive(output, jarX, jarY, active.PivotX, active.PivotY, boardWidth, boardHeight, cellSize, Color(active.Pair.PivotColor));
            var partnerX = active.PivotX + (active.Orientation == CapsuleOrientation.Horizontal ? 1 : 0);
            var partnerY = active.PivotY + (active.Orientation == CapsuleOrientation.Vertical ? 1 : 0);
            DrawActive(output, jarX, jarY, partnerX, partnerY, boardWidth, boardHeight, cellSize, Color(active.Pair.PartnerColor));
        }

        var infoX = jarX + jarWidth + 20;
        output.AppendLine($"""<text x="{N(infoX)}" y="{jarY + 24}" fill="#f2b84b" font-family="sans-serif" font-size="11" font-weight="800">ROUND RANK</text>""");
        output.AppendLine($"""<text x="{N(infoX)}" y="{jarY + 48}" fill="#f4f7fb" font-family="sans-serif" font-size="28" font-weight="800">#{standing.RoundRank}</text>""");
        output.AppendLine($"""<text x="{N(infoX)}" y="{jarY + 90}" fill="#9fb3c8" font-family="sans-serif" font-size="11">ROUNDS WON</text>""");
        output.AppendLine($"""<text x="{N(infoX)}" y="{jarY + 114}" fill="#f4f7fb" font-family="sans-serif" font-size="25" font-weight="800">{standing.RoundsWon}</text>""");
        output.AppendLine($"""<text x="{N(infoX)}" y="{jarY + 166}" fill="#9fb3c8" font-family="sans-serif" font-size="10">{(player.GameOver ? "BOARD ELIMINATED" : "IN THE ROUND")}</text>""");
    }

    private static void DrawActive(System.Text.StringBuilder output, double jarX, double jarY, int boardX, int boardY, int boardWidth, int boardHeight, double cellSize, string color)
    {
        if ((uint)boardX >= (uint)boardWidth || (uint)boardY >= (uint)boardHeight) return;
        var px = jarX + boardX * cellSize;
        var py = jarY + (boardHeight - 1 - boardY) * cellSize;
        var inset = Math.Min(1d, cellSize * 0.08);
        output.AppendLine($"""<rect x="{N(px + inset)}" y="{N(py + inset)}" width="{N(Math.Max(0.5, cellSize - inset * 2))}" height="{N(Math.Max(0.5, cellSize - inset * 2))}" rx="{N(cellSize * 0.2)}" fill="{color}" stroke="#ffffff" stroke-width="1.2"/>""");
    }

    private static string Color(CapsuleColor color) => color switch
    {
        CapsuleColor.Red => "#ff637d",
        CapsuleColor.Blue => "#56b4ff",
        CapsuleColor.Yellow => "#ffd166",
        CapsuleColor.Purple => "#bd93f9",
        _ => throw new ArgumentOutOfRangeException(nameof(color))
    };

    private static string Escape(string value) => SecurityElement.Escape(value) ?? string.Empty;

    private static string N(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}
