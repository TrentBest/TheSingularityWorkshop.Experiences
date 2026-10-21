using TheSingularityWorkshop.MicroBundleDomain;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.MicroBundles;

/// <summary>
/// Base for implemented, renderer-independent Drone Construction capabilities.
/// These bundles expose typed factories; the composition host remains responsible for lifecycle and resolution.
/// </summary>
public abstract class DroneConstructionMicroBundleBase : IMicroBundle
{
    protected DroneConstructionMicroBundleBase(ulong bundleId, string providerId)
    {
        if (bundleId == 0) throw new ArgumentOutOfRangeException(nameof(bundleId));
        if (string.IsNullOrWhiteSpace(providerId)) throw new ArgumentException("Provider identity is required.", nameof(providerId));
        Descriptor = new MicroBundleDescriptor(bundleId, "0.1.0-alpha.1", providers: [new MicroBundleProvider(providerId)]);
    }

    public MicroBundleDescriptor Descriptor { get; }
    public ulong Id => Descriptor.Id;
    public IReadOnlyList<MicroBundleDependencyRequest> Dependencies => [];

    public void Load(IMicroBundleLoadContext context) => ArgumentNullException.ThrowIfNull(context);

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
    public const ulong BundleId = 3410;
    public const string ProviderId = "workshop-drone-construction.height-field";
    public HeightFieldMicroBundle() : base(BundleId, ProviderId) { }

    public ImageHeightField Create(int width, int height, ReadOnlySpan<byte> rgb, int maximumHeight, ChannelHeightMapping mapping, string sourceDigest) =>
        ImageHeightField.FromRgb(width, height, rgb, maximumHeight, mapping, sourceDigest);
}

/// <summary>MicroBundle provider for target cells and block placement accounting.</summary>
public sealed class BuildGridMicroBundle : DroneConstructionMicroBundleBase
{
    public const ulong BundleId = 3411;
    public const string ProviderId = "workshop-drone-construction.build-grid";
    public BuildGridMicroBundle() : base(BundleId, ProviderId) { }

    public BuildGrid Create(int width, int height, Func<int, int, int> requiredHeight) => new(width, height, requiredHeight);
}

/// <summary>MicroBundle provider for finite block inventory and pickup operations.</summary>
public sealed class ResourceEmitterMicroBundle : DroneConstructionMicroBundleBase
{
    public const ulong BundleId = 3412;
    public const string ProviderId = "workshop-drone-construction.resource-emitter";
    public ResourceEmitterMicroBundle() : base(BundleId, ProviderId) { }

    public BlockEmitter Create(string emitterId, int initialInventory, int pickupCapacity = 1) => new(emitterId, initialInventory, pickupCapacity);
}

/// <summary>MicroBundle provider for one-way 2D cellular skylanes.</summary>
public sealed class SkylaneMicroBundle : DroneConstructionMicroBundleBase
{
    public const ulong BundleId = 3413;
    public const string ProviderId = "workshop-drone-construction.skylane";
    public SkylaneMicroBundle() : base(BundleId, ProviderId) { }

    public Skylane Create(string laneId, int width, int height, LaneDirection direction, int elevation, int cellsPerTick = 1) =>
        new(laneId, width, height, direction, elevation, cellsPerTick);
}

/// <summary>MicroBundle provider for 2D cellular hover/holding lanes.</summary>
public sealed class HoverLaneMicroBundle : DroneConstructionMicroBundleBase
{
    public const ulong BundleId = 3414;
    public const string ProviderId = "workshop-drone-construction.hover-lane";
    public HoverLaneMicroBundle() : base(BundleId, ProviderId) { }

    public HoverLane Create(string laneId, int width, int height) => new(laneId, width, height);
}

/// <summary>MicroBundle provider for bounded transition reservation and lookahead checks.</summary>
public sealed class SlipstreamMicroBundle : DroneConstructionMicroBundleBase
{
    public const ulong BundleId = 3415;
    public const string ProviderId = "workshop-drone-construction.slipstream";
    public SlipstreamMicroBundle() : base(BundleId, ProviderId) { }

    public Slipstream Create(string slipstreamId, int transitionCellCount, int lookaheadCells = 5) =>
        new(slipstreamId, transitionCellCount, lookaheadCells);
}

/// <summary>MicroBundle provider for shared exclusive cell occupancy and reservations.</summary>
public sealed class TrafficCellMicroBundle : DroneConstructionMicroBundleBase
{
    public const ulong BundleId = 3416;
    public const string ProviderId = "workshop-drone-construction.traffic-cell";
    public TrafficCellMicroBundle() : base(BundleId, ProviderId) { }

    public TrafficCellGrid Create(int width, int height) => new(width, height);
}
