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
        Assert.Equal(4, first.TotalAgentsInSnapshot);
        Assert.Equal(4, first.TotalAgentsInSpace);
        Assert.Equal(0, simulation.TickCount);
        Assert.Equal(0, simulation.Snapshot().Tick);
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


    [Fact]
    public void Select_ChangingObserverAndLodDoesNotChangeAuthoritativeOutcomes()
    {
        var observedSimulation = CreateSimulation();
        var controlSimulation = CreateSimulation();

        var observedSnapshot = observedSimulation.Snapshot();
        var firstSelection = ConstructionSnapshotPresentationSelector.Select(
            observedSnapshot, "run-a", 0, observedSimulation.SpaceId,
            new ConstructionObserverPose(new GridCell(0, 0)),
            new ConstructionSnapshotPresentationPolicy(maximumIndividualAgents: 1, individualRadius: 0));
        var secondSelection = ConstructionSnapshotPresentationSelector.Select(
            observedSnapshot, "run-a", 1, observedSimulation.SpaceId,
            new ConstructionObserverPose(new GridCell(3, 0)),
            new ConstructionSnapshotPresentationPolicy(maximumIndividualAgents: 3, individualRadius: 3));

        Assert.NotEqual(firstSelection.IndividualAgents.Select(agent => agent.AgentId),
            secondSelection.IndividualAgents.Select(agent => agent.AgentId));
        Assert.NotEqual(firstSelection.AggregatedAgentCount, secondSelection.AggregatedAgentCount);

        observedSimulation.Step();
        controlSimulation.Step();

        var observedOutcome = observedSimulation.Snapshot();
        var controlOutcome = controlSimulation.Snapshot();
        Assert.Equal(controlOutcome.Tick, observedOutcome.Tick);
        Assert.Equal(controlOutcome.RequiredBlocks, observedOutcome.RequiredBlocks);
        Assert.Equal(controlOutcome.PlacedBlocks, observedOutcome.PlacedBlocks);
        Assert.Equal(controlOutcome.IsComplete, observedOutcome.IsComplete);
        Assert.Equal(controlOutcome.Agents, observedOutcome.Agents);
        Assert.Equal(controlOutcome.Emitters, observedOutcome.Emitters);
        Assert.Equal(controlOutcome.CellPlacedBlocks, observedOutcome.CellPlacedBlocks);
    }


    [Fact]
    public void Select_WhenRequestedSpaceHasNoAgents_ReturnsExplicitEmptySelection()
    {
        var snapshot = CreateSimulation().Snapshot();

        var selection = ConstructionSnapshotPresentationSelector.Select(
            snapshot, "run-empty", sequence: 3, spaceId: "unobserved-space",
            new ConstructionObserverPose(new GridCell(0, 0)),
            new ConstructionSnapshotPresentationPolicy(maximumIndividualAgents: 8, individualRadius: 8));

        Assert.Equal(new ConstructionSnapshotProvenance("run-empty", snapshot.Tick, 3), selection.Provenance);
        Assert.Equal(4, selection.TotalAgentsInSnapshot);
        Assert.Equal(0, selection.TotalAgentsInSpace);
        Assert.Empty(selection.IndividualAgents);
        Assert.Empty(selection.Aggregates);
        Assert.Equal(0, selection.SelectedIndividualCount);
        Assert.Equal(0, selection.AggregatedAgentCount);
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
