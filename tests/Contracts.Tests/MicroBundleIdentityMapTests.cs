using TheSingularityWorkshop.Experiences.Contracts;
using Xunit;

namespace TheSingularityWorkshop.Experiences.Contracts.Tests;

public sealed class MicroBundleIdentityMapTests
{
    [Fact]
    public void ResolvesExplicitOntologyBindingsInBothDirections()
    {
        var map = MicroBundleIdentityMap.Create(
        [
            new("workshop-minesweeper-rules", 73001),
            new("workshop-minesweeper-presentation", 73002)
        ]);

        Assert.Equal(2, map.Count);
        Assert.True(map.TryResolveLocalHandle("workshop-minesweeper-rules", out var handle));
        Assert.Equal(73001UL, handle);
        Assert.True(map.TryGetOntologyIdentity(73002, out var ontology));
        Assert.Equal("workshop-minesweeper-presentation", ontology);
    }

    [Fact]
    public void DoesNotInferOrNormalizeUnknownOntologyIdentities()
    {
        var map = MicroBundleIdentityMap.Create([new("Minesweeper.Rules", 73001)]);

        Assert.False(map.TryResolveLocalHandle("minesweeper.rules", out var handle));
        Assert.Equal(0UL, handle);
        Assert.False(map.TryResolveLocalHandle(" Minesweeper.Rules ", out _));
    }

    [Fact]
    public void RejectsZeroLocalHandle()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            MicroBundleIdentityMap.Create([new("rules", 0)]));

        Assert.Contains("reserved zero", exception.Message);
    }

    [Fact]
    public void RejectsDuplicateOntologyIdentities()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            MicroBundleIdentityMap.Create(
            [
                new("rules", 73001),
                new("rules", 73002)
            ]));

        Assert.Contains("Duplicate ontology identity", exception.Message);
    }

    [Fact]
    public void RejectsLocalHandleAliasing()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            MicroBundleIdentityMap.Create(
            [
                new("rules", 73001),
                new("presentation", 73001)
            ]));

        Assert.Contains("assigned to more than one", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(" rules")]
    [InlineData("rules ")]
    [InlineData("rules\n")]
    public void RejectsInvalidOntologyIdentities(string ontology)
    {
        Assert.Throws<ArgumentException>(() =>
            MicroBundleIdentityMap.Create([new(ontology, 73001)]));
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
        var map = MicroBundleIdentityMap.Create([new("rules", 73001)]);

        Assert.False(map.TryGetOntologyIdentity(0, out var ontology));
        Assert.Equal(string.Empty, ontology);
    }
}
