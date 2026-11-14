namespace TheSingularityWorkshop.Experiences.GalacticWar;

/// <summary>A monotonic instant in a campaign's fictional calendar, not a wall-clock timestamp.</summary>
public readonly record struct CampaignInstant(long Value) : IComparable<CampaignInstant>
{
    /// <summary>Compares this instant with another campaign instant.</summary>
    public int CompareTo(CampaignInstant other) => Value.CompareTo(other.Value);
    /// <summary>Determines whether the left instant precedes the right instant.</summary>
    public static bool operator <(CampaignInstant left, CampaignInstant right) => left.Value < right.Value;
    /// <summary>Determines whether the left instant follows the right instant.</summary>
    public static bool operator >(CampaignInstant left, CampaignInstant right) => left.Value > right.Value;
    /// <summary>Determines whether the left instant does not follow the right instant.</summary>
    public static bool operator <=(CampaignInstant left, CampaignInstant right) => left.Value <= right.Value;
    /// <summary>Determines whether the left instant does not precede the right instant.</summary>
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
public enum AcquisitionMode
{
    /// <summary>The observer visually viewed the delivery.</summary>
    Viewed,
    /// <summary>The observer heard the delivery.</summary>
    Heard,
    /// <summary>The observer read the delivery.</summary>
    Read
}

/// <summary>A player's deliberate response to a station story opportunity.</summary>
public enum StationStoryChoice
{
    /// <summary>The player watched the presented report.</summary>
    Watch,
    /// <summary>The player chose to ask a particular observer.</summary>
    Ask,
    /// <summary>The player dismissed the story opportunity.</summary>
    Ignore,
    /// <summary>The player left the station story opportunity.</summary>
    Leave
}

/// <summary>A player decision made during a station story opportunity.</summary>
public sealed record StationStoryDecision(
    Guid DecisionId,
    string PlayerId,
    StationStoryChoice Choice,
    CampaignInstant ChosenAt,
    string? TargetObserverId = null,
    Guid? PresentationId = null);

/// <summary>A recorded moment when an acquired issue was actually presented to the player.</summary>
public sealed record PlayerPresentation(
    Guid PresentationId,
    string PlayerId,
    Guid IssueId,
    CampaignInstant PresentedAt);
