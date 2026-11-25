using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using TheSingularityWorkshop.Experiences.Warlords;
using Xunit;

namespace TheSingularityWorkshop.Experiences.Warlords.Tests;

public sealed class WarlordsGoldenVectorTests
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    [Fact]
    public void OpeningCampaignVectorReplaysToExactEventsAndFinalSnapshot()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "vectors", "four-kingdoms-opening-seed-17.json");
        using var document = JsonDocument.Parse(File.ReadAllBytes(path));
        var root = document.RootElement;

        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(WarlordsGame.RulesVersion, root.GetProperty("rulesVersion").GetString());
        Assert.Equal("four-kingdoms", root.GetProperty("scenario").GetProperty("key").GetString());
        Assert.Equal(1, root.GetProperty("scenario").GetProperty("version").GetInt32());

        var game = new WarlordsGame(
            WarlordsMap.CreateStarterMap(),
            root.GetProperty("seed").GetInt32());

        foreach (var command in root.GetProperty("commands").EnumerateArray())
        {
            var kind = command.GetProperty("kind").GetString();
            WarlordsEvent? acceptedEvent = null;

            switch (kind)
            {
                case "march":
                    acceptedEvent = game.March(new WarlordsMarch(
                        command.GetProperty("source").GetString()!,
                        command.GetProperty("destination").GetString()!,
                        command.GetProperty("armies").GetInt32()));
                    break;
                case "endTurn":
                    game.EndTurn();
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported fixture command '{kind}'.");
            }

            var expected = command.GetProperty("expected");
            Assert.Equal("accepted", expected.GetProperty("result").GetString());
            if (expected.TryGetProperty("event", out var expectedEvent))
            {
                Assert.NotNull(acceptedEvent);
                Assert.True(
                    JsonNode.DeepEquals(
                        JsonNode.Parse(expectedEvent.GetRawText()),
                        JsonSerializer.SerializeToNode(acceptedEvent, JsonOptions)),
                    $"Command {command.GetProperty("sequence").GetInt32()} event differs from its golden vector.");
            }
            else
            {
                Assert.Null(acceptedEvent);
            }
        }

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(root.GetProperty("expectedFinalSnapshot").GetRawText()),
                JsonSerializer.SerializeToNode(game.Capture(), JsonOptions)),
            "Final Warlords snapshot differs from the authoritative golden vector.");
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
