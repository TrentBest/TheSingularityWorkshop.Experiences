using TheSingularityWorkshop.Experiences.DroneConstruction;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class ConstructionSnapshotSvgViewTests
{
    [Fact]
    public void TryRefresh_RendersEachNewAcceptedSnapshotOnce()
    {
        var simulation = new ConstructionSimulation(
            width: 3,
            height: 1,
            requiredHeight: (x, _) => x == 2 ? 1 : 0,
            agents: [new ConstructionAgent(7, new GridCell(0, 0))],
            emitters: [new PositionedBlockEmitter(new GridCell(0, 0), new BlockEmitter("emitter", 1))]);
        var observer = new ConstructionSnapshotObserver("live-view-run");
        var view = new ConstructionSnapshotSvgView(observer, width: 3, height: 1);

        Assert.Equal("live-view-run", view.RunId);
        Assert.Null(view.RenderedTick);
        Assert.Equal(0, view.RenderedSequence);
        Assert.False(view.TryRefresh(out var absent));
        Assert.Null(absent);

        Assert.True(observer.TryPublish(simulation.Snapshot()));
        Assert.True(view.TryRefresh(out var initialSvg));
        Assert.NotNull(initialSvg);
        Assert.Contains("TICK 0", initialSvg);
        Assert.Equal(0, view.RenderedTick);
        Assert.Equal(1, view.RenderedSequence);

        Assert.False(view.TryRefresh(out var duplicate));
        Assert.Null(duplicate);

        ConstructionSimulationSnapshot latest = simulation.Snapshot();
        for (var i = 0; i < 12 && !latest.IsComplete; i++)
        {
            latest = simulation.Step();
            Assert.True(observer.TryPublish(latest));
        }

        Assert.True(latest.IsComplete);
        Assert.True(view.TryRefresh(out var completedSvg));
        Assert.NotNull(completedSvg);
        Assert.Contains($"TICK {latest.Tick}", completedSvg);
        Assert.Contains("1/1 blocks placed", completedSvg);
        Assert.Equal(latest.Tick, view.RenderedTick);
        Assert.Equal(view.RenderedSequence, observer.Sequence);
        Assert.False(view.TryRefresh(out _));
    }

    [Fact]
    public void TryRefresh_DoesNotConsumeSnapshotWhenRenderingFails()
    {
        var simulation = new ConstructionSimulation(
            width: 2,
            height: 1,
            requiredHeight: (_, _) => 0,
            agents: [],
            emitters: []);
        var observer = new ConstructionSnapshotObserver("invalid-view-run");
        Assert.True(observer.TryPublish(simulation.Snapshot()));
        var view = new ConstructionSnapshotSvgView(observer, width: 3, height: 1);

        Assert.Throws<ArgumentException>(() => view.TryRefresh(out _));
        Assert.Equal(0, view.RenderedSequence);
        Assert.Null(view.RenderedTick);
    }
}
