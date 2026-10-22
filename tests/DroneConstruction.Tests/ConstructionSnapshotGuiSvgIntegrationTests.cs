#if WORKSHOP_USE_REPOSITORY_SOURCES
using System.Globalization;
using TheSingularityWorkshop.Workshop.Gui;
using TheSingularityWorkshop.Workshop.Gui.Svg;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

/// <summary>
/// Proves the optional GUI.SVG boundary can manifest actual committed simulation snapshots.
/// This test is compiled only by the source-graph CI job; the simulation package stays renderer-independent.
/// </summary>
public sealed class ConstructionSnapshotGuiSvgIntegrationTests
{
    [Fact]
    public void Render_ManifestsCommittedSimulationChangesWithoutInventingMotion()
    {
        var simulation = new ConstructionSimulation(
            width: 3,
            height: 1,
            requiredHeight: (x, _) => x == 2 ? 1 : 0,
            agents: [new ConstructionAgent(7, new GridCell(0, 0))],
            emitters: [new PositionedBlockEmitter(new GridCell(0, 0), new BlockEmitter("emitter", 1))]);

        var initial = simulation.Snapshot();
        var initialSvg = RenderSnapshot(initial, width: 3, height: 1);
        Assert.Contains($"TICK {initial.Tick}", initialSvg);
        Assert.Contains("0/1 BLOCKS PLACED", initialSvg);
        Assert.Contains("agent-7", initialSvg);

        ConstructionSimulationSnapshot committed = initial;
        for (var i = 0; i < 12 && !committed.IsComplete; i++)
            committed = simulation.Step();

        Assert.True(committed.IsComplete);
        Assert.True(committed.Tick > initial.Tick);

        var completedSvg = RenderSnapshot(committed, width: 3, height: 1);
        Assert.Contains($"TICK {committed.Tick}", completedSvg);
        Assert.Contains("1/1 BLOCKS PLACED", completedSvg);
        Assert.Contains("block-2-0", completedSvg);
        Assert.DoesNotContain($"TICK {initial.Tick}", completedSvg);
    }


    [Fact]
    public void Observer_PublishesCommittedTicksToSvgAndRejectsDelayedSnapshots()
    {
        var simulation = new ConstructionSimulation(
            width: 3,
            height: 1,
            requiredHeight: (x, _) => x == 2 ? 1 : 0,
            agents: [new ConstructionAgent(7, new GridCell(0, 0))],
            emitters: [new PositionedBlockEmitter(new GridCell(0, 0), new BlockEmitter("emitter", 1))]);
        var observer = new ConstructionSnapshotObserver("integration-run-1");
        var initial = simulation.Snapshot();

        Assert.True(observer.TryPublish(initial));
        Assert.Equal(1, observer.Sequence);
        Assert.Equal("integration-run-1", observer.RunId);

        for (var i = 0; i < 12 && observer.LatestSnapshot is { IsComplete: false }; i++)
            Assert.True(observer.TryPublish(simulation.Step()));

        var latest = Assert.IsType<ConstructionSimulationSnapshot>(observer.LatestSnapshot);
        Assert.True(latest.IsComplete);
        Assert.True(latest.Tick > initial.Tick);
        Assert.False(observer.TryPublish(initial));
        Assert.False(observer.TryPublish(latest));
        Assert.Equal(latest.Tick, observer.LatestSnapshot!.Tick);
        Assert.Equal(latest.Tick, observer.Sequence);

        var svg = RenderSnapshot(observer.LatestSnapshot!, width: 3, height: 1);
        Assert.Contains($"TICK {latest.Tick}", svg);
        Assert.Contains("1/1 BLOCKS PLACED", svg);
        Assert.DoesNotContain($"TICK {initial.Tick}", svg);
    }

    private static string RenderSnapshot(ConstructionSimulationSnapshot snapshot, int width, int height)
    {
        const int cellSize = 64;
        const int left = 24;
        const int top = 56;
        var canvasWidth = left * 2 + width * cellSize;
        var canvasHeight = top + height * cellSize + 24;

        var scene = GuiBuilder.Create("svg-scene", "drone-construction-live")
            .Text("Drone Construction — committed simulation state")
            .Property("viewBox", $"0 0 {canvasWidth} {canvasHeight}")
            .Property("aria-label", "Drone Construction committed simulation snapshot");

        scene.Child("svg-rect", "background", n => n
            .Property("width", canvasWidth.ToString(CultureInfo.InvariantCulture))
            .Property("height", canvasHeight.ToString(CultureInfo.InvariantCulture))
            .Property("fill", "#101923"));
        AddText(scene, "tick-label", $"TICK {snapshot.Tick}", left, 24, "#f4f7fb");
        AddText(scene, "progress-label", $"{snapshot.PlacedBlocks}/{snapshot.RequiredBlocks} BLOCKS PLACED", left + 150, 24, "#a9bacb");

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var placed = snapshot.CellPlacedBlocks[(y * width) + x];
            var px = left + x * cellSize;
            var py = top + y * cellSize;
            scene.Child("svg-rect", $"cell-{x}-{y}", n => n
                .Property("x", px.ToString(CultureInfo.InvariantCulture))
                .Property("y", py.ToString(CultureInfo.InvariantCulture))
                .Property("width", cellSize.ToString(CultureInfo.InvariantCulture))
                .Property("height", cellSize.ToString(CultureInfo.InvariantCulture))
                .Property("fill", placed > 0 ? "#245b55" : "#172532")
                .Property("stroke", "#34485a"));
            if (placed > 0)
                scene.Child("svg-rect", $"block-{x}-{y}", n => n
                    .Property("x", (px + 6).ToString(CultureInfo.InvariantCulture))
                    .Property("y", (py + 6).ToString(CultureInfo.InvariantCulture))
                    .Property("width", (cellSize - 12).ToString(CultureInfo.InvariantCulture))
                    .Property("height", (cellSize - 12).ToString(CultureInfo.InvariantCulture))
                    .Property("rx", "3")
                    .Property("fill", "#49d6a2"));
        }

        foreach (var agent in snapshot.Agents)
        {
            var cx = left + agent.Position.X * cellSize + cellSize / 2;
            var cy = top + agent.Position.Y * cellSize + cellSize / 2;
            scene.Child("svg-circle", $"agent-{agent.AgentId}", n => n
                .Property("cx", cx.ToString(CultureInfo.InvariantCulture))
                .Property("cy", cy.ToString(CultureInfo.InvariantCulture))
                .Property("r", "8")
                .Property("fill", agent.IsCarryingBlock ? "#f3c969" : "#55d9ff"));
        }

        return new SvgGuiRenderer().Render(scene.Build(), canvasWidth, canvasHeight);
    }

    private static void AddText(GuiBuilder scene, string id, string text, int x, int y, string fill) =>
        scene.Child("svg-text", id, n => n.Text(text)
            .Property("x", x.ToString(CultureInfo.InvariantCulture))
            .Property("y", y.ToString(CultureInfo.InvariantCulture))
            .Property("font-size", "14")
            .Property("fill", fill));
}
#endif
