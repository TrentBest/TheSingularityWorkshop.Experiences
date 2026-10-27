using TheSingularityWorkshop.Experiences.CapsuleClinic;
using Xunit;

namespace TheSingularityWorkshop.Experiences.CapsuleClinic.Tests;

public sealed class ArenaPaneSelectorTests
{
    [Fact]
    public void SmallRoomShowsEveryPlayerAndKeepsLocalPlayerUpperLeft()
    {
        var standings = new[]
        {
            new ArenaPlayerStanding("p2", 2, 1, 2),
            new ArenaPlayerStanding("p1", 1, 3, 1),
            new ArenaPlayerStanding("p3", 3, 0, 3)
        };

        var panes = ArenaPaneSelector.Select("p2", standings);

        Assert.Equal(new[]
        {
            new ArenaPaneAssignment(ArenaPane.LocalUpperLeft, "p2"),
            new ArenaPaneAssignment(ArenaPane.ComparisonUpperRight, "p1"),
            new ArenaPaneAssignment(ArenaPane.ComparisonLowerLeft, "p3"),
            new ArenaPaneAssignment(ArenaPane.ComparisonLowerRight, null)
        }, panes);
    }

    [Fact]
    public void FirstPlaceInLargeRoomSeesRanksTwoThreeAndFour()
    {
        var standings = CreateStandings(10);

        var panes = ArenaPaneSelector.Select("p1", standings);

        Assert.Equal(new[] { "p1", "p2", "p3", "p4" }, panes.Select(p => p.PlayerId));
    }

    [Fact]
    public void SecondPlaceSeesOtherMembersOfTopFourWithoutDuplicateLocalPane()
    {
        var standings = CreateStandings(10);

        var panes = ArenaPaneSelector.Select("p2", standings);

        Assert.Equal(new[] { "p2", "p1", "p3", "p4" }, panes.Select(p => p.PlayerId));
        Assert.Equal(4, panes.Where(p => p.PlayerId is not null).Select(p => p.PlayerId).Distinct().Count());
    }

    [Fact]
    public void PlayerOutsideTopFourSeesLeaderAndNearestActiveRankNeighbors()
    {
        var standings = CreateStandings(10);

        var panes = ArenaPaneSelector.Select("p7", standings);

        Assert.Equal(new[] { "p7", "p1", "p6", "p8" }, panes.Select(p => p.PlayerId));
    }

    [Fact]
    public void TiesUseJoinOrderThenOrdinalIdForStableSelection()
    {
        var standings = new[]
        {
            new ArenaPlayerStanding("local", 1, 0, 0),
            new ArenaPlayerStanding("later", 2, 0, 3),
            new ArenaPlayerStanding("earlier-b", 2, 0, 1),
            new ArenaPlayerStanding("earlier-a", 2, 0, 1)
        };

        var panes = ArenaPaneSelector.Select("local", standings);

        Assert.Equal(new[] { "local", "earlier-a", "earlier-b", "later" }, panes.Select(p => p.PlayerId));
    }

    [Fact]
    public void MissingLocalPlayerAndDuplicateIdentitiesAreRejected()
    {
        Assert.Throws<KeyNotFoundException>(() =>
            ArenaPaneSelector.Select("missing", CreateStandings(4)));
        Assert.Throws<ArgumentException>(() =>
            ArenaPaneSelector.Select("p1", new[]
            {
                new ArenaPlayerStanding("p1", 1, 0, 0),
                new ArenaPlayerStanding("p1", 2, 0, 1)
            }));
    }

    private static ArenaPlayerStanding[] CreateStandings(int count) =>
        Enumerable.Range(1, count)
            .Select(rank => new ArenaPlayerStanding($"p{rank}", rank, count - rank, rank))
            .ToArray();
}
