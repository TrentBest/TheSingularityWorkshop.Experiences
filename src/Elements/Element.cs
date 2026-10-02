namespace TheSingularityWorkshop.Experiences.Elements;

public sealed record Element(
    ulong AtomicNumber,
    string Symbol,
    string Name,
    double AtomicWeight)
{
    public Element
    {
        if (AtomicNumber == 0)
            throw new ArgumentOutOfRangeException(nameof(AtomicNumber));

        ArgumentException.ThrowIfNullOrWhiteSpace(Symbol);
        ArgumentException.ThrowIfNullOrWhiteSpace(Name);

        if (AtomicWeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(AtomicWeight));
    }
}
