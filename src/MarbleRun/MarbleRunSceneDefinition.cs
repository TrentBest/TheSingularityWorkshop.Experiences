namespace TheSingularityWorkshop.Experiences.MarbleRun;

/// <summary>Declarative scene setup owned by the Marble Run Experience, not a Physics MicroBundle.</summary>
/// <remarks>Geometry generation and rendering are experience concerns; validated collision geometry and bodies are passed to Physics.</remarks>
public sealed record MarbleRunSceneDefinition(
    string SceneId, string CoordinateFrameId,
    double GravityX, double GravityY, double GravityZ,
    double MarbleRadiusMeters, double MarbleDensityKgPerCubicMeter,
    int RequestedMarbleCount, string GeometryRevision, string PhysicsModelId)
{
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(SceneId) &&
        !string.IsNullOrWhiteSpace(CoordinateFrameId) &&
        !string.IsNullOrWhiteSpace(GeometryRevision) &&
        !string.IsNullOrWhiteSpace(PhysicsModelId) &&
        double.IsFinite(GravityX) && double.IsFinite(GravityY) && double.IsFinite(GravityZ) &&
        double.IsFinite(MarbleRadiusMeters) && MarbleRadiusMeters > 0 &&
        double.IsFinite(MarbleDensityKgPerCubicMeter) && MarbleDensityKgPerCubicMeter > 0 &&
        RequestedMarbleCount > 0;

    /// <summary>Derives marble mass under the explicitly declared homogeneous-sphere assumption.</summary>
    public double MarbleMassKg()
    {
        if (!IsValid) throw new InvalidOperationException("A valid scene definition is required.");
        return MarbleDensityKgPerCubicMeter * (4d / 3d) * Math.PI *
            MarbleRadiusMeters * MarbleRadiusMeters * MarbleRadiusMeters;
    }
}