namespace TheSingularityWorkshop.Experiences.Chemistry;

/// <summary>
/// Performs deterministic integer atom and charge accounting for a proposed reaction.
/// This is conservation checking, not automatic equation balancing, reaction prediction, or kinetics.
/// </summary>
public static class ReactionBalanceAnalyzer
{
    public static ReactionBalanceReport Analyze(ReactionDefinition reaction)
    {
        ArgumentNullException.ThrowIfNull(reaction);
        var elementTotals = new SortedDictionary<string, long>(StringComparer.Ordinal);
        long chargeImbalance = 0;

        Accumulate(reaction.Reactants, -1);
        Accumulate(reaction.Products, 1);

        return new ReactionBalanceReport(elementTotals, chargeImbalance);

        void Accumulate(IEnumerable<ReactionParticipant> participants, long direction)
        {
            foreach (var participant in participants)
            {
                foreach (var (symbol, atomCount) in participant.Species.ElementCounts)
                {
                    var delta = checked(checked(atomCount * participant.Coefficient) * direction);
                    elementTotals[symbol] = checked(elementTotals.GetValueOrDefault(symbol) + delta);
                }

                var chargeDelta = checked(checked(participant.Species.Charge * participant.Coefficient) * direction);
                chargeImbalance = checked(chargeImbalance + chargeDelta);
            }
        }
    }
}
