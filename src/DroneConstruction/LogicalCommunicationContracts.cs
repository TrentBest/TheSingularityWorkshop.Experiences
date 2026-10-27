namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>
/// Stable logical routing identity for a region, layer, and lane. This is not a physical radio frequency.
/// </summary>
public sealed record LogicalCommunicationChannelKey
{
    /// <summary>Creates a channel identity scoped to one region, layer, and lane.</summary>
    public LogicalCommunicationChannelKey(string regionId, string layerId, string laneId)
    {
        if (string.IsNullOrWhiteSpace(regionId))
            throw new ArgumentException("A stable region identity is required.", nameof(regionId));
        if (string.IsNullOrWhiteSpace(layerId))
            throw new ArgumentException("A stable layer identity is required.", nameof(layerId));
        if (string.IsNullOrWhiteSpace(laneId))
            throw new ArgumentException("A stable lane identity is required.", nameof(laneId));

        RegionId = regionId;
        LayerId = layerId;
        LaneId = laneId;
    }

    /// <summary>Gets the stable region identity.</summary>
    public string RegionId { get; }

    /// <summary>Gets the stable layer identity.</summary>
    public string LayerId { get; }

    /// <summary>Gets the stable lane identity.</summary>
    public string LaneId { get; }
}

/// <summary>Classifies delivery priority independently of any physical transport.</summary>
public enum CommunicationPriority
{
    Routine = 0,
    Coordination = 1,
    Control = 2,
    Emergency = 3
}

/// <summary>
/// Immutable metadata for a deterministic logical message. Payload bytes are accounted for by the
/// caller; this envelope intentionally does not prescribe serialization or physical radio behavior.
/// </summary>
public sealed record CommunicationMessageEnvelope
{
    /// <summary>Creates validated metadata for one logical communication message.</summary>
    public CommunicationMessageEnvelope(
        LogicalCommunicationChannelKey channel,
        ulong senderId,
        ulong sequence,
        long createdTick,
        long expiresAtTick,
        string messageKind,
        int payloadBytes,
        CommunicationPriority priority,
        bool requiresAcknowledgement = false)
    {
        ArgumentNullException.ThrowIfNull(channel);
        if (senderId == 0) throw new ArgumentOutOfRangeException(nameof(senderId));
        if (sequence < 0) throw new ArgumentOutOfRangeException(nameof(sequence));
        if (createdTick < 0) throw new ArgumentOutOfRangeException(nameof(createdTick));
        if (expiresAtTick < createdTick) throw new ArgumentOutOfRangeException(nameof(expiresAtTick));
        if (string.IsNullOrWhiteSpace(messageKind))
            throw new ArgumentException("A message kind is required.", nameof(messageKind));
        if (payloadBytes < 0) throw new ArgumentOutOfRangeException(nameof(payloadBytes));
        if (!Enum.IsDefined(priority)) throw new ArgumentOutOfRangeException(nameof(priority));

        Channel = channel;
        SenderId = senderId;
        Sequence = sequence;
        CreatedTick = createdTick;
        ExpiresAtTick = expiresAtTick;
        MessageKind = messageKind;
        PayloadBytes = payloadBytes;
        Priority = priority;
        RequiresAcknowledgement = requiresAcknowledgement;
    }

    /// <summary>Gets the logical destination channel.</summary>
    public LogicalCommunicationChannelKey Channel { get; }
    /// <summary>Gets the stable sender identity.</summary>
    public ulong SenderId { get; }
    /// <summary>Gets the sender-local monotonically ordered message sequence.</summary>
    public ulong Sequence { get; }
    /// <summary>Gets the logical tick at which this message was created.</summary>
    public long CreatedTick { get; }
    /// <summary>Gets the last logical tick at which this message remains fresh.</summary>
    public long ExpiresAtTick { get; }
    /// <summary>Gets the application-defined message kind.</summary>
    public string MessageKind { get; }
    /// <summary>Gets payload size for deterministic traffic accounting.</summary>
    public int PayloadBytes { get; }
    /// <summary>Gets the delivery priority.</summary>
    public CommunicationPriority Priority { get; }
    /// <summary>Gets whether the sender requires an explicit acknowledgement.</summary>
    public bool RequiresAcknowledgement { get; }

    /// <summary>Returns whether the message is still fresh at the supplied logical tick.</summary>
    public bool IsFreshAt(long tick) => tick >= CreatedTick && tick <= ExpiresAtTick;
}
