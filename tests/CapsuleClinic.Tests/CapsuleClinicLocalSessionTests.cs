using TheSingularityWorkshop.Experiences.CapsuleClinic;
using Xunit;

namespace TheSingularityWorkshop.Experiences.CapsuleClinic.Tests;

public sealed class CapsuleClinicLocalSessionTests
{
    [Fact]
    public void SemanticIntentPassesThroughAuthorityAndReturnsUpdatedSnapshot()
    {
        using var session = new CapsuleClinicLocalSession(
            "local-room", "alice", 101, "bot", 202);

        var before = session.CaptureSnapshot();
        var beforeLocal = Assert.IsType<PuzzlePlayerView>(before.Panes[0].Player);
        var beforeActive = Assert.IsType<ActiveCapsule>(beforeLocal.Active);

        var result = session.SubmitIntent(CapsuleClinicIntents.MoveLeft);

        Assert.True(result.IntentRecognized);
        Assert.True(result.Decision.Accepted);
        Assert.Equal(CommandRejection.None, result.Decision.Rejection);
        Assert.Equal(1UL, result.Decision.Sequence);
        var afterLocal = Assert.IsType<PuzzlePlayerView>(result.Snapshot.Panes[0].Player);
        var afterActive = Assert.IsType<ActiveCapsule>(afterLocal.Active);
        Assert.Equal(beforeActive.PivotX - 1, afterActive.PivotX);
        Assert.Equal(1UL, afterLocal.LastAcceptedSequence);
        Assert.Equal("alice", result.Snapshot.LocalPlayerId);
    }

    [Fact]
    public void UnknownSemanticIntentCannotMutateStateOrConsumeSequence()
    {
        using var session = new CapsuleClinicLocalSession(
            "local-room", "alice", 101, "bot", 202);
        var before = session.CaptureSnapshot();

        var result = session.SubmitIntent("capsule.set-score");

        Assert.False(result.IntentRecognized);
        Assert.False(result.Decision.Accepted);
        Assert.Equal(CommandRejection.InvalidAction, result.Decision.Rejection);
        AssertSnapshotsEquivalent(before, result.Snapshot);
        Assert.Equal(0UL, Assert.IsType<PuzzlePlayerView>(result.Snapshot.Panes[0].Player).LastAcceptedSequence);
    }

    [Fact]
    public void FixedSeedsAndSemanticIntentSequenceProduceEquivalentSnapshots()
    {
        using var first = new CapsuleClinicLocalSession(
            "repeat-room", "alice", 1234, "bot", 9876);
        using var second = new CapsuleClinicLocalSession(
            "repeat-room", "alice", 1234, "bot", 9876);
        var intents = new[]
        {
            CapsuleClinicIntents.MoveLeft,
            CapsuleClinicIntents.Rotate,
            CapsuleClinicIntents.HardDrop,
            CapsuleClinicIntents.MoveRight,
            CapsuleClinicIntents.SoftDrop
        };

        foreach (var intent in intents)
        {
            var firstResult = first.SubmitIntent(intent);
            var secondResult = second.SubmitIntent(intent);

            Assert.Equal(firstResult.IntentRecognized, secondResult.IntentRecognized);
            Assert.Equal(firstResult.Decision.Accepted, secondResult.Decision.Accepted);
            Assert.Equal(firstResult.Decision.Step, secondResult.Decision.Step);
            AssertSnapshotsEquivalent(firstResult.Snapshot, secondResult.Snapshot);
        }
    }

    private static void AssertSnapshotsEquivalent(CapsuleArenaSnapshot expected, CapsuleArenaSnapshot actual)
    {
        Assert.Equal(expected.LocalPlayerId, actual.LocalPlayerId);
        Assert.Equal(expected.BoardWidth, actual.BoardWidth);
        Assert.Equal(expected.BoardHeight, actual.BoardHeight);
        Assert.Equal(expected.Panes.Select(pane => pane.Pane), actual.Panes.Select(pane => pane.Pane));
        for (var index = 0; index < expected.Panes.Count; index++)
        {
            var expectedPane = expected.Panes[index];
            var actualPane = actual.Panes[index];
            Assert.Equal(expectedPane.Standing, actualPane.Standing);
            Assert.Equal(expectedPane.Player?.PlayerId, actualPane.Player?.PlayerId);
            Assert.Equal(expectedPane.Player?.Board, actualPane.Player?.Board);
            Assert.Equal(expectedPane.Player?.Active, actualPane.Player?.Active);
            Assert.Equal(expectedPane.Player?.GameOver, actualPane.Player?.GameOver);
            Assert.Equal(expectedPane.Player?.LastAcceptedSequence, actualPane.Player?.LastAcceptedSequence);
        }
    }
}
