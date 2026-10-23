using TheSingularityWorkshop.MicroBundleDomain;
using TheSingularityWorkshop.Workshop.Gui;

namespace TheSingularityWorkshop.Experiences.Moniker;

/// <summary>
/// The canonical Workshop Moniker MicroBundle.
/// </summary>
public sealed class MonikerMicroBundle : IMicroBundle, IMicroBundleResolvedConfiguration
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

        var root = GuiBuilder.Create("Panel", "moniker-root")
            .Property("background", "#020711");

        root.Child("Column", "moniker-column", column =>
        {
            column.Property("horizontalAlignment", "Center")
                .Property("verticalAlignment", "Center")
                .Property("padding", "24");

            foreach (var (word, offset) in new[] { ("THE", 0), ("SINGULARITY", 3), ("WORKSHOP", 14) })
            {
                column.Child("Row", $"moniker-{word.ToLowerInvariant()}", row =>
                {
                    row.Property("horizontalAlignment", "Center");

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

                        row.Child("Text", $"moniker-{word.ToLowerInvariant()}-{index}", text =>
                            text.Text(word[index].ToString())
                                .Property("foreground", color)
                                .Property("fontSize", "76")
                                .Property("fontWeight", "700")
                                .Property("fontFamily", "Consolas"));
                    }
                });
            }
        });

        Root = root.Build();
    }

    /// <summary>
    /// Participates in deterministic arbitration. The Moniker currently requires no arbitration changes.
    /// </summary>
    /// <summary>
    /// This content bundle has no configuration overrides; its resolved defaults are empty.
    /// </summary>
    public ReadOnlyMemory<byte> ExportResolvedConfiguration() => ReadOnlyMemory<byte>.Empty;

    public bool Arbitrate(IMicroBundleArbitrationContext context, int roundIndex)
    {
        ArgumentNullException.ThrowIfNull(context);
        return false;
    }
}

/// <summary>
/// A reusable presentation behavior that can be composed with the canonical Moniker content bundle.
/// </summary>
/// <remarks>
/// Motion belongs to a separate MicroBundle. Implementations transform a semantic surface into a
/// presentation-ready surface without mutating the original content tree.
/// </remarks>
public interface IMonikerMotion
{
    /// <summary>Applies this motion's presentation parameters to a Moniker surface.</summary>
    /// <param name="root">The static semantic Moniker surface.</param>
    /// <returns>A presentation-ready surface that leaves <paramref name="root"/> unchanged.</returns>
    GuiNode Apply(GuiNode root);
}
