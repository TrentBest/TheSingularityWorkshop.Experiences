using System.Security.Cryptography;
using System.Text;
using TheSingularityWorkshop.Experiences.Contracts;
using TheSingularityWorkshop.Experiences.MicroBundleRepositoryIntegration;
using TheSingularityWorkshop.MicroBundleRepository.Core;
using TheSingularityWorkshop.MicroBundleRepository.Local;
using RepositoryIdentityBinding = TheSingularityWorkshop.MicroBundleRepository.Core.MicroBundleIdentityBinding;
using Xunit;

namespace TheSingularityWorkshop.Experiences.MicroBundleRepository.Integration.Tests;

public sealed class ExperienceRepositoryArtifactBinderTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "experiences-microbundle-binding-tests",
        Guid.NewGuid().ToString("N"));

    private readonly FileSystemMicroBundleRepository _repository;

    public ExperienceRepositoryArtifactBinderTests()
    {
        _repository = new FileSystemMicroBundleRepository(_root);
    }

    [Fact]
    public async Task ResolvesAndVerifiesEveryArtifactInDependencyFirstOrder()
    {
        var dependencyBytes = Encoding.UTF8.GetBytes("immutable dependency artifact");
        var rootBytes = Encoding.UTF8.GetBytes("immutable Experience root artifact");
        var dependencyBinding = await _repository.PutAsync(
            "workshop/runtime/core", "1.0.0", dependencyBytes);
        var rootBinding = await _repository.PutAsync(
            "workshop/sample/experience", "2.0.0", rootBytes);

        var dependency = ToArtifactIdentity(dependencyBinding.Identity);
        var root = ToArtifactIdentity(rootBinding.Identity);
        var resolution = Resolution(dependency, root);

        var result = await ExperienceRepositoryArtifactBinder.ResolveAsync(resolution, _repository);

        Assert.True(result.Success);
        Assert.Empty(result.Issues);
        Assert.Equal(
            new[] { dependency.StableKey, root.StableKey },
            result.OrderedBindings.Select(binding => binding.Artifact.StableKey));
        Assert.Equal(
            new[] { dependencyBinding.Address.BundleId, rootBinding.Address.BundleId },
            result.OrderedBindings.Select(binding => binding.LocalHandle));
    }

    [Fact]
    public async Task MissingDurableBindingFailsTheEntireClosure()
    {
        var missing = new ArtifactIdentity(
            "workshop/missing/artifact",
            "1.0.0",
            new string('a', 64));

        var result = await ExperienceRepositoryArtifactBinder.ResolveAsync(
            Resolution(missing),
            _repository);

        Assert.False(result.Success);
        Assert.Empty(result.OrderedBindings);
        Assert.Contains(result.Issues, issue => issue.Code == "binding.repository-identity-not-found");
    }

    [Fact]
    public async Task CorruptStoredBytesFailClosedBeforeAPlanIsReturned()
    {
        var bytes = Encoding.UTF8.GetBytes("original artifact bytes");
        var binding = await _repository.PutAsync("workshop/corrupt", "1.0.0", bytes);
        var identity = ToArtifactIdentity(binding.Identity);
        var artifactPath = Path.Combine(
            _root,
            binding.Address.BundleId.ToString(),
            binding.Address.Version,
            binding.Address.ContentHash + ".bundle");
        await File.WriteAllBytesAsync(artifactPath, Encoding.UTF8.GetBytes("tampered bytes"));

        var result = await ExperienceRepositoryArtifactBinder.ResolveAsync(
            Resolution(identity),
            _repository);

        Assert.False(result.Success);
        Assert.Empty(result.OrderedBindings);
        Assert.Contains(result.Issues, issue => issue.Code == "binding.repository-access-failed");
    }

    [Fact]
    public async Task AliasedRepositoryHandlesFailClosed()
    {
        var bytes = Encoding.UTF8.GetBytes("same immutable bytes");
        var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        var first = new ArtifactIdentity("workshop/first", "1.0.0", hash);
        var second = new ArtifactIdentity("workshop/second", "1.0.0", hash);
        var repository = new AliasingIdentityRepository(bytes);

        var result = await ExperienceRepositoryArtifactBinder.ResolveAsync(
            Resolution(first, second),
            repository);

        Assert.False(result.Success);
        Assert.Empty(result.OrderedBindings);
        Assert.Contains(result.Issues, issue => issue.Code == "binding.repository-map-invalid");
    }

    private static ArtifactIdentity ToArtifactIdentity(MicroBundleArtifactIdentity identity) =>
        new(identity.Ontology, identity.Version, identity.ContentHash);

    private static ExperienceResolution Resolution(params ArtifactIdentity[] artifacts) =>
        new(
            true,
            Array.Empty<CapabilityProviderSelection>(),
            artifacts.Select(artifact => new ArtifactDescriptor(artifact, Array.Empty<ArtifactDependency>())).ToArray(),
            Array.Empty<ContractIssue>());

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    private sealed class AliasingIdentityRepository(byte[] content) : IMicroBundleIdentityRepository
    {
        public ValueTask<MicroBundleArtifact?> GetAsync(
            MicroBundleArtifactAddress address,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult<MicroBundleArtifact?>(new MicroBundleArtifact(address, content));
        }

        public ValueTask PutAsync(
            MicroBundleArtifact artifact,
            CancellationToken cancellationToken = default) =>
            ValueTask.CompletedTask;

        public ValueTask<RepositoryIdentityBinding> PutAsync(
            string ontology,
            string version,
            ReadOnlyMemory<byte> artifactContent,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public ValueTask<RepositoryIdentityBinding?> ResolveAsync(
            MicroBundleArtifactIdentity identity,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var address = new MicroBundleArtifactAddress(42, identity.Version, identity.ContentHash);
            return ValueTask.FromResult<RepositoryIdentityBinding?>(
                new RepositoryIdentityBinding(identity, address));
        }

        public ValueTask<RepositoryIdentityBinding?> ResolveAsync(
            ulong bundleId,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult<RepositoryIdentityBinding?>(null);
    }
}
