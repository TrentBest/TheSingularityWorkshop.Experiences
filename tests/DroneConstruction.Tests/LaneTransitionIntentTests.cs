using TheSingularityWorkshop.Experiences.DroneConstruction;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class LaneTransitionIntentTests
{
    [Fact]
    public void Intent_PreservesQualifiedSpacesAndCopiesLookahead()
    {
        var source = new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, "yard", new GridCell(0, 0));
        var entry = new QualifiedGridLocation(ConstructionSpaceKind.Slipstream, "east-transition", new GridCell(0, 0));
        var destination = new QualifiedGridLocation(ConstructionSpaceKind.Skylane, "east-lane", new GridCell(8, 0));
        var lookahead = new[] { new GridCell(2, 0), new GridCell(3, 0) };

        var intent = new LaneTransitionIntent(source, entry, destination, lookahead);
        lookahead[0] = new GridCell(99, 99);

        Assert.Equal(source, intent.Source);
        Assert.Equal(entry, intent.TransitionEntry);
        Assert.Equal(destination, intent.Destination);
        Assert.Equal(new[] { new GridCell(2, 0), new GridCell(3, 0) }, intent.Lookahead);
    }

    [Fact]
    public void Intent_RejectsInvalidSpaceRoles()
    {
        var main = new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, "yard", new GridCell(0, 0));
        var transition = new QualifiedGridLocation(ConstructionSpaceKind.Slipstream, "transition", new GridCell(0, 0));
        var lane = new QualifiedGridLocation(ConstructionSpaceKind.Skylane, "lane", new GridCell(2, 0));
        var lookahead = new[] { new GridCell(1, 0) };

        Assert.Throws<ArgumentException>(() => new LaneTransitionIntent(transition, transition, lane, lookahead));
        Assert.Throws<ArgumentException>(() => new LaneTransitionIntent(main, main, lane, lookahead));
        Assert.Throws<ArgumentException>(() => new LaneTransitionIntent(main, transition, transition, lookahead));
    }

    [Fact]
    public void Intent_RejectsEmptyOrDuplicateLookahead()
    {
        var source = new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, "yard", new GridCell(0, 0));
        var entry = new QualifiedGridLocation(ConstructionSpaceKind.Slipstream, "transition", new GridCell(0, 0));
        var destination = new QualifiedGridLocation(ConstructionSpaceKind.Skylane, "lane", new GridCell(2, 0));

        Assert.Throws<ArgumentException>(() => new LaneTransitionIntent(source, entry, destination, Array.Empty<GridCell>()));
        Assert.Throws<ArgumentException>(() => new LaneTransitionIntent(
            source, entry, destination, new[] { new GridCell(1, 0), new GridCell(1, 0) }));
    }
}
