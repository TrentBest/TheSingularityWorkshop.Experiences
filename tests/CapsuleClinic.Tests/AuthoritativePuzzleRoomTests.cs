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
}
