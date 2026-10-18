namespace TheSingularityWorkshop.Experiences.MarbleRun;

/// <summary>Declarative scene setup owned by the Marble Run Experience, not a Physics MicroBundle.</summary>
/// <remarks>Geometry generation and rendering are experience concerns; validated collision geometry and bodies are passed to Physics.</remarks>
/// <param name="SceneId">Stable identity of this authored scene.</param>
/// <param name="CoordinateFrameId">Identity/version of the scene's coordinate frame.</param>
/// <param name="GravityX">World-frame gravity X component in metres per second squared.</param>
/// <param name="GravityY">World-frame gravity Y component in metres per second squared.</param>
/// <param name="GravityZ">World-frame gravity Z component in metres per second squared.</param>
/// <param name="MarbleRadiusMeters">Marble radius in metres.</param>
/// <param name="MarbleDensityKgPerCubicMeter">Declared homogeneous material density in kilograms per cubic metre.</param>
/// <param name="RequestedMarbleCount">Requested count; not a performance guarantee.</param>
/// <param name="GeometryRevision">Revision of authored terrain/track and collision-proxy input.</param>
/// <param name="PhysicsModelId">Stable identity/version of the intended Physics model.</param>
public sealed record MarbleRunSceneDefinition(
    string SceneId, string CoordinateFrameId,
    double GravityX, double GravityY, double GravityZ,
    double MarbleRadiusMeters, double MarbleDensityKgPerCubicMeter,
    int RequestedMarbleCount, string GeometryRevision, string PhysicsModelId)
{
    /// <summary>Gets whether identities and physical inputs are valid.</summary>
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
