using System.Text;
using TheSingularityWorkshop.Experiences.Contracts;
using TheSingularityWorkshop.Experiences.RepositoryIntegration;
using TheSingularityWorkshop.MicroBundleRepository.Local;
using Xunit;

namespace TheSingularityWorkshop.Experiences.RepositoryIntegration.Tests;

public sealed class VerifiedExperienceArtifactBindingPlanTests
{
    [Fact]
    public async Task ResolvesAndVerifiesCompleteDependencyClosureFromFilesystemRepository()
    {
        var root = NewRepositoryRoot();
        try
        {
            var repository = new FileSystemMicroBundleRepository(root);
            var dependencyBytes = Encoding.UTF8.GetBytes("verified dependency bytes");
            var experienceBytes = Encoding.UTF8.GetBytes("verified experience bytes");
            var dependency = await repository.PutAsync(
                "workshop/runtime/core", "1.0.0", dependencyBytes);
            var experience = await repository.PutAsync(
                "workshop/sample/experience", "2.0.0", experienceBytes);

            var dependencyIdentity = ToArtifactIdentity(dependency);
            var experienceIdentity = ToArtifactIdentity(experience);
            var identityMap = MicroBundleIdentityMap.Create(
            [
                new(dependencyIdentity, dependency.Address.BundleId),
                new(experienceIdentity, experience.Address.BundleId)
            ]);
            var sourcePlan = ExperienceArtifactBindingPlan.Create(
                new ExperienceResolution(
                    true,
                    [],
                    [
                        new ArtifactDescriptor(dependencyIdentity, []),
                        new ArtifactDescriptor(experienceIdentity, [])
                    ],
                    []),
                identityMap);

            Assert.True(sourcePlan.Success);

            var result = await VerifiedExperienceArtifactBindingPlan.CreateAsync(
                sourcePlan, repository);

            Assert.True(result.Success);
            Assert.Empty(result.Issues);
            Assert.Equal(
                new[] { dependencyIdentity.StableKey, experienceIdentity.StableKey },
                result.OrderedBindings.Select(binding => binding.Artifact.StableKey));
            Assert.Equal(
                new[] { dependency.Address, experience.Address },
                result.OrderedBindings.Select(binding => binding.Address));
            Assert.Equal(
                new[] { dependencyBytes.Length, experienceBytes.Length },
                result.OrderedBindings.Select(binding => binding.ContentLength));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task MissingPersistedIdentityFailsTheEntirePlan()
    {
        var root = NewRepositoryRoot();
        try
        {
            var repository = new FileSystemMicroBundleRepository(root);
            var artifact = new ArtifactIdentity(
                "workshop/missing/artifact", "1.0.0", new string('a', 64));
            var sourcePlan = CreateSourcePlan(artifact, 73001);

            var result = await VerifiedExperienceArtifactBindingPlan.CreateAsync(
                sourcePlan, repository);

            Assert.False(result.Success);
            Assert.Empty(result.OrderedBindings);
            Assert.Contains(result.Issues, issue =>
                issue.Code == "repository-binding.unresolved-artifact");
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task MissingArtifactBytesFailClosedEvenWhenIdentityWasPreviouslyPersisted()
    {
        var root = NewRepositoryRoot();
        try
        {
            var repository = new FileSystemMicroBundleRepository(root);
            var binding = await repository.PutAsync(
                "workshop/missing/bytes", "1.0.0", Encoding.UTF8.GetBytes("original bytes"));
            var artifact = ToArtifactIdentity(binding);
            var sourcePlan = CreateSourcePlan(artifact, binding.Address.BundleId);
            var artifactPath = Path.Combine(
                root,
                binding.Address.BundleId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                binding.Address.Version,
                binding.Address.ContentHash + ".bundle");
            File.Delete(artifactPath);

            var result = await VerifiedExperienceArtifactBindingPlan.CreateAsync(
                sourcePlan, repository);

            Assert.False(result.Success);
            Assert.Empty(result.OrderedBindings);
            Assert.Contains(result.Issues, issue =>
                issue.Code == "repository-binding.artifact-missing");
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task CorruptArtifactBytesFailClosedDuringRepositoryResolution()
    {
        var root = NewRepositoryRoot();
        try
        {
            var repository = new FileSystemMicroBundleRepository(root);
            var binding = await repository.PutAsync(
                "workshop/corrupt/bytes", "1.0.0", Encoding.UTF8.GetBytes("original bytes"));
            var artifact = ToArtifactIdentity(binding);
            var sourcePlan = CreateSourcePlan(artifact, binding.Address.BundleId);
            var artifactPath = Path.Combine(
                root,
                binding.Address.BundleId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                binding.Address.Version,
                binding.Address.ContentHash + ".bundle");
            await File.WriteAllTextAsync(artifactPath, "tampered bytes");

            var result = await VerifiedExperienceArtifactBindingPlan.CreateAsync(
                sourcePlan, repository);

            Assert.False(result.Success);
            Assert.Empty(result.OrderedBindings);
            Assert.Contains(result.Issues, issue =>
                issue.Code == "repository-binding.artifact-integrity-failed");
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task PersistedAddressMustMatchTheSourcePlanLocalHandle()
    {
        var root = NewRepositoryRoot();
        try
        {
            var repository = new FileSystemMicroBundleRepository(root);
            var binding = await repository.PutAsync(
                "workshop/address/mismatch", "1.0.0", Encoding.UTF8.GetBytes("address-bound bytes"));
            var artifact = ToArtifactIdentity(binding);
            var incorrectHandle = binding.Address.BundleId == 1 ? 2UL : 1UL;
            var sourcePlan = CreateSourcePlan(artifact, incorrectHandle);

            var result = await VerifiedExperienceArtifactBindingPlan.CreateAsync(
                sourcePlan, repository);

            Assert.False(result.Success);
            Assert.Empty(result.OrderedBindings);
            Assert.Contains(result.Issues, issue =>
                issue.Code == "repository-binding.identity-mismatch");
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task AliasedLocalHandlesFailBeforeRepositoryResolution()
    {
        var first = new ArtifactIdentity(
            "workshop/first", "1.0.0", new string('a', 64));
        var second = new ArtifactIdentity(
            "workshop/second", "1.0.0", new string('b', 64));
        var sourcePlan = new ExperienceArtifactBindingPlan(
            true,
            [
                new ExperienceArtifactBinding(first, 73001),
                new ExperienceArtifactBinding(second, 73001)
            ],
            []);

        var result = await VerifiedExperienceArtifactBindingPlan.CreateAsync(
            sourcePlan, new FileSystemMicroBundleRepository(NewRepositoryRoot()));

        Assert.False(result.Success);
        Assert.Empty(result.OrderedBindings);
        Assert.Contains(result.Issues, issue =>
            issue.Code == "repository-binding.aliased-handle");
    }

    [Fact]
    public async Task FailedSourcePlanCannotBeResolved()
    {
        var sourcePlan = new ExperienceArtifactBindingPlan(
            false,
            [],
            [new ContractIssue("binding.unresolved-artifact", "Missing dependency.")]);

        var result = await VerifiedExperienceArtifactBindingPlan.CreateAsync(
            sourcePlan, new FileSystemMicroBundleRepository(NewRepositoryRoot()));

        Assert.False(result.Success);
        Assert.Empty(result.OrderedBindings);
        Assert.Contains(result.Issues, issue =>
            issue.Code == "binding.unresolved-artifact");
    }

    private static ExperienceArtifactBindingPlan CreateSourcePlan(
        ArtifactIdentity artifact,
        ulong localHandle)
    {
        var map = MicroBundleIdentityMap.Create([new(artifact, localHandle)]);
        return ExperienceArtifactBindingPlan.Create(
            new ExperienceResolution(
                true, [], [new ArtifactDescriptor(artifact, [])], []),
            map);
    }

    private static ArtifactIdentity ToArtifactIdentity(
        TheSingularityWorkshop.MicroBundleRepository.Core.MicroBundleIdentityBinding binding) =>
        new(
            binding.Identity.Ontology,
            binding.Identity.Version,
            binding.Identity.ContentHash);

    private static string NewRepositoryRoot() =>
        Path.Combine(Path.GetTempPath(), "WorkshopExperienceRepositoryBinding", Guid.NewGuid().ToString("N"));
}
