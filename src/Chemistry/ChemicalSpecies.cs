using System.Collections.ObjectModel;

namespace TheSingularityWorkshop.Experiences.Chemistry;

/// <summary>
/// A versionable chemical species identity with an explicitly supplied elemental composition.
/// Composition is an input contract; this type does not infer chemistry from a drawing or formula string.
/// </summary>
public sealed class ChemicalSpecies
{
    private readonly ReadOnlyDictionary<string, long> _elementCounts;

    public ChemicalSpecies(
        string key,
        string displayName,
        IReadOnlyDictionary<string, long> elementCounts,
        long charge = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentNullException.ThrowIfNull(elementCounts);
        if (elementCounts.Count == 0)
            throw new ArgumentException("A species must contain at least one element.", nameof(elementCounts));

        var copy = new SortedDictionary<string, long>(StringComparer.Ordinal);
        foreach (var (symbol, count) in elementCounts)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(elementCounts), $"Element count for '{symbol}' must be positive.");
            if (!copy.TryAdd(symbol, count))
                throw new ArgumentException($"Duplicate element symbol '{symbol}'.", nameof(elementCounts));
        }

        Key = key;
        DisplayName = displayName;
        Charge = charge;
        _elementCounts = new ReadOnlyDictionary<string, long>(copy);
    }

    public string Key { get; }
    public string DisplayName { get; }
    public long Charge { get; }
    public IReadOnlyDictionary<string, long> ElementCounts => _elementCounts;
}
