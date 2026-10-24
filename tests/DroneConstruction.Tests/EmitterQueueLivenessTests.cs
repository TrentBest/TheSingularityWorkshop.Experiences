using TheSingularityWorkshop.Experiences.DroneConstruction;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class EmitterQueueLivenessTests
{
    [Fact]
    public void Simulation_AllowsHigherIdAgentAheadOfLowerIdAgentToClearEmitterApproach()
    {
        // Agent 1 has priority by ID but starts behind agent 2 on the only approach lane.
        // A global "wait for every lower ID" rule deadlocks both agents in this arrangement.
        var agents = new[]
        {
            new ConstructionAgent(1, new GridCell(2, 1)),
            new ConstructionAgent(2, new GridCell(3, 1))
        };
        var simulation = new ConstructionSimulation(
            width: 5,
            height: 3,
            requiredHeight: (x, y) => x == 4 && y != 1 ? 1 : 0,
            agents: agents,
            emitters: [new PositionedBlockEmitter(new GridCell(4, 1), new BlockEmitter("shared-emitter", 2))]);

        var snapshot = simulation.Snapshot();
        for (var i = 0; i < 80 && !snapshot.IsComplete; i++)
            snapshot = simulation.Step();

        Assert.True(snapshot.IsComplete);
        Assert.Equal(2, snapshot.PlacedBlocks);
        Assert.Equal(0, Assert.Single(snapshot.Emitters).Inventory);
        Assert.All(snapshot.Agents, agent =>
        {
            Assert.Equal(ConstructionAgentState.Idle, agent.State);
            Assert.False(agent.IsCarryingBlock);
            Assert.Null(agent.AssignedTaskId);
        });
    }

    [Fact]
    public void Simulation_ThreeAgentsQueuedOnNarrowApproach_AllCompleteWithoutPriorityDeadlock()
    {
        // Three agents share one narrow approach. The highest-ID agent starts nearest the
        // emitter; stable ID iteration must not force it to wait for agents physically behind it.
        // All three blocks stack at one target so this test isolates emitter-queue liveness from
        // unrelated route crossings between multiple target rows.
        var agents = new[]
        {
            new ConstructionAgent(1, new GridCell(1, 2)),
            new ConstructionAgent(2, new GridCell(2, 2)),
            new ConstructionAgent(3, new GridCell(3, 2))
        };
        var simulation = new ConstructionSimulation(
            width: 6,
            height: 5,
            requiredHeight: (x, y) => x == 5 && y == 2 ? 3 : 0,
            agents: agents,
            emitters: [new PositionedBlockEmitter(new GridCell(4, 2), new BlockEmitter("shared-emitter", 3))]);

        var snapshot = simulation.Snapshot();
        for (var i = 0; i < 160 && !snapshot.IsComplete; i++)
            snapshot = simulation.Step();

        Assert.True(snapshot.IsComplete);
        Assert.Equal(3, snapshot.PlacedBlocks);
        Assert.Equal(0, Assert.Single(snapshot.Emitters).Inventory);
        Assert.All(snapshot.Agents, agent =>
        {
            Assert.Equal(ConstructionAgentState.Idle, agent.State);
            Assert.False(agent.IsCarryingBlock);
            Assert.Null(agent.AssignedTaskId);
        });
    }
}
