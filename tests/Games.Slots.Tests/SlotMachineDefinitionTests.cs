using TheSingularityWorkshop.Games.Slots;
using Xunit;

namespace TheSingularityWorkshop.Games.Slots.Tests;

public sealed class SlotMachineDefinitionTests
{
    private static SlotMachineDefinition CreateMachine() => new(
        new[]
        {
            new[] { "Cherry", "Cherry", "Bell" },
            new[] { "Cherry", "Bell", "Bell" },
            new[] { "Cherry", "Bell", "Seven" }
        },
        new Dictionary<string, decimal>
        {
            ["Cherry"] = 2m,
            ["Bell"] = 4m,
            ["Seven"] = 10m
        });

    [Fact]
    public void MatchingConfiguredSymbolReturnsTotalPayout()
    {
        var result = CreateMachine().Evaluate(new[] { "Cherry", "Cherry", "Cherry" }, 3m);

        Assert.Equal(2m, result.PayoutMultiplier);
        Assert.Equal(6m, result.GrossPayout);
        Assert.Equal(3m, result.Stake);
    }

    [Fact]
    public void NonMatchingStopsLose()
    {
        var result = CreateMachine().Evaluate(new[] { "Cherry", "Bell", "Seven" }, 3m);

        Assert.Equal(0m, result.PayoutMultiplier);
        Assert.Equal(0m, result.GrossPayout);
    }

    [Fact]
    public void MatchingUnlistedSymbolDoesNotPay()
    {
        var machine = new SlotMachineDefinition(
            new[] { new[] { "Blank", "Star" }, new[] { "Blank", "Star" } },
            new Dictionary<string, decimal>());

        Assert.Equal(0m, machine.Evaluate(new[] { "Blank", "Blank" }, 1m).GrossPayout);
    }

    [Fact]
    public void ExactExpectedPayoutUsesRepeatedStopsAsWeights()
    {
        var machine = CreateMachine();

        // Cherry: (2/3)*(1/3)*(1/3) * 2 = 4/27
        // Bell:   (1/3)*(2/3)*(1/3) * 4 = 8/27
        // Seven:  (0/3)*(0/3)*(1/3) * 10 = 0
        Assert.Equal(12m / 27m, machine.CalculateExpectedPayoutPerSpin(1m));
        Assert.Equal(12m / 27m, machine.CalculateReturnToPlayer(1m));
    }

    [Fact]
    public void ZeroProbabilityPaytableEntriesDoNotContribute()
    {
        var machine = new SlotMachineDefinition(
            new[] { new[] { "A", "B" }, new[] { "A", "B" } },
            new Dictionary<string, decimal> { ["C"] = 100m });

        Assert.Equal(0m, machine.CalculateExpectedPayoutPerSpin(2m));
    }

    [Fact]
    public void RejectsWrongNumberOfStops()
    {
        Assert.Throws<ArgumentException>(() => CreateMachine().Evaluate(new[] { "Cherry" }, 1m));
    }

    [Fact]
    public void RejectsSymbolNotOnCorrespondingReel()
    {
        Assert.Throws<ArgumentException>(() => CreateMachine().Evaluate(new[] { "Seven", "Cherry", "Cherry" }, 1m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RejectsNonPositiveStake(decimal stake)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateMachine().Evaluate(new[] { "Cherry", "Cherry", "Cherry" }, stake));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateMachine().CalculateExpectedPayoutPerSpin(stake));
    }

    [Fact]
    public void RejectsEmptyReelAndNegativeMultiplier()
    {
        Assert.Throws<ArgumentException>(() => new SlotMachineDefinition(
            new[] { Array.Empty<string>() }, new Dictionary<string, decimal>()));

        Assert.Throws<ArgumentOutOfRangeException>(() => new SlotMachineDefinition(
            new[] { new[] { "A" } }, new Dictionary<string, decimal> { ["A"] = -1m }));
    }

    [Fact]
    public void ConfigurationIsCopiedAndResultStopsAreReadOnly()
    {
        var reels = new List<string[]> { new[] { "A", "B" } };
        var paytable = new Dictionary<string, decimal> { ["A"] = 3m };
        var machine = new SlotMachineDefinition(reels, paytable);
        reels[0][0] = "Changed";
        paytable["A"] = 100m;

        var result = machine.Evaluate(new[] { "A" }, 1m);

        Assert.Equal(3m, result.GrossPayout);
        Assert.Equal("A", machine.Reels[0][0]);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)result.Stops)[0] = "Changed");
    }
}
