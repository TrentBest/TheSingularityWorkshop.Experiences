namespace TheSingularityWorkshop.Experiences.Elements;

/// <summary>Immutable semantic identity for one chemical element.</summary>

public sealed record Element
{
    /// <summary>Creates an element from its identity seed.</summary>
    public Element(ulong atomicNumber, string symbol, string name, double atomicWeight)
    {
        if (atomicNumber == 0)
            throw new ArgumentOutOfRangeException(nameof(atomicNumber));

        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (atomicWeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(atomicWeight));

        AtomicNumber = atomicNumber;
        Symbol = symbol;
        Name = name;
        AtomicWeight = atomicWeight;
    }

    /// <summary>Atomic number.</summary>
    public ulong AtomicNumber { get; }
    /// <summary>Chemical symbol.</summary>
    public string Symbol { get; }
    /// <summary>Element name.</summary>
    public string Name { get; }
    /// <summary>Standard atomic weight represented by this seed record.</summary>
    public double AtomicWeight { get; }
}