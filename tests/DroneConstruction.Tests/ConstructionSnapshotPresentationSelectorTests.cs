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
        Assert.Equal(2, first.AggregateCount);
        Assert.Equal(0, first.OmittedFromSpaceCount);
        Assert.Equal(4, first.RepresentedAgentCount);
        Assert.True(first.PopulationIsReconciled);
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
        Assert.Equal(4, selection.OmittedFromSpaceCount);
        Assert.Equal(0, selection.AggregateCount);
        Assert.Equal(0, selection.RepresentedAgentCount);
        Assert.True(selection.PopulationIsReconciled);
        Assert.Empty(selection.IndividualAgents);
        Assert.Empty(selection.Aggregates);
        Assert.Equal(0, selection.SelectedIndividualCount);
        Assert.Equal(0, selection.AggregatedAgentCount);
    }



    [Fact]
    public void Select_CrowdBudgetAndObserverChangesCannotAlterActiveConstruction()
    {
        static ConstructionSimulation CreateActiveSimulation() => new(
            width: 4,
            height: 1,
            requiredHeight: (x, _) => x == 3 ? 1 : 0,
            agents: [new ConstructionAgent(10, new GridCell(0, 0))],
            emitters: [new PositionedBlockEmitter(
                new GridCell(0, 0), new BlockEmitter("emitter", initialInventory: 1))]);

        var observedSimulation = CreateActiveSimulation();
        var controlSimulation = CreateActiveSimulation();

        var observedSnapshot = observedSimulation.Snapshot();
        var aggregateOnly = ConstructionSnapshotPresentationSelector.Select(
            observedSnapshot, "crowd-run", 0, observedSimulation.SpaceId,
            new ConstructionObserverPose(new GridCell(0, 0)),
            new ConstructionSnapshotPresentationPolicy(maximumIndividualAgents: 0, individualRadius: 0));
        var individual = ConstructionSnapshotPresentationSelector.Select(
            observedSnapshot, "crowd-run", 1, observedSimulation.SpaceId,
            new ConstructionObserverPose(new GridCell(3, 0)),
            new ConstructionSnapshotPresentationPolicy(maximumIndividualAgents: 1, individualRadius: 10));

        Assert.Empty(aggregateOnly.IndividualAgents);
        Assert.Equal(1, aggregateOnly.AggregatedAgentCount);
        Assert.Single(individual.IndividualAgents);
        Assert.Equal(0, individual.AggregatedAgentCount);

        ConstructionSimulationSnapshot observedOutcome = observedSnapshot;
        ConstructionSimulationSnapshot controlOutcome = controlSimulation.Snapshot();
        for (var tick = 0; tick < 12 && !controlOutcome.IsComplete; tick++)
        {
            observedOutcome = observedSimulation.Step();
            controlOutcome = controlSimulation.Step();
        }

        Assert.True(controlOutcome.IsComplete);
        Assert.Equal(controlOutcome.Tick, observedOutcome.Tick);
        Assert.Equal(controlOutcome.RequiredBlocks, observedOutcome.RequiredBlocks);
        Assert.Equal(controlOutcome.PlacedBlocks, observedOutcome.PlacedBlocks);
        Assert.Equal(controlOutcome.IsComplete, observedOutcome.IsComplete);
        Assert.Equal(controlOutcome.Agents, observedOutcome.Agents);
        Assert.Equal(controlOutcome.Emitters, observedOutcome.Emitters);
        Assert.Equal(controlOutcome.CellPlacedBlocks, observedOutcome.CellPlacedBlocks);
        Assert.Equal(controlOutcome.Tick, observedSimulation.TickCount);
    }

    [Fact]
    public void Select_BudgetAndObserverMatrixAlwaysReconcilesPopulationWithoutDuplicateIdentity()
    {
        var snapshot = CreateSimulation().Snapshot();
        var sourceIds = snapshot.Agents.Select(agent => agent.AgentId).Order().ToArray();
        var sequence = 0L;

        foreach (var observerX in Enumerable.Range(-1, 7))
        foreach (var budget in Enumerable.Range(0, sourceIds.Length + 2))
        foreach (var radius in new[] { 0, 1, 3, 10 })
        {
            var policy = new ConstructionSnapshotPresentationPolicy(budget, radius);
            var observer = new ConstructionObserverPose(new GridCell(observerX, 0));

            var first = ConstructionSnapshotPresentationSelector.Select(
                snapshot, "matrix-run", sequence, "construction-main", observer, policy);
            var repeated = ConstructionSnapshotPresentationSelector.Select(
                snapshot, "matrix-run", sequence, "construction-main", observer, policy);
            sequence++;

            var representedIds = first.IndividualAgents.Select(agent => agent.AgentId)
                .Concat(first.Aggregates.SelectMany(aggregate => aggregate.MemberAgentIds))
                .Order()
                .ToArray();

            Assert.Equal(sourceIds, representedIds);
            Assert.Equal(representedIds.Length, representedIds.Distinct().Count());
            Assert.True(first.PopulationIsReconciled);
            Assert.Equal(first.TotalAgentsInSnapshot, first.RepresentedAgentCount);
            Assert.Equal(first.IndividualAgents.Select(agent => agent.AgentId),
                repeated.IndividualAgents.Select(agent => agent.AgentId));
            Assert.Equal(
                first.Aggregates.Select(aggregate => (aggregate.AggregateId, Members: string.Join(",", aggregate.MemberAgentIds))),
                repeated.Aggregates.Select(aggregate => (aggregate.AggregateId, Members: string.Join(",", aggregate.MemberAgentIds))));
            Assert.True(first.SelectedIndividualCount <= budget);
        }
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
