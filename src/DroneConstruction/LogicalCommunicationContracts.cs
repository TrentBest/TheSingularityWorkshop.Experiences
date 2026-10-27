namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>Defines the audience represented by a logical communication channel key.</summary>
public enum LogicalCommunicationScopeKind
{
    /// <summary>Participants admitted to one specific lane.</summary>
    Lane = 0,
    /// <summary>Participants coordinating within one region and layer, across lanes.</summary>
    LayerCoordination = 1,
    /// <summary>Region-level operational control audience.</summary>
    RegionControl = 2,
    /// <summary>Region-level emergency audience that is independent of lane membership.</summary>
    Emergency = 3
}

/// <summary>
/// Stable logical routing identity for a lane or an explicitly broader coordination audience.
/// This identity is not a physical radio frequency or an airspace admission grant.
/// </summary>
public sealed record LogicalCommunicationChannelKey
{
    /// <summary>Creates a lane channel scoped to one region, layer, and lane.</summary>
    public LogicalCommunicationChannelKey(string regionId, string layerId, string laneId)
        : this(LogicalCommunicationScopeKind.Lane, regionId, layerId, laneId)
    {
    }

    /// <summary>
    /// Creates a channel key with scope-specific identity. Lane requires region, layer, and lane;
    /// layer coordination requires region and layer; region control and emergency require region only.
    /// </summary>
    public LogicalCommunicationChannelKey(
        LogicalCommunicationScopeKind scopeKind,
        string regionId,
        string? layerId = null,
        string? laneId = null)
    {
        if (!Enum.IsDefined(scopeKind))
            throw new ArgumentOutOfRangeException(nameof(scopeKind));
        if (string.IsNullOrWhiteSpace(regionId))
            throw new ArgumentException("A stable region identity is required.", nameof(regionId));

        switch (scopeKind)
        {
            case LogicalCommunicationScopeKind.Lane:
                if (string.IsNullOrWhiteSpace(layerId))
                    throw new ArgumentException("A stable layer identity is required for a lane channel.", nameof(layerId));
                if (string.IsNullOrWhiteSpace(laneId))
                    throw new ArgumentException("A stable lane identity is required for a lane channel.", nameof(laneId));
                break;
            case LogicalCommunicationScopeKind.LayerCoordination:
                if (string.IsNullOrWhiteSpace(layerId))
                    throw new ArgumentException("A stable layer identity is required for layer coordination.", nameof(layerId));
                if (laneId is not null)
                    throw new ArgumentException("Layer coordination is not lane-specific.", nameof(laneId));
                break;
            case LogicalCommunicationScopeKind.RegionControl:
            case LogicalCommunicationScopeKind.Emergency:
                if (layerId is not null || laneId is not null)
                    throw new ArgumentException("Region-level scopes must not specify a layer or lane.");
                break;
        }

        ScopeKind = scopeKind;
        RegionId = regionId;
        LayerId = layerId;
        LaneId = laneId;
    }

    /// <summary>Gets the audience scope represented by this key.</summary>
    public LogicalCommunicationScopeKind ScopeKind { get; }
    /// <summary>Gets the stable region identity.</summary>
    public string RegionId { get; }
    /// <summary>Gets the stable layer identity, or null for region-level scopes.</summary>
    public string? LayerId { get; }
    /// <summary>Gets the stable lane identity, or null when the scope spans lanes.</summary>
    public string? LaneId { get; }
}

/// <summary>Classifies delivery priority independently of any physical transport.</summary>
public enum CommunicationPriority
{
    /// <summary>Best-effort traffic that may be delayed or coalesced.</summary>
    Routine = 0,

    /// <summary>Traffic coordinating nearby participants or lane transitions.</summary>
    Coordination = 1,

    /// <summary>Operational control traffic that must be handled ahead of routine traffic.</summary>
    Control = 2,

    /// <summary>Safety-critical traffic that must remain distinguishable during congestion.</summary>
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
