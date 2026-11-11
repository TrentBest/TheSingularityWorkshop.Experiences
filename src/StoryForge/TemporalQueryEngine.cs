namespace TheSingularityWorkshop.Experiences.StoryForge;

/// <summary>Request to inspect the story model at one coordinate.</summary>
public sealed record TemporalQuery
{
    /// <summary>Creates a query for a named story-time coordinate.</summary>
    public TemporalQuery(StoryTime time, string? focusEntityId = null, bool includeProposals = true)
    {
        if (focusEntityId is not null && string.IsNullOrWhiteSpace(focusEntityId))
            throw new ArgumentException("A focus entity id must be non-empty when supplied.", nameof(focusEntityId));
        Time = time;
        FocusEntityId = focusEntityId;
        IncludeProposals = includeProposals;
    }

    /// <summary>Gets the selected time coordinate.</summary>
    public StoryTime Time { get; }
    /// <summary>Gets an optional entity whose events and state should be emphasized.</summary>
    public string? FocusEntityId { get; }
    /// <summary>Gets whether proposed assertions are included in results.</summary>
    public bool IncludeProposals { get; }
}

/// <summary>Result of asking the story model what is known at a selected time.</summary>
public sealed record TemporalQueryResult
{
    internal TemporalQueryResult(
        StoryTime time,
        IEnumerable<StoryEvent> events,
        IEnumerable<EntityStateFact> stateFacts,
        IEnumerable<string> contestedProperties)
    {
        Time = time;
        Events = Array.AsReadOnly(events.ToArray());
        StateFacts = Array.AsReadOnly(stateFacts.ToArray());
        ContestedProperties = Array.AsReadOnly(contestedProperties.ToArray());
    }

    /// <summary>Gets the queried coordinate.</summary>
    public StoryTime Time { get; }
    /// <summary>Gets events occurring at exactly the selected coordinate.</summary>
    public IReadOnlyList<StoryEvent> Events { get; }
    /// <summary>Gets entity-state assertions valid at the selected coordinate.</summary>
    public IReadOnlyList<EntityStateFact> StateFacts { get; }
    /// <summary>Gets entity/property keys for which simultaneous values conflict.</summary>
    public IReadOnlyList<string> ContestedProperties { get; }
    /// <summary>Gets whether the query returned any records.</summary>
    public bool HasEvidence => Events.Count > 0 || StateFacts.Count > 0;
}

/// <summary>
/// A small immutable-in-use temporal index. It reports assertions and conflicts; it does not
/// invent missing facts or turn inferred/proposed assertions into canon.
/// </summary>
public sealed class TemporalQueryEngine
{
    private readonly StoryEvent[] _events;
    private readonly EntityStateFact[] _stateFacts;

    /// <summary>Creates an index from the supplied story events and state assertions.</summary>
    public TemporalQueryEngine(
        IEnumerable<StoryEvent>? events = null,
        IEnumerable<EntityStateFact>? stateFacts = null)
    {
        _events = (events ?? []).ToArray();
        _stateFacts = (stateFacts ?? []).ToArray();

        var duplicateEvent = _events.GroupBy(x => x.Id, StringComparer.Ordinal).FirstOrDefault(g => g.Count() > 1);
        if (duplicateEvent is not null)
            throw new ArgumentException($"Duplicate event id '{duplicateEvent.Key}'.", nameof(events));

        var duplicateFact = _stateFacts.GroupBy(x => x.Id, StringComparer.Ordinal).FirstOrDefault(g => g.Count() > 1);
        if (duplicateFact is not null)
            throw new ArgumentException($"Duplicate state fact id '{duplicateFact.Key}'.", nameof(stateFacts));
    }

    /// <summary>Answers what the current model records at a selected story-time coordinate.</summary>
    public TemporalQueryResult QueryAt(TemporalQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var events = _events
            .Where(x => SameTimeline(x.Time, query.Time) && x.Time.Value == query.Time.Value)
            .Where(x => query.IncludeProposals || x.Certainty != StoryCertainty.Proposed)
            .Where(x => query.FocusEntityId is null || x.ParticipantIds.Contains(query.FocusEntityId, StringComparer.Ordinal))
            .OrderBy(x => x.Title, StringComparer.Ordinal)
            .ThenBy(x => x.Id, StringComparer.Ordinal)
            .ToArray();

        var facts = _stateFacts
            .Where(x => SameTimeline(x.ValidDuring.Start, query.Time) && x.ValidDuring.Contains(query.Time))
            .Where(x => query.IncludeProposals || x.Certainty != StoryCertainty.Proposed)
            .Where(x => query.FocusEntityId is null || string.Equals(x.EntityId, query.FocusEntityId, StringComparison.Ordinal))
            .OrderBy(x => x.EntityId, StringComparer.Ordinal)
            .ThenBy(x => x.Property, StringComparer.Ordinal)
            .ThenBy(x => x.Id, StringComparer.Ordinal)
            .ToArray();

        var conflicts = facts
            .GroupBy(x => (x.EntityId, x.Property))
            .Where(group => group.Select(x => x.Value).Distinct(StringComparer.Ordinal).Skip(1).Any())
            .Select(group => $"{group.Key.EntityId}/{group.Key.Property}")
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

        return new TemporalQueryResult(query.Time, events, facts, conflicts);
    }

    private static bool SameTimeline(StoryTime first, StoryTime second) =>
        string.Equals(first.TimelineId, second.TimelineId, StringComparison.Ordinal);
}
