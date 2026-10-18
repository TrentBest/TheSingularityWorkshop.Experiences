namespace TheSingularityWorkshop.Experiences.DominoStudio;

/// <summary>Provenance carried by an image-derived or manually authored layout proposal.</summary>
/// <remarks>A proposal is not authoritative 3D truth. The image pipeline owns conversion; Physics owns motion.</remarks>
public sealed record DominoLayoutProposal(
    string SourceDigest,
    string ConverterId,
    string ConverterVersion,
    string CoordinateFrameId,
    string Interpretation,
    double CellWidthMeters,
    double CellDepthMeters,
    double Confidence,
    DominoLayoutGrid Grid)
{
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
