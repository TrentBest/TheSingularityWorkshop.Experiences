using System.Security.Cryptography;
using TheSingularityWorkshop.Experiences.Contracts;
using TheSingularityWorkshop.MicroBundleRepository.Core;

namespace TheSingularityWorkshop.Experiences.RepositoryIntegration;

/// <summary>
/// A verified binding from one portable Experience artifact identity to an address in one
/// MicroBundleRepository instance.
/// </summary>
public sealed record VerifiedExperienceArtifactBinding(
    ArtifactIdentity Artifact,
    MicroBundleArtifactAddress Address,
    int ContentLength);

/// <summary>
/// A complete, dependency-first, repository-local binding plan whose artifact bytes were retrieved
/// and SHA-256 verified. This is not an FSM_COS runtime manifest.
/// </summary>
public sealed record VerifiedExperienceArtifactBindingPlan(
    bool Success,
    IReadOnlyList<VerifiedExperienceArtifactBinding> OrderedBindings,
    IReadOnlyList<ContractIssue> Issues)
{
    /// <summary>
    /// Resolves and verifies every artifact in an existing dependency-first binding plan.
    /// No repository-local handle is treated as a runtime/domain ID.
    /// </summary>
    public static async ValueTask<VerifiedExperienceArtifactBindingPlan> CreateAsync(
        ExperienceArtifactBindingPlan bindingPlan,
        IMicroBundleIdentityRepository repository,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bindingPlan);
        ArgumentNullException.ThrowIfNull(repository);

        if (!bindingPlan.Success)
        {
            var issues = bindingPlan.Issues is { Count: > 0 }
                ? bindingPlan.Issues.ToArray()
                : [new ContractIssue("repository-binding.source-plan-failed",
                    "The source Experience binding plan failed; repository resolution was not attempted.")];
            return Failure(issues);
        }

        if (bindingPlan.OrderedBindings is null || bindingPlan.OrderedBindings.Count == 0)
            return Failure(new("repository-binding.empty-plan",
                "A successful source binding plan must contain at least one artifact."));

        var verified = new List<VerifiedExperienceArtifactBinding>(bindingPlan.OrderedBindings.Count);
        var artifactKeys = new HashSet<string>(StringComparer.Ordinal);
        var localHandles = new HashSet<ulong>();

        foreach (var source in bindingPlan.OrderedBindings)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (source is null || source.Artifact is null || !source.Artifact.IsValid)
                return Failure(new("repository-binding.invalid-artifact",
                    "The source binding plan contains an invalid portable artifact identity."));

            var artifact = source.Artifact;
            if (!artifactKeys.Add(artifact.StableKey))
                return Failure(new("repository-binding.duplicate-artifact",
                    $"The source binding plan contains duplicate artifact '{artifact.BundleId}@{artifact.Version}'."));

            if (source.LocalHandle == 0)
                return Failure(new("repository-binding.zero-handle",
                    $"Artifact '{artifact.BundleId}@{artifact.Version}' has the reserved zero repository-local handle."));

            if (!localHandles.Add(source.LocalHandle))
                return Failure(new("repository-binding.aliased-handle",
                    $"Repository-local handle '{source.LocalHandle}' is assigned to multiple artifacts."));

            MicroBundleIdentityBinding? persisted;
            try
            {
                var identity = new MicroBundleArtifactIdentity(
                    artifact.BundleId, artifact.Version, artifact.Sha256);
                persisted = await repository.ResolveAsync(identity, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (InvalidDataException exception)
            {
                return Failure(new("repository-binding.artifact-missing",
                    $"The repository identity index could not resolve verified bytes for '{artifact.BundleId}@{artifact.Version}': {exception.Message}"));
            }
            catch (ArgumentException exception)
            {
                return Failure(new("repository-binding.artifact-integrity-failed",
                    $"The repository artifact bytes failed integrity validation for '{artifact.BundleId}@{artifact.Version}': {exception.Message}"));
            }
            catch (Exception exception)
            {
                return Failure(new("repository-binding.resolve-failed",
                    $"Repository resolution failed for '{artifact.BundleId}@{artifact.Version}': {exception.Message}"));
            }

            if (persisted is null)
                return Failure(new("repository-binding.unresolved-artifact",
                    $"The repository has no persisted binding for '{artifact.BundleId}@{artifact.Version}' with the requested SHA-256."));

            var address = persisted.Address;
            if (persisted.Identity is null ||
                !StringComparer.Ordinal.Equals(persisted.Identity.Ontology, artifact.BundleId) ||
                !StringComparer.Ordinal.Equals(persisted.Identity.Version, artifact.Version) ||
                !StringComparer.OrdinalIgnoreCase.Equals(persisted.Identity.ContentHash, artifact.Sha256) ||
                address.BundleId == 0 ||
                address.BundleId != source.LocalHandle ||
                !StringComparer.Ordinal.Equals(address.Version, artifact.Version) ||
                !StringComparer.OrdinalIgnoreCase.Equals(address.ContentHash, artifact.Sha256))
            {
                return Failure(new("repository-binding.identity-mismatch",
                    $"The persisted repository binding does not exactly match the requested portable identity and source local handle for '{artifact.BundleId}@{artifact.Version}'."));
            }

            MicroBundleArtifact? stored;
            try
            {
                stored = await repository.GetAsync(address, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                return Failure(new("repository-binding.artifact-integrity-failed",
                    $"The repository could not retrieve verified bytes for '{artifact.BundleId}@{artifact.Version}': {exception.Message}"));
            }

            if (stored is null)
                return Failure(new("repository-binding.artifact-missing",
                    $"The repository binding for '{artifact.BundleId}@{artifact.Version}' points to missing artifact bytes."));

            if (stored.Address != address)
                return Failure(new("repository-binding.address-mismatch",
                    $"The repository returned an artifact at a different address for '{artifact.BundleId}@{artifact.Version}'."));

            var actualHash = Convert.ToHexString(SHA256.HashData(stored.Content.Span));
            if (!StringComparer.OrdinalIgnoreCase.Equals(actualHash, artifact.Sha256))
                return Failure(new("repository-binding.hash-mismatch",
                    $"Retrieved bytes for '{artifact.BundleId}@{artifact.Version}' do not match the requested SHA-256."));

            verified.Add(new VerifiedExperienceArtifactBinding(
                artifact, address, stored.Content.Length));
        }

        return new VerifiedExperienceArtifactBindingPlan(
            true, verified.AsReadOnly(), Array.Empty<ContractIssue>());
    }

    private static VerifiedExperienceArtifactBindingPlan Failure(ContractIssue issue) =>
        Failure([issue]);

    private static VerifiedExperienceArtifactBindingPlan Failure(IReadOnlyList<ContractIssue> issues) =>
        new(false, Array.Empty<VerifiedExperienceArtifactBinding>(), issues);
}
