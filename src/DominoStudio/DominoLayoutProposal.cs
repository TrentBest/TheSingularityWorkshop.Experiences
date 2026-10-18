namespace TheSingularityWorkshop.Experiences.DominoStudio;

/// <summary>Provenance carried by an image-derived or manually authored layout proposal.</summary>
/// <remarks>A proposal is not authoritative 3D truth. The image pipeline owns conversion; Physics owns motion.</remarks>
/// <param name="SourceDigest">SHA-256 hex digest of the imported source image or declared source artifact.</param>
/// <param name="ConverterId">Stable identity of the converter or authoring tool.</param>
/// <param name="ConverterVersion">Version of the converter or authoring tool.</param>
/// <param name="CoordinateFrameId">Declared frame in which the grid is interpreted.</param>
/// <param name="Interpretation">Explicit interpretation such as top-down or manually authored.</param>
/// <param name="CellWidthMeters">Grid-cell width in metres.</param>
/// <param name="CellDepthMeters">Grid-cell depth in metres.</param>
/// <param name="Confidence">Converter confidence in [0,1], not confidence that the physics will succeed.</param>
/// <param name="Grid">Editable occupancy/orientation grid.</param>
public sealed record DominoLayoutProposal(
    string SourceDigest, string ConverterId, string ConverterVersion, string CoordinateFrameId,
    string Interpretation, double CellWidthMeters, double CellDepthMeters, double Confidence, DominoLayoutGrid Grid)
{
    /// <summary>Gets whether the provenance and scale fields are valid.</summary>
    public bool IsValid =>
        IsDigest(SourceDigest) &&
        !string.IsNullOrWhiteSpace(ConverterId) &&
        !string.IsNullOrWhiteSpace(ConverterVersion) &&
        !string.IsNullOrWhiteSpace(CoordinateFrameId) &&
        !string.IsNullOrWhiteSpace(Interpretation) &&
        double.IsFinite(CellWidthMeters) && CellWidthMeters > 0 &&
        double.IsFinite(CellDepthMeters) && CellDepthMeters > 0 &&
        double.IsFinite(Confidence) && Confidence >= 0 && Confidence <= 1 &&
        Grid is not null;

    private static bool IsDigest(string value) =>
        value is { Length: 64 } && value.All(character =>
            character is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F');
}
