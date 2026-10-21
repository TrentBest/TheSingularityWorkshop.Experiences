using TheSingularityWorkshop.MicroBundleDomain;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.MicroBundles;

/// <summary>
/// Base for implemented, renderer-independent Drone Construction capabilities.
/// These bundles expose typed factories; the composition host remains responsible for lifecycle and resolution.
/// </summary>
public abstract class DroneConstructionMicroBundleBase : IMicroBundle
{
    /// <summary>Initializes the base provider identity and stable identifier.</summary>
    protected DroneConstructionMicroBundleBase(ulong bundleId, string providerId)
    {
        if (bundleId == 0) throw new ArgumentOutOfRangeException(nameof(bundleId));
        if (string.IsNullOrWhiteSpace(providerId)) throw new ArgumentException("Provider identity is required.", nameof(providerId));
        Descriptor = new MicroBundleDescriptor(bundleId, "0.1.0-alpha.1", providers: [new MicroBundleProvider(providerId)]);
    }

    /// <summary>Gets the immutable descriptor for this MicroBundle.</summary>
    public MicroBundleDescriptor Descriptor { get; }
    /// <summary>Gets this MicroBundle's stable identifier.</summary>
    public ulong Id => Descriptor.Id;
    /// <summary>Gets the declared MicroBundle dependencies.</summary>
    public IReadOnlyList<MicroBundleDependencyRequest> Dependencies => [];

    /// <summary>Validates the host load context; no services are resolved by this initial provider.</summary>
    public void Load(IMicroBundleLoadContext context) => ArgumentNullException.ThrowIfNull(context);

    /// <summary>Declines arbitration because this provider has no host-level resolution behavior yet.</summary>
    public bool Arbitrate(IMicroBundleArbitrationContext context, int roundIndex)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentOutOfRangeException.ThrowIfNegative(roundIndex);
        return false;
    }
}

/// <summary>MicroBundle provider for versioned RGB-to-height mapping.</summary>
public sealed class HeightFieldMicroBundle : DroneConstructionMicroBundleBase
{
    /// <summary>Gets the stable proposed identifier for this MicroBundle.</summary>
    public const ulong BundleId = 3410;
    /// <summary>Gets the stable provider identity for this MicroBundle.</summary>
    public const string ProviderId = "workshop-drone-construction.height-field";
    /// <summary>Gets the stable proposed identifier for this MicroBundle.</summary>
    public HeightFieldMicroBundle() : base(BundleId, ProviderId) { }

    /// <summary>Creates the typed domain capability provided by this MicroBundle.</summary>
    public ImageHeightField Create(int width, int height, ReadOnlySpan<byte> rgb, int maximumHeight, ChannelHeightMapping mapping, string sourceDigest) =>
        ImageHeightField.FromRgb(width, height, rgb, maximumHeight, mapping, sourceDigest);
}

/// <summary>MicroBundle provider for target cells and block placement accounting.</summary>
public sealed class BuildGridMicroBundle : DroneConstructionMicroBundleBase
{
    /// <summary>Gets the stable proposed identifier for this MicroBundle.</summary>
    public const ulong BundleId = 3411;
    /// <summary>Gets the stable provider identity for this MicroBundle.</summary>
    public const string ProviderId = "workshop-drone-construction.build-grid";
    /// <summary>Gets the stable proposed identifier for this MicroBundle.</summary>
    public BuildGridMicroBundle() : base(BundleId, ProviderId) { }

    /// <summary>Creates the typed domain capability provided by this MicroBundle.</summary>
    public BuildGrid Create(int width, int height, Func<int, int, int> requiredHeight) => new(width, height, requiredHeight);
}

/// <summary>MicroBundle provider for finite block inventory and pickup operations.</summary>
public sealed class ResourceEmitterMicroBundle : DroneConstructionMicroBundleBase
{
    /// <summary>Gets the stable proposed identifier for this MicroBundle.</summary>
    public const ulong BundleId = 3412;
    /// <summary>Gets the stable provider identity for this MicroBundle.</summary>
    public const string ProviderId = "workshop-drone-construction.resource-emitter";
    /// <summary>Gets the stable proposed identifier for this MicroBundle.</summary>
    public ResourceEmitterMicroBundle() : base(BundleId, ProviderId) { }

    /// <summary>Creates the typed domain capability provided by this MicroBundle.</summary>
    public BlockEmitter Create(string emitterId, int initialInventory, int pickupCapacity = 1) => new(emitterId, initialInventory, pickupCapacity);

    /// <summary>Creates the Experience's ten independently inventoried block emitters.</summary>
    public IReadOnlyList<BlockEmitter> CreateTen(Func<int, int> initialInventoryByIndex, int pickupCapacity = 1)
    {
        ArgumentNullException.ThrowIfNull(initialInventoryByIndex);
        if (pickupCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(pickupCapacity));
        var emitters = new BlockEmitter[10];
        for (var index = 0; index < emitters.Length; index++)
        {
            var inventory = initialInventoryByIndex(index);
            if (inventory < 0) throw new ArgumentOutOfRangeException(nameof(initialInventoryByIndex), "Emitter inventory cannot be negative.");
            emitters[index] = new BlockEmitter($"emitter-{index + 1:D2}", inventory, pickupCapacity);
        }
        return emitters;
    }
}

/// <summary>MicroBundle provider for one-way 2D cellular skylanes.</summary>
public sealed class SkylaneMicroBundle : DroneConstructionMicroBundleBase
{
    /// <summary>Gets the stable proposed identifier for this MicroBundle.</summary>
    public const ulong BundleId = 3413;
    /// <summary>Gets the stable provider identity for this MicroBundle.</summary>
    public const string ProviderId = "workshop-drone-construction.skylane";
    /// <summary>Gets the stable proposed identifier for this MicroBundle.</summary>
    public SkylaneMicroBundle() : base(BundleId, ProviderId) { }

    /// <summary>Creates the typed domain capability provided by this MicroBundle.</summary>
    public Skylane Create(string laneId, int width, int height, LaneDirection direction, int elevation, int cellsPerTick = 1) =>
        new(laneId, width, height, direction, elevation, cellsPerTick);
}

/// <summary>MicroBundle provider for 2D cellular hover/holding lanes.</summary>
public sealed class HoverLaneMicroBundle : DroneConstructionMicroBundleBase
{
    /// <summary>Gets the stable proposed identifier for this MicroBundle.</summary>
    public const ulong BundleId = 3414;
    /// <summary>Gets the stable provider identity for this MicroBundle.</summary>
    public const string ProviderId = "workshop-drone-construction.hover-lane";
    /// <summary>Gets the stable proposed identifier for this MicroBundle.</summary>
    public HoverLaneMicroBundle() : base(BundleId, ProviderId) { }

    /// <summary>Creates the typed domain capability provided by this MicroBundle.</summary>
    public HoverLane Create(string laneId, int width, int height) => new(laneId, width, height);
}

/// <summary>MicroBundle provider for bounded transition reservation and lookahead checks.</summary>
public sealed class SlipstreamMicroBundle : DroneConstructionMicroBundleBase
{
    /// <summary>Gets the stable proposed identifier for this MicroBundle.</summary>
    public const ulong BundleId = 3415;
    /// <summary>Gets the stable provider identity for this MicroBundle.</summary>
    public const string ProviderId = "workshop-drone-construction.slipstream";
    /// <summary>Gets the stable proposed identifier for this MicroBundle.</summary>
    public SlipstreamMicroBundle() : base(BundleId, ProviderId) { }

    /// <summary>Creates the typed domain capability provided by this MicroBundle.</summary>
    public Slipstream Create(string slipstreamId, int transitionCellCount, int lookaheadCells = 5) =>
        new(slipstreamId, transitionCellCount, lookaheadCells);
}

/// <summary>MicroBundle provider for shared exclusive cell occupancy and reservations.</summary>
public sealed class TrafficCellMicroBundle : DroneConstructionMicroBundleBase
{
    /// <summary>Gets the stable proposed identifier for this MicroBundle.</summary>
    public const ulong BundleId = 3416;
    /// <summary>Gets the stable provider identity for this MicroBundle.</summary>
    public const string ProviderId = "workshop-drone-construction.traffic-cell";
    /// <summary>Gets the stable proposed identifier for this MicroBundle.</summary>
    public TrafficCellMicroBundle() : base(BundleId, ProviderId) { }

    /// <summary>Creates the typed domain capability provided by this MicroBundle.</summary>
    public TrafficCellGrid Create(int width, int height) => new(width, height);
}

/// <summary>MicroBundle provider for stable construction-agent identity and lifecycle state.</summary>
public sealed class ConstructionAgentMicroBundle : DroneConstructionMicroBundleBase
{
    /// <summary>Gets the stable proposed identifier for this MicroBundle.</summary>
    public const ulong BundleId = 3417;
    /// <summary>Gets the stable provider identity for this MicroBundle.</summary>
    public const string ProviderId = "workshop-drone-construction.agent";
    /// <summary>Gets the stable proposed identifier for this MicroBundle.</summary>
    public ConstructionAgentMicroBundle() : base(BundleId, ProviderId) { }

    /// <summary>Creates the typed domain capability provided by this MicroBundle.</summary>
    public ConstructionAgent Create(ulong agentId, GridCell initialPosition) => new(agentId, initialPosition);
}

/// <summary>MicroBundle provider for deterministic, idempotent placement-task assignment.</summary>
public sealed class ConstructionTaskSchedulerMicroBundle : DroneConstructionMicroBundleBase
{
    /// <summary>Gets the stable proposed identifier for this MicroBundle.</summary>
    public const ulong BundleId = 3418;
    /// <summary>Gets the stable provider identity for this MicroBundle.</summary>
    public const string ProviderId = "workshop-drone-construction.task-scheduler";
    /// <summary>Gets the stable proposed identifier for this MicroBundle.</summary>
    public ConstructionTaskSchedulerMicroBundle() : base(BundleId, ProviderId) { }

    /// <summary>Creates the typed domain capability provided by this MicroBundle.</summary>
    public ConstructionTaskScheduler Create() => new();
}
