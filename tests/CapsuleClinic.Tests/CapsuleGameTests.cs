using TheSingularityWorkshop.Experiences.CapsuleClinic;
using Xunit;

namespace TheSingularityWorkshop.Experiences.CapsuleClinic.Tests;

public sealed class CapsuleGameTests
{
    [Fact]
    public void SameSeedAndActionsProduceSameActiveStateAndBoard()
    {
        var first = new CapsuleGame(seed: 1234);
        var second = new CapsuleGame(seed: 1234);
        var actions = new[] { CapsuleAction.MoveLeft, CapsuleAction.Rotate, CapsuleAction.HardDrop, CapsuleAction.MoveRight, CapsuleAction.Tick };

        foreach (var action in actions)
        {
            first.Step(action);
            second.Step(action);
            Assert.Equal(first.Active, second.Active);
            Assert.Equal(first.Board.Snapshot(), second.Board.Snapshot());
            Assert.Equal(first.GameOver, second.GameOver);
        }
    }

    [Fact]
    public void HardDropLocksCapsuleAndSpawnsNextPiece()
    {
        var game = new CapsuleGame(seed: 7);

        var result = game.Step(CapsuleAction.HardDrop);

        Assert.True(result.ActionAccepted);
        Assert.True(result.CapsuleLocked);
        Assert.False(result.GameOver);
        Assert.Equal(2, game.Board.OccupiedCount);
        Assert.NotNull(game.Active);
    }

    [Fact]
    public void MovementRejectsWallCollisionWithoutChangingActivePiece()
    {
        var game = new CapsuleGame(seed: 42, width: 4, height: 8);
        Assert.True(game.Step(CapsuleAction.MoveLeft).ActionAccepted);
        Assert.True(game.Step(CapsuleAction.MoveLeft).ActionAccepted);
        var atWall = game.Active;

        var result = game.Step(CapsuleAction.MoveLeft);

        Assert.False(result.ActionAccepted);
        Assert.Equal(atWall, game.Active);
    }

    [Fact]
    public void TooShortBoardStartsGameOverRatherThanSpawningInvalidPiece()
    {
        var game = new CapsuleGame(seed: 1, width: 4, height: 1);

        Assert.True(game.GameOver);
        Assert.Null(game.Active);
        Assert.False(game.Step(CapsuleAction.Tick).ActionAccepted);
    }

    [Fact]
    public void InvalidActionIsRejectedExplicitly()
    {
        var game = new CapsuleGame(seed: 1);

        Assert.Throws<ArgumentOutOfRangeException>(() => game.Step((CapsuleAction)99));
    }
}
