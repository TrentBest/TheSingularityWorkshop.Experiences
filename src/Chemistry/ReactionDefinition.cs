namespace TheSingularityWorkshop.Experiences.Chemistry;

/// <summary>
/// A proposed reaction equation. Construction records the proposal; it does not claim that the reaction is feasible.
/// </summary>
public sealed class ReactionDefinition
{
    public ReactionDefinition(
        string key,
        IEnumerable<ReactionParticipant> reactants,
        IEnumerable<ReactionParticipant> products)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(reactants);
        ArgumentNullException.ThrowIfNull(products);

        var reactantCopy = reactants.ToArray();
        var productCopy = products.ToArray();
        if (reactantCopy.Length == 0)
            throw new ArgumentException("A reaction must have at least one reactant.", nameof(reactants));
        if (productCopy.Length == 0)
            throw new ArgumentException("A reaction must have at least one product.", nameof(products));
        if (reactantCopy.Any(p => p is null) || productCopy.Any(p => p is null))
            throw new ArgumentException("Reaction participants cannot contain null entries.");

        Key = key;
        Reactants = Array.AsReadOnly(reactantCopy);
        Products = Array.AsReadOnly(productCopy);
    }

    public string Key { get; }
    public IReadOnlyList<ReactionParticipant> Reactants { get; }
    public IReadOnlyList<ReactionParticipant> Products { get; }
}
