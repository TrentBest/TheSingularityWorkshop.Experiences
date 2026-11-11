using TheSingularityWorkshop.Experiences.Contracts;
using Xunit;

namespace TheSingularityWorkshop.Experiences.Contracts.Tests;

public sealed class MicroBundleIdentityMapTests
{
    [Fact]
    public void ResolvesExplicitBindingsInBothDirections()
    {
        var map = MicroBundleIdentityMap.Create(
        [
            new("workshop-minesweeper-rules", 73001),
            new("workshop-minesweeper-presentation", 73002)
        ]);

        Assert.Equal(2, map.Count);
        Assert.True(map.TryResolve("workshop-minesweeper-rules", out var id));
        Assert.Equal(73001UL, id);
        Assert.True(map.TryGetBundleKey(73002, out var key));
        Assert.Equal("workshop-minesweeper-presentation", key);
    }

    [Fact]
    public void DoesNotInferOrNormalizeUnknownKeys()
    {
        var map = MicroBundleIdentityMap.Create([new("Minesweeper.Rules", 73001)]);

        Assert.False(map.TryResolve("minesweeper.rules", out var id));
        Assert.Equal(0UL, id);
        Assert.False(map.TryResolve(" Minesweeper.Rules ", out _));
    }

    [Fact]
    public void RejectsZeroNumericIdentity()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            MicroBundleIdentityMap.Create([new("rules", 0)]));

        Assert.Contains("reserved zero", exception.Message);
    }

    [Fact]
    public void RejectsDuplicateAuthoredKeys()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            MicroBundleIdentityMap.Create(
            [
                new("rules", 73001),
                new("rules", 73002)
            ]));

        Assert.Contains("Duplicate Experience bundle key", exception.Message);
    }

    [Fact]
    public void RejectsNumericIdentityAliasing()
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
    public void RejectsInvalidAuthoredKeys(string key)
    {
        Assert.Throws<ArgumentException>(() =>
            MicroBundleIdentityMap.Create([new(key, 73001)]));
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

        Assert.False(map.TryGetBundleKey(0, out var key));
        Assert.Equal(string.Empty, key);
    }
}
