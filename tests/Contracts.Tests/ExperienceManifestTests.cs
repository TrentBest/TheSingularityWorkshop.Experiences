using System.Text.Json;
using TheSingularityWorkshop.Experiences.Contracts;
using Xunit;

namespace TheSingularityWorkshop.Experiences.Contracts.Tests;

public sealed class ExperienceManifestTests
{
    private static ArtifactIdentity Artifact(string id, char hash = 'a') =>
        new(id, "1.0.0", new string(hash, 64));

    private static ExperienceManifest Manifest(
        IReadOnlyList<ArtifactIdentity>? artifacts = null,
        IReadOnlyList<SemanticInteractionRequirement>? requirements = null) =>
        new()
        {
            ExperienceId = 4201,
            ExperienceKey = "minesweeper",
            Version = "0.1.0",
            Title = "Minesweeper",
            Artifacts = artifacts ?? [Artifact("minesweeper.rules")],
            Requirements = requirements ??
            [
                new("workshop.input.pointer", new(new(1, 0, 0), new(2, 0, 0)), Required: true,
                    Rationale: "The desktop manifestation requires pointer selection.")
            ]
        };

    [Fact]
    public void CanonicalJsonIsStableAcrossArtifactAndRequirementInputOrder()
    {
        var first = Manifest(
            [Artifact("zeta"), Artifact("alpha", 'b')],
            [
                new("workshop.input.pointer", new(new(1, 0, 0), new(2, 0, 0))),
                new("workshop.presentation.grid", new(new(1, 0, 0)))
            ]);
        var second = Manifest(
            [Artifact("alpha", 'b'), Artifact("zeta")],
            [
                new("workshop.presentation.grid", new(new(1, 0, 0))),
                new("workshop.input.pointer", new(new(1, 0, 0), new(2, 0, 0)))
            ]);

        Assert.Equal(first.ToCanonicalJson(), second.ToCanonicalJson());
    }

    [Fact]
    public void CanonicalJsonRoundTripsThroughStrictParser()
    {
        var manifest = Manifest();

        var parsed = ExperienceManifest.Parse(manifest.ToCanonicalJson());

        Assert.Equal(manifest.ExperienceId, parsed.ExperienceId);
        Assert.Equal(manifest.ExperienceKey, parsed.ExperienceKey);
        Assert.Equal(manifest.Artifacts.Single().StableKey, parsed.Artifacts.Single().StableKey);
        Assert.Equal(manifest.Requirements.Single().CapabilityId, parsed.Requirements.Single().CapabilityId);
    }

    [Fact]
    public void UnsupportedSchemaVersionIsRejected()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            (Manifest() with { SchemaVersion = ExperienceManifest.CurrentSchemaVersion + 1 }).Validate());

        Assert.Contains("Unsupported", exception.Message);
    }

    [Fact]
    public void MissingOrInvalidArtifactContentHashIsRejected()
    {
        var missingHash = new ArtifactIdentity("bundle", "1.0.0", null!);
        var malformedHash = new ArtifactIdentity("bundle", "1.0.0", "not-a-hash");

        Assert.Throws<InvalidOperationException>(() => Manifest([missingHash]).Validate());
        Assert.Throws<InvalidOperationException>(() => Manifest([malformedHash]).Validate());
    }

    [Fact]
    public void DuplicateArtifactIdentityIsRejected()
    {
        var artifact = Artifact("same");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            Manifest([artifact, artifact]).Validate());

        Assert.Contains("Duplicate Experience artifact", exception.Message);
    }

    [Fact]
    public void DuplicateOrInvalidSemanticRequirementIsRejected()
    {
        var requirement = new SemanticInteractionRequirement(
            "workshop.input.pointer", new(new(1, 0, 0)));
        var duplicate = Manifest(requirements: [requirement, requirement]);
        var invalidRange = Manifest(requirements:
        [
            new("workshop.input.pointer", new(new(2, 0, 0), new(1, 0, 0)))
        ]);

        Assert.Throws<InvalidOperationException>(() => duplicate.Validate());
        Assert.Throws<InvalidOperationException>(() => invalidRange.Validate());
    }

    [Fact]
    public void InvalidStableIdentityIsRejected()
    {
        Assert.Throws<InvalidOperationException>(() => (Manifest() with { ExperienceId = 0 }).Validate());
        Assert.Throws<InvalidOperationException>(() => (Manifest() with { ExperienceKey = " minesweeper" }).Validate());
        Assert.Throws<InvalidOperationException>(() => (Manifest() with { Version = "0.1.0\n" }).Validate());
    }


    [Fact]
    public void CheckedInExampleParsesAsAValidManifest()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "contracts", "experience-manifest.example.json");
        var parsed = ExperienceManifest.Parse(File.ReadAllText(path));

        Assert.Equal(4201UL, parsed.ExperienceId);
        Assert.Equal("minesweeper", parsed.ExperienceKey);
        Assert.Equal(2, parsed.Requirements.Count);
        Assert.All(parsed.Artifacts, artifact => Assert.True(artifact.IsValid));
    }

    [Fact]
    public void ParserRejectsUnknownProperties()
    {
        var json = Manifest().ToCanonicalJson();
        var extended = json.Insert(json.Length - 1, ",\"untrustedRuntimeCommand\":\"execute\"");

        Assert.Throws<JsonException>(() => ExperienceManifest.Parse(extended));
    }

    [Fact]
    public void ParserRejectsUnknownSchemaVersionAndMalformedJson()
    {
        var json = Manifest().ToCanonicalJson()
            .Replace("\"schemaVersion\":1", "\"schemaVersion\":99", StringComparison.Ordinal);

        Assert.Throws<InvalidOperationException>(() => ExperienceManifest.Parse(json));
        Assert.Throws<JsonException>(() => ExperienceManifest.Parse("{"));
    }
}
