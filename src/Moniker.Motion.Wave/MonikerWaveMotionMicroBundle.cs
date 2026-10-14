using System.Globalization;
using TheSingularityWorkshop.Experiences.Moniker;
using TheSingularityWorkshop.MicroBundleDomain;
using TheSingularityWorkshop.Workshop.Gui;

namespace TheSingularityWorkshop.Experiences.Moniker.Motion.Wave;

/// <summary>
/// A reusable wave motion capability for semantic Moniker surfaces.
/// </summary>
/// <remarks>
/// The bundle contains presentation behavior only. It does not own the Moniker's content,
/// does not depend on a host renderer, and never mutates the source GUI tree.
/// </remarks>
public sealed class MonikerWaveMotionMicroBundle : IMicroBundle, IMonikerMotion
{
    /// <summary>Stable identity of the wave motion MicroBundle.</summary>
    public const ulong BundleId = 3102;

    /// <summary>Provider identity advertised by this motion bundle.</summary>
    public const string ProviderId = "workshop-moniker-motion-wave";

    /// <summary>Describes this independently addressable motion capability.</summary>
    public MicroBundleDescriptor Descriptor { get; } =
        new(BundleId, "1.0.0", providers: [new MicroBundleProvider(ProviderId)]);

    /// <summary>Gets the bundle identity.</summary>
    public ulong Id => Descriptor.Id;

    /// <summary>This motion has no required MicroBundle dependencies.</summary>
    public IReadOnlyList<MicroBundleDependencyRequest> Dependencies => [];

    /// <summary>Loads the motion capability. The static definition requires no runtime resources.</summary>
    public void Load(IMicroBundleLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
    }

    /// <inheritdoc />
    public GuiNode Apply(GuiNode root)
    {
        ArgumentNullException.ThrowIfNull(root);

        var phaseIndex = 0;
        return ApplyWave(root, ref phaseIndex);
    }

    /// <summary>Participates in arbitration without changing the bundle definition.</summary>
    public bool Arbitrate(IMicroBundleArbitrationContext context, int roundIndex)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentOutOfRangeException.ThrowIfNegative(roundIndex);
        return false;
    }

    private static GuiNode ApplyWave(GuiNode node, ref int phaseIndex)
    {
        var properties = new Dictionary<string, string>(node.Properties, StringComparer.Ordinal);

        // GUI node kinds are represented by the same string values accepted by GuiBuilder.Create.
        // Keeping this capability on the public string contract avoids a dependency on a removed enum.
        if (node.Kind == "Text" &&
            node.Id.StartsWith("moniker-", StringComparison.Ordinal))
        {
            properties["wavePeriod"] = "3.8";
            properties["waveAmplitude"] = "7";
            properties["waveRotation"] = "2.5";
            properties["wavePhase"] =
                ((phaseIndex++ % 12) / 12d).ToString(CultureInfo.InvariantCulture);
        }

        var children = new GuiNode[node.Children.Count];
        for (var index = 0; index < node.Children.Count; index++)
            children[index] = ApplyWave(node.Children[index], ref phaseIndex);

        return new GuiNode(
            node.Kind,
            node.Id,
            node.Text,
            node.Source,
            properties,
            children);
    }
}
