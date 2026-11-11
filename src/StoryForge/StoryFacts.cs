namespace TheSingularityWorkshop.Experiences.StoryForge;

/// <summary>Describes how strongly a story assertion is held.</summary>
public enum StoryCertainty
{
    /// <summary>A human-approved or otherwise explicitly established fact.</summary>
    Confirmed,
    /// <summary>A plausible interpretation that remains open to revision.</summary>
    Inferred,
    /// <summary>An unresolved proposal that must not silently become canon.</summary>
    Proposed,
    /// <summary>Accounts disagree about this assertion.</summary>
    Contested
}

/// <summary>A traceable source or authoring decision behind a story assertion.</summary>
public sealed record StoryProvenance
{
    /// <summary>Creates a provenance reference.</summary>
    public StoryProvenance(string sourceId, string description, string? locator = null)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
            throw new ArgumentException("A provenance source identifier is required.", nameof(sourceId));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A provenance description is required.", nameof(description));

        SourceId = sourceId;
        Description = description;
        Locator = locator;
    }

    /// <summary>Gets the stable source or decision identifier.</summary>
    public string SourceId { get; }
    /// <summary>Gets a concise explanation of the source.</summary>
    public string Description { get; }
    /// <summary>Gets an optional location within the source, such as a paragraph or note ID.</summary>
    public string? Locator { get; }
}

/// <summary>An event asserted to occur at a coordinate on a named timeline.</summary>
public sealed record StoryEvent
{
    /// <summary>Creates a story event without imposing a genre-specific event schema.</summary>
    public StoryEvent(
        string id,
        string title,
        StoryTime time,
        IEnumerable<string>? participantIds = null,
        StoryCertainty certainty = StoryCertainty.Confirmed,
        IEnumerable<StoryProvenance>? provenance = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Event id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Event title is required.", nameof(title));

        Id = id;
        Title = title;
        Time = time;
        ParticipantIds = Array.AsReadOnly((participantIds ?? []).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).ToArray());
        Certainty = certainty;
        Provenance = Array.AsReadOnly((provenance ?? []).ToArray());
        Description = description;
    }

    /// <summary>Gets the stable event identifier.</summary>
    public string Id { get; }
    /// <summary>Gets the event title.</summary>
    public string Title { get; }
    /// <summary>Gets the timeline coordinate at which the event occurs.</summary>
    public StoryTime Time { get; }
    /// <summary>Gets the identifiers of entities participating in the event.</summary>
    public IReadOnlyList<string> ParticipantIds { get; }
    /// <summary>Gets the current certainty assigned to this account.</summary>
    public StoryCertainty Certainty { get; }
    /// <summary>Gets traceable sources and authoring decisions supporting this event.</summary>
    public IReadOnlyList<StoryProvenance> Provenance { get; }
    /// <summary>Gets optional explanatory text.</summary>
    public string? Description { get; }
}

/// <summary>An entity property asserted to hold throughout a story-time interval.</summary>
public sealed record EntityStateFact
{
    /// <summary>Creates a time-bounded entity-state assertion.</summary>
    public EntityStateFact(
        string id,
        string entityId,
        string property,
        string value,
        StoryTimeRange validDuring,
        StoryCertainty certainty = StoryCertainty.Confirmed,
        IEnumerable<StoryProvenance>? provenance = null)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Fact id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(entityId)) throw new ArgumentException("Entity id is required.", nameof(entityId));
        if (string.IsNullOrWhiteSpace(property)) throw new ArgumentException("Property is required.", nameof(property));
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value is required.", nameof(value));

        Id = id;
        EntityId = entityId;
        Property = property;
        Value = value;
        ValidDuring = validDuring ?? throw new ArgumentNullException(nameof(validDuring));
        Certainty = certainty;
        Provenance = Array.AsReadOnly((provenance ?? []).ToArray());
    }

    /// <summary>Gets the stable assertion identifier.</summary>
    public string Id { get; }
    /// <summary>Gets the entity whose state is described.</summary>
    public string EntityId { get; }
    /// <summary>Gets the name of the state property.</summary>
    public string Property { get; }
    /// <summary>Gets the asserted property value.</summary>
    public string Value { get; }
    /// <summary>Gets the half-open interval during which the assertion applies.</summary>
    public StoryTimeRange ValidDuring { get; }
    /// <summary>Gets the certainty assigned to the assertion.</summary>
    public StoryCertainty Certainty { get; }
    /// <summary>Gets traceable sources and authoring decisions supporting the assertion.</summary>
    public IReadOnlyList<StoryProvenance> Provenance { get; }
}
