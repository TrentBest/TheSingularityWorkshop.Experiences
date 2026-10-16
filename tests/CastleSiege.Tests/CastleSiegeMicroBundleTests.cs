using TheSingularityWorkshop.Experiences.CastleSiege;
using Xunit;

namespace TheSingularityWorkshop.Experiences.CastleSiege.Tests;

public sealed class CastleSiegeMicroBundleTests
{
    [Fact]
    public void Bundle_ExposesUniqueStableIdentityAndNoUnneededDependencies()
    {
        var bundle = new CastleSiegeMicroBundle();

        Assert.Equal(3301UL, bundle.Id);
        Assert.Equal(CastleSiegeMicroBundle.ExperienceId, bundle.Id);
        Assert.Equal("workshop-castle-siege", Assert.Single(bundle.Descriptor.Providers).Id);
        Assert.Empty(bundle.Dependencies);
    }

    [Fact]
    public void Load_ProducesEntrySurfaceThatDoesNotClaimSimulationExists()
    {
        var bundle = new CastleSiegeMicroBundle();

        bundle.Load(new TestLoadContext());

        Assert.NotNull(bundle.Root);
        Assert.True(bundle.Root!.TryFind("castle-siege-title", out var title));
        Assert.Equal("CASTLE SIEGE", title!.Text);
        Assert.True(bundle.Root.TryFind("castle-siege-physics-status", out var status));
        Assert.Equal("PHYSICS ADAPTER NOT CONNECTED", status!.Text);
        Assert.True(bundle.Root.TryFind("castle-siege-scope", out var scope));
        Assert.Contains("no computed trajectory", scope!.Text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_RejectsMissingLoadContext()
    {
        var bundle = new CastleSiegeMicroBundle();

        Assert.Throws<ArgumentNullException>(() => bundle.Load(null!));
    }

    [Fact]
    public void Arbitrate_RejectsMissingContextAndDoesNotMutateComposition()
    {
        var bundle = new CastleSiegeMicroBundle();

        Assert.Throws<ArgumentNullException>(() => bundle.Arbitrate(null!, 0));
        Assert.False(bundle.Arbitrate(new TestArbitrationContext(), 0));
    }


    [Fact]
    public void Manifest_UsesStableIdentityAndDeclaresRequiredPhysicsCapability()
    {
        var manifest = new CastleSiegeExperienceManifest();

        manifest.Validate();

        Assert.Equal(1, manifest.SchemaVersion);
        Assert.Equal(3301UL, manifest.ExperienceId);
        Assert.Equal("castle-siege", manifest.ExperienceKey);
        Assert.Contains(manifest.Capabilities, c =>
            c.CapabilityId == "physics.projectile-trajectory.constant-acceleration" &&
            c.Level == CapabilityRequirementLevel.Required);
    }

    [Fact]
    public void Manifest_CanonicalJsonIsStableRegardlessOfCollectionOrder()
    {
        var manifest = new CastleSiegeExperienceManifest
        {
            Artifacts =
            [
                new ExperienceArtifactReference("z-artifact", "1.0.0", null),
                new ExperienceArtifactReference("a-artifact", "1.0.0", null)
            ],
            Capabilities =
            [
                new ExperienceCapabilityRequirement("z-capability", CapabilityRequirementLevel.Optional, "Optional."),
                new ExperienceCapabilityRequirement("a-capability", CapabilityRequirementLevel.Required, "Required.")
            ]
        };
        var reversed = manifest with
        {
            Artifacts = manifest.Artifacts.Reverse().ToArray(),
            Capabilities = manifest.Capabilities.Reverse().ToArray()
        };

        Assert.Equal(manifest.ToCanonicalJson(), reversed.ToCanonicalJson());
        Assert.Equal(manifest.ToCanonicalJson(), CastleSiegeExperienceManifest.Parse(manifest.ToCanonicalJson()).ToCanonicalJson());
    }

    [Fact]
    public void Manifest_RejectsUnsupportedSchemaAndInvalidArtifactHash()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new CastleSiegeExperienceManifest { SchemaVersion = 99 }.Validate());

        var invalidHash = new CastleSiegeExperienceManifest
        {
            Artifacts = [new ExperienceArtifactReference("castle-siege", "0.1.0", "not-a-sha256")]
        };
        Assert.Throws<InvalidOperationException>(() => invalidHash.Validate());
    }


    [Fact]
    public void TrajectoryAdapter_ProjectsActualPhysicsRecordShapeWithoutClaimingCollision()
    {
        var evidence = ProjectileTrajectoryEvidenceAdapter.Read(PhysicsRunJson);

        Assert.Equal("kinematics.constant-acceleration-projectile", evidence.ModelId);
        Assert.Equal("1", evidence.ModelVersion);
        Assert.Equal("world, Z-up", evidence.CoordinateFrame);
        Assert.Equal(2, evidence.CompletedSteps);
        Assert.Equal(3, evidence.Samples.Count);
        Assert.Equal(2d, evidence.Samples[^1].ElapsedTime);
        Assert.Equal(20d, evidence.Samples[^1].PositionMetres.X);
    }

    [Fact]
    public void TrajectoryAdapter_RejectsUnknownUnitsAndMismatchedSampleCount()
    {
        var wrongUnits = PhysicsRunJson.Replace("\"position\": \"m\"", "\"position\": \"cm\"", StringComparison.Ordinal);
        Assert.Throws<InvalidOperationException>(() => ProjectileTrajectoryEvidenceAdapter.Read(wrongUnits));

        var wrongCount = PhysicsRunJson.Replace("\"stepCount\": 2", "\"stepCount\": 3", StringComparison.Ordinal);
        Assert.Throws<InvalidOperationException>(() => ProjectileTrajectoryEvidenceAdapter.Read(wrongCount));
    }

    private const string PhysicsRunJson = """
    {
      "modelId": "kinematics.constant-acceleration-projectile",
      "modelVersion": "1",
      "schemaVersion": 1,
      "units": {
        "acceleration": "m/s^2",
        "elapsedTime": "s",
        "position": "m",
        "velocity": "m/s"
      },
      "definition": {
        "initialState": {
          "position": { "x": 0, "y": 0, "z": 0 },
          "velocity": { "x": 10, "y": 0, "z": 0 },
          "elapsedTime": 0
        },
        "acceleration": { "x": 0, "y": 0, "z": 0 },
        "deltaTime": 1,
        "stepCount": 2,
        "coordinateFrame": "world, Z-up"
      },
      "samples": [
        { "position": { "x": 0, "y": 0, "z": 0 }, "velocity": { "x": 10, "y": 0, "z": 0 }, "elapsedTime": 0 },
        { "position": { "x": 10, "y": 0, "z": 0 }, "velocity": { "x": 10, "y": 0, "z": 0 }, "elapsedTime": 1 },
        { "position": { "x": 20, "y": 0, "z": 0 }, "velocity": { "x": 10, "y": 0, "z": 0 }, "elapsedTime": 2 }
      ]
    }
    """;

    private sealed class TestLoadContext : TheSingularityWorkshop.MicroBundleDomain.IMicroBundleLoadContext
    {
        public ulong RuntimeId => 3301;

        public bool TryGetConfiguration(ulong bundleId, out ReadOnlyMemory<byte> configuration)
        {
            configuration = default;
            return false;
        }
    }

    private sealed class TestArbitrationContext : TheSingularityWorkshop.MicroBundleDomain.IMicroBundleArbitrationContext
    {
        public ulong RuntimeId => 3301;

        public IReadOnlyList<TheSingularityWorkshop.MicroBundleDomain.IMicroBundle> Bundles => [];

        public object? ExperienceContext => null;
    }
}