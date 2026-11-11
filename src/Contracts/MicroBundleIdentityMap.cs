namespace TheSingularityWorkshop.Experiences.Contracts;

/// <summary>
/// Maps canonical string ontology identities to repository-local numeric handles.
/// </summary>
/// <remarks>
/// The ontology string is the identity and remains stable across repositories. The non-zero
/// numeric ID is only a local address/handle assigned by a particular repository for locating
/// the stored artifact in its blob data. It is not a global identity and must not be exchanged
/// as though it were portable between repositories. The repository/host supplies the bindings;
/// this type does not allocate handles, hash ontology strings, or infer mappings.
/// </remarks>
public sealed class MicroBundleIdentityMap
{
    private readonly IReadOnlyDictionary<string, ulong> _idsByKey;
    private readonly IReadOnlyDictionary<ulong, string> _keysById;

    private MicroBundleIdentityMap(
        Dictionary<string, ulong> idsByKey,
        Dictionary<ulong, string> keysById)
    {
        _idsByKey = new System.Collections.ObjectModel.ReadOnlyDictionary<string, ulong>(idsByKey);
        _keysById = new System.Collections.ObjectModel.ReadOnlyDictionary<ulong, string>(keysById);
    }

    /// <summary>Gets the number of explicit ontology-to-local-handle bindings.</summary>
    public int Count => _idsByKey.Count;

    /// <summary>
    /// Creates a validated one-to-one mapping for one local repository. No handles are generated
    /// or inferred; the repository supplies its persisted bindings.
    /// </summary>
    /// <exception cref="ArgumentNullException">The binding sequence is null.</exception>
    /// <exception cref="ArgumentException">An ontology key or local handle is invalid, or either side is duplicated.</exception>
    public static MicroBundleIdentityMap Create(IEnumerable<MicroBundleIdentityBinding> bindings)
    {
        ArgumentNullException.ThrowIfNull(bindings);

        var idsByKey = new Dictionary<string, ulong>(StringComparer.Ordinal);
        var keysById = new Dictionary<ulong, string>();

        foreach (var binding in bindings)
        {
            if (binding is null)
                throw new ArgumentException("Identity bindings cannot contain null entries.", nameof(bindings));

            if (string.IsNullOrWhiteSpace(binding.BundleKey) ||
                !string.Equals(binding.BundleKey, binding.BundleKey.Trim(), StringComparison.Ordinal) ||
                binding.BundleKey.Any(char.IsControl))
                throw new ArgumentException("Ontology keys must be non-empty, trimmed, and contain no control characters.", nameof(bindings));

            if (binding.BundleId == 0)
                throw new ArgumentException($"Ontology key '{binding.BundleKey}' cannot map to the reserved zero local handle.", nameof(bindings));

            if (!idsByKey.TryAdd(binding.BundleKey, binding.BundleId))
                throw new ArgumentException($"Duplicate ontology key '{binding.BundleKey}'.", nameof(bindings));

            if (!keysById.TryAdd(binding.BundleId, binding.BundleKey))
                throw new ArgumentException($"Local MicroBundle handle '{binding.BundleId}' is assigned to more than one ontology key in this repository.", nameof(bindings));
        }

        return new MicroBundleIdentityMap(idsByKey, keysById);
    }

    /// <summary>Resolves an exact, ordinal ontology identity to its local repository handle.</summary>
    public bool TryResolve(string bundleKey, out ulong bundleId)
    {
        if (string.IsNullOrEmpty(bundleKey))
        {
            bundleId = 0;
            return false;
        }

        return _idsByKey.TryGetValue(bundleKey, out bundleId);
    }

    /// <summary>Resolves a local repository handle to its exact canonical ontology identity.</summary>
    public bool TryGetBundleKey(ulong bundleId, out string bundleKey)
    {
        if (bundleId == 0)
        {
            bundleKey = string.Empty;
            return false;
        }

        return _keysById.TryGetValue(bundleId, out bundleKey!);
    }
}

/// <summary>A persisted binding between a canonical string ontology identity and a repository-local handle.</summary>
public sealed record MicroBundleIdentityBinding(string BundleKey, ulong BundleId);
