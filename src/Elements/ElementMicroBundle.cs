using TheSingularityWorkshop.MicroBundleDomain;

namespace TheSingularityWorkshop.Experiences.Elements;

/// <summary>Domain-owned MicroBundle exposing the semantic Element identity.</summary>

public sealed class ElementMicroBundle : IMicroBundle
{
    /// <summary>Stable MicroBundle identifier for the Elements capability.</summary>
    public const ulong BundleId = 3301;
    /// <summary>Provider identity for the Elements capability.</summary>
    public const string ProviderId = "workshop-elements";

    /// <summary>Bundle descriptor.</summary>
    public MicroBundleDescriptor Descriptor { get; } =
        new(BundleId, "1.0.0", providers: [new MicroBundleProvider(ProviderId)]);

    /// <summary>Stable bundle identifier.</summary>
    public ulong Id => Descriptor.Id;

    /// <summary>No required dependencies for the initial atomic Element.</summary>
    public IReadOnlyList<MicroBundleDependencyRequest> Dependencies => [];

    /// <summary>The semantic Element definition exposed by this bundle.</summary>
    public Element Definition => Iron.Definition;

    /// <summary>Accepts the composition load context.</summary>
    public void Load(IMicroBundleLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
    }

    /// <summary>Participates in arbitration without changing the initial atomic definition.</summary>
    public bool Arbitrate(IMicroBundleArbitrationContext context, int roundIndex)
    {
        ArgumentNullException.ThrowIfNull(context);
        return false;
    }
}
