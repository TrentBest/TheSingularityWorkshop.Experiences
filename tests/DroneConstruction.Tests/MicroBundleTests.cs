using TheSingularityWorkshop.Experiences.DroneConstruction;
using TheSingularityWorkshop.Experiences.DroneConstruction.MicroBundles;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class MicroBundleTests
{
    [Fact]
    public void ImplementedBundlesExposeUniqueStableIdentitiesAndProviders()
    {
        var bundles = new DroneConstructionMicroBundleBase[]
        {
            new HeightFieldMicroBundle(),
            new BuildGridMicroBundle(),
            new ResourceEmitterMicroBundle(),
            new SkylaneMicroBundle(),
            new HoverLaneMicroBundle(),
            new SlipstreamMicroBundle(),
            new TrafficCellMicroBundle()
        };

        Assert.Equal(7, bundles.Select(bundle => bundle.Id).Distinct().Count());
        Assert.Equal(7, bundles.SelectMany(bundle => bundle.Descriptor.Providers).Select(provider => provider.Id).Distinct().Count());
        Assert.All(bundles, bundle => Assert.Empty(bundle.Dependencies));
    }

    [Fact]
    public void BuildGridBundleCreatesBoundedTargetAndCountsCompletion()
    {
        var grid = new BuildGridMicroBundle().Create(2, 1, (x, _) => x + 1);

        Assert.Equal(3, grid.RequiredBlockCount);
        Assert.False(grid.IsComplete);
        Assert.True(grid.TryPlaceBlock(0, 0));
        Assert.True(grid.TryPlaceBlock(1, 0));
        Assert.True(grid.TryPlaceBlock(1, 0));
        Assert.False(grid.TryPlaceBlock(1, 0));
        Assert.True(grid.IsComplete);
    }

    [Fact]
    public void SkylaneAndHoverLaneAreIndependentCellularGrids()
    {
        var sky = new SkylaneMicroBundle().Create("sky-east", 8, 2, LaneDirection.East, elevation: 3);
        var hover = new HoverLaneMicroBundle().Create("hover-east", 4, 3);

        Assert.Equal(LaneDirection.East, sky.Direction);
        Assert.Equal(3, sky.Elevation);
        Assert.Equal(8, sky.Cells.Width);
        Assert.Equal(4, hover.Cells.Width);
        Assert.NotSame(sky.Cells, hover.Cells);
    }

    [Fact]
    public void SlipstreamBundleKeepsFiveCellLookaheadConfigurable()
    {
        var slipstream = new SlipstreamMicroBundle().Create("transition", transitionCellCount: 2);
        Assert.Equal(5, slipstream.LookaheadCells);
        Assert.Equal(2, slipstream.TransitionCellCount);
    }
}
