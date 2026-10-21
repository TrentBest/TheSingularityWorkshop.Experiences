using TheSingularityWorkshop.Experiences.DroneConstruction;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class QualifiedGridLocationTests
{
    [Fact]
    public void EqualCoordinatesInDifferentSpacesAreDifferentLocations()
    {
        var simulationCell = new QualifiedGridLocation(
            ConstructionSpaceKind.SimulationGrid, "construction-main", new GridCell(2, 3));
        var laneCell = new QualifiedGridLocation(
            ConstructionSpaceKind.Skylane, "skylane-east-01", new GridCell(2, 3));

        Assert.NotEqual(simulationCell, laneCell);
        Assert.Equal(new GridCell(2, 3), simulationCell.Cell);
        Assert.Equal(new GridCell(2, 3), laneCell.Cell);
    }

    [Fact]
    public void SameKindAndSpaceAndCellProduceEqualLocations()
    {
        var first = new QualifiedGridLocation(
            ConstructionSpaceKind.HoverLane, "hover-west", new GridCell(1, 4));
        var second = new QualifiedGridLocation(
            ConstructionSpaceKind.HoverLane, "hover-west", new GridCell(1, 4));

        Assert.Equal(first, second);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void LocationRejectsMissingSpaceIdentity(string spaceId)
    {
        Assert.Throws<ArgumentException>(() => new QualifiedGridLocation(
            ConstructionSpaceKind.SimulationGrid, spaceId, new GridCell(0, 0)));
    }

    [Fact]
    public void LocationRejectsUndefinedSpaceKind()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new QualifiedGridLocation(
            (ConstructionSpaceKind)999, "construction-main", new GridCell(0, 0)));
    }
}
