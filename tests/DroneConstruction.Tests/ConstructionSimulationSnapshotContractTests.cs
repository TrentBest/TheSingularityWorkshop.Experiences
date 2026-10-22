using System.Collections.Generic;
using TheSingularityWorkshop.Experiences.DroneConstruction;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class ConstructionSimulationSnapshotContractTests
{
    [Fact]
    public void Snapshot_RemainsAStableObservationAfterLaterTicks()
    {
        var simulation = new ConstructionSimulation(
            width: 3,
            height: 1,
            requiredHeight: (x, _) => x == 2 ? 1 : 0,
            agents: [new ConstructionAgent(7, new GridCell(0, 0))],
            emitters: [new PositionedBlockEmitter(new GridCell(0, 0), new BlockEmitter("emitter-01", 1))]);

        var observed = simulation.Snapshot();
        var observedAgent = Assert.Single(observed.Agents);
        var observedEmitter = Assert.Single(observed.Emitters);
        var observedCells = observed.CellPlacedBlocks.ToArray();

        for (var i = 0; i < 8 && !simulation.Snapshot().IsComplete; i++)
            simulation.Step();

        Assert.Equal(0, observed.Tick);
        Assert.Equal(0, observed.PlacedBlocks);
        Assert.False(observed.IsComplete);
        Assert.Equal(new GridCell(0, 0), observedAgent.Position);
        Assert.Equal(1, observedEmitter.Inventory);
        Assert.Equal(new[] { 0, 0, 0 }, observed.CellPlacedBlocks);
        Assert.Equal(observedCells, observed.CellPlacedBlocks);
    }

    [Fact]
    public void Snapshot_CollectionsRejectMutationThroughMutableInterfaces()
    {
        var simulation = new ConstructionSimulation(
            width: 1,
            height: 1,
            requiredHeight: (_, _) => 0,
            agents: [new ConstructionAgent(7, new GridCell(0, 0))],
            emitters: [new PositionedBlockEmitter(new GridCell(0, 0), new BlockEmitter("emitter-01", 1))]);

        var snapshot = simulation.Snapshot();
        var agents = Assert.IsAssignableFrom<IList<ConstructionAgentSnapshot>>(snapshot.Agents);
        var emitters = Assert.IsAssignableFrom<IList<ConstructionEmitterSnapshot>>(snapshot.Emitters);
        var cells = Assert.IsAssignableFrom<IList<int>>(snapshot.CellPlacedBlocks);

        Assert.True(agents.IsReadOnly);
        Assert.True(emitters.IsReadOnly);
        Assert.True(cells.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => agents.Add(new ConstructionAgentSnapshot(
            8, new GridCell(0, 0), ConstructionAgentState.Idle, false, null,
            new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, simulation.SpaceId, new GridCell(0, 0)))));
        Assert.Throws<NotSupportedException>(() => emitters.Clear());
        Assert.Throws<NotSupportedException>(() => cells[0] = 99);
    }
}
