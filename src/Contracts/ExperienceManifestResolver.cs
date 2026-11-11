namespace TheSingularityWorkshop.Experiences.Contracts;

/// <summary>
/// The result of resolving an authored Experience manifest against semantic providers and an immutable
/// artifact catalog. Ordered artifacts are dependency-first and ready for a later materialization stage.
/// </summary>
public sealed record ExperienceResolution(
    bool Success,
    IReadOnlyList<CapabilityProviderSelection> CapabilitySelections,
    IReadOnlyList<ArtifactDescriptor> OrderedArtifacts,
    IReadOnlyList<ContractIssue> Issues);

/// <summary>
/// Resolves manifest-declared semantic requirements and immutable artifact roots without performing IO,
/// loading executable code, or selecting a host.
/// </summary>
public static class ExperienceManifestResolver
{
    /// <summary>
    /// Validates the manifest, selects unambiguous providers, and resolves the complete immutable artifact
    /// dependency closure. Required missing capabilities, ambiguous providers, missing artifacts, and
    /// dependency cycles are returned as structured issues.
    /// </summary>
    public static ExperienceResolution Resolve(
        ExperienceManifest manifest,
        IEnumerable<SemanticCapabilityProvider> providers,
        IEnumerable<ArtifactDescriptor> catalog)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(providers);
        ArgumentNullException.ThrowIfNull(catalog);

        try
        {
            manifest.Validate();
        }
        catch (InvalidOperationException exception)
        {
            return Failure(new ContractIssue("manifest.invalid", exception.Message));
        }

        var providerResolution = SemanticRequirementResolver.ResolveProviders(
            manifest.Requirements, providers);
        if (!providerResolution.Success)
            return Failure(providerResolution.Issues);

        var roots = manifest.Artifacts
            .Concat(providerResolution.SelectedProviders.Select(provider => provider.Artifact))
            .GroupBy(artifact => artifact.StableKey, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(artifact => artifact.StableKey, StringComparer.Ordinal)
            .ToArray();

        var closure = SemanticRequirementResolver.ResolveArtifactClosure(roots, catalog);
        if (!closure.Success)
            return Failure(closure.Issues);

        return new ExperienceResolution(
            true,
            providerResolution.Selections,
            closure.OrderedArtifacts,
            Array.Empty<ContractIssue>());
    }

    private static ExperienceResolution Failure(ContractIssue issue) =>
        Failure([issue]);

    private static ExperienceResolution Failure(IReadOnlyList<ContractIssue> issues) =>
        new(
            false,
            Array.Empty<CapabilityProviderSelection>(),
            Array.Empty<ArtifactDescriptor>(),
            issues);
}
