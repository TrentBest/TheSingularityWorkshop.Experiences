using TheSingularityWorkshop.Experiences.DroneConstruction;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class ConstructionSnapshotSvgRendererTests
{
    [Fact]
    public void Render_UsesCommittedSnapshotValuesAndEscapesNoDynamicMarkup()
    {
        var agent = new ConstructionAgent(7, new GridCell(0, 0));
        var simulation = new ConstructionSimulation(
            width: 3,
            height: 1,
            requiredHeight: (x, _) => x == 2 ? 1 : 0,
            agents: [agent],
            emitters: [new PositionedBlockEmitter(new GridCell(0, 0), new BlockEmitter("emitter", 1))]);

        var initialSvg = ConstructionSnapshotSvgRenderer.Render(simulation.Snapshot(), 3, 1);
        Assert.Contains("TICK 0", initialSvg);
        Assert.Contains("0/1 blocks placed", initialSvg);
        Assert.Contains("Emitter inventory", initialSvg);
        Assert.Contains("Agent", initialSvg);

        ConstructionSimulationSnapshot snapshot = simulation.Snapshot();
        for (var i = 0; i < 12 && !snapshot.IsComplete; i++)
            snapshot = simulation.Step();

        var completedSvg = ConstructionSnapshotSvgRenderer.Render(snapshot, 3, 1);
        Assert.Contains($"TICK {snapshot.Tick}", completedSvg);
        Assert.Contains("1/1 blocks placed", completedSvg);
        Assert.Contains("COMPLETE", completedSvg);
        Assert.Contains("fill=\"#245b55\"", completedSvg);
        Assert.Contains("fill=\"#49d6a2\"", completedSvg);
        Assert.DoesNotContain("TICK 0", completedSvg);
    }

    [Fact]
    public void Render_RejectsSnapshotWhenGridDimensionsDoNotMatch()
    {
        var simulation = new ConstructionSimulation(
            width: 2,
            height: 1,
            requiredHeight: (_, _) => 0,
            agents: [],
            emitters: []);

        Assert.Throws<ArgumentException>(() =>
            ConstructionSnapshotSvgRenderer.Render(simulation.Snapshot(), 3, 1));
    }

    [Fact]
    public void Render_RejectsInvalidCanvasDimensions()
    {
        var simulation = new ConstructionSimulation(
            width: 1,
            height: 1,
            requiredHeight: (_, _) => 0,
            agents: [],
            emitters: []);
        var snapshot = simulation.Snapshot();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ConstructionSnapshotSvgRenderer.Render(snapshot, 0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ConstructionSnapshotSvgRenderer.Render(snapshot, 1, 1, cellSize: 8));
    }
}
