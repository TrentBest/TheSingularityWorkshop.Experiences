using TheSingularityWorkshop.Experiences.CapsuleClinic;
using Xunit;

namespace TheSingularityWorkshop.Experiences.CapsuleClinic.Tests;

public sealed class CapsuleAgentPolicyTests
{
    [Fact]
    public void ObservationIsAnIndependentSnapshot()
    {
        using var game = new CapsuleGame(seed: 101);
        game.Step(CapsuleAction.HardDrop);
        var observation = CapsuleAgentObservation.Capture(game);
        var cells = observation.Cells.ToArray();
        var heights = observation.ColumnHeights.ToArray();

        game.Step(CapsuleAction.HardDrop);

        Assert.Equal(cells, observation.Cells);
        Assert.Equal(heights, observation.ColumnHeights);
        Assert.Equal(2, observation.Cells.Count(cell => cell.HasValue));
    }

    [Theory]
    [InlineData(CapsuleAgentStyle.Steady)]
    [InlineData(CapsuleAgentStyle.LowColumn)]
    [InlineData(CapsuleAgentStyle.ColorOpportunist)]
    public void PolicyIsDeterministicForTheSameObservation(CapsuleAgentStyle style)
    {
        using var game = new CapsuleGame(seed: 2026);
        var observation = CapsuleAgentObservation.Capture(game);
        var policy = new CapsuleAgentPolicy(style);

        Assert.Equal(policy.ChooseAction(observation), policy.ChooseAction(observation));
    }

    [Fact]
    public void LowColumnStylePrefersTheLowestAvailableColumn()
    {
        using var game = new CapsuleGame(seed: 77, width: 6, height: 10);
        Assert.True(game.Board.TryPlaceVirus(1, 0, CapsuleColor.Blue));
        Assert.True(game.Board.TryPlaceVirus(2, 0, CapsuleColor.Yellow));
        Assert.True(game.Board.TryPlaceVirus(3, 0, CapsuleColor.Purple));
        Assert.True(game.Board.TryPlaceVirus(4, 0, CapsuleColor.Red));
        Assert.True(game.Board.TryPlaceVirus(5, 0, CapsuleColor.Blue));
        var policy = new CapsuleAgentPolicy(CapsuleAgentStyle.LowColumn);
        var observation = CapsuleAgentObservation.Capture(game);

        var action = policy.ChooseAction(observation);

        // The active capsule spawns near the center; the policy heads toward the least-filled
        // column rather than dropping blindly into the center.
        Assert.Equal(CapsuleAction.MoveLeft, action);
    }

    [Fact]
    public void EndedGameDoesNotProposeAnotherPlacement()
    {
        using var game = new CapsuleGame(seed: 1, width: 4, height: 1);
        var policy = new CapsuleAgentPolicy(CapsuleAgentStyle.Steady);

        Assert.Equal(CapsuleAction.Tick, policy.ChooseAction(CapsuleAgentObservation.Capture(game)));
    }
}
