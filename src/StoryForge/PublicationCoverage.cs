namespace TheSingularityWorkshop.Experiences.StoryForge;

/// <summary>Maps an authored unit to the interval of story time it presents.</summary>
public sealed record PublicationCoverage
{
    public PublicationCoverage(string workId, string unitId, string unitKind, int sequence, StoryTimeRange storyRange)
    {
        if (string.IsNullOrWhiteSpace(workId)) throw new ArgumentException("Work id is required.", nameof(workId));
        if (string.IsNullOrWhiteSpace(unitId)) throw new ArgumentException("Unit id is required.", nameof(unitId));
        if (string.IsNullOrWhiteSpace(unitKind)) throw new ArgumentException("Unit kind is required.", nameof(unitKind));
        if (sequence < 0) throw new ArgumentOutOfRangeException(nameof(sequence), "Sequence must be zero or greater.");
        WorkId = workId;
        UnitId = unitId;
        UnitKind = unitKind;
        Sequence = sequence;
        StoryRange = storyRange ?? throw new ArgumentNullException(nameof(storyRange));
    }

    public string WorkId { get; }
    public string UnitId { get; }
    public string UnitKind { get; }
    public int Sequence { get; }
    public StoryTimeRange StoryRange { get; }
}

/// <summary>An immutable published-edition snapshot with fixed identity and coverage.</summary>
public sealed record PublishedEdition
{
    public PublishedEdition(string editionId, string workId, string version, string contentHash, IEnumerable<PublicationCoverage> coverage)
    {
        if (string.IsNullOrWhiteSpace(editionId)) throw new ArgumentException("Edition id is required.", nameof(editionId));
        if (string.IsNullOrWhiteSpace(workId)) throw new ArgumentException("Work id is required.", nameof(workId));
        if (string.IsNullOrWhiteSpace(version)) throw new ArgumentException("Version is required.", nameof(version));
        if (string.IsNullOrWhiteSpace(contentHash)) throw new ArgumentException("Content hash is required.", nameof(contentHash));
        EditionId = editionId;
        WorkId = workId;
        Version = version;
        ContentHash = contentHash;
        Coverage = Array.AsReadOnly((coverage ?? throw new ArgumentNullException(nameof(coverage))).ToArray());
    }

    public string EditionId { get; }
    public string WorkId { get; }
    public string Version { get; }
    public string ContentHash { get; }
    public IReadOnlyList<PublicationCoverage> Coverage { get; }
}
