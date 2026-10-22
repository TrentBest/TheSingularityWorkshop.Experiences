using TheSingularityWorkshop.Experiences.DroneConstruction;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class ConstructionSimulationTests
{
    [Fact]
    public void Simulation_CompletesPickupTravelAndPlacementFromCommittedState()
    {
        var agent = new ConstructionAgent(1, new GridCell(0, 0));
        var emitter = new BlockEmitter("emitter-01", initialInventory: 1);
        var simulation = new ConstructionSimulation(
            width: 3,
            height: 1,
            requiredHeight: (x, _) => x == 2 ? 1 : 0,
            agents: [agent],
            emitters: [new PositionedBlockEmitter(new GridCell(0, 0), emitter)]);

        var initial = simulation.Snapshot();
        Assert.Equal(0, initial.Tick);
        Assert.Equal(1, initial.RequiredBlocks);
        Assert.Equal(0, initial.PlacedBlocks);

        ConstructionSimulationSnapshot snapshot = initial;
        for (var i = 0; i < 12 && !snapshot.IsComplete; i++)
            snapshot = simulation.Step();

        Assert.True(snapshot.IsComplete);
        Assert.Equal(1, snapshot.PlacedBlocks);
        Assert.Equal(1, snapshot.CellPlacedBlocks[2]);
        Assert.Equal(0, snapshot.Emitters[0].Inventory);
        Assert.False(snapshot.Agents[0].IsCarryingBlock);
        Assert.Equal(ConstructionAgentState.Idle, snapshot.Agents[0].State);
        Assert.Null(snapshot.Agents[0].AssignedTaskId);
        Assert.Equal(new GridCell(2, 0), snapshot.Agents[0].Position);
        Assert.Equal(new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, "construction-main", new GridCell(2, 0)), snapshot.Agents[0].Location);
        Assert.Equal(snapshot.Tick, simulation.TickCount);
    }

    [Fact]
    public void Simulation_ExposesQualifiedAgentLocationAndUpdatesItAfterMovement()
    {
        var agent = new ConstructionAgent(1, new GridCell(0, 0));
        var simulation = new ConstructionSimulation(
            width: 3,
            height: 1,
            requiredHeight: (x, _) => x == 2 ? 1 : 0,
            agents: [agent],
            emitters: [new PositionedBlockEmitter(new GridCell(0, 0), new BlockEmitter("emitter", 1))],
            spaceId: "yard-east");

        var initial = simulation.GetAgentLocation(1);
        Assert.Equal(ConstructionSpaceKind.SimulationGrid, initial.SpaceKind);
        Assert.Equal("yard-east", initial.SpaceId);
        Assert.Equal(new GridCell(0, 0), initial.Cell);

        for (var i = 0; i < 6; i++)
            simulation.Step();

        var current = simulation.GetAgentLocation(1);
        Assert.Equal("yard-east", current.SpaceId);
        Assert.Equal(agent.Position, current.Cell);
        var snapshotLocation = Assert.Single(simulation.Snapshot().Agents).Location;
        Assert.Equal(current, snapshotLocation);
        Assert.NotEqual(initial.Cell, current.Cell);
    }

    [Fact]
    public void Simulation_RejectsUnknownAgentLocationQueryAndBlankSpaceIdentity()
    {
        var simulation = new ConstructionSimulation(
            width: 1,
            height: 1,
            requiredHeight: (_, _) => 0,
            agents: [],
            emitters: [],
            spaceId: "yard-east");

        Assert.Throws<KeyNotFoundException>(() => simulation.GetAgentLocation(404));
        Assert.Throws<ArgumentException>(() => new ConstructionSimulation(
            width: 1,
            height: 1,
            requiredHeight: (_, _) => 0,
            agents: [],
            emitters: [],
            spaceId: " "));
    }

    [Fact]
    public void Simulation_DoesNotConsumeInventoryWhenAllEmittersAreEmpty()
    {
        var agent = new ConstructionAgent(1, new GridCell(0, 0));
        var emitter = new BlockEmitter("empty", initialInventory: 0);
        var simulation = new ConstructionSimulation(
            width: 2,
            height: 1,
            requiredHeight: (x, _) => x == 1 ? 1 : 0,
            agents: [agent],
            emitters: [new PositionedBlockEmitter(new GridCell(0, 0), emitter)]);

        var snapshot = simulation.Step();
        snapshot = simulation.Step();

        Assert.Equal(0, snapshot.PlacedBlocks);
        Assert.Equal(0, snapshot.Emitters[0].Inventory);
        Assert.False(snapshot.Agents[0].IsCarryingBlock);
        Assert.Equal(ConstructionAgentState.Idle, snapshot.Agents[0].State);
        Assert.Equal(1, snapshot.RequiredBlocks);
    }

    [Fact]
    public void Simulation_RejectsDuplicateInitialAgentPositions()
    {
        var agents = new[]
        {
            new ConstructionAgent(1, new GridCell(0, 0)),
            new ConstructionAgent(2, new GridCell(0, 0))
        };

        Assert.Throws<ArgumentException>(() => new ConstructionSimulation(
            width: 2,
            height: 1,
            requiredHeight: (_, _) => 0,
            agents: agents,
            emitters: [new PositionedBlockEmitter(new GridCell(1, 0), new BlockEmitter("emitter", 1))]));
    }
    [Fact]
    public void Simulation_RejectsAgentsThatAlreadyHaveLifecycleState()
    {
        var agent = new ConstructionAgent(1, new GridCell(0, 0));
        Assert.True(agent.TryAssign(new PlacementTask(99, 1, 0, 1)));

        Assert.Throws<ArgumentException>(() => new ConstructionSimulation(
            width: 2,
            height: 1,
            requiredHeight: (x, _) => x == 1 ? 1 : 0,
            agents: [agent],
            emitters: [new PositionedBlockEmitter(new GridCell(0, 0), new BlockEmitter("emitter", 1))]));
    }

    [Fact]
    public void Simulation_RejectsNullEmitterBeforeOrderingResources()
    {
        var emitters = new PositionedBlockEmitter[]
        {
            null!
        };

        Assert.Throws<ArgumentException>(() => new ConstructionSimulation(
            width: 1,
            height: 1,
            requiredHeight: (_, _) => 0,
            agents: [],
            emitters: emitters));
    }

    [Fact]
    public void SnapshotCollectionsCannotBeMutatedByConsumers()
    {
        var simulation = new ConstructionSimulation(
            width: 1,
            height: 1,
            requiredHeight: (_, _) => 0,
            agents: [new ConstructionAgent(1, new GridCell(0, 0))],
            emitters: [new PositionedBlockEmitter(new GridCell(0, 0), new BlockEmitter("emitter", 0))]);

        var snapshot = simulation.Snapshot();

        Assert.IsAssignableFrom<IList<ConstructionAgentSnapshot>>(snapshot.Agents);
        Assert.Throws<NotSupportedException>(() => ((IList<ConstructionAgentSnapshot>)snapshot.Agents).Clear());
        Assert.IsAssignableFrom<IList<int>>(snapshot.CellPlacedBlocks);
        Assert.Throws<NotSupportedException>(() => ((IList<int>)snapshot.CellPlacedBlocks)[0] = 99);
    }


    [Fact]
    public void Simulation_DoesNotMoveThroughAnOccupiedTrafficCell()
    {
        var movingAgent = new ConstructionAgent(1, new GridCell(0, 0));
        var blockingAgent = new ConstructionAgent(2, new GridCell(1, 0));
        var simulation = new ConstructionSimulation(
            width: 3,
            height: 1,
            requiredHeight: (x, _) => x == 2 ? 1 : 0,
            agents: [movingAgent, blockingAgent],
            emitters: [new PositionedBlockEmitter(new GridCell(0, 0), new BlockEmitter("emitter", 1))]);

        ConstructionSimulationSnapshot snapshot = simulation.Snapshot();
        for (var i = 0; i < 12; i++)
            snapshot = simulation.Step();

        var mover = Assert.Single(snapshot.Agents, agent => agent.AgentId == 1);
        var blocker = Assert.Single(snapshot.Agents, agent => agent.AgentId == 2);
        Assert.Equal(new GridCell(0, 0), mover.Position);
        Assert.Equal(ConstructionAgentState.Traveling, mover.State);
        Assert.True(mover.IsCarryingBlock);
        Assert.Equal(new GridCell(1, 0), blocker.Position);
        Assert.Equal(0, snapshot.PlacedBlocks);
        Assert.False(snapshot.IsComplete);
    }


    [Fact]
    public void Simulation_IntegratesSlipstreamEntryTraversalExitAndResumesMainGridMovement()
    {
        var agent = new ConstructionAgent(1, new GridCell(0, 0));
        var simulation = new ConstructionSimulation(
            width: 10,
            height: 1,
            requiredHeight: (x, _) => x == 9 ? 1 : 0,
            agents: [agent],
            emitters: [new PositionedBlockEmitter(new GridCell(0, 0), new BlockEmitter("emitter", 1))],
            spaceId: "yard");
        var transitionGrid = new TrafficCellGrid(4, 1);
        var destinationSlipstream = new Slipstream("yard-to-skylane", transitionCellCount: 4, lookaheadCells: 5);
        var lifecycle = new SlipstreamLifecycle(destinationSlipstream, transitionGrid);

        Assert.True(simulation.TryRegisterTrafficSpace(ConstructionSpaceKind.Slipstream, "transition-east", transitionGrid));
        var source = new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, "yard", new GridCell(0, 0));
        var entry = new QualifiedGridLocation(ConstructionSpaceKind.Slipstream, "transition-east", new GridCell(0, 0));
        var destination = new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, "yard", new GridCell(8, 0));
        GridCell[] lookahead = [new(2, 0), new(3, 0), new(4, 0), new(5, 0), new(6, 0)];

        Assert.True(simulation.TryEnterSlipstream(1, source, entry, lifecycle, destination, lookahead));
        Assert.Equal(entry, simulation.GetAgentLocation(1));
        Assert.Equal(new GridCell(0, 0), agent.Position);
        Assert.Equal(entry, Assert.Single(simulation.Snapshot().Agents).Location);

        Assert.True(simulation.TryAdvanceSlipstream(1, "transition-east", lifecycle, new GridCell(1, 0)));
        Assert.True(simulation.TryAdvanceSlipstream(1, "transition-east", lifecycle, new GridCell(2, 0)));
        Assert.True(simulation.TryAdvanceSlipstream(1, "transition-east", lifecycle, new GridCell(3, 0)));
        Assert.Equal(new GridCell(3, 0), agent.Position);

        Assert.True(simulation.TryExitSlipstream(1, "transition-east", lifecycle, destination));
        Assert.Equal(destination, simulation.GetAgentLocation(1));
        Assert.Equal(destination.Cell, agent.Position);
        Assert.Null(lifecycle.AgentId);

        // The ordinary simulation tick now owns the agent again and moves it using main-grid coordinates.
        simulation.Step();
        simulation.Step();
        Assert.Equal(new GridCell(7, 0), agent.Position);
        Assert.Equal(new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, "yard", agent.Position),
            simulation.GetAgentLocation(1));
    }

    [Fact]
    public void Simulation_BlockedSlipstreamExitRetainsReservationAndSnapshotLocation()
    {
        var agent = new ConstructionAgent(1, new GridCell(0, 0));
        var simulation = new ConstructionSimulation(
            width: 2,
            height: 1,
            requiredHeight: (_, _) => 0,
            agents: [agent],
            emitters: [new PositionedBlockEmitter(new GridCell(0, 0), new BlockEmitter("emitter", 0))],
            spaceId: "yard");
        var transitionGrid = new TrafficCellGrid(2, 1);
        var laneGrid = new TrafficCellGrid(10, 1);
        var slipstream = new Slipstream("yard-to-hover", transitionCellCount: 2, lookaheadCells: 5);
        var lifecycle = new SlipstreamLifecycle(slipstream, transitionGrid);
        Assert.True(simulation.TryRegisterTrafficSpace(ConstructionSpaceKind.Slipstream, "transition", transitionGrid));
        Assert.True(simulation.TryRegisterTrafficSpace(ConstructionSpaceKind.HoverLane, "hover-east", laneGrid));

        var source = new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, "yard", new GridCell(0, 0));
        var entry = new QualifiedGridLocation(ConstructionSpaceKind.Slipstream, "transition", new GridCell(0, 0));
        var destination = new QualifiedGridLocation(ConstructionSpaceKind.HoverLane, "hover-east", new GridCell(8, 0));
        GridCell[] lookahead = [new(2, 0), new(3, 0), new(4, 0), new(5, 0), new(6, 0)];

        Assert.True(simulation.TryEnterSlipstream(1, source, entry, lifecycle, destination, lookahead));
        Assert.True(laneGrid.TryOccupy(destination.Cell, 99));

        Assert.False(simulation.TryExitSlipstream(1, "transition", lifecycle, destination));
        Assert.Equal(1UL, lifecycle.AgentId);
        Assert.Equal(entry, simulation.GetAgentLocation(1));
        Assert.Equal(entry, Assert.Single(simulation.Snapshot().Agents).Location);
        Assert.Equal(entry.Cell, agent.Position);

        Assert.True(laneGrid.Release(destination.Cell, 99));
        Assert.True(simulation.TryExitSlipstream(1, "transition", lifecycle, destination));
        Assert.Equal(destination, simulation.GetAgentLocation(1));
        Assert.Null(lifecycle.AgentId);
    }

    [Fact]
    public void Simulation_AdvancesSkylaneByDeclaredDirectionAndCellsPerTick()
    {
        var agent = new ConstructionAgent(1, new GridCell(0, 0));
        var simulation = new ConstructionSimulation(
            width: 2,
            height: 1,
            requiredHeight: (_, _) => 0,
            agents: [agent],
            emitters: [new PositionedBlockEmitter(new GridCell(0, 0), new BlockEmitter("emitter", 0))],
            spaceId: "yard");
        var transitionGrid = new TrafficCellGrid(1, 1);
        var lane = new Skylane("skylane-east", width: 8, height: 1, LaneDirection.East, elevation: 4, cellsPerTick: 2);
        var lifecycle = new SlipstreamLifecycle(
            new Slipstream("yard-to-skylane", transitionCellCount: 1, lookaheadCells: 5),
            transitionGrid);

        Assert.True(simulation.TryRegisterTrafficSpace(ConstructionSpaceKind.Slipstream, "transition", transitionGrid));
        Assert.True(simulation.TryRegisterTrafficSpace(ConstructionSpaceKind.Skylane, lane.LaneId, lane.Cells));
        var source = new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, "yard", new GridCell(0, 0));
        var entry = new QualifiedGridLocation(ConstructionSpaceKind.Slipstream, "transition", new GridCell(0, 0));
        var destination = new QualifiedGridLocation(ConstructionSpaceKind.Skylane, lane.LaneId, new GridCell(0, 0));
        GridCell[] lookahead = [new(2, 0), new(3, 0), new(4, 0), new(5, 0), new(6, 0)];

        Assert.True(simulation.TryEnterSlipstream(1, source, entry, lifecycle, destination, lookahead));
        Assert.True(simulation.TryExitSlipstream(1, "transition", lifecycle, destination));

        Assert.True(simulation.TryAdvanceSkylane(1, lane.LaneId, lane, new GridCell(7, 0)));
        Assert.Equal(new GridCell(2, 0), agent.Position);
        Assert.Equal(new QualifiedGridLocation(ConstructionSpaceKind.Skylane, lane.LaneId, new GridCell(2, 0)), simulation.GetAgentLocation(1));
        Assert.False(simulation.TryAdvanceSkylane(1, lane.LaneId, lane, new GridCell(0, 0)));
        Assert.Equal(new GridCell(2, 0), agent.Position);
        Assert.True(simulation.TryAdvanceSkylane(1, lane.LaneId, lane, new GridCell(7, 0)));
        Assert.Equal(new GridCell(4, 0), agent.Position);
    }

    [Fact]
    public void Simulation_SkylaneTraversalDoesNotSkipAnOccupiedIntermediateCell()
    {
        var agent = new ConstructionAgent(1, new GridCell(0, 0));
        var simulation = new ConstructionSimulation(
            width: 2,
            height: 1,
            requiredHeight: (_, _) => 0,
            agents: [agent],
            emitters: [new PositionedBlockEmitter(new GridCell(0, 0), new BlockEmitter("emitter", 0))],
            spaceId: "yard");
        var transitionGrid = new TrafficCellGrid(1, 1);
        var lane = new Skylane("skylane-east", width: 8, height: 1, LaneDirection.East, elevation: 4, cellsPerTick: 2);
        var lifecycle = new SlipstreamLifecycle(
            new Slipstream("yard-to-skylane", transitionCellCount: 1, lookaheadCells: 5),
            transitionGrid);

        Assert.True(lane.Cells.TryOccupy(new GridCell(1, 0), 99));
        Assert.True(simulation.TryRegisterTrafficSpace(ConstructionSpaceKind.Slipstream, "transition", transitionGrid));
        Assert.True(simulation.TryRegisterTrafficSpace(ConstructionSpaceKind.Skylane, lane.LaneId, lane.Cells));
        var source = new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, "yard", new GridCell(0, 0));
        var entry = new QualifiedGridLocation(ConstructionSpaceKind.Slipstream, "transition", new GridCell(0, 0));
        var destination = new QualifiedGridLocation(ConstructionSpaceKind.Skylane, lane.LaneId, new GridCell(0, 0));
        GridCell[] lookahead = [new(2, 0), new(3, 0), new(4, 0), new(5, 0), new(6, 0)];

        Assert.True(simulation.TryEnterSlipstream(1, source, entry, lifecycle, destination, lookahead));
        Assert.True(simulation.TryExitSlipstream(1, "transition", lifecycle, destination));

        Assert.False(simulation.TryAdvanceSkylane(1, lane.LaneId, lane, new GridCell(7, 0)));
        Assert.Equal(destination, simulation.GetAgentLocation(1));
        Assert.Equal(new GridCell(0, 0), agent.Position);
        Assert.True(lane.Cells.Release(new GridCell(1, 0), 99));
        Assert.True(simulation.TryAdvanceSkylane(1, lane.LaneId, lane, new GridCell(7, 0)));
        Assert.Equal(new GridCell(2, 0), agent.Position);
    }

    [Fact]
    public void Simulation_SkylaneTraversalRejectsMismatchedLaneIdentityGridAndTarget()
    {
        var agent = new ConstructionAgent(1, new GridCell(0, 0));
        var simulation = new ConstructionSimulation(
            width: 2,
            height: 1,
            requiredHeight: (_, _) => 0,
            agents: [agent],
            emitters: [new PositionedBlockEmitter(new GridCell(0, 0), new BlockEmitter("emitter", 0))],
            spaceId: "yard");
        var transitionGrid = new TrafficCellGrid(1, 1);
        var lane = new Skylane("skylane-east", 8, 1, LaneDirection.East, elevation: 4, cellsPerTick: 2);
        var sameIdDifferentGrid = new Skylane("skylane-east", 8, 1, LaneDirection.East, elevation: 4, cellsPerTick: 2);
        var lifecycle = new SlipstreamLifecycle(
            new Slipstream("yard-to-skylane", transitionCellCount: 1, lookaheadCells: 5),
            transitionGrid);

        Assert.True(simulation.TryRegisterTrafficSpace(ConstructionSpaceKind.Slipstream, "transition", transitionGrid));
        Assert.True(simulation.TryRegisterTrafficSpace(ConstructionSpaceKind.Skylane, lane.LaneId, lane.Cells));
        var source = new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, "yard", new GridCell(0, 0));
        var entry = new QualifiedGridLocation(ConstructionSpaceKind.Slipstream, "transition", new GridCell(0, 0));
        var destination = new QualifiedGridLocation(ConstructionSpaceKind.Skylane, lane.LaneId, new GridCell(0, 0));
        GridCell[] lookahead = [new(2, 0), new(3, 0), new(4, 0), new(5, 0), new(6, 0)];

        Assert.True(simulation.TryEnterSlipstream(1, source, entry, lifecycle, destination, lookahead));
        Assert.True(simulation.TryExitSlipstream(1, "transition", lifecycle, destination));

        Assert.False(simulation.TryAdvanceSkylane(1, "skylane-west", lane, new GridCell(7, 0)));
        Assert.False(simulation.TryAdvanceSkylane(1, lane.LaneId, sameIdDifferentGrid, new GridCell(7, 0)));
        Assert.False(simulation.TryAdvanceSkylane(1, lane.LaneId, lane, new GridCell(8, 0)));
        Assert.Equal(destination, simulation.GetAgentLocation(1));
        Assert.Equal(new GridCell(0, 0), agent.Position);
        Assert.True(simulation.TryAdvanceSkylane(1, lane.LaneId, lane, new GridCell(7, 0)));
        Assert.Equal(new GridCell(2, 0), agent.Position);
    }


    [Fact]
    public void Simulation_RegisteredSlipstreamLifecycleOwnsTransitionAndRejectsSubstitution()
    {
        var agent = new ConstructionAgent(1, new GridCell(0, 0));
        var simulation = new ConstructionSimulation(
            width: 10,
            height: 1,
            requiredHeight: (_, _) => 0,
            agents: [agent],
            emitters: []);
        var transitionGrid = new TrafficCellGrid(3, 1);
        var lifecycle = new SlipstreamLifecycle(
            new Slipstream("resource-east", transitionCellCount: 3, lookaheadCells: 5),
            transitionGrid);
        var substitute = new SlipstreamLifecycle(
            new Slipstream("substitute-east", transitionCellCount: 3, lookaheadCells: 5),
            new TrafficCellGrid(3, 1));

        Assert.True(simulation.TryRegisterTrafficSpace(ConstructionSpaceKind.Slipstream, "transition-east", transitionGrid));
        Assert.False(simulation.TryRegisterSlipstreamLifecycle("transition-east", substitute));
        Assert.True(simulation.TryRegisterSlipstreamLifecycle("transition-east", lifecycle));
        Assert.False(simulation.TryRegisterSlipstreamLifecycle("transition-east", lifecycle));

        var source = new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, simulation.SpaceId, new GridCell(0, 0));
        var entry = new QualifiedGridLocation(ConstructionSpaceKind.Slipstream, "transition-east", new GridCell(0, 0));
        var destination = new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, simulation.SpaceId, new GridCell(8, 0));
        GridCell[] lookahead = [new(2, 0), new(3, 0), new(4, 0), new(5, 0), new(6, 0)];

        Assert.False(simulation.TryEnterSlipstream(1, source, entry, substitute, destination, lookahead));
        Assert.True(simulation.TryEnterSlipstream(1, source, entry, destination, lookahead));
        Assert.False(simulation.TryAdvanceSlipstream(1, "transition-east", substitute, new GridCell(1, 0)));
        Assert.True(simulation.TryAdvanceSlipstream(1, "transition-east", new GridCell(1, 0)));
        Assert.False(simulation.TryExitSlipstream(1, "transition-east", substitute, destination));
        Assert.True(simulation.TryExitSlipstream(1, "transition-east", destination));
        Assert.Equal(destination, simulation.GetAgentLocation(1));
        Assert.Null(lifecycle.AgentId);
    }

}
