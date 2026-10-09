namespace TheSingularityWorkshop.Experiences.StoryForge;

/// <summary>Maps an authored unit to the interval of story time it presents.</summary>
public sealed record PublicationCoverage
{
    /// <summary>Creates a coverage record for a chapter, volume, episode, or other unit.</summary>
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

    /// <summary>Gets the work identifier.</summary>
    public string WorkId { get; }
    /// <summary>Gets the authored unit identifier.</summary>
    public string UnitId { get; }
    /// <summary>Gets the kind of unit, such as chapter, book, or episode.</summary>
    public string UnitKind { get; }
    /// <summary>Gets the zero-based or project-defined publication sequence number.</summary>
    public int Sequence { get; }
    /// <summary>Gets the interval of story time covered by this unit.</summary>
    public StoryTimeRange StoryRange { get; }
}

/// <summary>An immutable published-edition snapshot with fixed identity and coverage.</summary>
public sealed record PublishedEdition
{
    /// <summary>Creates an edition snapshot. Later authoring should create a new snapshot.</summary>
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

    /// <summary>Gets the immutable edition identifier.</summary>
    public string EditionId { get; }
    /// <summary>Gets the work identifier.</summary>
    public string WorkId { get; }
    /// <summary>Gets the edition version label.</summary>
    public string Version { get; }
    /// <summary>Gets the content hash identifying the published bytes.</summary>
    public string ContentHash { get; }
    /// <summary>Gets a read-only snapshot of the edition's publication coverage.</summary>
    public IReadOnlyList<PublicationCoverage> Coverage { get; }
}
