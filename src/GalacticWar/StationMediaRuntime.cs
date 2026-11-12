namespace TheSingularityWorkshop.Experiences.GalacticWar;

/// <summary>
/// Append-only runtime ledger for one station's broadcast availability and observer receipts.
/// It stores opaque authored-artifact references, not a second StoryBuilder narrative database.
/// </summary>
public sealed class StationMediaRuntime
{
    private readonly Dictionary<Guid, SignalOccurrence> _occurrences = [];
    private readonly Dictionary<Guid, BroadcastIssue> _issues = [];
    private readonly Dictionary<Guid, MediaDelivery> _deliveries = [];
    private readonly Dictionary<Guid, ObserverReceipt> _receipts = [];
    private readonly Dictionary<Guid, StationStoryDecision> _decisions = [];
    private readonly Dictionary<Guid, PlayerPresentation> _presentations = [];

    /// <summary>Creates an empty media ledger for the specified station.</summary>
    /// <param name="stationId">Stable station identity.</param>
    public StationMediaRuntime(string stationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stationId);
        StationId = stationId;
    }

    /// <summary>Gets the station identity owned by this ledger.</summary>
    public string StationId { get; }
    /// <summary>Gets recorded campaign signal occurrences.</summary>
    public IReadOnlyCollection<SignalOccurrence> SignalOccurrences => _occurrences.Values;
    /// <summary>Gets immutable broadcast issues.</summary>
    public IReadOnlyCollection<BroadcastIssue> Issues => _issues.Values;
    /// <summary>Gets scheduled station-channel deliveries.</summary>
    public IReadOnlyCollection<MediaDelivery> Deliveries => _deliveries.Values;
    /// <summary>Gets evidence of observer acquisitions.</summary>
    public IReadOnlyCollection<ObserverReceipt> Receipts => _receipts.Values;
    /// <summary>Gets recorded player story decisions.</summary>
    public IReadOnlyCollection<StationStoryDecision> Decisions => _decisions.Values;
    /// <summary>Gets recorded player-facing presentations.</summary>
    public IReadOnlyCollection<PlayerPresentation> Presentations => _presentations.Values;

    /// <summary>Records an actual campaign occurrence without promoting an authored possibility by implication.</summary>
    public void RecordOccurrence(SignalOccurrence occurrence)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        if (occurrence.OccurrenceId == Guid.Empty || occurrence.AuthoredEventId == Guid.Empty)
            throw new ArgumentException("Occurrence and authored event identities are required.", nameof(occurrence));
        ArgumentException.ThrowIfNullOrWhiteSpace(occurrence.LocationId);
        if (occurrence.OccurredAt.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(occurrence), "Campaign time cannot be negative.");
        AddUnique(_occurrences, occurrence.OccurrenceId, occurrence, "signal occurrence");
    }

    /// <summary>Issues an immutable broadcast edition referencing authored content.</summary>
    public void IssueBroadcast(BroadcastIssue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);
        if (issue.IssueId == Guid.Empty || issue.SignalOccurrenceId == Guid.Empty)
            throw new ArgumentException("Issue and signal occurrence identities are required.", nameof(issue));
        ArgumentException.ThrowIfNullOrWhiteSpace(issue.AuthoredContentArtifactId);
        ArgumentException.ThrowIfNullOrWhiteSpace(issue.AuthoredSourceId);
        if (!_occurrences.TryGetValue(issue.SignalOccurrenceId, out var occurrence))
            throw new InvalidOperationException("A broadcast cannot reference an unrecorded campaign occurrence.");
        if (issue.PublishedAt < occurrence.OccurredAt)
            throw new InvalidOperationException("A broadcast cannot precede the signal occurrence it reports.");

        if (issue.CorrectsIssueId is { } correctedId)
        {
            if (!_issues.TryGetValue(correctedId, out var corrected))
                throw new InvalidOperationException("A correction must reference an existing issue.");
            if (corrected.SignalOccurrenceId != issue.SignalOccurrenceId)
                throw new InvalidOperationException("A correction must concern the same signal occurrence.");
            if (issue.PublishedAt < corrected.PublishedAt)
                throw new InvalidOperationException("A correction cannot be published before the issue it corrects.");
        }

        AddUnique(_issues, issue.IssueId, issue, "broadcast issue");
    }

    /// <summary>Makes an issued broadcast available on a station channel; this does not imply observer receipt.</summary>
    public void ScheduleDelivery(MediaDelivery delivery)
    {
        ArgumentNullException.ThrowIfNull(delivery);
        if (delivery.DeliveryId == Guid.Empty || delivery.IssueId == Guid.Empty)
            throw new ArgumentException("Delivery and issue identities are required.", nameof(delivery));
        ArgumentException.ThrowIfNullOrWhiteSpace(delivery.ChannelId);
        if (!string.Equals(delivery.StationId, StationId, StringComparison.Ordinal))
            throw new InvalidOperationException("The delivery targets a different station.");
        if (!_issues.TryGetValue(delivery.IssueId, out var issue))
            throw new InvalidOperationException("A delivery must reference an issued broadcast.");
        if (delivery.AvailableAt < issue.PublishedAt)
            throw new InvalidOperationException("A channel cannot carry an issue before it is published.");
        AddUnique(_deliveries, delivery.DeliveryId, delivery, "media delivery");
    }

    /// <summary>Records one observer's actual acquisition of an available channel delivery.</summary>
    public void RecordReceipt(ObserverReceipt receipt)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        if (receipt.ReceiptId == Guid.Empty || receipt.DeliveryId == Guid.Empty)
            throw new ArgumentException("Receipt and delivery identities are required.", nameof(receipt));
        ArgumentException.ThrowIfNullOrWhiteSpace(receipt.ObserverId);
        if (!Enum.IsDefined(receipt.Mode))
            throw new ArgumentOutOfRangeException(nameof(receipt), "Acquisition mode is not defined.");
        if (!_deliveries.TryGetValue(receipt.DeliveryId, out var delivery))
            throw new InvalidOperationException("A receipt must reference an existing delivery.");
        if (receipt.ReceivedAt < delivery.AvailableAt)
            throw new InvalidOperationException("An observer cannot receive a delivery before it becomes available.");
        if (_receipts.Values.Any(existing =>
            existing.DeliveryId == receipt.DeliveryId &&
            string.Equals(existing.ObserverId, receipt.ObserverId, StringComparison.Ordinal)))
            throw new InvalidOperationException("The observer already has a receipt for this delivery.");
        AddUnique(_receipts, receipt.ReceiptId, receipt, "observer receipt");
    }

    /// <summary>Returns only issues this observer actually acquired by the requested campaign instant.</summary>
    public IReadOnlyList<BroadcastIssue> GetReceivedIssues(string observerId, CampaignInstant asOf)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(observerId);
        return _receipts.Values
            .Where(receipt => string.Equals(receipt.ObserverId, observerId, StringComparison.Ordinal) && receipt.ReceivedAt <= asOf)
            .OrderBy(receipt => receipt.ReceivedAt)
            .Select(receipt => _issues[_deliveries[receipt.DeliveryId].IssueId])
            .ToArray();
    }

    /// <summary>Returns channel-available issues without implying that any observer received them.</summary>
    public IReadOnlyList<BroadcastIssue> GetChannelIssues(string channelId, CampaignInstant asOf)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channelId);
        return _deliveries.Values
            .Where(delivery => string.Equals(delivery.ChannelId, channelId, StringComparison.Ordinal) && delivery.AvailableAt <= asOf)
            .OrderBy(delivery => delivery.AvailableAt)
            .Select(delivery => _issues[delivery.IssueId])
            .ToArray();
    }

    /// <summary>Records player-facing presentation after that player has acquired the referenced issue.</summary>
    public void RecordPresentation(PlayerPresentation presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        if (presentation.PresentationId == Guid.Empty || presentation.IssueId == Guid.Empty)
            throw new ArgumentException("Presentation and issue identities are required.", nameof(presentation));
        ArgumentException.ThrowIfNullOrWhiteSpace(presentation.PlayerId);
        if (presentation.PresentedAt.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(presentation), "Campaign time cannot be negative.");

        var hasViewedReceipt = _receipts.Values.Any(receipt =>
            string.Equals(receipt.ObserverId, presentation.PlayerId, StringComparison.Ordinal) &&
            receipt.Mode == AcquisitionMode.Viewed &&
            receipt.ReceivedAt <= presentation.PresentedAt &&
            _deliveries[receipt.DeliveryId].IssueId == presentation.IssueId);
        if (!hasViewedReceipt)
            throw new InvalidOperationException("A player presentation requires a prior Viewed receipt for that issue.");

        AddUnique(_presentations, presentation.PresentationId, presentation, "player presentation");
    }

    /// <summary>Returns issues actually presented to the player by the requested campaign instant.</summary>
    public IReadOnlyList<BroadcastIssue> GetPresentedIssues(string playerId, CampaignInstant asOf)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playerId);
        return _presentations.Values
            .Where(item => string.Equals(item.PlayerId, playerId, StringComparison.Ordinal) && item.PresentedAt <= asOf)
            .OrderBy(item => item.PresentedAt)
            .Select(item => _issues[item.IssueId])
            .ToArray();
    }

    /// <summary>Records a player choice without forcing a quest or manufacturing observer knowledge.</summary>
    public void RecordPlayerChoice(StationStoryDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);
        if (decision.DecisionId == Guid.Empty)
            throw new ArgumentException("Decision identity is required.", nameof(decision));
        ArgumentException.ThrowIfNullOrWhiteSpace(decision.PlayerId);
        if (!Enum.IsDefined(decision.Choice))
            throw new ArgumentOutOfRangeException(nameof(decision), "Story choice is not defined.");
        if (decision.ChosenAt.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(decision), "Campaign time cannot be negative.");

        if (decision.Choice == StationStoryChoice.Watch &&
            !_presentations.Values.Any(item =>
                string.Equals(item.PlayerId, decision.PlayerId, StringComparison.Ordinal) &&
                item.PresentedAt <= decision.ChosenAt))
            throw new InvalidOperationException("Watching a report requires an actual player presentation by that time.");

        if (decision.Choice == StationStoryChoice.Ask)
            ArgumentException.ThrowIfNullOrWhiteSpace(decision.TargetObserverId);
        else if (decision.TargetObserverId is not null)
            throw new ArgumentException("Only an Ask choice may name an observer.", nameof(decision));

        AddUnique(_decisions, decision.DecisionId, decision, "player story decision");
    }

    private static void AddUnique<T>(Dictionary<Guid, T> target, Guid id, T value, string kind)
    {
        if (!target.TryAdd(id, value))
            throw new InvalidOperationException($"The {kind} identity '{id}' already exists.");
    }
}
