using TheSingularityWorkshop.Experiences.DroneConstruction;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class LaneTransitionAdmissionPolicyTests
{
    [Fact]
    public void Submit_RequiresWaitingAgentAtQualifiedSource_AndDoesNotReplaceIntent()
    {
        var (simulation, agent, intent, _) = CreateReadyScenario(registerLifecycle: true);
        var policy = new LaneTransitionAdmissionPolicy(simulation);

        Assert.False(policy.TrySubmit(99, intent));
        // A different qualified source is not the agent's committed location.
        var mismatchedSourceIntent = new LaneTransitionIntent(
            new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, "yard", new GridCell(1, 0)),
            intent.TransitionEntry,
            intent.Destination,
            intent.Lookahead);
        Assert.False(policy.TrySubmit(agent.AgentId, mismatchedSourceIntent));

        Assert.True(policy.TrySubmit(agent.AgentId, intent));
        Assert.False(policy.TrySubmit(agent.AgentId, intent));
        Assert.Equal(1, policy.PendingCount);
        Assert.True(policy.TryGetPending(agent.AgentId, out var pending));
        Assert.Equal(intent, pending);
    }

    [Fact]
    public void Admit_FailedAdmissionRetainsIntent_AndSuccessfulRetryConsumesIt()
    {
        var (simulation, agent, intent, destinationGrid) = CreateReadyScenario(registerLifecycle: true);
        var policy = new LaneTransitionAdmissionPolicy(simulation);
        Assert.True(policy.TrySubmit(agent.AgentId, intent));

        // Occupying a lookahead cell makes the admission fail without consuming intent.
        Assert.True(destinationGrid!.TryOccupy(new GridCell(4, 0), 77));
        Assert.False(policy.TryAdmit(agent.AgentId));
        Assert.Equal(1, policy.PendingCount);
        Assert.Equal(intent.Source, simulation.GetAgentLocation(agent.AgentId));

        Assert.True(destinationGrid.Release(new GridCell(4, 0), 77));
        Assert.True(policy.TryAdmit(agent.AgentId));
        Assert.Equal(0, policy.PendingCount);
        Assert.Equal(intent.TransitionEntry, simulation.GetAgentLocation(agent.AgentId));
        Assert.False(policy.TryAdmit(agent.AgentId));
    }

    [Fact]
    public void Cancel_RemovesIntentWithoutMovingAgent()
    {
        var (simulation, agent, intent, _) = CreateReadyScenario(registerLifecycle: true);
        var policy = new LaneTransitionAdmissionPolicy(simulation);
        Assert.True(policy.TrySubmit(agent.AgentId, intent));

        Assert.True(policy.TryCancel(agent.AgentId));
        Assert.False(policy.TryCancel(agent.AgentId));
        Assert.Equal(intent.Source, simulation.GetAgentLocation(agent.AgentId));
        Assert.Equal(0, policy.PendingCount);
    }

    [Fact]
    public void Step_RetriesBlockedTransitionAdmissionAndConsumesIntentOnSuccess()
    {
        var (simulation, agent, intent, destinationGrid) = CreateReadyScenario(registerLifecycle: true);
        var policy = new LaneTransitionAdmissionPolicy(simulation);
        Assert.True(policy.TrySubmit(agent.AgentId, intent));

        // A blocked lookahead must leave the agent at its source and retain the intent.
        Assert.True(destinationGrid!.TryOccupy(new GridCell(4, 0), 77));
        simulation.Step();
        Assert.Equal(intent.Source, simulation.GetAgentLocation(agent.AgentId));
        Assert.Equal(1, policy.PendingCount);

        // The next tick retries deterministically once the protected cell is clear.
        Assert.True(destinationGrid.Release(new GridCell(4, 0), 77));
        var admittedSnapshot = simulation.Step();
        Assert.Equal(intent.TransitionEntry, simulation.GetAgentLocation(agent.AgentId));
        Assert.Equal(0, policy.PendingCount);
        Assert.Equal(ConstructionAgentState.AwaitingTransition,
            admittedSnapshot.Agents.Single(item => item.AgentId == agent.AgentId).State);
    }

    [Fact]
    public void Step_AutonomouslyTraversesPathAndRetriesBlockedIntermediateCellAndExit()
    {
        var (autoSimulation, autoAgent, baseIntent, autoDestinationGrid) = CreateReadyScenario(registerLifecycle: true);
        var autoIntent = new LaneTransitionIntent(
            baseIntent.Source,
            baseIntent.TransitionEntry,
            baseIntent.Destination,
            baseIntent.Lookahead,
            [new GridCell(0, 0), new GridCell(1, 0), new GridCell(2, 0)]);
        var policy = new LaneTransitionAdmissionPolicy(autoSimulation);
        Assert.True(policy.TrySubmit(autoAgent.AgentId, autoIntent));

        // Admission commits only the entry move; it must not also advance the path in this tick.
        autoSimulation.Step();
        Assert.Equal(autoIntent.TransitionEntry, autoSimulation.GetAgentLocation(autoAgent.AgentId));
        Assert.Equal(0, policy.PendingCount);

        // A blocked intermediate cell is retried without losing transition occupancy or reservation.
        Assert.True(autoDestinationGrid!.TryOccupy(new GridCell(1, 0), 77));
        autoSimulation.Step();
        Assert.Equal(autoIntent.TransitionEntry, autoSimulation.GetAgentLocation(autoAgent.AgentId));
        Assert.True(autoDestinationGrid.Release(new GridCell(1, 0), 77));
        autoSimulation.Step();
        Assert.Equal(new QualifiedGridLocation(ConstructionSpaceKind.Slipstream, "east-transition", new GridCell(1, 0)),
            autoSimulation.GetAgentLocation(autoAgent.AgentId));
        autoSimulation.Step();
        Assert.Equal(new QualifiedGridLocation(ConstructionSpaceKind.Slipstream, "east-transition", new GridCell(2, 0)),
            autoSimulation.GetAgentLocation(autoAgent.AgentId));

        // A blocked exit is also retried automatically on a later tick.
        Assert.True(autoDestinationGrid.TryOccupy(new GridCell(8, 0), 78));
        autoSimulation.Step();
        Assert.Equal(new QualifiedGridLocation(ConstructionSpaceKind.Slipstream, "east-transition", new GridCell(2, 0)),
            autoSimulation.GetAgentLocation(autoAgent.AgentId));
        Assert.True(autoDestinationGrid.Release(new GridCell(8, 0), 78));
        autoSimulation.Step();
        Assert.Equal(autoIntent.Destination, autoSimulation.GetAgentLocation(autoAgent.AgentId));
    }

    [Fact]
    public void Constructor_RejectsMalformedExplicitTransitionPaths()
    {
        var (_, _, intent, _) = CreateReadyScenario(registerLifecycle: true);
        Assert.Throws<ArgumentException>(() => new LaneTransitionIntent(
            intent.Source, intent.TransitionEntry, intent.Destination, intent.Lookahead,
            [new GridCell(1, 0), new GridCell(2, 0)]));
        Assert.Throws<ArgumentException>(() => new LaneTransitionIntent(
            intent.Source, intent.TransitionEntry, intent.Destination, intent.Lookahead,
            [new GridCell(0, 0), new GridCell(2, 0)]));
    }

    private static (ConstructionSimulation Simulation, ConstructionAgent Agent, LaneTransitionIntent Intent, TrafficCellGrid? DestinationGrid)
        CreateReadyScenario(bool registerLifecycle)
    {
        var agent = new ConstructionAgent(1, new GridCell(0, 0));
        var simulation = new ConstructionSimulation(12, 1, (_, _) => 0, [agent], [], "yard");
        Assert.True(agent.TryAssign(new PlacementTask(1, 0, 0, 1)));
        Assert.True(agent.TryTransition(ConstructionAgentState.CarryingBlock));
        Assert.True(agent.TryTransition(ConstructionAgentState.AwaitingTransition));

        var transitionGrid = new TrafficCellGrid(3, 1);
        var destinationGrid = new TrafficCellGrid(12, 1);
        var lifecycle = new SlipstreamLifecycle(new Slipstream("east-resource", 3, 5), transitionGrid);
        Assert.True(simulation.TryRegisterTrafficSpace(ConstructionSpaceKind.Slipstream, "east-transition", transitionGrid));
        Assert.True(simulation.TryRegisterTrafficSpace(ConstructionSpaceKind.Skylane, "east-lane", destinationGrid));
        if (registerLifecycle)
            Assert.True(simulation.TryRegisterSlipstreamLifecycle("east-transition", lifecycle));

        var intent = new LaneTransitionIntent(
            new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, "yard", new GridCell(0, 0)),
            new QualifiedGridLocation(ConstructionSpaceKind.Slipstream, "east-transition", new GridCell(0, 0)),
            new QualifiedGridLocation(ConstructionSpaceKind.Skylane, "east-lane", new GridCell(8, 0)),
            [new GridCell(2, 0), new GridCell(3, 0), new GridCell(4, 0), new GridCell(5, 0), new GridCell(6, 0)]);
        return (simulation, agent, intent, destinationGrid);
    }
}
