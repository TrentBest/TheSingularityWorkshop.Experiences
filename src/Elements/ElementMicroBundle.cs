using TheSingularityWorkshop.FSM_COS;
using TheSingularityWorkshop.MicroBundleDomain;

namespace TheSingularityWorkshop.Experiences.Elements;

public sealed class ElementMicroBundle : IMicroBundle
{
    public const ulong BundleId = 3301;
    public const string ProviderId = "workshop-elements";

    public MicroBundleDescriptor Descriptor { get; } =
        new(BundleId, "1.0.0", providers: [new MicroBundleProvider(ProviderId)]);

    public ulong Id => Descriptor.Id;

    public IReadOnlyList<BundleRequest> Dependencies => [];

    public Element Definition => Iron.Definition;

    public void Load(MicroBundleLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
    }

    public bool Arbitrate(ArbitrationContext context, int roundIndex)
    {
        ArgumentNullException.ThrowIfNull(context);
        return false;
    }
}
