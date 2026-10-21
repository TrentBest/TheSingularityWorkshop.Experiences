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
        Assert.Equal(snapshot.Tick, simulation.TickCount);
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

}
