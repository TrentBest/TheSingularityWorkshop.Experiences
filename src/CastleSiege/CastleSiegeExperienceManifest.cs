using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheSingularityWorkshop.Experiences.CastleSiege;

/// <summary>
/// Versioned, renderer-independent declaration of the Castle Siege Experience and its required artifacts.
/// This is distinct from a MicroBundle descriptor: it identifies the Experience as authored content.
/// </summary>
public sealed record CastleSiegeExperienceManifest
{
    public const int CurrentSchemaVersion = 1;
    public const ulong StableExperienceId = 3301;
    public const string StableExperienceKey = "castle-siege";

    private static readonly JsonSerializerOptions CanonicalOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;
    public ulong ExperienceId { get; init; } = StableExperienceId;
    public string ExperienceKey { get; init; } = StableExperienceKey;
    public string Version { get; init; } = "0.1.0-alpha.1";
    public string Title { get; init; } = "Castle Siege";
    public IReadOnlyList<ExperienceArtifactReference> Artifacts { get; init; } =
    [
        new("workshop-castle-siege", "0.1.0-alpha.1", null)
    ];
    public IReadOnlyList<ExperienceCapabilityRequirement> Capabilities { get; init; } =
    [
        new("physics.projectile-trajectory.constant-acceleration", CapabilityRequirementLevel.Required,
            "A deterministic sampled projectile trajectory is required for the first physics-backed slice.")
    ];

    /// <summary>Validates identity, artifact references, and capability requirements before use.</summary>
    public void Validate()
    {
        if (SchemaVersion != CurrentSchemaVersion)
            throw new InvalidOperationException($"Unsupported Castle Siege manifest schema version: {SchemaVersion}.");
        if (ExperienceId != StableExperienceId)
            throw new InvalidOperationException($"Castle Siege Experience ID must remain {StableExperienceId}.");
        if (!string.Equals(ExperienceKey, StableExperienceKey, StringComparison.Ordinal))
            throw new InvalidOperationException($"Castle Siege Experience key must remain '{StableExperienceKey}'.");
        if (string.IsNullOrWhiteSpace(Version))
            throw new InvalidOperationException("Experience version is required.");
        if (string.IsNullOrWhiteSpace(Title))
            throw new InvalidOperationException("Experience title is required.");
        if (Artifacts is null || Artifacts.Count == 0)
            throw new InvalidOperationException("At least one artifact reference is required.");
        if (Capabilities is null)
            throw new InvalidOperationException("Capability requirements must be declared.");

        var artifactIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var artifact in Artifacts)
        {
            if (artifact is null || string.IsNullOrWhiteSpace(artifact.ArtifactId))
                throw new InvalidOperationException("Every artifact requires a non-empty identity.");
            if (!artifactIds.Add(artifact.ArtifactId))
                throw new InvalidOperationException($"Duplicate artifact identity: '{artifact.ArtifactId}'.");
            if (string.IsNullOrWhiteSpace(artifact.Version))
                throw new InvalidOperationException($"Artifact '{artifact.ArtifactId}' requires a version.");
            if (artifact.Sha256 is not null &&
                (artifact.Sha256.Length != 64 || artifact.Sha256.Any(c => !Uri.IsHexDigit(c))))
                throw new InvalidOperationException($"Artifact '{artifact.ArtifactId}' has an invalid SHA-256 hash.");
        }

        var capabilityIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var capability in Capabilities)
        {
            if (capability is null || string.IsNullOrWhiteSpace(capability.CapabilityId))
                throw new InvalidOperationException("Every capability requirement needs a non-empty identity.");
            if (!capabilityIds.Add(capability.CapabilityId))
                throw new InvalidOperationException($"Duplicate capability requirement: '{capability.CapabilityId}'.");
            if (!Enum.IsDefined(capability.Level))
                throw new InvalidOperationException($"Capability '{capability.CapabilityId}' has an unknown requirement level.");
            if (string.IsNullOrWhiteSpace(capability.Rationale))
                throw new InvalidOperationException($"Capability '{capability.CapabilityId}' requires a rationale.");
        }
    }

    /// <summary>
    /// Produces a deterministic compact JSON representation by sorting unordered collections ordinally.
    /// A null hash means the artifact is referenced by identity/version but is not yet content-pinned.
    /// </summary>
    public string ToCanonicalJson()
    {
        Validate();
        var canonical = this with
        {
            Artifacts = Artifacts.OrderBy(x => x.ArtifactId, StringComparer.Ordinal).ToArray(),
            Capabilities = Capabilities.OrderBy(x => x.CapabilityId, StringComparer.Ordinal).ToArray()
        };
        return JsonSerializer.Serialize(canonical, CanonicalOptions);
    }

    /// <summary>Parses and validates a manifest JSON document.</summary>
    public static CastleSiegeExperienceManifest Parse(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var manifest = JsonSerializer.Deserialize<CastleSiegeExperienceManifest>(json, CanonicalOptions)
            ?? throw new JsonException("The Castle Siege manifest was empty.");
        manifest.Validate();
        return manifest;
    }
}

/// <summary>An immutable reference to an independently versioned Experience artifact.</summary>
public sealed record ExperienceArtifactReference(
    string ArtifactId,
    string Version,
    string? Sha256);

/// <summary>How strongly an Experience depends on a named capability.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<CapabilityRequirementLevel>))]
public enum CapabilityRequirementLevel
{
    Required,
    Preferred,
    Optional,
    Delegable,
    FrameCritical
}

/// <summary>A creator-declared capability need; it does not itself grant execution authority.</summary>
public sealed record ExperienceCapabilityRequirement(
    string CapabilityId,
    CapabilityRequirementLevel Level,
    string Rationale);
