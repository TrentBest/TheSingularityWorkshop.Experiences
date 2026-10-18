using Xunit;
using TheSingularityWorkshop.Experiences.Chemistry;

namespace TheSingularityWorkshop.Experiences.Chemistry.Tests;

public sealed class ReactionBalanceAnalyzerTests
{
    private static ChemicalSpecies Species(string key, long charge, params (string Symbol, long Count)[] atoms) =>
        new(key, key, atoms.ToDictionary(atom => atom.Symbol, atom => atom.Count, StringComparer.Ordinal), charge);

    [Fact]
    public void BalancedWaterFormationConservesAtomsAndCharge()
    {
        var hydrogen = Species("H2", 0, ("H", 2));
        var oxygen = Species("O2", 0, ("O", 2));
        var water = Species("H2O", 0, ("H", 2), ("O", 1));
        var reaction = new ReactionDefinition("water-formation",
            [new(hydrogen, 2), new(oxygen)],
            [new(water, 2)]);

        var report = ReactionBalanceAnalyzer.Analyze(reaction);

        Assert.True(report.IsBalanced);
        Assert.Equal(0, report.ChargeImbalance);
        Assert.All(report.ElementImbalances.Values, value => Assert.Equal(0, value));
    }

    [Fact]
    public void DetectsAnUnbalancedElementAndUsesProductsMinusReactants()
    {
        var hydrogen = Species("H2", 0, ("H", 2));
        var oxygen = Species("O2", 0, ("O", 2));
        var water = Species("H2O", 0, ("H", 2), ("O", 1));
        var reaction = new ReactionDefinition("unbalanced-water",
            [new(hydrogen), new(oxygen)],
            [new(water)]);

        var report = ReactionBalanceAnalyzer.Analyze(reaction);

        Assert.False(report.IsBalanced);
        Assert.Equal(0, report.ElementImbalances["H"]);
        Assert.Equal(-1, report.ElementImbalances["O"]);
    }

    [Fact]
    public void DetectsChargeImbalanceEvenWhenAtomCountsMatch()
    {
        var positiveHydrogen = Species("H+", 1, ("H", 1));
        var hydrogen = Species("H", 0, ("H", 1));
        var reaction = new ReactionDefinition("charge-audit", [new(positiveHydrogen)], [new(hydrogen)]);

        var report = ReactionBalanceAnalyzer.Analyze(reaction);

        Assert.False(report.IsBalanced);
        Assert.Equal(-1, report.ChargeImbalance);
    }

    [Fact]
    public void RejectsNonPositiveAtomCountsAndCoefficients()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Species("invalid", 0, ("H", 0)));
        var hydrogen = Species("H2", 0, ("H", 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ReactionParticipant(hydrogen, 0));
    }

    [Fact]
    public void ProtectsSpeciesCompositionFromCallerMutation()
    {
        var source = new Dictionary<string, long> { ["H"] = 2 };
        var species = new ChemicalSpecies("H2", "Hydrogen", source);
        source["H"] = 99;

        Assert.Equal(2, species.ElementCounts["H"]);
    }
}
