using TheSingularityWorkshop.Experiences.CapsuleClinic;
using TheSingularityWorkshop.Renderer.VisualizeAdapter;
using TheSingularityWorkshop.Visualize;
using TheSingularityWorkshop.Visualize.GuiSvg;

namespace TheSingularityWorkshop.Experiences.CapsuleClinic.VisualizeIntegration;

/// <summary>Renders an authoritative Capsule Clinic snapshot through Renderer, Visualize, and GUI.SVG.</summary>
public static class CapsuleClinicPipelineSvgRenderer
{
    /// <summary>Produces one SVG document from the supplied authoritative snapshot.</summary>
    /// <param name="snapshot">Snapshot captured from the authoritative Capsule Clinic session.</param>
    /// <param name="cancellationToken">Cancellation requested by the HTTP request or host.</param>
    /// <returns>The complete SVG document.</returns>
    public static async Task<string> RenderAsync(
        CapsuleArenaSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        var presentation = CapsuleClinicRenderFrameFactory.Create(snapshot);
        var adaptedFrame = new RendererFrameAdapter().Adapt(presentation.Frame);
        string? svg = null;
        await using var visualizer = new Visualizer(
            new GuiSvgVisualizerBackend(document => svg = document,
                sceneLabel: "Capsule Clinic arena rendered through Renderer, Visualize, and GUI.SVG"),
            presentation.ResourceCatalog);
        var result = await visualizer.RenderAsync(adaptedFrame, cancellationToken);
        if (result.SubmittedDrawCount != presentation.CommandCount)
            throw new InvalidOperationException(
                $"Expected {presentation.CommandCount} submitted draws; got {result.SubmittedDrawCount}.");
        return svg ?? throw new InvalidOperationException("GUI.SVG did not produce a document.");
    }
}
