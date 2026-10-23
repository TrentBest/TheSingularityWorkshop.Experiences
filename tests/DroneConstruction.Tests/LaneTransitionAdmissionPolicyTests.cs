using TheSingularityWorkshop.Experiences.DroneConstruction;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class LaneTransitionAdmissionPolicyTests
{
    [Fact]
    public void Submit_RequiresWaitingAgentAtQualifiedSource_AndDoesNotReplaceIntent()
    {
        var (simulation, agent, intent, _, _) = CreateReadyScenario(registerLifecycle: true);
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
        var (simulation, agent, intent, destinationGrid, _) = CreateReadyScenario(registerLifecycle: true);
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
        var (simulation, agent, intent, _, _) = CreateReadyScenario(registerLifecycle: true);
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
        var (simulation, agent, intent, destinationGrid, _) = CreateReadyScenario(registerLifecycle: true);
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
        var (autoSimulation, autoAgent, baseIntent, autoDestinationGrid, autoTransitionGrid) = CreateReadyScenario(registerLifecycle: true);
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
        Assert.True(autoTransitionGrid.TryOccupy(new GridCell(1, 0), 77));
        autoSimulation.Step();
        Assert.Equal(autoIntent.TransitionEntry, autoSimulation.GetAgentLocation(autoAgent.AgentId));
        Assert.True(autoTransitionGrid.Release(new GridCell(1, 0), 77));
        autoSimulation.Step();
        Assert.Equal(new QualifiedGridLocation(ConstructionSpaceKind.Slipstream, "east-transition", new GridCell(1, 0)),
            autoSimulation.GetAgentLocation(autoAgent.AgentId));
        autoSimulation.Step();
        Assert.Equal(new QualifiedGridLocation(ConstructionSpaceKind.Slipstream, "east-transition", new GridCell(2, 0)),
            autoSimulation.GetAgentLocation(autoAgent.AgentId));

        // A blocked exit is also retried automatically on a later tick.
        Assert.True(autoDestinationGrid!.TryOccupy(new GridCell(8, 0), 78));
        autoSimulation.Step();
        Assert.Equal(new QualifiedGridLocation(ConstructionSpaceKind.Slipstream, "east-transition", new GridCell(2, 0)),
            autoSimulation.GetAgentLocation(autoAgent.AgentId));
        Assert.True(autoDestinationGrid.Release(new GridCell(8, 0), 78));
        autoSimulation.Step();
        Assert.Equal(autoIntent.Destination, autoSimulation.GetAgentLocation(autoAgent.AgentId));
    }

    [Fact]
    public void Step_ChainsExplicitTransitionExitIntoDownstreamSkylaneTargetOnFollowingTick()
    {
        var (simulation, agent, baseIntent, _, _) = CreateReadyScenario(registerLifecycle: true);
        var intent = new LaneTransitionIntent(
            baseIntent.Source,
            baseIntent.TransitionEntry,
            baseIntent.Destination,
            baseIntent.Lookahead,
            [new GridCell(0, 0), new GridCell(1, 0), new GridCell(2, 0)],
            downstreamLaneTarget: new GridCell(11, 0));
        var policy = new LaneTransitionAdmissionPolicy(simulation);
        Assert.True(policy.TrySubmit(agent.AgentId, intent));

        simulation.Step(); // Admit at transition entry.
        simulation.Step(); // Transition cell 1.
        simulation.Step(); // Transition cell 2.
        simulation.Step(); // Exit to the exact destination; do not also move along the lane.
        Assert.Equal(intent.Destination, simulation.GetAgentLocation(agent.AgentId));
        Assert.True(simulation.TryGetSkylaneDestination(agent.AgentId, out var target));
        Assert.Equal(new GridCell(11, 0), target);

        simulation.Step(); // Downstream lane leg starts on the next tick.
        Assert.Equal(
            new QualifiedGridLocation(ConstructionSpaceKind.Skylane, "east-lane", new GridCell(9, 0)),
            simulation.GetAgentLocation(agent.AgentId));
    }

    [Fact]
    public void Step_RejectsInvalidDownstreamTargetBeforeTransitionAdmission()
    {
        var (simulation, agent, baseIntent, _, _) = CreateReadyScenario(registerLifecycle: true);
        var invalidIntent = new LaneTransitionIntent(
            baseIntent.Source,
            baseIntent.TransitionEntry,
            baseIntent.Destination,
            baseIntent.Lookahead,
            [new GridCell(0, 0), new GridCell(1, 0), new GridCell(2, 0)],
            downstreamLaneTarget: new GridCell(7, 0)); // Eastbound lane target is behind the exit.

        var policy = new LaneTransitionAdmissionPolicy(simulation);
        Assert.True(policy.TrySubmit(agent.AgentId, invalidIntent));

        simulation.Step();

        // The route is rejected before any occupancy transfer; pending intent remains inspectable.
        Assert.Equal(invalidIntent.Source, simulation.GetAgentLocation(agent.AgentId));
        Assert.Equal(1, policy.PendingCount);
        Assert.False(simulation.TryGetSkylaneDestination(agent.AgentId, out _));
    }

    [Fact]
    public void Step_CompetingTransitionRequestsUseStableAgentIdOrderAndRetainLoserIntent()
    {
        var first = new ConstructionAgent(1, new GridCell(0, 0));
        var second = new ConstructionAgent(2, new GridCell(1, 0));
        var simulation = new ConstructionSimulation(12, 1, (_, _) => 0, [second, first], [], "yard");
        foreach (var agent in new[] { first, second })
        {
            Assert.True(agent.TryAssign(new PlacementTask(agent.AgentId, agent.Position.X, 0, 1)));
            Assert.True(agent.TryTransition(ConstructionAgentState.CarryingBlock));
            Assert.True(agent.TryTransition(ConstructionAgentState.AwaitingTransition));
        }

        var transitionGrid = new TrafficCellGrid(3, 1);
        var lane = new Skylane("east-lane", 12, 1, LaneDirection.East, elevation: 1);
        var lifecycle = new SlipstreamLifecycle(new Slipstream("east-resource", 3, 5), transitionGrid);
        Assert.True(simulation.TryRegisterTrafficSpace(ConstructionSpaceKind.Slipstream, "east-transition", transitionGrid));
        Assert.True(simulation.TryRegisterSkylane(lane));
        Assert.True(simulation.TryRegisterSlipstreamLifecycle("east-transition", lifecycle));

        var policy = new LaneTransitionAdmissionPolicy(simulation);
        LaneTransitionIntent IntentFor(ConstructionAgent agent) => new(
            new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, "yard", agent.Position),
            new QualifiedGridLocation(ConstructionSpaceKind.Slipstream, "east-transition", new GridCell(0, 0)),
            new QualifiedGridLocation(ConstructionSpaceKind.Skylane, "east-lane", new GridCell(8, 0)),
            [new GridCell(2, 0), new GridCell(3, 0), new GridCell(4, 0), new GridCell(5, 0), new GridCell(6, 0)],
            [new GridCell(0, 0), new GridCell(1, 0), new GridCell(2, 0)]);

        Assert.True(policy.TrySubmit(second.AgentId, IntentFor(second)));
        Assert.True(policy.TrySubmit(first.AgentId, IntentFor(first)));

        simulation.Step();

        // The simulation sorts agents by ID, so the lower ID wins even though input order was reversed.
        Assert.Equal(
            new QualifiedGridLocation(ConstructionSpaceKind.Slipstream, "east-transition", new GridCell(0, 0)),
            simulation.GetAgentLocation(first.AgentId));
        Assert.Equal(
            new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, "yard", new GridCell(1, 0)),
            simulation.GetAgentLocation(second.AgentId));
        Assert.Equal(1, policy.PendingCount);
        Assert.True(policy.TryGetPending(second.AgentId, out var retained));
        Assert.NotNull(retained);
        var expectedRetained = IntentFor(second);
        Assert.Equal(expectedRetained.Source, retained!.Source);
        Assert.Equal(expectedRetained.TransitionEntry, retained.TransitionEntry);
        Assert.Equal(expectedRetained.Destination, retained.Destination);
        Assert.Equal(expectedRetained.Lookahead, retained.Lookahead);
        Assert.Equal(expectedRetained.TransitionPath, retained.TransitionPath);
        Assert.Equal(expectedRetained.DownstreamLaneTarget, retained.DownstreamLaneTarget);

        // The waiting request remains pending while the first agent owns the shared transition.
        simulation.Step();
        Assert.Equal(
            new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, "yard", new GridCell(1, 0)),
            simulation.GetAgentLocation(second.AgentId));
        Assert.Equal(1, policy.PendingCount);

        // Once the first agent clears the transition, the retained request can be admitted.
        simulation.Step(); // Transition cell 1.
        simulation.Step(); // Transition cell 2.
        simulation.Step(); // First agent exits to the lane; no same-tick lane movement.
        Assert.Equal(
            new QualifiedGridLocation(ConstructionSpaceKind.Skylane, "east-lane", new GridCell(8, 0)),
            simulation.GetAgentLocation(first.AgentId));
        Assert.Equal(1, policy.PendingCount);

        simulation.Step(); // The second agent's retained intent is admitted.
        Assert.Equal(
            new QualifiedGridLocation(ConstructionSpaceKind.Slipstream, "east-transition", new GridCell(0, 0)),
            simulation.GetAgentLocation(second.AgentId));
        Assert.Equal(0, policy.PendingCount);
    }

    [Fact]
    public void Constructor_RejectsMalformedExplicitTransitionPaths()
    {
        var (_, _, intent, _, _) = CreateReadyScenario(registerLifecycle: true);
        Assert.Throws<ArgumentException>(() => new LaneTransitionIntent(
            intent.Source, intent.TransitionEntry, intent.Destination, intent.Lookahead,
            [new GridCell(1, 0), new GridCell(2, 0)]));
        Assert.Throws<ArgumentException>(() => new LaneTransitionIntent(
            intent.Source, intent.TransitionEntry, intent.Destination, intent.Lookahead,
            [new GridCell(0, 0), new GridCell(2, 0)]));
    }

    private static (ConstructionSimulation Simulation, ConstructionAgent Agent, LaneTransitionIntent Intent, TrafficCellGrid? DestinationGrid, TrafficCellGrid TransitionGrid)
        CreateReadyScenario(bool registerLifecycle)
    {
        var agent = new ConstructionAgent(1, new GridCell(0, 0));
        var simulation = new ConstructionSimulation(12, 1, (_, _) => 0, [agent], [], "yard");
        Assert.True(agent.TryAssign(new PlacementTask(1, 0, 0, 1)));
        Assert.True(agent.TryTransition(ConstructionAgentState.CarryingBlock));
        Assert.True(agent.TryTransition(ConstructionAgentState.AwaitingTransition));

        var transitionGrid = new TrafficCellGrid(3, 1);
        var lane = new Skylane("east-lane", 12, 1, LaneDirection.East, elevation: 1);
        var destinationGrid = lane.Cells;
        var lifecycle = new SlipstreamLifecycle(new Slipstream("east-resource", 3, 5), transitionGrid);
        Assert.True(simulation.TryRegisterTrafficSpace(ConstructionSpaceKind.Slipstream, "east-transition", transitionGrid));
        Assert.True(simulation.TryRegisterSkylane(lane));
        if (registerLifecycle)
            Assert.True(simulation.TryRegisterSlipstreamLifecycle("east-transition", lifecycle));

        var intent = new LaneTransitionIntent(
            new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, "yard", new GridCell(0, 0)),
            new QualifiedGridLocation(ConstructionSpaceKind.Slipstream, "east-transition", new GridCell(0, 0)),
            new QualifiedGridLocation(ConstructionSpaceKind.Skylane, "east-lane", new GridCell(8, 0)),
            [new GridCell(2, 0), new GridCell(3, 0), new GridCell(4, 0), new GridCell(5, 0), new GridCell(6, 0)]);
        return (simulation, agent, intent, destinationGrid, transitionGrid);
    }
}
