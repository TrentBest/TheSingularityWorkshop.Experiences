namespace TheSingularityWorkshop.Experiences.Contracts;

/// <summary>A semantic contract version. Versions are compared numerically, never lexically.</summary>
public readonly record struct ContractVersion(int Major, int Minor, int Patch) : IComparable<ContractVersion>
{
    public int CompareTo(ContractVersion other)
    {
        var major = Major.CompareTo(other.Major);
        if (major != 0) return major;
        var minor = Minor.CompareTo(other.Minor);
        return minor != 0 ? minor : Patch.CompareTo(other.Patch);
    }

    public override string ToString() => $"{Major}.{Minor}.{Patch}";
}

/// <summary>A half-open semantic contract version interval: minimum inclusive, maximum exclusive.</summary>
public sealed record ContractVersionRange(ContractVersion MinimumInclusive, ContractVersion? MaximumExclusive = null)
{
    public bool IsValid =>
        MinimumInclusive.Major >= 0 && MinimumInclusive.Minor >= 0 && MinimumInclusive.Patch >= 0 &&
        (MaximumExclusive is null ||
         (MaximumExclusive.Value.Major >= 0 && MaximumExclusive.Value.Minor >= 0 &&
          MaximumExclusive.Value.Patch >= 0 &&
          MinimumInclusive.CompareTo(MaximumExclusive.Value) < 0));

    public bool Contains(ContractVersion version) =>
        IsValid && version.CompareTo(MinimumInclusive) >= 0 &&
        (MaximumExclusive is null || version.CompareTo(MaximumExclusive.Value) < 0);

    public override string ToString() => MaximumExclusive is null
        ? $">={MinimumInclusive}"
        : $">={MinimumInclusive} <{MaximumExclusive.Value}";
}

/// <summary>What an Experience requires, independent of the host's provider or input device.</summary>
public sealed record SemanticInteractionRequirement(
    string CapabilityId,
    ContractVersionRange ContractVersions,
    bool Required = true,
    string? Rationale = null);

/// <summary>The immutable identity of one published MicroBundle artifact.</summary>
public sealed record ArtifactIdentity(string BundleId, string Version, string Sha256)
{
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(BundleId) &&
        !string.IsNullOrWhiteSpace(Version) &&
        !string.IsNullOrWhiteSpace(Sha256) &&
        Sha256.Length == 64 &&
        Sha256.All(Uri.IsHexDigit);

    public string StableKey => $"{BundleId}\n{Version}\n{Sha256.ToLowerInvariant()}";
}

/// <summary>A provider's declared semantic contract support and the immutable artifact implementing it.</summary>
public sealed record SemanticCapabilityProvider(
    string ProviderId,
    ArtifactIdentity Artifact,
    IReadOnlyList<ProvidedSemanticCapability> Capabilities);

public sealed record ProvidedSemanticCapability(string CapabilityId, ContractVersion ContractVersion);

/// <summary>An artifact's immutable dependency edge.</summary>
public sealed record ArtifactDependency(ArtifactIdentity Artifact);

/// <summary>Catalog metadata for an artifact; metadata does not prove implementation correctness.</summary>
public sealed record ArtifactDescriptor(
    ArtifactIdentity Identity,
    IReadOnlyList<ArtifactDependency> Dependencies);

public sealed record ContractIssue(string Code, string Message);

public sealed record CapabilityProviderSelection(
    string CapabilityId,
    ContractVersion ContractVersion,
    SemanticCapabilityProvider Provider);

public sealed record ProviderResolution(
    bool Success,
    IReadOnlyList<CapabilityProviderSelection> Selections,
    IReadOnlyList<ContractIssue> Issues)
{
    public IReadOnlyList<SemanticCapabilityProvider> SelectedProviders =>
        Selections.Select(selection => selection.Provider)
            .GroupBy(provider => provider.Artifact.StableKey, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(provider => provider.Artifact.StableKey, StringComparer.Ordinal)
            .ToArray();
}

public sealed record ArtifactClosureResolution(
    bool Success,
    IReadOnlyList<ArtifactDescriptor> OrderedArtifacts,
    IReadOnlyList<ContractIssue> Issues);

/// <summary>Pure deterministic validation and matching; no IO, host APIs, or package dependencies.</summary>
public static class SemanticRequirementResolver
{
    public static ProviderResolution ResolveProviders(
        IEnumerable<SemanticInteractionRequirement> requirements,
        IEnumerable<SemanticCapabilityProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(requirements);
        ArgumentNullException.ThrowIfNull(providers);

        var issues = new List<ContractIssue>();
        var requirementList = requirements.ToArray();
        var providerList = providers.ToArray();

        foreach (var requirement in requirementList)
        {
            if (string.IsNullOrWhiteSpace(requirement.CapabilityId))
            {
                issues.Add(new("requirement.invalid-id", "A semantic requirement has an empty capability ID."));
                continue;
            }

            if (requirement.ContractVersions is null || !requirement.ContractVersions.IsValid)
            {
                issues.Add(new("requirement.invalid-version-range",
                    $"Requirement '{requirement.CapabilityId}' has an invalid contract version range."));
                continue;
            }

            var matches = providerList
                .Where(provider => IsValidProvider(provider) &&
                    provider.Capabilities.Any(capability =>
                        string.Equals(capability.CapabilityId, requirement.CapabilityId, StringComparison.Ordinal) &&
                        requirement.ContractVersions.Contains(capability.ContractVersion)))
                .GroupBy(provider => provider.Artifact.StableKey, StringComparer.Ordinal)
                .Select(group => group.OrderBy(provider => provider.ProviderId, StringComparer.Ordinal).First())
                .OrderBy(provider => provider.Artifact.StableKey, StringComparer.Ordinal)
                .ToArray();

            if (matches.Length == 0)
            {
                if (requirement.Required)
                    issues.Add(new("provider.missing",
                        $"No valid provider satisfies required capability '{requirement.CapabilityId}' ({requirement.ContractVersions})."));
                continue;
            }

            if (matches.Length > 1)
            {
                issues.Add(new("provider.ambiguous",
                    $"Capability '{requirement.CapabilityId}' has {matches.Length} compatible artifact providers; selection policy must resolve the ambiguity explicitly."));
                continue;
            }
        }

        if (issues.Count > 0)
            return new(false, Array.Empty<CapabilityProviderSelection>(), issues);

        var selections = new List<CapabilityProviderSelection>();
        foreach (var requirement in requirementList
                     .OrderBy(item => item.CapabilityId, StringComparer.Ordinal)
                     .ThenBy(item => item.ContractVersions.MinimumInclusive))
        {
            if (string.IsNullOrWhiteSpace(requirement.CapabilityId) || requirement.ContractVersions is null)
                continue;

            var matches = providerList
                .Where(provider => IsValidProvider(provider) &&
                    provider.Capabilities.Any(capability =>
                        string.Equals(capability.CapabilityId, requirement.CapabilityId, StringComparison.Ordinal) &&
                        requirement.ContractVersions.Contains(capability.ContractVersion)))
                .GroupBy(provider => provider.Artifact.StableKey, StringComparer.Ordinal)
                .Select(group => group.OrderBy(provider => provider.ProviderId, StringComparer.Ordinal).First())
                .ToArray();

            if (matches.Length == 1)
            {
                var provided = matches[0].Capabilities
                    .Where(capability =>
                        string.Equals(capability.CapabilityId, requirement.CapabilityId, StringComparison.Ordinal) &&
                        requirement.ContractVersions.Contains(capability.ContractVersion))
                    .OrderBy(capability => capability.ContractVersion)
                    .First();
                selections.Add(new(requirement.CapabilityId, provided.ContractVersion, matches[0]));
            }
        }

        return new(true, selections, Array.Empty<ContractIssue>());
    }

    public static ArtifactClosureResolution ResolveArtifactClosure(
        IEnumerable<ArtifactIdentity> roots,
        IEnumerable<ArtifactDescriptor> catalog)
    {
        ArgumentNullException.ThrowIfNull(roots);
        ArgumentNullException.ThrowIfNull(catalog);

        var issues = new List<ContractIssue>();
        var descriptors = catalog.ToArray();
        var byKey = new Dictionary<string, ArtifactDescriptor>(StringComparer.Ordinal);

        foreach (var descriptor in descriptors)
        {
            if (descriptor.Identity is null || !descriptor.Identity.IsValid)
            {
                issues.Add(new("artifact.invalid-identity", "Catalog contains an artifact with an invalid immutable identity."));
                continue;
            }

            if (!byKey.TryAdd(descriptor.Identity.StableKey, descriptor))
                issues.Add(new("artifact.duplicate-identity",
                    $"Catalog contains duplicate descriptor identity '{descriptor.Identity.BundleId}@{descriptor.Identity.Version}'."));
        }

        if (issues.Count > 0)
            return new(false, Array.Empty<ArtifactDescriptor>(), issues);

        var ordered = new List<ArtifactDescriptor>();
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);

        bool Visit(ArtifactIdentity identity)
        {
            if (identity is null || !identity.IsValid)
            {
                issues.Add(new("artifact.invalid-dependency", "A root or dependency has an invalid immutable identity."));
                return false;
            }

            var key = identity.StableKey;
            if (visited.Contains(key)) return true;
            if (!byKey.TryGetValue(key, out var descriptor))
            {
                issues.Add(new("artifact.missing-dependency",
                    $"Artifact '{identity.BundleId}@{identity.Version}' with the requested content hash is absent from the catalog."));
                return false;
            }

            if (!visiting.Add(key))
            {
                issues.Add(new("artifact.dependency-cycle",
                    $"Dependency cycle detected at '{identity.BundleId}@{identity.Version}'."));
                return false;
            }

            foreach (var dependency in descriptor.Dependencies
                         .OrderBy(item => item.Artifact?.StableKey ?? string.Empty, StringComparer.Ordinal))
            {
                if (!Visit(dependency.Artifact))
                {
                    visiting.Remove(key);
                    return false;
                }
            }

            visiting.Remove(key);
            visited.Add(key);
            ordered.Add(descriptor);
            return true;
        }

        foreach (var root in roots.OrderBy(item => item?.StableKey ?? string.Empty, StringComparer.Ordinal))
            Visit(root);

        return issues.Count > 0
            ? new(false, Array.Empty<ArtifactDescriptor>(), issues)
            : new(true, ordered, Array.Empty<ContractIssue>());
    }

    private static bool IsValidProvider(SemanticCapabilityProvider provider) =>
        provider is not null &&
        !string.IsNullOrWhiteSpace(provider.ProviderId) &&
        provider.Artifact is not null && provider.Artifact.IsValid &&
        provider.Capabilities is not null &&
        provider.Capabilities.All(capability =>
            capability is not null &&
            !string.IsNullOrWhiteSpace(capability.CapabilityId) &&
            capability.ContractVersion.Major >= 0 &&
            capability.ContractVersion.Minor >= 0 &&
            capability.ContractVersion.Patch >= 0);
}
