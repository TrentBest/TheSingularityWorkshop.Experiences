namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>
/// Renders a committed construction snapshot as standalone SVG without mutating simulation state.
/// This is a renderer-neutral bridge; it does not schedule ticks or interpolate agent motion.
/// </summary>
public static class ConstructionSnapshotSvgRenderer
{
    /// <summary>Renders one immutable snapshot on a bounded grid.</summary>
    /// <param name="snapshot">The committed simulation snapshot to portray.</param>
    /// <param name="width">Grid width used by the simulation that produced the snapshot.</param>
    /// <param name="height">Grid height used by the simulation that produced the snapshot.</param>
    /// <param name="spaceId">Stable identity of the simulation-grid space whose agents should be shown.</param>
    /// <param name="cellSize">Pixel width and height of each grid cell.</param>
    public static string Render(ConstructionSimulationSnapshot snapshot, int width, int height, string spaceId = "construction-main", int cellSize = 48)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (string.IsNullOrWhiteSpace(spaceId)) throw new ArgumentException("A stable simulation-space identity is required.", nameof(spaceId));
        if (cellSize < 16) throw new ArgumentOutOfRangeException(nameof(cellSize), "Cell size must be at least 16 pixels.");
        if (snapshot.CellPlacedBlocks.Count != checked(width * height))
            throw new ArgumentException("Snapshot cell data must match the supplied grid dimensions.", nameof(snapshot));

        var margin = 24;
        var header = 58;
        var footer = 36;
        var drawingWidth = checked(width * cellSize);
        var drawingHeight = checked(height * cellSize);
        var svgWidth = checked(drawingWidth + margin * 2);
        var svgHeight = checked(drawingHeight + header + footer + margin);
        var output = new System.Text.StringBuilder();
        output.AppendLine($"""<svg xmlns="http://www.w3.org/2000/svg" role="img" aria-labelledby="title description" viewBox="0 0 {svgWidth} {svgHeight}" width="{svgWidth}" height="{svgHeight}">""");
        output.AppendLine("""<title id="title">Drone construction committed snapshot</title>""");
        output.AppendLine($"""<desc id="description">Tick {snapshot.Tick}. {snapshot.PlacedBlocks} of {snapshot.RequiredBlocks} blocks placed. {snapshot.Agents.Count} agents.</desc>""");
        output.AppendLine("""<rect width="100%" height="100%" rx="12" fill="#101923"/>""");
        output.AppendLine($"""<text x="{margin}" y="29" fill="#f4f7fb" font-family="sans-serif" font-size="17" font-weight="700">DRONE CONSTRUCTION · TICK {snapshot.Tick}</text>""");
        output.AppendLine($"""<text x="{margin}" y="49" fill="#a9bacb" font-family="sans-serif" font-size="11">{snapshot.PlacedBlocks}/{snapshot.RequiredBlocks} blocks placed · {snapshot.Agents.Count} agents · {(snapshot.IsComplete ? "COMPLETE" : "IN PROGRESS")}</text>""");

        var originY = header;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var px = margin + x * cellSize;
            var py = originY + y * cellSize;
            var placed = snapshot.CellPlacedBlocks[(y * width) + x];
            output.AppendLine($"""<rect x="{px}" y="{py}" width="{cellSize}" height="{cellSize}" fill="{(placed > 0 ? "#245b55" : "#172532")}" stroke="#34485a" stroke-width="1"/>""");
            if (placed > 0)
            {
                var blockSize = Math.Max(6, cellSize - 12);
                output.AppendLine($"""<rect x="{px + 6}" y="{py + 6}" width="{blockSize}" height="{blockSize}" rx="3" fill="#49d6a2"/>""");
                output.AppendLine($"""<text x="{px + cellSize / 2}" y="{py + cellSize / 2 + 4}" text-anchor="middle" fill="#09241e" font-family="sans-serif" font-size="12" font-weight="700">{placed}</text>""");
            }
        }

        foreach (var emitter in snapshot.Emitters)
        {
            if ((uint)emitter.Position.X >= (uint)width || (uint)emitter.Position.Y >= (uint)height) continue;
            var cx = margin + emitter.Position.X * cellSize + cellSize / 2;
            var cy = originY + emitter.Position.Y * cellSize + cellSize / 2;
            output.AppendLine($"""<circle cx="{cx}" cy="{cy}" r="{Math.Max(5, cellSize / 5)}" fill="#f2b84b" stroke="#101923" stroke-width="2"/>""");
            output.AppendLine($"""<text x="{cx}" y="{cy + 3}" text-anchor="middle" fill="#101923" font-family="sans-serif" font-size="10" font-weight="700">{emitter.Inventory}</text>""");
        }

        foreach (var agent in snapshot.Agents)
        {
            // Coordinates are local to a qualified space; do not project a lane agent onto this grid.
            if (agent.Location.SpaceKind != ConstructionSpaceKind.SimulationGrid
                || !StringComparer.Ordinal.Equals(agent.Location.SpaceId, spaceId)
                || (uint)agent.Position.X >= (uint)width
                || (uint)agent.Position.Y >= (uint)height) continue;
            var cx = margin + agent.Position.X * cellSize + cellSize / 2;
            var cy = originY + agent.Position.Y * cellSize + cellSize / 2;
            var color = agent.IsCarryingBlock ? "#ff8c69" : "#75b9ff";
            output.AppendLine($"""<circle cx="{cx}" cy="{cy}" r="{Math.Max(4, cellSize / 6)}" fill="{color}" stroke="#f4f7fb" stroke-width="2"/>""");
            output.AppendLine($"""<text x="{cx}" y="{cy + 3}" text-anchor="middle" fill="#101923" font-family="sans-serif" font-size="9" font-weight="700">{agent.AgentId}</text>""");
        }

        var legendY = originY + drawingHeight + 23;
        output.AppendLine($"""<circle cx="{margin + 5}" cy="{legendY - 4}" r="5" fill="#f2b84b"/><text x="{margin + 15}" y="{legendY}" fill="#c4d0dc" font-family="sans-serif" font-size="10">Emitter inventory</text>""");
        output.AppendLine($"""<circle cx="{margin + 132}" cy="{legendY - 4}" r="5" fill="#75b9ff"/><text x="{margin + 142}" y="{legendY}" fill="#c4d0dc" font-family="sans-serif" font-size="10">Agent</text>""");
        output.AppendLine($"""<circle cx="{margin + 207}" cy="{legendY - 4}" r="5" fill="#ff8c69"/><text x="{margin + 217}" y="{legendY}" fill="#c4d0dc" font-family="sans-serif" font-size="10">Agent carrying block</text>""");
        output.AppendLine("</svg>");
        return output.ToString();
    }
}
