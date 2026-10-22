using TheSingularityWorkshop.Experiences.DroneConstruction;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class ConstructionSpaceCatalogTests
{
    [Fact]
    public void ResolvesAgentLocationFromTheGridThatCurrentlyOwnsIt()
    {
        var simulationGrid = new TrafficCellGrid(3, 1);
        var hoverGrid = new TrafficCellGrid(3, 1);
        var catalog = new ConstructionSpaceCatalog();
        Assert.True(catalog.TryRegister(ConstructionSpaceKind.SimulationGrid, "yard", simulationGrid));
        Assert.True(catalog.TryRegister(ConstructionSpaceKind.HoverLane, "hover-east", hoverGrid));
        Assert.True(simulationGrid.TryOccupy(new GridCell(0, 0), 7));

        Assert.True(catalog.TryGetAgentLocation(7, out var initial));
        Assert.Equal(new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, "yard", new GridCell(0, 0)), initial);

        Assert.True(simulationGrid.TryTransferTo(new GridCell(0, 0), new GridCell(2, 0), 7, hoverGrid));

        Assert.True(catalog.TryGetAgentLocation(7, out var moved));
        Assert.Equal(new QualifiedGridLocation(ConstructionSpaceKind.HoverLane, "hover-east", new GridCell(2, 0)), moved);
    }

    [Fact]
    public void RejectsDuplicateSpaceIdentityAndGridAliases()
    {
        var grid = new TrafficCellGrid(1, 1);
        var catalog = new ConstructionSpaceCatalog();

        Assert.True(catalog.TryRegister(ConstructionSpaceKind.SimulationGrid, "yard", grid));
        Assert.False(catalog.TryRegister(ConstructionSpaceKind.SimulationGrid, "yard", new TrafficCellGrid(1, 1)));
        Assert.False(catalog.TryRegister(ConstructionSpaceKind.HoverLane, "yard", grid));
        Assert.Equal(1, catalog.Count);
    }

    [Fact]
    public void DoesNotGuessWhenAnAgentIsMissingOrOccupiesTwoRegisteredSpaces()
    {
        var first = new TrafficCellGrid(1, 1);
        var second = new TrafficCellGrid(1, 1);
        var catalog = new ConstructionSpaceCatalog();
        Assert.True(catalog.TryRegister(ConstructionSpaceKind.SimulationGrid, "one", first));
        Assert.True(catalog.TryRegister(ConstructionSpaceKind.Skylane, "two", second));

        Assert.False(catalog.TryGetAgentLocation(7, out var missing));
        Assert.Null(missing);
        Assert.Throws<KeyNotFoundException>(() => catalog.GetAgentLocation(7));

        Assert.True(first.TryOccupy(new GridCell(0, 0), 7));
        Assert.True(second.TryOccupy(new GridCell(0, 0), 7));
        Assert.False(catalog.TryGetAgentLocation(7, out var ambiguous));
        Assert.Null(ambiguous);
        Assert.Throws<KeyNotFoundException>(() => catalog.GetAgentLocation(7));
    }

    [Fact]
    public void ResolvesOnlyRegisteredSpacesAndInBoundsCells()
    {
        var grid = new TrafficCellGrid(2, 1);
        var catalog = new ConstructionSpaceCatalog();
        Assert.True(catalog.TryRegister(ConstructionSpaceKind.SimulationGrid, "yard", grid));

        Assert.True(catalog.TryResolve(new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, "yard", new GridCell(1, 0)), out var resolved));
        Assert.Same(grid, resolved);
        Assert.False(catalog.TryResolve(new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, "unknown", new GridCell(0, 0)), out _));
        Assert.False(catalog.TryResolve(new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, "yard", new GridCell(2, 0)), out _));
    }

    [Fact]
    public void RejectsInvalidRegistrationAndAgentIdentity()
    {
        var catalog = new ConstructionSpaceCatalog();
        var grid = new TrafficCellGrid(1, 1);

        Assert.Throws<ArgumentOutOfRangeException>(() => catalog.TryRegister((ConstructionSpaceKind)999, "yard", grid));
        Assert.Throws<ArgumentException>(() => catalog.TryRegister(ConstructionSpaceKind.SimulationGrid, " ", grid));
        Assert.Throws<ArgumentNullException>(() => catalog.TryRegister(ConstructionSpaceKind.SimulationGrid, "yard", null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => catalog.TryGetAgentLocation(0));
    }
}
