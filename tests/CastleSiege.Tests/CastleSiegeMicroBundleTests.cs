using TheSingularityWorkshop.Experiences.CastleSiege;
using Xunit;

namespace TheSingularityWorkshop.Experiences.CastleSiege.Tests;

public sealed class CastleSiegeMicroBundleTests
{
    [Fact]
    public void Bundle_ExposesUniqueStableIdentityAndNoUnneededDependencies()
    {
        var bundle = new CastleSiegeMicroBundle();

        Assert.Equal(3301UL, bundle.Id);
        Assert.Equal(CastleSiegeMicroBundle.ExperienceId, bundle.Id);
        Assert.Equal("workshop-castle-siege", Assert.Single(bundle.Descriptor.Providers).Id);
        Assert.Empty(bundle.Dependencies);
    }

    [Fact]
    public void Load_ProducesEntrySurfaceThatDoesNotClaimSimulationExists()
    {
        var bundle = new CastleSiegeMicroBundle();

        bundle.Load(new TestLoadContext());

        Assert.NotNull(bundle.Root);
        Assert.True(bundle.Root!.TryFind("castle-siege-title", out var title));
        Assert.Equal("CASTLE SIEGE", title!.Text);
        Assert.True(bundle.Root.TryFind("castle-siege-physics-status", out var status));
        Assert.Equal("PHYSICS ADAPTER NOT CONNECTED", status!.Text);
        Assert.True(bundle.Root.TryFind("castle-siege-scope", out var scope));
        Assert.Contains("no computed trajectory", scope!.Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_RejectsMissingLoadContext()
    {
        var bundle = new CastleSiegeMicroBundle();

        Assert.Throws<ArgumentNullException>(() => bundle.Load(null!));
    }

    [Fact]
    public void Arbitrate_RejectsMissingContextAndDoesNotMutateComposition()
    {
        var bundle = new CastleSiegeMicroBundle();

        Assert.Throws<ArgumentNullException>(() => bundle.Arbitrate(null!, 0));
        Assert.False(bundle.Arbitrate(new TestArbitrationContext(), 0));
    }

    private sealed class TestLoadContext : TheSingularityWorkshop.MicroBundleDomain.IMicroBundleLoadContext
    {
        public ulong RuntimeId => 3301;

        public bool TryGetConfiguration(ulong bundleId, out ReadOnlyMemory<byte> configuration)
        {
            configuration = default;
            return false;
        }
    }

    private sealed class TestArbitrationContext : TheSingularityWorkshop.MicroBundleDomain.IMicroBundleArbitrationContext
    {
        public ulong RuntimeId => 3301;

        public IReadOnlyList<TheSingularityWorkshop.MicroBundleDomain.IMicroBundle> Bundles => [];

        public object? ExperienceContext => null;
    }
}