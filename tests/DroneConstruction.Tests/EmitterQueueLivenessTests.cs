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
        // Targets are distinct and offset beyond the emitter so completed agents do not occupy
        // a shared target cell or block the queue's approach lane.
        var agents = new[]
        {
            new ConstructionAgent(1, new GridCell(1, 2)),
            new ConstructionAgent(2, new GridCell(2, 2)),
            new ConstructionAgent(3, new GridCell(3, 2))
        };
        var simulation = new ConstructionSimulation(
            width: 8,
            height: 5,
            requiredHeight: (x, y) =>
                (x == 5 && y == 0) || (x == 7 && y == 1) || (x == 6 && y == 4) ? 1 : 0,
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

    [Fact]
    public void Simulation_SharedEmitterAndRepeatedAssignments_EveryAgentCompletesWork()
    {
        // More tasks than agents forces repeated assignments. Count each agent's committed
        // completions so system-wide completion cannot conceal an agent receiving no service.
        var agents = new[]
        {
            new ConstructionAgent(1, new GridCell(0, 3)),
            new ConstructionAgent(2, new GridCell(1, 3)),
            new ConstructionAgent(3, new GridCell(2, 3))
        };
        var simulation = new ConstructionSimulation(
            width: 12,
            height: 4,
            requiredHeight: (x, y) => y == 0 ? 1 : 0,
            agents: agents,
            emitters: [new PositionedBlockEmitter(new GridCell(0, 2), new BlockEmitter("shared-emitter", 12))]);

        var completionsByAgent = agents.ToDictionary(agent => agent.AgentId, _ => 0);
        var snapshot = simulation.Snapshot();
        for (var tick = 0; tick < 2_000 && !snapshot.IsComplete; tick++)
        {
            var previousStates = snapshot.Agents.ToDictionary(agent => agent.AgentId, agent => agent.State);
            snapshot = simulation.Step();
            foreach (var agent in snapshot.Agents)
            {
                if (previousStates[agent.AgentId] == ConstructionAgentState.PlacingBlock
                    && agent.State == ConstructionAgentState.Idle)
                    completionsByAgent[agent.AgentId]++;
            }
        }

        Assert.True(snapshot.IsComplete,
            $"Expected all 12 tasks to complete under shared-emitter contention; placed {snapshot.PlacedBlocks} at tick {snapshot.Tick}.");
        Assert.Equal(12, snapshot.PlacedBlocks);
        Assert.Equal(0, Assert.Single(snapshot.Emitters).Inventory);
        Assert.All(completionsByAgent, entry =>
            Assert.True(entry.Value > 0, $"Agent {entry.Key} did not complete any placement task."));
        Assert.Equal(12, completionsByAgent.Values.Sum());
        Assert.All(snapshot.Agents, agent =>
        {
            Assert.Equal(ConstructionAgentState.Idle, agent.State);
            Assert.False(agent.IsCarryingBlock);
            Assert.Null(agent.AssignedTaskId);
        });
    }
}
