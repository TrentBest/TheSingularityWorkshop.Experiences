using TheSingularityWorkshop.Experiences.CapsuleClinic;
using Xunit;

namespace TheSingularityWorkshop.Experiences.CapsuleClinic.Tests;

public sealed class CapsuleClinicKeyboardBindingsTests
{
    [Theory]
    [InlineData("ArrowLeft", CapsuleClinicIntents.MoveLeft)]
    [InlineData("ArrowRight", CapsuleClinicIntents.MoveRight)]
    [InlineData("ArrowUp", CapsuleClinicIntents.Rotate)]
    [InlineData("ArrowDown", CapsuleClinicIntents.SoftDrop)]
    [InlineData(" ", CapsuleClinicIntents.HardDrop)]
    public void MapsSupportedKeysToStableSemanticIntents(string key, string expectedIntent)
    {
        Assert.True(CapsuleClinicKeyboardBindings.TryMapKey(key, out var intent));
        Assert.Equal(expectedIntent, intent);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("ArrowLeft ")]
    [InlineData("Space")]
    [InlineData("Escape")]
    public void UnknownKeysAreIgnoredWithoutProducingAnIntent(string? key)
    {
        Assert.False(CapsuleClinicKeyboardBindings.TryMapKey(key, out var intent));
        Assert.Equal(string.Empty, intent);
    }

    [Fact]
    public void KeyboardIntentCanBeSubmittedThroughAuthorityAndRenderedFromReturnedSnapshot()
    {
        using var session = new CapsuleClinicLocalSession(
            "keyboard-roundtrip", "alice", 101, "bot", 202);
        var before = session.CaptureSnapshot();
        var beforePlayer = Assert.IsType<PuzzlePlayerView>(before.Panes[0].Player);
        var beforeCapsule = Assert.IsType<ActiveCapsule>(beforePlayer.Active);

        Assert.True(CapsuleClinicKeyboardBindings.TryMapKey("ArrowLeft", out var intent));
        var result = session.SubmitIntent(intent);
        var presentation = CapsuleArenaGuiAdapter.Build(result.Snapshot);
        var svg = CapsuleArenaSvgRenderer.Render(result.Snapshot);

        Assert.True(result.IntentRecognized);
        Assert.True(result.Decision.Accepted);
        Assert.Equal(1UL, result.Decision.Sequence);
        var afterPlayer = Assert.IsType<PuzzlePlayerView>(result.Snapshot.Panes[0].Player);
        var afterCapsule = Assert.IsType<ActiveCapsule>(afterPlayer.Active);
        Assert.Equal(beforeCapsule.PivotX - 1, afterCapsule.PivotX);
        Assert.Equal("alice", presentation.Properties["localPlayerId"]);
        Assert.Contains("alice", svg, StringComparison.Ordinal);
    }
}
