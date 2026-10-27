using System.Text.Json.Nodes;
using TheSingularityWorkshop.Experiences.CapsuleClinic;
using Xunit;

namespace TheSingularityWorkshop.Experiences.CapsuleClinic.Tests;

public sealed class CapsuleReplayTests
{
    [Fact]
    public void ReplayRoundTripsAsVersionedJsonAndReproducesGameState()
    {
        var replay = new CapsuleReplay(1234, 10, 20)
            .Append(CapsuleAction.MoveLeft)
            .Append(CapsuleAction.Rotate)
            .Append(CapsuleAction.HardDrop)
            .Append(CapsuleAction.MoveRight)
            .Append(CapsuleAction.Tick);

        var restored = CapsuleReplay.FromJson(replay.ToJson());
        using var first = replay.Play();
        using var second = restored.Play();

        Assert.Equal(CapsuleReplay.CurrentFormatVersion, restored.FormatVersion);
        Assert.Equal(replay.Seed, restored.Seed);
        Assert.Equal(replay.Width, restored.Width);
        Assert.Equal(replay.Height, restored.Height);
        Assert.Equal(replay.Actions, restored.Actions);
        Assert.Equal(first.Game.Active, second.Game.Active);
        Assert.Equal(first.Game.GameOver, second.Game.GameOver);
        Assert.Equal(first.Game.Score, second.Game.Score);
        Assert.Equal(first.Game.Board.Snapshot(), second.Game.Board.Snapshot());
        Assert.Equal(first.StepResults, second.StepResults);
    }

    [Fact]
    public void AppendingCreatesANewReplayWithoutMutatingTheOriginal()
    {
        var original = new CapsuleReplay(9);
        var updated = original.Append(CapsuleAction.HardDrop);

        Assert.Empty(original.Actions);
        Assert.Equal(new[] { CapsuleAction.HardDrop }, updated.Actions);
    }

    [Fact]
    public void ReplayRejectsUnsupportedFormatVersion()
    {
        var node = JsonNode.Parse(new CapsuleReplay(1).ToJson())!;
        node["formatVersion"] = CapsuleReplay.CurrentFormatVersion + 1;

        Assert.Throws<FormatException>(() => CapsuleReplay.FromJson(node.ToJsonString()));
    }

    [Fact]
    public void ReplayRejectsInvalidActionsAndDimensions()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CapsuleReplay(1, width: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CapsuleReplay(1, actions: new[] { (CapsuleAction)99 }));

        var node = JsonNode.Parse(new CapsuleReplay(1).ToJson())!;
        node["actions"] = new JsonArray(99);

        Assert.Throws<FormatException>(() => CapsuleReplay.FromJson(node.ToJsonString()));
    }

    [Fact]
    public void ReplayEnforcesActionCountLimit()
    {
        var actions = Enumerable.Repeat(CapsuleAction.Tick, CapsuleReplay.MaximumActions + 1);

        Assert.Throws<ArgumentOutOfRangeException>(() => new CapsuleReplay(1, actions: actions));
    }
}
