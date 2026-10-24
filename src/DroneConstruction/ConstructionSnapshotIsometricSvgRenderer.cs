namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>
/// Projects committed construction snapshots into a browser-friendly isometric SVG scene.
/// This is a 2.5D presentation projection, not a rigid-body flight or collision simulation.
/// </summary>
public static class ConstructionSnapshotIsometricSvgRenderer
{
    /// <summary>Renders a snapshot with stacked block geometry and elevated lane markers.</summary>
    public static string Render(
        ConstructionSimulationSnapshot snapshot,
        int width,
        int height,
        string spaceId = "construction-main",
        int cellSize = 32,
        int blockHeight = 12)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (string.IsNullOrWhiteSpace(spaceId)) throw new ArgumentException("A stable simulation-space identity is required.", nameof(spaceId));
        if (cellSize < 12) throw new ArgumentOutOfRangeException(nameof(cellSize));
        if (blockHeight < 4) throw new ArgumentOutOfRangeException(nameof(blockHeight));
        if (snapshot.CellPlacedBlocks.Count != checked(width * height))
            throw new ArgumentException("Snapshot cell data must match the supplied grid dimensions.", nameof(snapshot));

        var half = cellSize / 2.0;
        var sceneWidth = (width + height) * half + 160;
        var sceneHeight = (width + height) * half / 2.0 + 5 * blockHeight + 190;
        var originX = sceneWidth / 2.0;
        var originY = 90.0 + height * half / 2.0;
        var output = new System.Text.StringBuilder();
        output.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" role=\"img\" aria-labelledby=\"title description\" viewBox=\"0 0 {Math.Ceiling(sceneWidth)} {Math.Ceiling(sceneHeight)}\" width=\"100%\" height=\"100%\">");
        output.AppendLine("<title id=\"title\">Drone construction isometric web view</title>");
        output.AppendLine($"<desc id=\"description\">Tick {snapshot.Tick}; {snapshot.PlacedBlocks} of {snapshot.RequiredBlocks} blocks placed. Isometric block stacks and lane elevation markers are projected from a committed snapshot.</desc>");
        output.AppendLine("<rect width=\"100%\" height=\"100%\" rx=\"12\" fill=\"#101923\"/>");
        output.AppendLine($"<text x=\"24\" y=\"30\" fill=\"#f4f7fb\" font-family=\"sans-serif\" font-size=\"17\" font-weight=\"700\">ISOMETRIC DRONE CONSTRUCTION · TICK {snapshot.Tick}</text>");
        output.AppendLine($"<text x=\"24\" y=\"52\" fill=\"#a9bacb\" font-family=\"sans-serif\" font-size=\"11\">{snapshot.PlacedBlocks}/{snapshot.RequiredBlocks} blocks · {snapshot.Agents.Count} agents · max target stack 5</text>");

        (double X, double Y) Project(double x, double y, double z = 0) =>
            (originX + (x - y) * half, originY + (x + y) * half / 2.0 - z * blockHeight);

        // Draw the floor diamonds first so all stack geometry reads as a single world.
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var p = Project(x, y);
            var points = $"{p.X},{p.Y} {p.X + half},{p.Y + half / 2} {p.X},{p.Y + half} {p.X - half},{p.Y + half / 2}";
            output.AppendLine($"<polygon points=\"{points}\" fill=\"#172532\" stroke=\"#34485a\" stroke-width=\"1\"/>");
        }

        // Far-to-near order reduces visual overlap for this simple isometric projection.
        for (var diagonal = 0; diagonal < width + height - 1; diagonal++)
        for (var y = 0; y < height; y++)
        {
            var x = diagonal - y;
            if ((uint)x >= (uint)width) continue;
            var count = snapshot.CellPlacedBlocks[(y * width) + x];
            for (var z = 0; z < Math.Min(5, count); z++)
            {
                var p = Project(x, y, z);
                var top = $"{p.X},{p.Y} {p.X + half},{p.Y + half / 2} {p.X},{p.Y + half} {p.X - half},{p.Y + half / 2}";
                var left = $"{p.X - half},{p.Y + half / 2} {p.X},{p.Y + half} {p.X},{p.Y + half + blockHeight} {p.X - half},{p.Y + half / 2 + blockHeight}";
                var right = $"{p.X},{p.Y + half} {p.X + half},{p.Y + half / 2} {p.X + half},{p.Y + half / 2 + blockHeight} {p.X},{p.Y + half + blockHeight}";
                output.AppendLine($"<polygon points=\"{left}\" fill=\"#23856b\" stroke=\"#101923\" stroke-width=\"1\"/>");
                output.AppendLine($"<polygon points=\"{right}\" fill=\"#17634f\" stroke=\"#101923\" stroke-width=\"1\"/>");
                output.AppendLine($"<polygon points=\"{top}\" fill=\"#49d6a2\" stroke=\"#101923\" stroke-width=\"1\"/>");
            }
        }

        foreach (var emitter in snapshot.Emitters)
        {
            if ((uint)emitter.Position.X >= (uint)width || (uint)emitter.Position.Y >= (uint)height) continue;
            var p = Project(emitter.Position.X, emitter.Position.Y, 0);
            output.AppendLine($"<circle cx=\"{p.X}\" cy=\"{p.Y + half / 2}\" r=\"{Math.Max(4, half / 3)}\" fill=\"#f2b84b\" stroke=\"#101923\" stroke-width=\"2\"/>");
            output.AppendLine($"<text x=\"{p.X}\" y=\"{p.Y + half / 2 + 3}\" text-anchor=\"middle\" fill=\"#101923\" font-family=\"sans-serif\" font-size=\"9\" font-weight=\"700\">{emitter.Inventory}</text>");
        }

        foreach (var agent in snapshot.Agents)
        {
            if (agent.Location.SpaceKind != ConstructionSpaceKind.SimulationGrid
                || !StringComparer.Ordinal.Equals(agent.Location.SpaceId, spaceId)
                || (uint)agent.Position.X >= (uint)width
                || (uint)agent.Position.Y >= (uint)height) continue;
            var lane = (int)((agent.AgentId - 1) % 4);
            var elevation = 10 - lane;
            // Lane labels are visualized as relative altitude layers, not measured flight physics.
            var relativeLayer = 4 - lane;
            var p = Project(agent.Position.X, agent.Position.Y, relativeLayer);
            var color = agent.IsCarryingBlock ? "#ff8c69" : "#75b9ff";
            output.AppendLine($"<path d=\"M {p.X - 7} {p.Y} l 7 -4 l 7 4 l -7 4 Z\" fill=\"{color}\" stroke=\"#f4f7fb\" stroke-width=\"1.5\"/>");
            output.AppendLine($"<text x=\"{p.X + 9}\" y=\"{p.Y - 4}\" fill=\"#f2b84b\" stroke=\"#101923\" stroke-width=\"2\" paint-order=\"stroke\" font-family=\"sans-serif\" font-size=\"10\" font-weight=\"800\">E{elevation}</text>");
        }

        output.AppendLine($"<text x=\"24\" y=\"{sceneHeight - 44}\" fill=\"#c4d0dc\" font-family=\"sans-serif\" font-size=\"11\">Isometric projection · grid {width} × {height} · lane elevations 10 / 9 / 8 / 7 · build stacks capped visually at 5</text>");
        output.AppendLine($"<text x=\"24\" y=\"{sceneHeight - 25}\" fill=\"#7890a5\" font-family=\"sans-serif\" font-size=\"10\">Presentation only: current simulation remains discrete-grid; no real-time 3D flight dynamics are implied.</text>");
        output.AppendLine("</svg>");
        return output.ToString();
    }
}
