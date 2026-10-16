using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Xunit;

namespace TheSingularityWorkshop.Experiences.CastleSiege.Tests;

public sealed class CastleSiegeBattlespaceContentTests
{
    private const string ContractDirectory = "contracts";
    private const string SchemaFile = "castle-siege-battlespace.schema.json";
    private const string ExampleFile = "castle-siege-battlespace.example.json";

    [Fact]
    public void AuthoredExample_UsesStableUniqueIdsAndValidDeclaredBounds()
    {
        using var document = JsonDocument.Parse(ReadContract(ExampleFile));

        ValidateAuthoredContent(document.RootElement);
    }

    [Fact]
    public void Schema_DeclaresIdentityGeometryAndCoordinateFrameBoundaries()
    {
        using var document = JsonDocument.Parse(ReadContract(SchemaFile));
        var schema = document.RootElement;

        Assert.Equal("https://json-schema.org/draft/2020-12/schema", schema.GetProperty("$schema").GetString());
        Assert.Equal("castle-siege", schema.GetProperty("properties").GetProperty("experienceKey").GetProperty("const").GetString());
        Assert.Contains("coordinateFrame", schema.GetProperty("required").EnumerateArray().Select(x => x.GetString()));
        Assert.Contains("objects", schema.GetProperty("required").EnumerateArray().Select(x => x.GetString()));

        var objectSchema = schema.GetProperty("properties").GetProperty("objects").GetProperty("items");
        Assert.Equal("^[a-z0-9]+(?:[.-][a-z0-9]+)*$", objectSchema.GetProperty("properties").GetProperty("id").GetProperty("pattern").GetString());
        Assert.Contains("target-region", objectSchema.GetProperty("properties").GetProperty("role").GetProperty("enum").EnumerateArray().Select(x => x.GetString()));

        var positiveVector = schema.GetProperty("$defs").GetProperty("positiveVector3");
        Assert.Equal(3, positiveVector.GetProperty("minItems").GetInt32());
        Assert.Equal(3, positiveVector.GetProperty("maxItems").GetInt32());
        Assert.Equal(0, positiveVector.GetProperty("prefixItems")[0].GetProperty("exclusiveMinimum").GetInt32());
    }

    [Fact]
    public void AuthoredContent_RejectsDuplicateSemanticIds()
    {
        var root = JsonNode.Parse(ReadContract(ExampleFile))!;
        var objects = root["objects"]!.AsArray();
        objects[2]!["id"] = objects[1]!["id"]!.GetValue<string>();

        using var document = JsonDocument.Parse(root.ToJsonString());

        Assert.Throws<InvalidOperationException>(() => ValidateAuthoredContent(document.RootElement));
    }

    [Fact]
    public void AuthoredContent_RejectsZeroOrNegativeDimensions()
    {
        var root = JsonNode.Parse(ReadContract(ExampleFile))!;
        root["objects"]![0]!["bounds"]!["sizeMeters"]![0] = 0;

        using var document = JsonDocument.Parse(root.ToJsonString());

        Assert.Throws<InvalidOperationException>(() => ValidateAuthoredContent(document.RootElement));
    }

    private static string ReadContract(string fileName) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, ContractDirectory, fileName));

    // This validates authored-content invariants that JSON Schema alone cannot express here,
    // notably uniqueness of semantic IDs across objects.
    private static void ValidateAuthoredContent(JsonElement root)
    {
        Require(root.ValueKind == JsonValueKind.Object, "Root must be an object.");
        Require(root.GetProperty("schemaVersion").GetInt32() == 1, "Unsupported schema version.");
        Require(root.GetProperty("experienceKey").GetString() == "castle-siege", "Unexpected Experience key.");
        Require(!string.IsNullOrWhiteSpace(root.GetProperty("contentRevision").GetString()), "Content revision is required.");

        var frame = root.GetProperty("coordinateFrame");
        Require(!string.IsNullOrWhiteSpace(frame.GetProperty("id").GetString()), "Coordinate-frame ID is required.");
        Require(frame.GetProperty("handedness").GetString() is "right-handed" or "left-handed", "Invalid handedness.");
        Require(frame.GetProperty("upAxis").GetString() is "x" or "y" or "z", "Invalid up axis.");
        Require(frame.GetProperty("lengthUnit").GetString() == "m", "Lengths must be expressed in metres.");

        var objects = root.GetProperty("objects");
        Require(objects.ValueKind == JsonValueKind.Array && objects.GetArrayLength() > 0, "At least one authored object is required.");

        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in objects.EnumerateArray())
        {
            var id = item.GetProperty("id").GetString();
            Require(!string.IsNullOrWhiteSpace(id) && Regex.IsMatch(id, "^[a-z0-9]+(?:[.-][a-z0-9]+)*$", RegexOptions.CultureInvariant), "Object ID is invalid.");
            Require(ids.Add(id!), $"Duplicate object ID: {id}");
            Require(item.GetProperty("role").GetString() is "terrain" or "castle-section" or "target-region" or "siege-apparatus", "Invalid object role.");
            Require(!string.IsNullOrWhiteSpace(item.GetProperty("label").GetString()), "Object label is required.");

            var bounds = item.GetProperty("bounds");
            Require(bounds.GetProperty("kind").GetString() == "axis-aligned-box", "Unsupported bounds kind.");
            ValidateVector(bounds.GetProperty("centerMeters"), requirePositive: false);
            ValidateVector(bounds.GetProperty("sizeMeters"), requirePositive: true);
        }
    }

    private static void ValidateVector(JsonElement vector, bool requirePositive)
    {
        Require(vector.ValueKind == JsonValueKind.Array && vector.GetArrayLength() == 3, "A vector must contain exactly three values.");

        foreach (var component in vector.EnumerateArray())
        {
            Require(component.ValueKind == JsonValueKind.Number && component.TryGetDouble(out var value) && double.IsFinite(value), "Vector components must be finite numbers.");
            if (requirePositive)
            {
                Require(component.GetDouble() > 0, "Size dimensions must be positive.");
            }
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
