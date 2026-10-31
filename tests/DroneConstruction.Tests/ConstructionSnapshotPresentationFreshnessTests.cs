using TheSingularityWorkshop.Experiences.DroneConstruction;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class ConstructionSnapshotPresentationFreshnessTests
{
    [Fact]
    public void SelectIfFresh_ReturnsSelectionOnlyWithinCallerOwnedAgeBudget()
    {
        var simulation = CreateSimulation();
        var snapshot = simulation.Snapshot();
        var capturedAt = new DateTimeOffset(2026, 10, 31, 6, 0, 0, TimeSpan.Zero);
        var policy = new ConstructionSnapshotFreshnessPolicy(TimeSpan.FromSeconds(5));
        var presentationPolicy = new ConstructionSnapshotPresentationPolicy(1, 2);

        var fresh = ConstructionSnapshotPresentationSelector.SelectIfFresh(
            snapshot, capturedAt, capturedAt.AddSeconds(5), policy,
            "run-a", 4, simulation.SpaceId, new ConstructionObserverPose(new GridCell(0, 0)),
            presentationPolicy);

        Assert.Equal(ConstructionSnapshotFreshness.Fresh, fresh.Freshness);
        Assert.True(fresh.IsPresentable);
        Assert.NotNull(fresh.Selection);
        Assert.Equal(new ConstructionSnapshotProvenance("run-a", snapshot.Tick, 4),
            fresh.Selection!.Provenance);
        Assert.Equal(0, simulation.TickCount);
    }

    [Theory]
    [InlineData(6, ConstructionSnapshotFreshness.Stale)]
    [InlineData(-1, ConstructionSnapshotFreshness.FutureDated)]
    public void SelectIfFresh_RejectsStaleAndFutureDatedSnapshots(int elapsedSeconds,
        ConstructionSnapshotFreshness expected)
    {
        var simulation = CreateSimulation();
        var snapshot = simulation.Snapshot();
        var capturedAt = new DateTimeOffset(2026, 10, 31, 6, 0, 0, TimeSpan.Zero);
        var evaluatedAt = capturedAt.AddSeconds(elapsedSeconds);

        var result = ConstructionSnapshotPresentationSelector.SelectIfFresh(
            snapshot, capturedAt, evaluatedAt,
            new ConstructionSnapshotFreshnessPolicy(TimeSpan.FromSeconds(5)),
            "run-a", 0, simulation.SpaceId, new ConstructionObserverPose(new GridCell(0, 0)),
            new ConstructionSnapshotPresentationPolicy(4, 4));

        Assert.Equal(expected, result.Freshness);
        Assert.False(result.IsPresentable);
        Assert.Null(result.Selection);
        Assert.Equal(0, simulation.TickCount);
        Assert.Equal(snapshot, simulation.Snapshot());
    }

    [Fact]
    public void SelectIfFresh_ValidatesRequiredInputs()
    {
        var snapshot = CreateSimulation().Snapshot();
        Assert.Throws<ArgumentNullException>(() =>
            ConstructionSnapshotPresentationSelector.SelectIfFresh(
                snapshot, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null!,
                "run-a", 0, "construction-main", new ConstructionObserverPose(new GridCell(0, 0)),
                new ConstructionSnapshotPresentationPolicy(1, 1)));
    }

    private static ConstructionSimulation CreateSimulation() => new(
        width: 1,
        height: 1,
        requiredHeight: (_, _) => 0,
        agents: [new ConstructionAgent(1, new GridCell(0, 0))],
        emitters: []);
}
