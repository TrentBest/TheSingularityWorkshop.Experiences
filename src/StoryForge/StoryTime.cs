namespace TheSingularityWorkshop.Experiences.StoryForge;

/// <summary>A coordinate on a named story-time axis, independent of wall-clock time.</summary>
public readonly record struct StoryTime
{
    public StoryTime(string timelineId, decimal value)
    {
        if (string.IsNullOrWhiteSpace(timelineId))
            throw new ArgumentException("A timeline identifier is required.", nameof(timelineId));
        TimelineId = timelineId;
        Value = value;
    }

    public string TimelineId { get; }
    public decimal Value { get; }
}

/// <summary>Defines a named timeline, its unit, and its tick interval.</summary>
public sealed record StoryTimeline
{
    public StoryTimeline(string id, string name, string unit, decimal tickInterval)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Timeline id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Timeline name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(unit)) throw new ArgumentException("Time unit is required.", nameof(unit));
        if (tickInterval <= 0) throw new ArgumentOutOfRangeException(nameof(tickInterval), "Tick interval must be positive.");
        Id = id;
        Name = name;
        Unit = unit;
        TickInterval = tickInterval;
    }

    public string Id { get; }
    public string Name { get; }
    public string Unit { get; }
    public decimal TickInterval { get; }
    public StoryTime At(decimal value) => new(Id, value);
    public bool Contains(StoryTime time) => string.Equals(time.TimelineId, Id, StringComparison.Ordinal);
}

/// <summary>A half-open time interval [start, end) on one timeline.</summary>
public sealed record StoryTimeRange
{
    public StoryTimeRange(StoryTime start, StoryTime end)
    {
        if (!string.Equals(start.TimelineId, end.TimelineId, StringComparison.Ordinal))
            throw new ArgumentException("Range endpoints must belong to the same timeline.");
        if (end.Value < start.Value)
            throw new ArgumentOutOfRangeException(nameof(end), "Range end cannot precede its start.");
        Start = start;
        End = end;
    }

    public StoryTime Start { get; }
    public StoryTime End { get; }
    public decimal Duration => End.Value - Start.Value;
    public bool Contains(StoryTime time) =>
        string.Equals(time.TimelineId, Start.TimelineId, StringComparison.Ordinal)
        && time.Value >= Start.Value && time.Value < End.Value;
}

/// <summary>A timeline marker pointing to a story object or event.</summary>
public sealed record TimelineMarker
{
    public TimelineMarker(string id, string label, StoryTime time, string targetId, string targetKind)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Marker id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Marker label is required.", nameof(label));
        if (string.IsNullOrWhiteSpace(targetId)) throw new ArgumentException("Target id is required.", nameof(targetId));
        if (string.IsNullOrWhiteSpace(targetKind)) throw new ArgumentException("Target kind is required.", nameof(targetKind));
        Id = id;
        Label = label;
        Time = time;
        TargetId = targetId;
        TargetKind = targetKind;
    }

    public string Id { get; }
    public string Label { get; }
    public StoryTime Time { get; }
    public string TargetId { get; }
    public string TargetKind { get; }
}
