using TheSingularityWorkshop.Experiences.Contracts;
using Xunit;

namespace TheSingularityWorkshop.Experiences.Contracts.Tests;

public sealed class SemanticRequirementResolverTests
{
    private static readonly ContractVersion V1 = new(1, 0, 0);
    private static readonly ContractVersion V2 = new(2, 0, 0);
    private static readonly ContractVersionRange V1Range = new(V1, V2);
    private static ArtifactIdentity Artifact(string id, string hashChar = "a") =>
        new(id, "1.0.0", new string(hashChar[0], 64));

    [Fact]
    public void ProviderMatchingIsIndependentOfInputOrder()
    {
        var first = new SemanticCapabilityProvider("windows", Artifact("input.windows"),
            [new("workshop.userio.target-selection", V1)]);
        var second = new SemanticCapabilityProvider("browser", Artifact("input.browser"),
            [new("workshop.userio.target-selection", V1)]);
        var requirement = new SemanticInteractionRequirement("workshop.userio.target-selection", V1Range);

        var result = SemanticRequirementResolver.ResolveProviders([requirement], [first, second]);
        var reversed = SemanticRequirementResolver.ResolveProviders([requirement], [second, first]);

        Assert.False(result.Success);
        Assert.Contains(result.Issues, issue => issue.Code == "provider.ambiguous");
        Assert.Equal(result.Issues, reversed.Issues);
    }

    [Fact]
    public void MultipleCapabilitiesOnSameArtifactResolveToOneProvider()
    {
        var artifact = Artifact("userio.shared");
        var provider = new SemanticCapabilityProvider("shared", artifact,
        [
            new("workshop.userio.target-selection", V1),
            new("workshop.userio.gantry-motion", V1)
        ]);
        var requirements = new[]
        {
            new SemanticInteractionRequirement("workshop.userio.target-selection", V1Range),
            new SemanticInteractionRequirement("workshop.userio.gantry-motion", V1Range)
        };

        var result = SemanticRequirementResolver.ResolveProviders(requirements, [provider]);

        Assert.True(result.Success);
        Assert.Single(result.SelectedProviders);
        Assert.Equal(artifact.StableKey, result.SelectedProviders[0].Artifact.StableKey);
        Assert.Equal(2, result.Selections.Count);
        Assert.Contains(result.Selections, selection => selection.CapabilityId == "workshop.userio.target-selection");
        Assert.Contains(result.Selections, selection => selection.CapabilityId == "workshop.userio.gantry-motion");
    }

    [Fact]
    public void MissingRequiredCapabilityFailsButMissingOptionalCapabilityDoesNot()
    {
        var missing = new SemanticInteractionRequirement("workshop.userio.rotation", V1Range);
        var required = SemanticRequirementResolver.ResolveProviders([missing], []);
        var optional = SemanticRequirementResolver.ResolveProviders(
            [missing with { Required = false }], []);

        Assert.False(required.Success);
        Assert.Contains(required.Issues, issue => issue.Code == "provider.missing");
        Assert.True(optional.Success);
    }

    [Fact]
    public void IncompatibleVersionDoesNotSatisfyRequirement()
    {
        var provider = new SemanticCapabilityProvider("v2", Artifact("input.v2"),
            [new("workshop.userio.target-selection", V2)]);
        var result = SemanticRequirementResolver.ResolveProviders(
            [new("workshop.userio.target-selection", V1Range)], [provider]);

        Assert.False(result.Success);
        Assert.Contains(result.Issues, issue => issue.Code == "provider.missing");
    }

    [Fact]
    public void InvalidArtifactHashCannotSatisfyRequirement()
    {
        var provider = new SemanticCapabilityProvider("bad", new("input", "1.0.0", "not-a-hash"),
            [new("workshop.userio.target-selection", V1)]);
        var result = SemanticRequirementResolver.ResolveProviders(
            [new("workshop.userio.target-selection", V1Range)], [provider]);

        Assert.False(result.Success);
        Assert.Contains(result.Issues, issue => issue.Code == "provider.missing");
    }

    [Fact]
    public void ArtifactClosureIsDeterministicAndDependenciesPrecedeDependents()
    {
        var root = Artifact("experience");
        var dependency = Artifact("physics", "b");
        var catalog = new[]
        {
            new ArtifactDescriptor(root, [new(dependency)]),
            new ArtifactDescriptor(dependency, [])
        };

        var result = SemanticRequirementResolver.ResolveArtifactClosure([root], catalog.Reverse());

        Assert.True(result.Success);
        Assert.Equal(new[] { dependency.StableKey, root.StableKey },
            result.OrderedArtifacts.Select(item => item.Identity.StableKey));
    }

    [Fact]
    public void MissingOrWrongHashDependencyFailsClosure()
    {
        var root = Artifact("experience");
        var dependency = Artifact("physics", "b");
        var wrongHash = Artifact("physics", "c");
        var result = SemanticRequirementResolver.ResolveArtifactClosure(
            [root],
            [new ArtifactDescriptor(root, [new(dependency)]), new ArtifactDescriptor(wrongHash, [])]);

        Assert.False(result.Success);
        Assert.Contains(result.Issues, issue => issue.Code == "artifact.missing-dependency");
    }

    [Fact]
    public void DependencyCycleFailsClosure()
    {
        var a = Artifact("a");
        var b = Artifact("b", "b");
        var result = SemanticRequirementResolver.ResolveArtifactClosure(
            [a],
            [new ArtifactDescriptor(a, [new(b)]), new ArtifactDescriptor(b, [new(a)])]);

        Assert.False(result.Success);
        Assert.Contains(result.Issues, issue => issue.Code == "artifact.dependency-cycle");
    }

    [Fact]
    public void DuplicateArtifactDescriptorIsRejected()
    {
        var artifact = Artifact("same");
        var result = SemanticRequirementResolver.ResolveArtifactClosure(
            [artifact],
            [new ArtifactDescriptor(artifact, []), new ArtifactDescriptor(artifact, [])]);

        Assert.False(result.Success);
        Assert.Contains(result.Issues, issue => issue.Code == "artifact.duplicate-identity");
    }
}
