using System.Globalization;
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.FSM_COS;
using TheSingularityWorkshop.MicroBundleDomain;
using TheSingularityWorkshop.Workshop.Gui;

namespace TheSingularityWorkshop.Experiences.Moniker;

public static class MonikerExperience
{
    public const ulong ExperienceId = 3101;
    public const ulong RuntimeId = 3111;

    public static RuntimeManifest CreateManifest() =>
        new(RuntimeId, [new MicroBundleDependencyRequest(MonikerMicroBundle.BundleId, null)], new Context());

    public static GuiNode ExecutePresentation(GuiNode root)
    {
        ArgumentNullException.ThrowIfNull(root);
        var phaseIndex = 0;
        return ApplyWave(root, ref phaseIndex);
    }

    private static GuiNode ApplyWave(GuiNode node, ref int phaseIndex)
    {
        var properties = new Dictionary<string, string>(node.Properties, StringComparer.Ordinal);

        if (node.Kind == GuiKinds.Text && node.Id.StartsWith("moniker-", StringComparison.Ordinal))
        {
            properties["wavePeriod"] = "3.8";
            properties["waveAmplitude"] = "7";
            properties["waveRotation"] = "2.5";
            properties["wavePhase"] = ((phaseIndex++ % 12) / 12d).ToString(CultureInfo.InvariantCulture);
        }

        var children = new GuiNode[node.Children.Count];
        for (var index = 0; index < node.Children.Count; index++)
            children[index] = ApplyWave(node.Children[index], ref phaseIndex);

        return new GuiNode(node.Kind, node.Id, node.Text, node.Source, properties, children);
    }

    private sealed class Context : IStateContext
    {
        public string Name { get; set; } = "Workshop.Moniker";
        public bool IsValid { get; set; } = true;
    }
}