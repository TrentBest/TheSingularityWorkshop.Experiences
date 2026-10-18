using System.Collections.ObjectModel;

namespace TheSingularityWorkshop.Experiences.Chemistry;

/// <summary>
/// Conservation audit for a proposed reaction. Imbalances use products minus reactants.
/// Positive values mean excess on the product side; negative values mean excess on the reactant side.
/// </summary>
public sealed class ReactionBalanceReport
{
    internal ReactionBalanceReport(IDictionary<string, long> elementImbalances, long chargeImbalance)
    {
        ElementImbalances = new ReadOnlyDictionary<string, long>(
            new SortedDictionary<string, long>(elementImbalances, StringComparer.Ordinal));
        ChargeImbalance = chargeImbalance;
    }

    public IReadOnlyDictionary<string, long> ElementImbalances { get; }
    public long ChargeImbalance { get; }
    public bool IsBalanced => ElementImbalances.Values.All(value => value == 0) && ChargeImbalance == 0;
}
