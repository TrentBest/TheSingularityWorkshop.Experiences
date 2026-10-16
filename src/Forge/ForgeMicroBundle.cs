using TheSingularityWorkshop.MicroBundleDomain;
using TheSingularityWorkshop.Workshop.Gui;

namespace TheSingularityWorkshop.Experiences.Forge;

/// <summary>
/// The Forge Experience's initial semantic GUI surface.
/// </summary>
public sealed class ForgeMicroBundle : IMicroBundle
{
    /// <summary>Stable identity of the Forge Experience.</summary>
    public const ulong ExperienceId = 3201;

    /// <summary>Stable identity of the Forge runtime.</summary>
    public const ulong RuntimeId = 3211;

    /// <summary>Stable identity of the Forge MicroBundle.</summary>
    public const ulong BundleId = 3201;

    /// <summary>Provider identity advertised by the Forge bundle.</summary>
    public const string ProviderId = "workshop-forge";

    /// <summary>Describes the immutable Forge bundle identity and provider.</summary>
    public MicroBundleDescriptor Descriptor { get; } =
        new(BundleId, "1.0.0", providers: [new MicroBundleProvider(ProviderId)]);

    /// <summary>Gets the bundle identity.</summary>
    public ulong Id => Descriptor.Id;

    /// <summary>Gets the Forge bundle dependencies.</summary>
    public IReadOnlyList<MicroBundleDependencyRequest> Dependencies => [];

    /// <summary>Gets the semantic GUI surface produced when the bundle has been loaded.</summary>
    public GuiNode? Root { get; private set; }

    /// <summary>Materializes the semantic Forge GUI surface.</summary>
    public void Load(IMicroBundleLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var root = GuiBuilder.Create("Panel", "forge-root")
            .Property("background", "#020711");

        root.Child("Column", "forge-column", column =>
        {
            column.Property("horizontalAlignment", "Center")
                .Property("verticalAlignment", "Center")
                .Property("padding", "28");

            column.Child("Text", "forge-kicker", text =>
                text.Text("THE SINGULARITY WORKSHOP / FORGE")
                    .Property("foreground", "#00A8FF")
                    .Property("fontSize", "14")
                    .Property("fontWeight", "700")
                    .Property("fontFamily", "Consolas")
                    .Property("margin", "0,0,0,18"));

            column.Child("Text", "forge-title", text =>
                text.Text("THE FORGE")
                    .Property("foreground", "#FFFFFF")
                    .Property("fontSize", "64")
                    .Property("fontWeight", "700")
                    .Property("fontFamily", "Consolas")
                    .Property("margin", "0,0,0,10"));

            column.Child("Text", "forge-subtitle", text =>
                text.Text("Build it. Enter it. Follow the rabbit hole.")
                    .Property("foreground", "#A6BCD0")
                    .Property("fontSize", "17")
                    .Property("fontFamily", "Consolas")
                    .Property("margin", "0,0,0,28"));

            column.Child("Button", "forge-explore", button =>
                button.Text("EXPLORE EXPERIENCES  →")
                    .Property("width", "360")
                    .Property("height", "72")
                    .Property("margin", "12")
                    .Property("fontSize", "18")
                    .Property("fontWeight", "700")
                    .Property("fontFamily", "Consolas")
                    .Property("foreground", "#FFFFFF")
                    .Property("background", "#061A2A")
                    .Property("horizontalAlignment", "Center")
                    .Property("tooltip", "Enter the Experience district"));
        });

        Root = root.Build();
    }

    /// <summary>
    /// Participates in deterministic arbitration. The Forge currently requires no arbitration changes.
    /// </summary>
    public bool Arbitrate(IMicroBundleArbitrationContext context, int roundIndex)
    {
        ArgumentNullException.ThrowIfNull(context);
        return false;
    }
}
