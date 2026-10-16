using TheSingularityWorkshop.MicroBundleDomain;
using TheSingularityWorkshop.Workshop.Gui;

namespace TheSingularityWorkshop.Experiences.CastleSiege;

/// <summary>
/// The initial semantic entry surface for the Castle Siege Experience.
/// </summary>
/// <remarks>
/// This is an Experience shell, not a playable simulation. Its status text is deliberately explicit
/// because a trajectory runner, target-intersection adapter, and damage model are not yet connected.
/// </remarks>
public sealed class CastleSiegeMicroBundle : IMicroBundle
{
    /// <summary>Stable identity reserved for the Castle Siege Experience and its initial bundle.</summary>
    public const ulong ExperienceId = 3301;

    /// <summary>Stable identity of the initial Castle Siege MicroBundle.</summary>
    public const ulong BundleId = 3301;

    /// <summary>Provider identity advertised by the Castle Siege bundle.</summary>
    public const string ProviderId = "workshop-castle-siege";

    /// <summary>Describes the initial Castle Siege bundle identity and provider.</summary>
    public MicroBundleDescriptor Descriptor { get; } =
        new(BundleId, "0.1.0-alpha.1", providers: [new MicroBundleProvider(ProviderId)]);

    /// <summary>Gets the bundle identity.</summary>
    public ulong Id => Descriptor.Id;

    /// <summary>Gets the required MicroBundles. The initial entry surface has no dependencies.</summary>
    public IReadOnlyList<MicroBundleDependencyRequest> Dependencies => [];

    /// <summary>Gets the semantic GUI surface produced when the bundle is loaded.</summary>
    public GuiNode? Root { get; private set; }

    /// <summary>Builds an honest, non-interactive entry surface while simulation integration is pending.</summary>
    public void Load(IMicroBundleLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var root = GuiBuilder.Create("Panel", "castle-siege-root")
            .Property("background", "#020711");

        root.Child("Column", "castle-siege-column", column =>
        {
            column.Property("horizontalAlignment", "Center")
                .Property("verticalAlignment", "Center")
                .Property("padding", "28");

            column.Child("Text", "castle-siege-kicker", text =>
                text.Text("THE SINGULARITY WORKSHOP / EXPERIENCE")
                    .Property("foreground", "#00A8FF")
                    .Property("fontSize", "13")
                    .Property("fontWeight", "700")
                    .Property("fontFamily", "Consolas")
                    .Property("margin", "0,0,0,16"));

            column.Child("Text", "castle-siege-title", text =>
                text.Text("CASTLE SIEGE")
                    .Property("foreground", "#FFFFFF")
                    .Property("fontSize", "54")
                    .Property("fontWeight", "700")
                    .Property("fontFamily", "Consolas")
                    .Property("margin", "0,0,0,12"));

            column.Child("Text", "castle-siege-subtitle", text =>
                text.Text("Build the defence. Test the shot. Explain the result.")
                    .Property("foreground", "#A6BCD0")
                    .Property("fontSize", "17")
                    .Property("fontFamily", "Consolas")
                    .Property("margin", "0,0,0,26"));

            column.Child("Text", "castle-siege-physics-status", text =>
                text.Text("PHYSICS RUN NOT CONNECTED")
                    .Property("foreground", "#FFD34D")
                    .Property("fontSize", "15")
                    .Property("fontWeight", "700")
                    .Property("fontFamily", "Consolas")
                    .Property("margin", "0,0,0,12"));

            column.Child("Text", "castle-siege-scope", text =>
                text.Text("Entry surface only — no computed trajectory, hit, damage, or multiplayer is claimed.")
                    .Property("foreground", "#D6E0EA")
                    .Property("fontSize", "14")
                    .Property("fontFamily", "Consolas")
                    .Property("margin", "0,0,0,18"));

            column.Child("Text", "castle-siege-next", text =>
                text.Text("NEXT: deterministic trajectory → target intersection → impact evidence")
                    .Property("foreground", "#52E05A")
                    .Property("fontSize", "13")
                    .Property("fontWeight", "700")
                    .Property("fontFamily", "Consolas"));
        });

        Root = root.Build();
    }

    /// <summary>
    /// Participates in deterministic arbitration. The initial entry surface requires no arbitration changes.
    /// </summary>
    public bool Arbitrate(IMicroBundleArbitrationContext context, int roundIndex)
    {
        ArgumentNullException.ThrowIfNull(context);
        return false;
    }
}