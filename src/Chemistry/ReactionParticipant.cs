namespace TheSingularityWorkshop.Experiences.Chemistry;

/// <summary>A species participating in a reaction with a positive integer stoichiometric coefficient.</summary>
public sealed record ReactionParticipant
{
    public ReactionParticipant(ChemicalSpecies species, long coefficient = 1)
    {
        ArgumentNullException.ThrowIfNull(species);
        if (coefficient <= 0)
            throw new ArgumentOutOfRangeException(nameof(coefficient), "Stoichiometric coefficients must be positive integers.");

        Species = species;
        Coefficient = coefficient;
    }

    public ChemicalSpecies Species { get; }
    public long Coefficient { get; }
}
