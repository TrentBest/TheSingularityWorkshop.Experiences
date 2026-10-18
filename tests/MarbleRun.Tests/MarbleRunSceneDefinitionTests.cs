using Xunit;

namespace TheSingularityWorkshop.Experiences.MarbleRun.Tests;

public sealed class MarbleRunSceneDefinitionTests
{
    [Fact]
    public void DerivesMassFromDeclaredHomogeneousSphereDensity()
    {
        var scene = new MarbleRunSceneDefinition("track", "world-v1", 0, -9.81, 0,
            0.01, 1000, 1000, "geometry-v1", "cpu-reference-v1");
        Assert.True(scene.IsValid);
        Assert.Equal(1000 * (4d / 3d) * Math.PI * 0.01 * 0.01 * 0.01, scene.MarbleMassKg(), 12);
    }

    [Fact]
    public void InvalidSceneCannotProduceMass()
    {
        var scene = new MarbleRunSceneDefinition("track", "world-v1", 0, -9.81, 0,
            0, 1000, 100, "geometry-v1", "cpu-reference-v1");
        Assert.False(scene.IsValid);
        Assert.Throws<InvalidOperationException>(() => scene.MarbleMassKg());
    }
}