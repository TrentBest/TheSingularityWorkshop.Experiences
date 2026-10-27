using TheSingularityWorkshop.Experiences.CapsuleClinic;
using Xunit;

namespace TheSingularityWorkshop.Experiences.CapsuleClinic.Tests;

public sealed class AuthoritativePuzzleRoomTests
{
    [Fact]
    public void RoomAdmitsOnlyUniquePlayersUpToConfiguredCapacity()
    {
        var room = new AuthoritativePuzzleRoom("room-1", maxPlayers: 2);

        Assert.True(room.JoinPlayer("alice", 1));
        Assert.True(room.JoinPlayer("bob", 2));
        Assert.False(room.JoinPlayer("alice", 3));
        Assert.False(room.JoinPlayer("carol", 4));
        Assert.Equal(2, room.PlayerCount);
    }

    [Fact]
    public void DefaultRoomDoesNotImposeAnArbitraryPlayerCap()
    {
        var room = new AuthoritativePuzzleRoom("room-unbounded");

        for (var i = 0; i < 12; i++)
            Assert.True(room.JoinPlayer($"player-{i}", i + 1));

        Assert.Null(room.MaxPlayers);
        Assert.Equal(12, room.PlayerCount);
    }

    [Fact]
    public void ValidCommandConsumesSequenceAndReturnsAuthoritativeState()
    {
        var room = new AuthoritativePuzzleRoom("room-1");
        Assert.True(room.JoinPlayer("alice", 123));

        var decision = room.ApplyCommand("alice",
            new GameCommandEnvelope(1, "room-1", "alice", 1, CapsuleAction.MoveLeft));

        Assert.True(decision.Accepted);
        Assert.Equal(CommandRejection.None, decision.Rejection);
        Assert.Equal(1UL, room.GetPlayerView("alice").LastAcceptedSequence);
        Assert.Equal(room.Width * room.Height, room.GetPlayerView("alice").Board.Count);
    }

    [Theory]
    [InlineData(2, "room-1", "alice", CommandRejection.ProtocolMismatch)]
    [InlineData(1, "other-room", "alice", CommandRejection.WrongRoom)]
    [InlineData(1, "room-1", "bob", CommandRejection.IdentityMismatch)]
    public void InvalidProtocolRoomOrIdentityDoesNotConsumeSequence(
        int version, string roomId, string playerId, CommandRejection expected)
    {
        var room = new AuthoritativePuzzleRoom("room-1");
        Assert.True(room.JoinPlayer("alice", 123));

        var decision = room.ApplyCommand("alice",
            new GameCommandEnvelope(version, roomId, playerId, 1, CapsuleAction.MoveLeft));

        Assert.False(decision.Accepted);
        Assert.Equal(expected, decision.Rejection);
        Assert.Equal(0UL, room.GetPlayerView("alice").LastAcceptedSequence);
    }

    [Fact]
    public void DuplicateAndOutOfOrderCommandsAreRejected()
    {
        var room = new AuthoritativePuzzleRoom("room-1");
        Assert.True(room.JoinPlayer("alice", 123));
        var first = new GameCommandEnvelope(1, "room-1", "alice", 1, CapsuleAction.MoveLeft);

        Assert.True(room.ApplyCommand("alice", first).Accepted);
        Assert.Equal(CommandRejection.InvalidSequence, room.ApplyCommand("alice", first).Rejection);
        Assert.Equal(CommandRejection.InvalidSequence,
            room.ApplyCommand("alice", first with { Sequence = 3 }).Rejection);
        Assert.Equal(1UL, room.GetPlayerView("alice").LastAcceptedSequence);
    }

    [Fact]
    public void InvalidActionIsRejectedBeforeSequenceIsConsumed()
    {
        var room = new AuthoritativePuzzleRoom("room-1");
        Assert.True(room.JoinPlayer("alice", 123));

        var decision = room.ApplyCommand("alice",
            new GameCommandEnvelope(1, "room-1", "alice", 1, (CapsuleAction)99));

        Assert.False(decision.Accepted);
        Assert.Equal(CommandRejection.InvalidAction, decision.Rejection);
        Assert.Equal(0UL, room.GetPlayerView("alice").LastAcceptedSequence);
    }

    [Fact]
    public void PlayerCannotReadAnotherUnjoinedPlayerAndSnapshotsAreIndependent()
    {
        var room = new AuthoritativePuzzleRoom("room-1");
        Assert.True(room.JoinPlayer("alice", 123));
        var snapshot = room.GetPlayerView("alice").Board;

        Assert.Throws<KeyNotFoundException>(() => room.GetPlayerView("bob"));
        room.ApplyCommand("alice", new GameCommandEnvelope(1, "room-1", "alice", 1, CapsuleAction.HardDrop));
        Assert.Equal(0, snapshot.Count(cell => cell.HasValue));
        Assert.Equal(2, room.GetPlayerView("alice").Board.Count(cell => cell.HasValue));
    }

    [Fact]
    public void MatchLifecycleUsesAuthoritativeSurvivorsAndPreservesSequenceAcrossRounds()
    {
        var room = new AuthoritativePuzzleRoom("match-room", width: 2, height: 3);
        Assert.True(room.JoinPlayer("alice", 101));
        Assert.True(room.JoinPlayer("bob", 202));
        Assert.True(room.StartMatch(roundsToWin: 2));
        Assert.Equal(CapsuleMatchState.InRound, room.MatchState);
        Assert.False(room.JoinPlayer("carol", 303));

        var aliceDrop = room.ApplyCommand("alice",
            new GameCommandEnvelope(1, "match-room", "alice", 1, CapsuleAction.HardDrop));
        Assert.True(aliceDrop.Accepted);
        Assert.True(aliceDrop.Step.GameOver);
        Assert.Null(room.EvaluateRoundAtHostBoundary());

        var bobDrop = room.ApplyCommand("bob",
            new GameCommandEnvelope(1, "match-room", "bob", 1, CapsuleAction.HardDrop));
        Assert.True(bobDrop.Accepted);
        Assert.True(bobDrop.Step.GameOver);
        var draw = room.EvaluateRoundAtHostBoundary();

        Assert.NotNull(draw);
        Assert.True(draw.IsDraw);
        Assert.Null(draw.WinnerPlayerId);
        Assert.Equal(CapsuleMatchState.RoundComplete, room.MatchState);
        Assert.Equal(CommandRejection.MatchNotInRound,
            room.ApplyCommand("alice", new GameCommandEnvelope(1, "match-room", "alice", 2, CapsuleAction.Tick)).Rejection);

        Assert.True(room.StartNextRound());
        Assert.Equal(CapsuleMatchState.InRound, room.MatchState);
        Assert.False(room.GetPlayerView("alice").GameOver);
        Assert.Equal(1UL, room.GetPlayerView("alice").LastAcceptedSequence);
        Assert.Null(room.EvaluateRoundAtHostBoundary());
    }

    [Fact]
    public void RoundEvaluationWaitsForHostBoundaryAndRecordsSingleSurvivor()
    {
        var room = new AuthoritativePuzzleRoom("winner-room", width: 2, height: 3);
        Assert.True(room.JoinPlayer("alice", 101));
        Assert.True(room.JoinPlayer("bob", 202));
        Assert.True(room.StartMatch(roundsToWin: 1));

        var decision = room.ApplyCommand("alice",
            new GameCommandEnvelope(1, "winner-room", "alice", 1, CapsuleAction.HardDrop));
        Assert.True(decision.Accepted);
        Assert.True(decision.Step.GameOver);

        var result = room.EvaluateRoundAtHostBoundary();

        Assert.NotNull(result);
        Assert.Equal("bob", result.WinnerPlayerId);
        Assert.False(result.IsDraw);
        Assert.Equal(CapsuleMatchState.MatchComplete, room.MatchState);
        Assert.Equal("bob", room.MatchWinnerPlayerId);
        Assert.Equal(CommandRejection.MatchNotInRound,
            room.ApplyCommand("bob", new GameCommandEnvelope(1, "winner-room", "bob", 1, CapsuleAction.Tick)).Rejection);
        Assert.False(room.StartNextRound());
    }

    [Fact]
    public void MatchStandingsReflectRoundWinsAndAuthoritativeAliveState()
    {
        var room = new AuthoritativePuzzleRoom("standings-room", width: 2, height: 3);
        Assert.True(room.JoinPlayer("alice", 101));
        Assert.True(room.JoinPlayer("bob", 202));
        Assert.True(room.StartMatch(roundsToWin: 2));

        Assert.All(room.GetStandings(), standing => Assert.True(standing.IsActive));

        room.ApplyCommand("alice",
            new GameCommandEnvelope(1, "standings-room", "alice", 1, CapsuleAction.HardDrop));
        var standings = room.GetStandings();

        Assert.False(Assert.Single(standings, standing => standing.PlayerId == "alice").IsActive);
        Assert.True(Assert.Single(standings, standing => standing.PlayerId == "bob").IsActive);
    }

}
