using System.Security.Cryptography;
using TheSingularityWorkshop.Experiences.Contracts;
using TheSingularityWorkshop.MicroBundleRepository.Core;
using ExperienceIdentityBinding = TheSingularityWorkshop.Experiences.Contracts.MicroBundleIdentityBinding;

namespace TheSingularityWorkshop.Experiences.MicroBundleRepositoryIntegration;

/// <summary>
/// Resolves a complete Experience artifact closure against one durable MicroBundle repository.
/// The result contains repository-local bindings only; it is not an FSM_COS runtime manifest.
/// </summary>
public static class ExperienceRepositoryArtifactBinder
{
    /// <summary>
    /// Resolves and verifies every artifact in dependency-first order. A missing binding, missing
    /// artifact, corrupt bytes, identity mismatch, or aliased local handle fails the entire plan.
    /// </summary>
    public static async ValueTask<ExperienceArtifactBindingPlan> ResolveAsync(
        ExperienceResolution resolution,
        IMicroBundleIdentityRepository repository,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resolution);
        ArgumentNullException.ThrowIfNull(repository);

        if (!resolution.Success)
            return ExperienceArtifactBindingPlan.Create(resolution, MicroBundleIdentityMap.Create([]));

        if (resolution.OrderedArtifacts is null)
            return Failure(
                "binding.invalid-resolution",
                "The successful Experience resolution contains a null ordered artifact list.");

        var bindings = new List<ExperienceIdentityBinding>(resolution.OrderedArtifacts.Count);

        foreach (var descriptor in resolution.OrderedArtifacts)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (descriptor?.Identity is not { IsValid: true } artifact)
                return Failure(
                    "binding.invalid-artifact",
                    "The resolved dependency closure contains an invalid artifact identity.");

            MicroBundleArtifactIdentity portableIdentity;
            try
            {
                portableIdentity = new MicroBundleArtifactIdentity(
                    artifact.BundleId,
                    artifact.Version,
                    artifact.Sha256);
            }
            catch (ArgumentException exception)
            {
                return Failure(
                    "binding.invalid-repository-identity",
                    $"Artifact '{artifact.BundleId}@{artifact.Version}' violates the repository identity contract: {exception.Message}");
            }

            try
            {
                var repositoryBinding = await repository.ResolveAsync(portableIdentity, cancellationToken);
                if (repositoryBinding is null)
                    return Failure(
                        "binding.repository-identity-not-found",
                        $"No durable repository binding exists for '{artifact.BundleId}@{artifact.Version}' with the requested SHA-256.");

                if (!IdentityMatches(portableIdentity, repositoryBinding.Identity) ||
                    repositoryBinding.Address.BundleId == 0 ||
                    !string.Equals(repositoryBinding.Address.Version, portableIdentity.Version, StringComparison.Ordinal) ||
                    !string.Equals(repositoryBinding.Address.ContentHash, portableIdentity.ContentHash, StringComparison.Ordinal))
                {
                    return Failure(
                        "binding.repository-identity-mismatch",
                        $"The repository returned a binding that does not match '{artifact.BundleId}@{artifact.Version}'.");
                }

                var storedArtifact = await repository.GetAsync(repositoryBinding.Address, cancellationToken);
                if (storedArtifact is null)
                    return Failure(
                        "binding.repository-artifact-missing",
                        $"The repository binding for '{artifact.BundleId}@{artifact.Version}' points to missing artifact bytes.");

                if (storedArtifact.Address != repositoryBinding.Address)
                    return Failure(
                        "binding.repository-address-mismatch",
                        $"The repository returned bytes under a different address for '{artifact.BundleId}@{artifact.Version}'.");

                var actualHash = Convert.ToHexString(SHA256.HashData(storedArtifact.Content.Span)).ToLowerInvariant();
                if (!string.Equals(actualHash, portableIdentity.ContentHash, StringComparison.Ordinal))
                    return Failure(
                        "binding.repository-content-hash-mismatch",
                        $"The stored bytes for '{artifact.BundleId}@{artifact.Version}' do not match the requested SHA-256.");

                bindings.Add(new ExperienceIdentityBinding(artifact, repositoryBinding.Address.BundleId));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                return Failure(
                    "binding.repository-access-failed",
                    $"Repository verification failed for '{artifact.BundleId}@{artifact.Version}' ({exception.GetType().Name}): {exception.Message}");
            }
        }

        try
        {
            var identityMap = MicroBundleIdentityMap.Create(bindings);
            return ExperienceArtifactBindingPlan.Create(resolution, identityMap);
        }
        catch (ArgumentException exception)
        {
            return Failure(
                "binding.repository-map-invalid",
                $"The repository returned conflicting or aliased local bindings: {exception.Message}");
        }
    }

    private static bool IdentityMatches(
        MicroBundleArtifactIdentity expected,
        MicroBundleArtifactIdentity actual) =>
        actual is not null &&
        string.Equals(expected.Ontology, actual.Ontology, StringComparison.Ordinal) &&
        string.Equals(expected.Version, actual.Version, StringComparison.Ordinal) &&
        string.Equals(expected.ContentHash, actual.ContentHash, StringComparison.Ordinal);

    private static ExperienceArtifactBindingPlan Failure(string code, string message) =>
        new(
            false,
            Array.Empty<ExperienceArtifactBinding>(),
            [new ContractIssue(code, message)]);
}
