using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheSingularityWorkshop.Experiences.Contracts;

/// <summary>
/// A host-independent, versioned declaration of one authored Experience and the immutable artifacts
/// that provide its required semantic capabilities.
/// </summary>
/// <remarks>
/// This is the general Experiences contract. It deliberately does not describe a renderer, host,
/// runtime composition plan, repository location, or executable loading mechanism.
/// </remarks>
public sealed record ExperienceManifest
{
    public const int CurrentSchemaVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    /// <summary>Gets the manifest schema version. Unknown versions are rejected.</summary>
    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    /// <summary>Gets the stable, non-zero numeric Experience identity.</summary>
    public ulong ExperienceId { get; init; }

    /// <summary>Gets the stable, human-readable Experience key.</summary>
    public string ExperienceKey { get; init; } = string.Empty;

    /// <summary>Gets the authored Experience version.</summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>Gets the display title. This is not part of the stable identity.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the immutable artifact roots required by this Experience. Each identity includes its
    /// content SHA-256; a version without a content hash is not an executable artifact identity.
    /// </summary>
    public IReadOnlyList<ArtifactIdentity> Artifacts { get; init; } = [];

    /// <summary>Gets the semantic capabilities required or optionally consumed by this Experience.</summary>
    public IReadOnlyList<SemanticInteractionRequirement> Requirements { get; init; } = [];

    /// <summary>Validates schema, stable identity, artifact roots, and semantic requirements.</summary>
    /// <exception cref="InvalidOperationException">The manifest violates the schema or identity contract.</exception>
    public void Validate()
    {
        if (SchemaVersion != CurrentSchemaVersion)
            throw new InvalidOperationException($"Unsupported Experience manifest schema version: {SchemaVersion}.");
        if (ExperienceId == 0)
            throw new InvalidOperationException("Experience ID must be non-zero.");
        if (!IsValidIdentityPart(ExperienceKey))
            throw new InvalidOperationException("Experience key must be non-empty, trimmed, and contain no control characters.");
        if (!IsValidIdentityPart(Version))
            throw new InvalidOperationException("Experience version must be non-empty, trimmed, and contain no control characters.");
        if (string.IsNullOrWhiteSpace(Title))
            throw new InvalidOperationException("Experience title is required.");
        if (Artifacts is null || Artifacts.Count == 0)
            throw new InvalidOperationException("At least one immutable artifact root is required.");
        if (Requirements is null)
            throw new InvalidOperationException("The semantic requirement list cannot be null.");

        var artifactKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var artifact in Artifacts)
        {
            if (artifact is null || !artifact.IsValid)
                throw new InvalidOperationException("Every Experience artifact must have a valid bundle ID, version, and SHA-256.");
            if (!artifactKeys.Add(artifact.StableKey))
                throw new InvalidOperationException($"Duplicate Experience artifact identity: '{artifact.BundleId}@{artifact.Version}'.");
        }

        var capabilityIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var requirement in Requirements)
        {
            if (requirement is null || !IsValidIdentityPart(requirement.CapabilityId))
                throw new InvalidOperationException("Every semantic requirement must have a valid capability ID.");
            if (!capabilityIds.Add(requirement.CapabilityId))
                throw new InvalidOperationException($"Duplicate semantic requirement: '{requirement.CapabilityId}'.");
            if (requirement.ContractVersions is null || !requirement.ContractVersions.IsValid)
                throw new InvalidOperationException($"Semantic requirement '{requirement.CapabilityId}' has an invalid contract version range.");
        }
    }

    /// <summary>
    /// Serializes a validated manifest to deterministic compact JSON. Artifact and requirement
    /// collections are sorted ordinally; this is a repository-local canonical form, not a
    /// cross-language signing or cryptographic canonical-JSON standard.
    /// </summary>
    public string ToCanonicalJson()
    {
        Validate();
        var canonical = this with
        {
            Artifacts = Artifacts.OrderBy(item => item.StableKey, StringComparer.Ordinal).ToArray(),
            Requirements = Requirements
                .OrderBy(item => item.CapabilityId, StringComparer.Ordinal)
                .ThenBy(item => item.ContractVersions.MinimumInclusive)
                .ToArray()
        };
        return JsonSerializer.Serialize(canonical, JsonOptions);
    }

    /// <summary>Parses and validates a JSON manifest, rejecting unknown properties and schema versions.</summary>
    /// <exception cref="ArgumentException">The JSON input is null, empty, or whitespace.</exception>
    /// <exception cref="JsonException">The JSON is malformed or contains unknown properties.</exception>
    /// <exception cref="InvalidOperationException">The parsed manifest violates the contract.</exception>
    public static ExperienceManifest Parse(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var manifest = JsonSerializer.Deserialize<ExperienceManifest>(json, JsonOptions)
            ?? throw new JsonException("The Experience manifest was empty.");
        manifest.Validate();
        return manifest;
    }

    private static bool IsValidIdentityPart(string value) =>
        !string.IsNullOrWhiteSpace(value) &&
        string.Equals(value, value.Trim(), StringComparison.Ordinal) &&
        !value.Any(char.IsControl);
}
