namespace TheSingularityWorkshop.Experiences.Contracts;

/// <summary>
/// A repository-local binding for one artifact in a resolved Experience dependency closure.
/// The portable artifact identity is retained unchanged beside its local repository handle.
/// </summary>
public sealed record ExperienceArtifactBinding(ArtifactIdentity Artifact, ulong LocalHandle);

/// <summary>
/// A validated, dependency-first projection of an Experience resolution onto one repository's
/// explicit ontology-identity map. This is not an FSM_COS runtime manifest.
/// </summary>
public sealed record ExperienceArtifactBindingPlan(
    bool Success,
    IReadOnlyList<ExperienceArtifactBinding> OrderedBindings,
    IReadOnlyList<ContractIssue> Issues)
{
    /// <summary>
    /// Binds every artifact in a successful dependency-first resolution. No artifact is skipped,
    /// no handle is inferred, and the source manifest or resolution is never modified.
    /// </summary>
    public static ExperienceArtifactBindingPlan Create(
        ExperienceResolution resolution,
        MicroBundleIdentityMap identityMap)
    {
        ArgumentNullException.ThrowIfNull(resolution);
        ArgumentNullException.ThrowIfNull(identityMap);

        if (!resolution.Success)
        {
            var issues = resolution.Issues is { Count: > 0 }
                ? resolution.Issues.ToArray()
                : [new ContractIssue("binding.resolution-failed",
                    "The Experience resolution did not succeed; no local binding plan can be created.")];
            return Failure(issues);
        }

        if (resolution.OrderedArtifacts is null)
            return Failure(new("binding.invalid-resolution",
                "The successful Experience resolution contains a null ordered artifact list."));

        var bindings = new List<ExperienceArtifactBinding>(resolution.OrderedArtifacts.Count);
        var artifactKeys = new HashSet<string>(StringComparer.Ordinal);
        var localHandles = new HashSet<ulong>();

        foreach (var descriptor in resolution.OrderedArtifacts)
        {
            if (descriptor?.Identity is not { IsValid: true } artifact)
                return Failure(new("binding.invalid-artifact",
                    "The resolved dependency closure contains an artifact with an invalid identity."));

            if (!artifactKeys.Add(artifact.StableKey))
                return Failure(new("binding.duplicate-artifact",
                    $"The resolved dependency closure contains duplicate artifact identity '{artifact.BundleId}@{artifact.Version}'."));

            if (!identityMap.TryResolveLocalHandle(artifact, out var localHandle) || localHandle == 0)
                return Failure(new("binding.unresolved-artifact",
                    $"No repository-local handle is registered for '{artifact.BundleId}@{artifact.Version}' with the requested SHA-256."));

            if (!localHandles.Add(localHandle))
                return Failure(new("binding.aliased-local-handle",
                    $"Repository-local handle '{localHandle}' is assigned to more than one artifact in the resolved dependency closure."));

            bindings.Add(new ExperienceArtifactBinding(artifact, localHandle));
        }

        if (bindings.Count == 0)
            return Failure(new("binding.empty-closure",
                "A successful Experience resolution must contain at least one artifact to bind."));

        return new ExperienceArtifactBindingPlan(
            true,
            bindings.AsReadOnly(),
            Array.Empty<ContractIssue>());
    }

    private static ExperienceArtifactBindingPlan Failure(ContractIssue issue) =>
        Failure([issue]);

    private static ExperienceArtifactBindingPlan Failure(IReadOnlyList<ContractIssue> issues) =>
        new(false, Array.Empty<ExperienceArtifactBinding>(), issues);
}
