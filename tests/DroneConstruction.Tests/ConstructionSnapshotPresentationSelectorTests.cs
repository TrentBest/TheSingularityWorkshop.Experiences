using TheSingularityWorkshop.Experiences.DroneConstruction;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class ConstructionSnapshotPresentationSelectorTests
{
    [Fact]
    public void Select_BoundsIndividualsAndAggregatesTheRestDeterministically()
    {
        var simulation = CreateSimulation();
        var snapshot = simulation.Snapshot();
        var policy = new ConstructionSnapshotPresentationPolicy(maximumIndividualAgents: 2, individualRadius: 2);

        var first = ConstructionSnapshotPresentationSelector.Select(
            snapshot, "run-a", sequence: 7, simulation.SpaceId,
            new ConstructionObserverPose(new GridCell(1, 0)), policy);
        var second = ConstructionSnapshotPresentationSelector.Select(
            snapshot, "run-a", sequence: 7, simulation.SpaceId,
            new ConstructionObserverPose(new GridCell(1, 0)), policy);

        Assert.Equal(new ConstructionSnapshotProvenance("run-a", snapshot.Tick, 7), first.Provenance);
        Assert.Equal(4, first.TotalAgentsInSpace);
        Assert.Equal(2, first.SelectedIndividualCount);
        Assert.Equal(2, first.AggregatedAgentCount);
        Assert.Equal(new ulong[] { 20, 10 }, first.IndividualAgents.Select(agent => agent.AgentId));
        Assert.Equal(first.IndividualAgents.Select(agent => agent.AgentId),
            second.IndividualAgents.Select(agent => agent.AgentId));
        Assert.Equal(first.Aggregates.Select(aggregate => aggregate.AggregateId),
            second.Aggregates.Select(aggregate => aggregate.AggregateId));
        Assert.Equal(new ulong[] { 30, 40 },
            first.Aggregates.SelectMany(aggregate => aggregate.MemberAgentIds).Order());
    }

    [Fact]
    public void Select_UsesOnlyRequestedSimulationSpaceAndAllowsZeroIndividualBudget()
    {
        var simulation = CreateSimulation();
        var snapshot = simulation.Snapshot();

        var selection = ConstructionSnapshotPresentationSelector.Select(
            snapshot, "run-b", sequence: 0, simulation.SpaceId,
            new ConstructionObserverPose(new GridCell(0, 0)),
            new ConstructionSnapshotPresentationPolicy(maximumIndividualAgents: 0, individualRadius: 100));

        Assert.Empty(selection.IndividualAgents);
        Assert.Equal(4, selection.AggregatedAgentCount);
        Assert.Equal(4, selection.TotalAgentsInSpace);
    }

    [Fact]
    public void Select_RejectsInvalidBudgetsAndProvenance()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ConstructionSnapshotPresentationPolicy(maximumIndividualAgents: -1, individualRadius: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ConstructionSnapshotPresentationPolicy(maximumIndividualAgents: 1, individualRadius: -1));

        var snapshot = CreateSimulation().Snapshot();
        Assert.Throws<ArgumentException>(() => ConstructionSnapshotPresentationSelector.Select(
            snapshot, " ", 0, "construction-main", new ConstructionObserverPose(new GridCell(0, 0)),
            new ConstructionSnapshotPresentationPolicy(1, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ConstructionSnapshotPresentationSelector.Select(
            snapshot, "run-a", -1, "construction-main", new ConstructionObserverPose(new GridCell(0, 0)),
            new ConstructionSnapshotPresentationPolicy(1, 1)));
    }

    private static ConstructionSimulation CreateSimulation() => new(
        width: 4,
        height: 1,
        requiredHeight: (_, _) => 0,
        agents:
        [
            new ConstructionAgent(10, new GridCell(0, 0)),
            new ConstructionAgent(20, new GridCell(1, 0)),
            new ConstructionAgent(30, new GridCell(2, 0)),
            new ConstructionAgent(40, new GridCell(3, 0))
        ],
        emitters: []);
}
