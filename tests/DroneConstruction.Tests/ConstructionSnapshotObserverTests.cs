using TheSingularityWorkshop.Experiences.DroneConstruction;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class ConstructionSnapshotObserverTests
{
    [Fact]
    public void Observer_TracksOnlyStrictlyNewerCommittedTicks()
    {
        var simulation = CreateSimulation();
        var observer = new ConstructionSnapshotObserver("run-2026-10-22-a");
        var tick0 = simulation.Snapshot();

        Assert.Equal("run-2026-10-22-a", observer.RunId);
        Assert.Null(observer.LatestSnapshot);
        Assert.Equal(0, observer.Sequence);

        Assert.True(observer.TryPublish(tick0));
        Assert.Same(tick0, observer.LatestSnapshot);
        Assert.Equal(1, observer.Sequence);

        Assert.False(observer.TryPublish(tick0));
        Assert.Equal(1, observer.Sequence);

        var tick1 = simulation.Step();
        Assert.True(observer.TryPublish(tick1));
        Assert.Same(tick1, observer.LatestSnapshot);
        Assert.Equal(2, observer.Sequence);

        Assert.False(observer.TryPublish(tick0));
        Assert.Same(tick1, observer.LatestSnapshot);
        Assert.Equal(2, observer.Sequence);
    }

    [Fact]
    public void Observer_RequiresExplicitRunIdentityAndNonNullSnapshots()
    {
        Assert.Throws<ArgumentException>(() => new ConstructionSnapshotObserver(" "));
        var observer = new ConstructionSnapshotObserver("run-a");
        Assert.Throws<ArgumentNullException>(() => observer.TryPublish(null!));
    }

    [Fact]
    public void SeparateObserversKeepTickZeroRunsDistinct()
    {
        var firstRun = new ConstructionSnapshotObserver("run-a");
        var secondRun = new ConstructionSnapshotObserver("run-b");
        var firstSnapshot = CreateSimulation().Snapshot();
        var secondSnapshot = CreateSimulation().Snapshot();

        Assert.True(firstRun.TryPublish(firstSnapshot));
        Assert.True(secondRun.TryPublish(secondSnapshot));
        Assert.Equal(0, firstRun.LatestSnapshot!.Tick);
        Assert.Equal(0, secondRun.LatestSnapshot!.Tick);
        Assert.NotEqual(firstRun.RunId, secondRun.RunId);
    }

    private static ConstructionSimulation CreateSimulation() => new(
        width: 1,
        height: 1,
        requiredHeight: (_, _) => 0,
        agents: [new ConstructionAgent(7, new GridCell(0, 0))],
        emitters: [new PositionedBlockEmitter(new GridCell(0, 0), new BlockEmitter("emitter-01", 1))]);
}
