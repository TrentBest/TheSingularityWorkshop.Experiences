using TheSingularityWorkshop.Experiences.Contracts;
using Xunit;

namespace TheSingularityWorkshop.Experiences.Contracts.Tests;

public sealed class ExperienceArtifactBindingPlanTests
{
    private static ArtifactIdentity Artifact(string id, char hash = 'a') =>
        new(id, "1.0.0", new string(hash, 64));

    private static ExperienceResolution Resolution(params ArtifactIdentity[] artifacts) =>
        new(true, [], artifacts.Select(item => new ArtifactDescriptor(item, [])).ToArray(), []);

    [Fact]
    public void CreatesCompletePlanInResolvedDependencyOrder()
    {
        var dependency = Artifact("runtime.core", 'b');
        var root = Artifact("sample.experience", 'a');
        var map = MicroBundleIdentityMap.Create([
            new(dependency, 42),
            new(root, 84)
        ]);

        var result = ExperienceArtifactBindingPlan.Create(
            Resolution(dependency, root), map);

        Assert.True(result.Success);
        Assert.Empty(result.Issues);
        Assert.Equal(
            new[] { dependency.StableKey, root.StableKey },
            result.OrderedBindings.Select(binding => binding.Artifact.StableKey));
        Assert.Equal(new ulong[] { 42, 84 },
            result.OrderedBindings.Select(binding => binding.LocalHandle));
    }

    [Fact]
    public void MissingBindingFailsTheWholePlanInsteadOfSkippingArtifact()
    {
        var dependency = Artifact("runtime.core", 'b');
        var root = Artifact("sample.experience", 'a');
        var map = MicroBundleIdentityMap.Create([new(root, 84)]);

        var result = ExperienceArtifactBindingPlan.Create(
            Resolution(dependency, root), map);

        Assert.False(result.Success);
        Assert.Empty(result.OrderedBindings);
        Assert.Contains(result.Issues, issue => issue.Code == "binding.unresolved-artifact");
    }

    [Fact]
    public void FailedResolutionCannotProduceBindingPlan()
    {
        var resolution = new ExperienceResolution(
            false, [], [], [new("artifact.missing-dependency", "Missing dependency.")]);
        var map = MicroBundleIdentityMap.Create([]);

        var result = ExperienceArtifactBindingPlan.Create(resolution, map);

        Assert.False(result.Success);
        Assert.Empty(result.OrderedBindings);
        Assert.Contains(result.Issues, issue => issue.Code == "artifact.missing-dependency");
    }

    [Fact]
    public void DuplicateArtifactInResolvedClosureFailsClosed()
    {
        var artifact = Artifact("sample.experience");
        var map = MicroBundleIdentityMap.Create([new(artifact, 84)]);

        var result = ExperienceArtifactBindingPlan.Create(
            Resolution(artifact, artifact), map);

        Assert.False(result.Success);
        Assert.Empty(result.OrderedBindings);
        Assert.Contains(result.Issues, issue => issue.Code == "binding.duplicate-artifact");
    }

    [Fact]
    public void InvalidResolvedArtifactFailsClosed()
    {
        var invalid = new ArtifactIdentity(" ", "1.0.0", new string('a', 64));
        var map = MicroBundleIdentityMap.Create([]);

        var result = ExperienceArtifactBindingPlan.Create(Resolution(invalid), map);

        Assert.False(result.Success);
        Assert.Empty(result.OrderedBindings);
        Assert.Contains(result.Issues, issue => issue.Code == "binding.invalid-artifact");
    }

    [Fact]
    public void EmptySuccessfulClosureFailsClosed()
    {
        var result = ExperienceArtifactBindingPlan.Create(
            Resolution(), MicroBundleIdentityMap.Create([]));

        Assert.False(result.Success);
        Assert.Empty(result.OrderedBindings);
        Assert.Contains(result.Issues, issue => issue.Code == "binding.empty-closure");
    }
}
