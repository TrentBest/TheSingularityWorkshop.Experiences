using TheSingularityWorkshop.Experiences.CapsuleClinic;
using Xunit;

namespace TheSingularityWorkshop.Experiences.CapsuleClinic.Tests;

public sealed class CapsuleArenaSnapshotTests
{
    [Fact]
    public void CaptureCombinesAuthoritativeStandingsAndBoardSnapshotsInFixedPaneOrder()
    {
        using var room = new AuthoritativePuzzleRoom("arena");
        Assert.True(room.JoinPlayer("alice", 101));
        Assert.True(room.JoinPlayer("bob", 202));
        Assert.True(room.JoinPlayer("carol", 303));
        Assert.True(room.StartMatch(roundsToWin: 2));

        var snapshot = CapsuleArenaSnapshot.Capture(room, "bob");

        Assert.Equal("bob", snapshot.LocalPlayerId);
        Assert.Equal(new[]
        {
            ArenaPane.LocalUpperLeft,
            ArenaPane.ComparisonUpperRight,
            ArenaPane.ComparisonLowerLeft,
            ArenaPane.ComparisonLowerRight
        }, snapshot.Panes.Select(p => p.Pane));
        Assert.Equal(new[] { "bob", "alice", "carol", null }, snapshot.Panes.Select(p => p.Player?.PlayerId));
        Assert.Equal(new[] { "bob", "alice", "carol", null }, snapshot.Panes.Select(p => p.Standing?.PlayerId));
        Assert.All(snapshot.Panes.Take(3), pane =>
        {
            Assert.NotNull(pane.Player);
            Assert.NotNull(pane.Standing);
            Assert.NotEmpty(pane.Player!.Board);
        });
        Assert.Null(snapshot.Panes[3].Player);
        Assert.Null(snapshot.Panes[3].Standing);
    }

    [Fact]
    public void CaptureRejectsLocalPlayerWhoHasNotJoinedTheActiveMatchRoster()
    {
        using var room = new AuthoritativePuzzleRoom("arena");
        Assert.True(room.JoinPlayer("alice", 101));
        Assert.True(room.JoinPlayer("bob", 202));
        Assert.True(room.StartMatch());

        Assert.Throws<KeyNotFoundException>(() => CapsuleArenaSnapshot.Capture(room, "mallory"));
    }

    [Fact]
    public void CaptureDoesNotExposeBoardsForPlayersOutsideSelectedPanes()
    {
        using var room = new AuthoritativePuzzleRoom("arena");
        for (var index = 1; index <= 8; index++)
            Assert.True(room.JoinPlayer($"p{index}", index));
        Assert.True(room.StartMatch());

        var snapshot = CapsuleArenaSnapshot.Capture(room, "p1");

        Assert.Equal(new[] { "p1", "p2", "p3", "p4" }, snapshot.Panes.Select(p => p.Player?.PlayerId));
        Assert.DoesNotContain(snapshot.Panes, pane => pane.Player?.PlayerId == "p8");
    }
}
