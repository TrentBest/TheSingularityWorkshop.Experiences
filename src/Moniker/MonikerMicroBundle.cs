using TheSingularityWorkshop.MicroBundleDomain;
using TheSingularityWorkshop.Workshop.Gui;

namespace TheSingularityWorkshop.Experiences.Moniker;

/// <summary>
/// The canonical Workshop Moniker MicroBundle.
/// </summary>
public sealed class MonikerMicroBundle : IMicroBundle
{
    /// <summary>
    /// Stable identity of the Workshop Moniker MicroBundle.
    /// </summary>
    public const ulong BundleId = 3101;

    /// <summary>
    /// Provider identity advertised by the Moniker bundle.
    /// </summary>
    public const string ProviderId = "workshop-moniker";

    /// <summary>
    /// Describes the immutable Moniker bundle identity and provider.
    /// </summary>
    public MicroBundleDescriptor Descriptor { get; } =
        new(BundleId, "1.0.0", providers: [new MicroBundleProvider(ProviderId)]);

    /// <summary>
    /// Gets the bundle identity.
    /// </summary>
    public ulong Id => Descriptor.Id;

    /// <summary>
    /// Gets the Moniker bundle dependencies.
    /// </summary>
    public IReadOnlyList<MicroBundleDependencyRequest> Dependencies => [];

    /// <summary>
    /// Gets the semantic GUI surface produced when the bundle has been loaded.
    /// </summary>
    public GuiNode? Root { get; private set; }

    /// <summary>
    /// Materializes the semantic Workshop Moniker GUI surface.
    /// </summary>
    public void Load(IMicroBundleLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var builder = GuiBuilders.Column("moniker-column")
            .Property("horizontalAlignment", "Center")
            .Property("verticalAlignment", "Center")
            .Property("padding", "24");

        foreach (var (word, offset) in new[] { ("THE", 0), ("SINGULARITY", 3), ("WORKSHOP", 14) })
        {
            var row = GuiBuilders.Row($"moniker-{word.ToLowerInvariant()}")
                .Property("horizontalAlignment", "Center");

            for (var index = 0; index < word.Length; index++)
            {
                var phase = (offset + index) % 6;
                var color = phase switch
                {
                    0 => "#FF3030",
                    1 => "#FF7A00",
                    2 => "#FFD34D",
                    3 => "#52E05A",
                    4 => "#00A8FF",
                    _ => "#FF2CFF"
                };

                row.Child(GuiBuilders.Text(
                    $"moniker-{word.ToLowerInvariant()}-{index}", word[index].ToString())
                    .Property("foreground", color)
                    .Property("fontSize", "76")
                    .Property("fontWeight", "700")
                    .Property("fontFamily", "Consolas"));
            }

            builder.Child(row);
        }

        Root = GuiBuilders.Panel("moniker-root")
            .Property("background", "#020711")
            .Child(builder)
            .Build();
    }

    /// <summary>
    /// Participates in deterministic arbitration. The Moniker currently requires no arbitration changes.
    /// </summary>
    public bool Arbitrate(IMicroBundleArbitrationContext context, int roundIndex)
    {
        ArgumentNullException.ThrowIfNull(context);
        return false;
    }
}
