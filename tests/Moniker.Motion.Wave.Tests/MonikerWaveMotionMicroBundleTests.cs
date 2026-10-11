using TheSingularityWorkshop.Experiences.Moniker.Motion.Wave;
using TheSingularityWorkshop.Workshop.Gui;
using Xunit;

namespace TheSingularityWorkshop.Experiences.Moniker.Motion.Wave.Tests;

public sealed class MonikerWaveMotionMicroBundleTests
{
    [Fact]
    public void Bundle_ExposesItsOwnStableIdentity()
    {
        var bundle = new MonikerWaveMotionMicroBundle();

        Assert.Equal(3102UL, bundle.Id);
        Assert.Equal("workshop-moniker-motion-wave", bundle.Descriptor.Providers.Single().Id);
        Assert.Empty(bundle.Dependencies);
    }

    [Fact]
    public void Apply_AddsWaveParametersOnlyToMonikerGlyphs_WithoutMutatingSource()
    {
        var root = GuiBuilders.Panel("moniker-root")
            .Child(GuiBuilders.Text("moniker-word-0", "T"))
            .Child(GuiBuilders.Text("unrelated-label", "Hello"))
            .Build();

        var result = new MonikerWaveMotionMicroBundle().Apply(root);

        Assert.True(result.TryFind("moniker-word-0", out var movingGlyph));
        Assert.Equal("3.8", movingGlyph!.Properties["wavePeriod"]);
        Assert.Equal("7", movingGlyph.Properties["waveAmplitude"]);
        Assert.Equal("2.5", movingGlyph.Properties["waveRotation"]);
        Assert.Equal("T", movingGlyph.Text);
        Assert.False(root.Find("moniker-word-0").Properties.ContainsKey("wavePeriod"));

        Assert.True(result.TryFind("unrelated-label", out var unrelated));
        Assert.False(unrelated!.Properties.ContainsKey("wavePeriod"));
        Assert.False(root.Find("unrelated-label").Properties.ContainsKey("wavePeriod"));
    }

    [Fact]
    public void Apply_AssignsDeterministicPhasesAcrossGlyphs()
    {
        var root = GuiBuilders.Panel("moniker-root")
            .Child(GuiBuilders.Text("moniker-glyph-0", "A"))
            .Child(GuiBuilders.Text("moniker-glyph-1", "B"))
            .Build();

        var motion = new MonikerWaveMotionMicroBundle();
        var first = motion.Apply(root);
        var second = motion.Apply(root);

        Assert.True(first.TryFind("moniker-glyph-0", out var firstGlyph));
        Assert.True(second.TryFind("moniker-glyph-0", out var secondGlyph));
        Assert.Equal(firstGlyph!.Properties["wavePhase"], secondGlyph!.Properties["wavePhase"]);
        Assert.True(first.TryFind("moniker-glyph-1", out var nextGlyph));
        Assert.NotEqual(firstGlyph.Properties["wavePhase"], nextGlyph!.Properties["wavePhase"]);
    }
}
