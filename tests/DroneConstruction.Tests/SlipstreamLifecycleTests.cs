using TheSingularityWorkshop.Experiences.DroneConstruction;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class SlipstreamLifecycleTests
{
    [Fact]
    public void Exit_WhenDestinationIsBlocked_PreservesTransitionOccupancyAndReservationForRetry()
    {
        var agent = new ConstructionAgent(1, new GridCell(0, 0));
        var simulation = new ConstructionSimulation(12, 1, (_, _) => 0, [agent], [], "yard");
        Assert.True(agent.TryAssign(new PlacementTask(1, 0, 0, 1)));
        Assert.True(agent.TryTransition(ConstructionAgentState.CarryingBlock));
        Assert.True(agent.TryTransition(ConstructionAgentState.AwaitingTransition));

        var transitionGrid = new TrafficCellGrid(3, 1);
        var destinationGrid = new TrafficCellGrid(12, 1);
        var slipstream = new Slipstream("east-resource", 3, 5);
        var lifecycle = new SlipstreamLifecycle(slipstream, transitionGrid);

        Assert.True(simulation.TryRegisterTrafficSpace(
            ConstructionSpaceKind.Slipstream, "east-transition", transitionGrid));
        Assert.True(simulation.TryRegisterTrafficSpace(
            ConstructionSpaceKind.Skylane, "east-lane", destinationGrid));
        Assert.True(simulation.TryRegisterSlipstreamLifecycle("east-transition", lifecycle));

        var source = new QualifiedGridLocation(
            ConstructionSpaceKind.SimulationGrid, "yard", new GridCell(0, 0));
        var entry = new QualifiedGridLocation(
            ConstructionSpaceKind.Slipstream, "east-transition", new GridCell(0, 0));
        var destination = new QualifiedGridLocation(
            ConstructionSpaceKind.Skylane, "east-lane", new GridCell(8, 0));
        GridCell[] lookahead =
        [
            new GridCell(2, 0), new GridCell(3, 0), new GridCell(4, 0),
            new GridCell(5, 0), new GridCell(6, 0)
        ];

        Assert.True(simulation.TryEnterSlipstream(agent.AgentId, source, entry, destination, lookahead));
        Assert.True(simulation.TryAdvanceSlipstream(agent.AgentId, "east-transition", new GridCell(1, 0)));
        Assert.True(simulation.TryAdvanceSlipstream(agent.AgentId, "east-transition", new GridCell(2, 0)));
        Assert.Equal(1UL, slipstream.ReservedBy);

        // Occupy the reserved destination after admission to simulate a conflict or violated
        // external reservation discipline; exit must still fail without losing the agent.
        Assert.True(destinationGrid.TryOccupy(destination.Cell, 77));
        Assert.False(simulation.TryExitSlipstream(agent.AgentId, "east-transition", destination));
        Assert.Equal(new QualifiedGridLocation(
            ConstructionSpaceKind.Slipstream, "east-transition", new GridCell(2, 0)),
            simulation.GetAgentLocation(agent.AgentId));
        Assert.True(transitionGrid.TryGetOccupant(new GridCell(2, 0), out var occupant));
        Assert.Equal(agent.AgentId, occupant);
        Assert.Equal(1UL, slipstream.ReservedBy);

        Assert.True(destinationGrid.Release(destination.Cell, 77));
        Assert.True(simulation.TryExitSlipstream(agent.AgentId, "east-transition", destination));
        Assert.Equal(destination, simulation.GetAgentLocation(agent.AgentId));
        Assert.Null(slipstream.ReservedBy);
        Assert.True(destinationGrid.TryGetOccupant(destination.Cell, out occupant));
        Assert.Equal(agent.AgentId, occupant);
    }
}
