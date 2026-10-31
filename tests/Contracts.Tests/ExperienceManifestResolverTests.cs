using TheSingularityWorkshop.Experiences.Contracts;
using Xunit;

namespace TheSingularityWorkshop.Experiences.Contracts.Tests;

public sealed class ExperienceManifestResolverTests
{
    private static ArtifactIdentity Artifact(string id, char hash = 'a') =>
        new(id, "1.0.0", new string(hash, 64));

    private static ExperienceManifest Manifest(
        IReadOnlyList<ArtifactIdentity>? roots = null,
        IReadOnlyList<SemanticInteractionRequirement>? requirements = null) =>
        new()
        {
            ExperienceId = 4201,
            ExperienceKey = "minesweeper",
            Version = "0.1.0",
            Title = "Minesweeper",
            Artifacts = roots ?? [Artifact("minesweeper.experience")],
            Requirements = requirements ?? []
        };

    [Fact]
    public void ResolvesManifestRootsAndSelectedProviderDependencies()
    {
        var experience = Artifact("minesweeper.experience");
        var rules = Artifact("minesweeper.rules", 'b');
        var input = Artifact("desktop.pointer", 'c');
        var requirement = new SemanticInteractionRequirement(
            "workshop.input.pointer", new(new(1, 0, 0), new(2, 0, 0)));
        var provider = new SemanticCapabilityProvider(
            "desktop-pointer", input, [new("workshop.input.pointer", new(1, 0, 0))]);
        var catalog = new[]
        {
            new ArtifactDescriptor(experience, [new(rules)]),
            new ArtifactDescriptor(rules, []),
            new ArtifactDescriptor(input, [])
        };

        var result = ExperienceManifestResolver.Resolve(
            Manifest([experience], [requirement]), [provider], catalog);

        Assert.True(result.Success);
        Assert.Empty(result.Issues);
        Assert.Single(result.CapabilitySelections);
        Assert.Equal(
            new[] { rules.StableKey, experience.StableKey, input.StableKey }.OrderBy(x => x, StringComparer.Ordinal),
            result.OrderedArtifacts.Select(item => item.Identity.StableKey).OrderBy(x => x, StringComparer.Ordinal));
        Assert.True(result.OrderedArtifacts.IndexOf(rules) < 0);
    }

    [Fact]
    public void RequiredCapabilityWithoutProviderFailsResolution()
    {
        var requirement = new SemanticInteractionRequirement(
            "workshop.input.pointer", new(new(1, 0, 0)), Required: true);

        var result = ExperienceManifestResolver.Resolve(
            Manifest(requirements: [requirement]),
            [],
            [new ArtifactDescriptor(Artifact("minesweeper.experience"), [])]);

        Assert.False(result.Success);
        Assert.Contains(result.Issues, issue => issue.Code == "provider.missing");
        Assert.Empty(result.OrderedArtifacts);
    }

    [Fact]
    public void MissingOptionalCapabilityDoesNotPreventArtifactResolution()
    {
        var requirement = new SemanticInteractionRequirement(
            "workshop.input.haptics", new(new(1, 0, 0)), Required: false);
        var root = Artifact("minesweeper.experience");

        var result = ExperienceManifestResolver.Resolve(
            Manifest([root], [requirement]),
            [],
            [new ArtifactDescriptor(root, [])]);

        Assert.True(result.Success);
        Assert.Empty(result.CapabilitySelections);
        Assert.Single(result.OrderedArtifacts);
    }

    [Fact]
    public void MissingManifestArtifactFailsWithClosureDiagnostic()
    {
        var root = Artifact("minesweeper.experience");

        var result = ExperienceManifestResolver.Resolve(
            Manifest([root]),
            [],
            []);

        Assert.False(result.Success);
        Assert.Contains(result.Issues, issue => issue.Code == "artifact.missing-dependency");
    }

    [Fact]
    public void AmbiguousCapabilityProvidersFailInsteadOfChoosingArbitrarily()
    {
        var root = Artifact("minesweeper.experience");
        var first = Artifact("input.first", 'b');
        var second = Artifact("input.second", 'c');
        var requirement = new SemanticInteractionRequirement(
            "workshop.input.pointer", new(new(1, 0, 0)));
        var providers = new[]
        {
            new SemanticCapabilityProvider("first", first, [new("workshop.input.pointer", new(1, 0, 0))]),
            new SemanticCapabilityProvider("second", second, [new("workshop.input.pointer", new(1, 0, 0))])
        };
        var catalog = new[]
        {
            new ArtifactDescriptor(root, []),
            new ArtifactDescriptor(first, []),
            new ArtifactDescriptor(second, [])
        };

        var result = ExperienceManifestResolver.Resolve(
            Manifest([root], [requirement]), providers, catalog);

        Assert.False(result.Success);
        Assert.Contains(result.Issues, issue => issue.Code == "provider.ambiguous");
    }

    [Fact]
    public void InvalidManifestReturnsStructuredIssue()
    {
        var invalid = Manifest() with { ExperienceId = 0 };

        var result = ExperienceManifestResolver.Resolve(invalid, [], []);

        Assert.False(result.Success);
        Assert.Contains(result.Issues, issue => issue.Code == "manifest.invalid");
    }

    [Fact]
    public void ResolutionIsIndependentOfCatalogAndProviderInputOrder()
    {
        var rootA = Artifact("experience.a");
        var rootB = Artifact("experience.b", 'b');
        var providerArtifact = Artifact("input.pointer", 'c');
        var requirement = new SemanticInteractionRequirement(
            "workshop.input.pointer", new(new(1, 0, 0)));
        var provider = new SemanticCapabilityProvider(
            "pointer", providerArtifact, [new("workshop.input.pointer", new(1, 0, 0))]);
        var catalog = new[]
        {
            new ArtifactDescriptor(rootA, []),
            new ArtifactDescriptor(rootB, []),
            new ArtifactDescriptor(providerArtifact, [])
        };
        var manifest = Manifest([rootA, rootB], [requirement]);

        var first = ExperienceManifestResolver.Resolve(manifest, [provider], catalog);
        var reversed = ExperienceManifestResolver.Resolve(manifest, [provider], catalog.Reverse());

        Assert.True(first.Success);
        Assert.True(reversed.Success);
        Assert.Equal(
            first.OrderedArtifacts.Select(item => item.Identity.StableKey),
            reversed.OrderedArtifacts.Select(item => item.Identity.StableKey));
    }
}
