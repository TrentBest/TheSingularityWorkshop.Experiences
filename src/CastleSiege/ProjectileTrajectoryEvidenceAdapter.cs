using System.Collections.ObjectModel;
using System.Text.Json;

namespace TheSingularityWorkshop.Experiences.CastleSiege;

/// <summary>
/// Adapts the version-1 JSON evidence record emitted by Physics' projectile trajectory experiment.
/// This adapter validates and projects existing samples; it does not calculate or extrapolate physics.
/// </summary>
public static class ProjectileTrajectoryEvidenceAdapter
{
    public const string SupportedModelId = "kinematics.constant-acceleration-projectile";
    public const string SupportedModelVersion = "1";
    public const int SupportedRecordSchemaVersion = 1;

    /// <summary>Reads and validates actual Physics trajectory-run JSON into a presentation-neutral evidence object.</summary>
    public static CastleSiegeTrajectoryEvidence Read(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (RequiredInt(root, "schemaVersion") != SupportedRecordSchemaVersion)
            throw new InvalidOperationException("Unsupported Physics trajectory record schema version.");
        if (!string.Equals(RequiredString(root, "modelId"), SupportedModelId, StringComparison.Ordinal))
            throw new InvalidOperationException("Unsupported Physics trajectory model.");
        if (!string.Equals(RequiredString(root, "modelVersion"), SupportedModelVersion, StringComparison.Ordinal))
            throw new InvalidOperationException("Unsupported Physics trajectory model version.");

        var units = Required(root, "units");
        RequireUnit(units, "position", "m");
        RequireUnit(units, "velocity", "m/s");
        RequireUnit(units, "acceleration", "m/s^2");
        RequireUnit(units, "elapsedTime", "s");

        var definition = Required(root, "definition");
        var coordinateFrame = RequiredString(definition, "coordinateFrame");
        var deltaTime = RequiredFiniteNumber(definition, "deltaTime");
        var stepCount = RequiredInt(definition, "stepCount");
        if (deltaTime <= 0d || stepCount <= 0)
            throw new InvalidOperationException("Physics trajectory definition has an invalid timestep or step count.");

        var samplesElement = Required(root, "samples");
        if (samplesElement.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException("Physics trajectory samples must be an array.");
        var samples = new List<CastleSiegeTrajectorySample>();
        foreach (var sample in samplesElement.EnumerateArray())
        {
            var position = ReadVector(Required(sample, "position"), "position");
            var velocity = ReadVector(Required(sample, "velocity"), "velocity");
            var elapsed = RequiredFiniteNumber(sample, "elapsedTime");
            if (elapsed < 0d || (samples.Count > 0 && elapsed <= samples[^1].ElapsedTime))
                throw new InvalidOperationException("Physics trajectory sample times must be non-negative and strictly increasing.");
            samples.Add(new CastleSiegeTrajectorySample(samples.Count, elapsed, position, velocity));
        }

        if (samples.Count != stepCount + 1)
            throw new InvalidOperationException("Physics trajectory sample count does not match the declared step count.");
        if (Math.Abs((samples[^1].ElapsedTime - samples[0].ElapsedTime) - stepCount * deltaTime) >
            Math.Max(1e-9, Math.Abs(stepCount * deltaTime) * 1e-9))
            throw new InvalidOperationException("Physics trajectory sample duration does not match its declared timestep.");

        return new CastleSiegeTrajectoryEvidence(
            SupportedModelId,
            SupportedModelVersion,
            coordinateFrame,
            deltaTime,
            stepCount,
            new ReadOnlyCollection<CastleSiegeTrajectorySample>(samples));
    }

    private static JsonElement Required(JsonElement parent, string name)
    {
        if (parent.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException($"Expected an object while reading '{name}'.");
        foreach (var property in parent.EnumerateObject())
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                return property.Value;
        throw new InvalidOperationException($"Required Physics trajectory field '{name}' is missing.");
    }

    private static string RequiredString(JsonElement parent, string name)
    {
        var element = Required(parent, name);
        if (element.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(element.GetString()))
            throw new InvalidOperationException($"Physics trajectory field '{name}' must be a non-empty string.");
        return element.GetString()!;
    }

    private static int RequiredInt(JsonElement parent, string name)
    {
        var element = Required(parent, name);
        if (!element.TryGetInt32(out var value))
            throw new InvalidOperationException($"Physics trajectory field '{name}' must be an integer.");
        return value;
    }

    private static double RequiredFiniteNumber(JsonElement parent, string name)
    {
        var element = Required(parent, name);
        if (!element.TryGetDouble(out var value) || !double.IsFinite(value))
            throw new InvalidOperationException($"Physics trajectory field '{name}' must be finite.");
        return value;
    }

    private static void RequireUnit(JsonElement units, string name, string expected)
    {
        if (!string.Equals(RequiredString(units, name), expected, StringComparison.Ordinal))
            throw new InvalidOperationException($"Physics trajectory unit '{name}' must be '{expected}'.");
    }

    private static CastleSiegeVector3 ReadVector(JsonElement element, string name) =>
        new(RequiredFiniteNumber(element, "x"), RequiredFiniteNumber(element, "y"), RequiredFiniteNumber(element, "z"));
}

/// <summary>A validated, presentation-neutral sample copied from the Physics evidence record.</summary>
public sealed record CastleSiegeTrajectorySample(
    int Index,
    double ElapsedTime,
    CastleSiegeVector3 PositionMetres,
    CastleSiegeVector3 VelocityMetresPerSecond);

/// <summary>Three-dimensional finite vector with explicitly documented SI-unit interpretation.</summary>
public readonly record struct CastleSiegeVector3(double X, double Y, double Z);

/// <summary>
/// Validated trajectory evidence from Physics. This is not a collision, hit/miss, impact, or damage result.
/// </summary>
public sealed record CastleSiegeTrajectoryEvidence(
    string ModelId,
    string ModelVersion,
    string CoordinateFrame,
    double DeltaTimeSeconds,
    int CompletedSteps,
    ReadOnlyCollection<CastleSiegeTrajectorySample> Samples);
