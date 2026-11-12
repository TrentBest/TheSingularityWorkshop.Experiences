namespace TheSingularityWorkshop.Experiences.GalacticWar;

/// <summary>A monotonic instant in a campaign's fictional calendar, not a wall-clock timestamp.</summary>
public readonly record struct CampaignInstant(long Value) : IComparable<CampaignInstant>
{
    public int CompareTo(CampaignInstant other) => Value.CompareTo(other.Value);
    public static bool operator <(CampaignInstant left, CampaignInstant right) => left.Value < right.Value;
    public static bool operator >(CampaignInstant left, CampaignInstant right) => left.Value > right.Value;
    public static bool operator <=(CampaignInstant left, CampaignInstant right) => left.Value <= right.Value;
    public static bool operator >=(CampaignInstant left, CampaignInstant right) => left.Value >= right.Value;
}

/// <summary>One actual campaign occurrence linked to, but not identical with, its authored event.</summary>
public sealed record SignalOccurrence(Guid OccurrenceId, Guid AuthoredEventId, string LocationId, CampaignInstant OccurredAt);

/// <summary>A runtime-issued broadcast reference. Authored wording and claims remain in the StoryBuilder artifact.</summary>
public sealed record BroadcastIssue(
    Guid IssueId,
    Guid SignalOccurrenceId,
    string AuthoredContentArtifactId,
    string AuthoredSourceId,
    CampaignInstant PublishedAt,
    Guid? CorrectsIssueId = null);

/// <summary>A channel's availability of one issued broadcast at one station.</summary>
public sealed record MediaDelivery(Guid DeliveryId, Guid IssueId, string ChannelId, string StationId, CampaignInstant AvailableAt);

/// <summary>Evidence that one observer actually acquired one delivery.</summary>
public sealed record ObserverReceipt(Guid ReceiptId, Guid DeliveryId, string ObserverId, CampaignInstant ReceivedAt, AcquisitionMode Mode);

/// <summary>How the observer acquired channel content; this does not prove its claims are true.</summary>
public enum AcquisitionMode { Viewed, Heard, Read, Retold }

/// <summary>A player's deliberate response to a station story opportunity.</summary>
public enum StationStoryChoice
{
    Watch,
    Ask,
    Ignore,
    Leave
}

/// <summary>A player decision made during a station story opportunity.</summary>
public sealed record StationStoryDecision(
    Guid DecisionId,
    string PlayerId,
    StationStoryChoice Choice,
    CampaignInstant ChosenAt,
    string? TargetObserverId = null);
