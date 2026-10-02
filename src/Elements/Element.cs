namespace TheSingularityWorkshop.Experiences.Elements;

public sealed record Element
{
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

    public ulong AtomicNumber { get; }
    public string Symbol { get; }
    public string Name { get; }
    public double AtomicWeight { get; }
}