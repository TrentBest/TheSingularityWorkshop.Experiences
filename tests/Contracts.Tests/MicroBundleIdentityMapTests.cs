using TheSingularityWorkshop.Experiences.Contracts;
using Xunit;

namespace TheSingularityWorkshop.Experiences.Contracts.Tests;

public sealed class MicroBundleIdentityMapTests
{
    private static ArtifactIdentity Artifact(string ontology, string version = "1.0.0", char hash = 'a') =>
        new(ontology, version, new string(hash, 64));

    [Fact]
    public void ResolvesExplicitImmutableArtifactBindingsInBothDirections()
    {
        var rules = Artifact("workshop-minesweeper-rules");
        var presentation = Artifact("workshop-minesweeper-presentation");
        var map = MicroBundleIdentityMap.Create(
        [
            new(rules, 73001),
            new(presentation, 73002)
        ]);

        Assert.Equal(2, map.Count);
        Assert.True(map.TryResolveLocalHandle(rules, out var handle));
        Assert.Equal(73001UL, handle);
        Assert.True(map.TryGetArtifactIdentity(73002, out var resolved));
        Assert.Equal(presentation.StableKey, resolved.StableKey);
    }

    [Fact]
    public void DistinguishesVersionsAndContentHashesOfTheSameOntology()
    {
        var v1 = Artifact("workshop-minesweeper-rules", "1.0.0", 'a');
        var v2 = Artifact("workshop-minesweeper-rules", "2.0.0", 'b');
        var map = MicroBundleIdentityMap.Create([new(v1, 73001), new(v2, 73002)]);

        Assert.True(map.TryResolveLocalHandle(v1, out var first));
        Assert.True(map.TryResolveLocalHandle(v2, out var second));
        Assert.Equal(73001UL, first);
        Assert.Equal(73002UL, second);
    }

    [Fact]
    public void DoesNotInferOrNormalizeUnknownOntologyIdentities()
    {
        var map = MicroBundleIdentityMap.Create([new(Artifact("Minesweeper.Rules"), 73001)]);

        Assert.False(map.TryResolveLocalHandle(Artifact("minesweeper.rules"), out var handle));
        Assert.Equal(0UL, handle);
        Assert.False(map.TryResolveLocalHandle(Artifact(" Minesweeper.Rules "), out _));
    }

    [Fact]
    public void RejectsZeroLocalHandle()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            MicroBundleIdentityMap.Create([new(Artifact("rules"), 0)]));

        Assert.Contains("reserved zero", exception.Message);
    }

    [Fact]
    public void RejectsDuplicateImmutableArtifactIdentities()
    {
        var artifact = Artifact("rules");
        var exception = Assert.Throws<ArgumentException>(() =>
            MicroBundleIdentityMap.Create([new(artifact, 73001), new(artifact, 73002)]));

        Assert.Contains("Duplicate immutable artifact identity", exception.Message);
    }

    [Fact]
    public void RejectsLocalHandleAliasing()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            MicroBundleIdentityMap.Create(
            [
                new(Artifact("rules"), 73001),
                new(Artifact("presentation"), 73001)
            ]));

        Assert.Contains("assigned to more than one", exception.Message);
    }

    [Fact]
    public void RejectsInvalidArtifactIdentity()
    {
        var invalid = new ArtifactIdentity("rules", "1.0.0", "not-a-hash");
        Assert.Throws<ArgumentException>(() =>
            MicroBundleIdentityMap.Create([new(invalid, 73001)]));
    }

    [Fact]
    public void RejectsNullBindingSequence()
    {
        Assert.Throws<ArgumentNullException>(() =>
            MicroBundleIdentityMap.Create(null!));
    }

    [Fact]
    public void ZeroCannotBeReverseResolved()
    {
        var map = MicroBundleIdentityMap.Create([new(Artifact("rules"), 73001)]);

        Assert.False(map.TryGetArtifactIdentity(0, out _));
    }
}
