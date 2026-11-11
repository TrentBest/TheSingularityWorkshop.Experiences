namespace TheSingularityWorkshop.Experiences.Contracts;

/// <summary>
/// An explicit mapping from an Experience-authored bundle key to the numeric MicroBundle identity
/// used by MicroBundleDomain, MicroBundleRepository, and FSM_COS.
/// </summary>
/// <remarks>
/// This contract deliberately does not allocate IDs, hash keys, or infer mappings. The host or
/// deployment configuration must supply the authoritative bindings. Both directions are unique so
/// an Experience key cannot alias another runtime bundle and a runtime bundle cannot be claimed by
/// multiple authored keys within one mapping.
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

    /// <summary>Gets the number of explicit identity bindings.</summary>
    public int Count => _idsByKey.Count;

    /// <summary>
    /// Creates a validated one-to-one mapping. No IDs are generated or inferred.
    /// </summary>
    /// <exception cref="ArgumentNullException">The binding sequence is null.</exception>
    /// <exception cref="ArgumentException">A key or ID is invalid, or either side is duplicated.</exception>
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
                throw new ArgumentException("Bundle keys must be non-empty, trimmed, and contain no control characters.", nameof(bindings));

            if (binding.BundleId == 0)
                throw new ArgumentException($"Bundle key '{binding.BundleKey}' cannot map to the reserved zero bundle ID.", nameof(bindings));

            if (!idsByKey.TryAdd(binding.BundleKey, binding.BundleId))
                throw new ArgumentException($"Duplicate Experience bundle key '{binding.BundleKey}'.", nameof(bindings));

            if (!keysById.TryAdd(binding.BundleId, binding.BundleKey))
                throw new ArgumentException($"Numeric MicroBundle ID '{binding.BundleId}' is assigned to more than one Experience bundle key.", nameof(bindings));
        }

        return new MicroBundleIdentityMap(idsByKey, keysById);
    }

    /// <summary>Resolves an exact, ordinal Experience bundle key to its canonical numeric ID.</summary>
    public bool TryResolve(string bundleKey, out ulong bundleId)
    {
        if (string.IsNullOrEmpty(bundleKey))
        {
            bundleId = 0;
            return false;
        }

        return _idsByKey.TryGetValue(bundleKey, out bundleId);
    }

    /// <summary>Resolves a non-zero numeric MicroBundle ID to its exact Experience bundle key.</summary>
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

/// <summary>An owner-supplied binding between an authored bundle key and a numeric runtime identity.</summary>
public sealed record MicroBundleIdentityBinding(string BundleKey, ulong BundleId);
