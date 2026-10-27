namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>
/// Deterministic in-memory logical-channel subscription registry. It routes by logical identity only;
/// it does not model radio range, interference, frequency assignment, delivery latency, or acknowledgements.
/// </summary>
public sealed class LogicalCommunicationRouter
{
    private readonly Dictionary<LogicalCommunicationChannelKey, SortedSet<ulong>> _subscribers = new();

    /// <summary>Gets the total number of active channel subscriptions.</summary>
    public int SubscriptionCount => _subscribers.Values.Sum(subscribers => subscribers.Count);

    /// <summary>
    /// Adds a subscriber to a channel. Returns false for an invalid subscriber identity or a duplicate
    /// subscription; callers should explicitly unsubscribe from an old lane when changing lanes.
    /// </summary>
    public bool TrySubscribe(LogicalCommunicationChannelKey channel, ulong subscriberId)
    {
        ArgumentNullException.ThrowIfNull(channel);
        if (subscriberId == 0)
            return false;

        if (!_subscribers.TryGetValue(channel, out var subscribers))
        {
            subscribers = new SortedSet<ulong>();
            _subscribers.Add(channel, subscribers);
        }

        return subscribers.Add(subscriberId);
    }

    /// <summary>Removes one subscription and removes an empty channel bucket.</summary>
    public bool TryUnsubscribe(LogicalCommunicationChannelKey channel, ulong subscriberId)
    {
        ArgumentNullException.ThrowIfNull(channel);
        if (!_subscribers.TryGetValue(channel, out var subscribers) || !subscribers.Remove(subscriberId))
            return false;

        if (subscribers.Count == 0)
            _subscribers.Remove(channel);

        return true;
    }

    /// <summary>Returns a snapshot of subscriber identities in ascending, deterministic order.</summary>
    public IReadOnlyList<ulong> GetSubscribers(LogicalCommunicationChannelKey channel)
    {
        ArgumentNullException.ThrowIfNull(channel);
        return _subscribers.TryGetValue(channel, out var subscribers)
            ? Array.AsReadOnly(subscribers.ToArray())
            : Array.Empty<ulong>();
    }

    /// <summary>
    /// Routes a fresh message to subscribers of its exact logical channel, excluding its sender.
    /// The returned recipient snapshot is sorted by subscriber identity. This method does not enqueue,
    /// retry, acknowledge, or guarantee transport delivery.
    /// </summary>
    public IReadOnlyList<ulong> GetRecipients(CommunicationMessageEnvelope message, long currentTick)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (!message.IsFreshAt(currentTick)
            || !_subscribers.TryGetValue(message.Channel, out var subscribers)
            || subscribers.Count == 0)
            return Array.Empty<ulong>();

        return Array.AsReadOnly(subscribers.Where(id => id != message.SenderId).ToArray());
    }
}
