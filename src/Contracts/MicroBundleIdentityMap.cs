namespace TheSingularityWorkshop.Experiences.Contracts;

/// <summary>Maps canonical string ontology identities to repository-local numeric handles.</summary>
/// <remarks>
/// The ontology string is the portable identity and remains stable across repositories. The
/// non-zero numeric handle is only a local address assigned by a particular repository for
/// locating the stored artifact in its blob data. It is not a global identity and is not portable
/// between repositories. The repository supplies persisted bindings; this type does not allocate
/// handles, hash ontology strings, or infer mappings.
/// </remarks>
public sealed class MicroBundleIdentityMap
{
    private readonly IReadOnlyDictionary<string, ulong> _handlesByOntology;
    private readonly IReadOnlyDictionary<ulong, string> _ontologyByHandle;

    private MicroBundleIdentityMap(
        Dictionary<string, ulong> handlesByOntology,
        Dictionary<ulong, string> ontologyByHandle)
    {
        _handlesByOntology = new System.Collections.ObjectModel.ReadOnlyDictionary<string, ulong>(handlesByOntology);
        _ontologyByHandle = new System.Collections.ObjectModel.ReadOnlyDictionary<ulong, string>(ontologyByHandle);
    }

    /// <summary>Gets the number of explicit ontology-to-local-handle bindings.</summary>
    public int Count => _handlesByOntology.Count;

    /// <summary>Creates a validated one-to-one mapping for one local repository.</summary>
    public static MicroBundleIdentityMap Create(IEnumerable<MicroBundleIdentityBinding> bindings)
    {
        ArgumentNullException.ThrowIfNull(bindings);

        var handlesByOntology = new Dictionary<string, ulong>(StringComparer.Ordinal);
        var ontologyByHandle = new Dictionary<ulong, string>();

        foreach (var binding in bindings)
        {
            if (binding is null)
                throw new ArgumentException("Identity bindings cannot contain null entries.", nameof(bindings));

            if (string.IsNullOrWhiteSpace(binding.OntologyIdentity) ||
                !string.Equals(binding.OntologyIdentity, binding.OntologyIdentity.Trim(), StringComparison.Ordinal) ||
                binding.OntologyIdentity.Any(char.IsControl))
                throw new ArgumentException("Ontology identities must be non-empty, trimmed, and contain no control characters.", nameof(bindings));

            if (binding.LocalHandle == 0)
                throw new ArgumentException($"Ontology identity '{binding.OntologyIdentity}' cannot map to the reserved zero local handle.", nameof(bindings));

            if (!handlesByOntology.TryAdd(binding.OntologyIdentity, binding.LocalHandle))
                throw new ArgumentException($"Duplicate ontology identity '{binding.OntologyIdentity}'.", nameof(bindings));

            if (!ontologyByHandle.TryAdd(binding.LocalHandle, binding.OntologyIdentity))
                throw new ArgumentException($"Local MicroBundle handle '{binding.LocalHandle}' is assigned to more than one ontology identity in this repository.", nameof(bindings));
        }

        return new MicroBundleIdentityMap(handlesByOntology, ontologyByHandle);
    }

    /// <summary>Resolves an exact, ordinal ontology identity to its local repository handle.</summary>
    public bool TryResolveLocalHandle(string ontologyIdentity, out ulong localHandle)
    {
        if (string.IsNullOrEmpty(ontologyIdentity))
        {
            localHandle = 0;
            return false;
        }

        return _handlesByOntology.TryGetValue(ontologyIdentity, out localHandle);
    }

    /// <summary>Resolves a local repository handle to its exact canonical ontology identity.</summary>
    public bool TryGetOntologyIdentity(ulong localHandle, out string ontologyIdentity)
    {
        if (localHandle == 0)
        {
            ontologyIdentity = string.Empty;
            return false;
        }

        return _ontologyByHandle.TryGetValue(localHandle, out ontologyIdentity!);
    }
}

/// <summary>A persisted binding between a canonical string ontology identity and a repository-local handle.</summary>
public sealed record MicroBundleIdentityBinding(string OntologyIdentity, ulong LocalHandle);
