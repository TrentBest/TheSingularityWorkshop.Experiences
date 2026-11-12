namespace TheSingularityWorkshop.Experiences.Contracts;

/// <summary>Maps immutable ontology artifact identities to repository-local numeric handles.</summary>
/// <remarks>
/// The ontology string is the portable identity and remains stable across repositories. A stored
/// artifact revision is identified by the ontology string, version, and SHA-256 together. The
/// non-zero numeric handle is only a local address assigned by a particular repository for
/// locating that exact artifact blob. It is not a global identity and is not portable between
/// repositories. The repository supplies persisted bindings; this type does not allocate handles,
/// hash ontology strings, or infer mappings.
/// </remarks>
public sealed class MicroBundleIdentityMap
{
    private readonly IReadOnlyDictionary<string, ulong> _handlesByArtifact;
    private readonly IReadOnlyDictionary<ulong, ArtifactIdentity> _artifactsByHandle;

    private MicroBundleIdentityMap(
        Dictionary<string, ulong> handlesByArtifact,
        Dictionary<ulong, ArtifactIdentity> artifactsByHandle)
    {
        _handlesByArtifact = new System.Collections.ObjectModel.ReadOnlyDictionary<string, ulong>(handlesByArtifact);
        _artifactsByHandle = new System.Collections.ObjectModel.ReadOnlyDictionary<ulong, ArtifactIdentity>(artifactsByHandle);
    }

    /// <summary>Gets the number of explicit immutable-artifact-to-local-handle bindings.</summary>
    public int Count => _handlesByArtifact.Count;

    /// <summary>Creates a validated one-to-one mapping for one local repository.</summary>
    public static MicroBundleIdentityMap Create(IEnumerable<MicroBundleIdentityBinding> bindings)
    {
        ArgumentNullException.ThrowIfNull(bindings);

        var handlesByArtifact = new Dictionary<string, ulong>(StringComparer.Ordinal);
        var artifactsByHandle = new Dictionary<ulong, ArtifactIdentity>();

        foreach (var binding in bindings)
        {
            if (binding is null)
                throw new ArgumentException("Identity bindings cannot contain null entries.", nameof(bindings));

            if (binding.Artifact is null || !binding.Artifact.IsValid)
                throw new ArgumentException("Every binding must contain a valid ontology identity, version, and SHA-256.", nameof(bindings));

            if (binding.LocalHandle == 0)
                throw new ArgumentException($"Artifact '{binding.Artifact.BundleId}@{binding.Artifact.Version}' cannot map to the reserved zero local handle.", nameof(bindings));

            var key = binding.Artifact.StableKey;
            if (!handlesByArtifact.TryAdd(key, binding.LocalHandle))
                throw new ArgumentException($"Duplicate immutable artifact identity '{binding.Artifact.BundleId}@{binding.Artifact.Version}'.", nameof(bindings));

            if (!artifactsByHandle.TryAdd(binding.LocalHandle, binding.Artifact))
                throw new ArgumentException($"Local MicroBundle handle '{binding.LocalHandle}' is assigned to more than one immutable artifact in this repository.", nameof(bindings));
        }

        return new MicroBundleIdentityMap(handlesByArtifact, artifactsByHandle);
    }

    /// <summary>Resolves an exact ontology identity, version, and content hash to its local handle.</summary>
    public bool TryResolveLocalHandle(ArtifactIdentity artifact, out ulong localHandle)
    {
        if (artifact is null || !artifact.IsValid)
        {
            localHandle = 0;
            return false;
        }

        return _handlesByArtifact.TryGetValue(artifact.StableKey, out localHandle);
    }

    /// <summary>Resolves a local repository handle to the immutable artifact identity it stores.</summary>
    public bool TryGetArtifactIdentity(ulong localHandle, out ArtifactIdentity artifact)
    {
        if (localHandle == 0)
        {
            artifact = null!;
            return false;
        }

        return _artifactsByHandle.TryGetValue(localHandle, out artifact!);
    }
}

/// <summary>A persisted binding between an immutable ontology artifact identity and a repository-local handle.</summary>
public sealed record MicroBundleIdentityBinding(ArtifactIdentity Artifact, ulong LocalHandle);
