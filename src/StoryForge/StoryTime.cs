namespace TheSingularityWorkshop.Experiences.StoryForge;

/// <summary>A coordinate on a named story-time axis, independent of wall-clock time.</summary>
public readonly record struct StoryTime
{
    /// <summary>Creates a coordinate on the supplied timeline.</summary>
    /// <param name="timelineId">Stable identifier for the time domain.</param>
    /// <param name="value">Coordinate value in the timeline's declared unit.</param>
    public StoryTime(string timelineId, decimal value)
    {
        if (string.IsNullOrWhiteSpace(timelineId))
            throw new ArgumentException("A timeline identifier is required.", nameof(timelineId));
        TimelineId = timelineId;
        Value = value;
    }

    /// <summary>Gets the stable identifier of the time domain.</summary>
    public string TimelineId { get; }
    /// <summary>Gets the coordinate within the time domain.</summary>
    public decimal Value { get; }
}

/// <summary>Defines a named timeline, its unit, and its tick interval.</summary>
public sealed record StoryTimeline
{
    /// <summary>Creates a named timeline with a positive tick interval.</summary>
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

    /// <summary>Gets the stable timeline identifier.</summary>
    public string Id { get; }
    /// <summary>Gets the user-facing timeline name.</summary>
    public string Name { get; }
    /// <summary>Gets the declared unit, such as years, days, or beats.</summary>
    public string Unit { get; }
    /// <summary>Gets the interval between displayed major ticks.</summary>
    public decimal TickInterval { get; }
    /// <summary>Creates a coordinate on this timeline.</summary>
    public StoryTime At(decimal value) => new(Id, value);
    /// <summary>Returns whether the coordinate belongs to this timeline.</summary>
    public bool Contains(StoryTime time) => string.Equals(time.TimelineId, Id, StringComparison.Ordinal);
}

/// <summary>A half-open time interval [start, end) on one timeline.</summary>
public sealed record StoryTimeRange
{
    /// <summary>Creates a range whose endpoints belong to the same timeline.</summary>
    public StoryTimeRange(StoryTime start, StoryTime end)
    {
        if (!string.Equals(start.TimelineId, end.TimelineId, StringComparison.Ordinal))
            throw new ArgumentException("Range endpoints must belong to the same timeline.");
        if (end.Value < start.Value)
            throw new ArgumentOutOfRangeException(nameof(end), "Range end cannot precede its start.");
        Start = start;
        End = end;
    }

    /// <summary>Gets the inclusive start coordinate.</summary>
    public StoryTime Start { get; }
    /// <summary>Gets the exclusive end coordinate.</summary>
    public StoryTime End { get; }
    /// <summary>Gets the numeric distance between the endpoints.</summary>
    public decimal Duration => End.Value - Start.Value;
    /// <summary>Returns whether the coordinate is inside this half-open range.</summary>
    public bool Contains(StoryTime time) =>
        string.Equals(time.TimelineId, Start.TimelineId, StringComparison.Ordinal)
        && time.Value >= Start.Value && time.Value < End.Value;
}

/// <summary>A timeline marker pointing to a story object or event.</summary>
public sealed record TimelineMarker
{
    /// <summary>Creates a labeled marker that targets a story object.</summary>
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

    /// <summary>Gets the stable marker identifier.</summary>
    public string Id { get; }
    /// <summary>Gets the marker's display label.</summary>
    public string Label { get; }
    /// <summary>Gets the marked story-time coordinate.</summary>
    public StoryTime Time { get; }
    /// <summary>Gets the identifier of the marked story object.</summary>
    public string TargetId { get; }
    /// <summary>Gets the kind of the marked object, such as event or character.</summary>
    public string TargetKind { get; }
}
