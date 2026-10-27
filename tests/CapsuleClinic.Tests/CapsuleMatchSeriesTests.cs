using TheSingularityWorkshop.Experiences.CapsuleClinic;
using Xunit;

namespace TheSingularityWorkshop.Experiences.CapsuleClinic.Tests;

public sealed class CapsuleMatchSeriesTests
{
    [Fact]
    public void SeriesRequiresTwoPlayersAndStartsAtRoundOne()
    {
        using var series = new CapsuleMatchSeries(roundsToWin: 2);
        Assert.False(series.StartMatch());
        Assert.True(series.JoinPlayer("alice"));
        Assert.False(series.StartMatch());
        Assert.True(series.JoinPlayer("bob"));
        Assert.True(series.StartMatch());
        Assert.Equal(CapsuleMatchState.InRound, series.State);
        Assert.Equal(1, series.RoundNumber);
    }

    [Fact]
    public void HostRecordedWinnerGetsRoundWinAndCanWinMatch()
    {
        using var series = new CapsuleMatchSeries(roundsToWin: 2);
        series.JoinPlayer("alice");
        series.JoinPlayer("bob");
        Assert.True(series.StartMatch());

        var first = series.CompleteRound(["alice"]);

        Assert.Equal("alice", first.WinnerPlayerId);
        Assert.False(first.IsDraw);
        Assert.Equal(CapsuleMatchState.RoundComplete, series.State);
        Assert.Null(series.MatchWinnerPlayerId);
        Assert.True(series.StartNextRound());
        var second = series.CompleteRound(["alice"]);

        Assert.Equal("alice", second.WinnerPlayerId);
        Assert.Equal(CapsuleMatchState.MatchComplete, series.State);
        Assert.Equal("alice", series.MatchWinnerPlayerId);
        Assert.False(series.StartNextRound());
    }

    [Theory]
    [MemberData(nameof(DrawSurvivorSets))]
    public void ZeroOrMultipleSurvivorsProduceDraw(IEnumerable<string> survivors)
    {
        using var series = CreateStartedSeries();

        var result = series.CompleteRound(survivors);

        Assert.True(result.IsDraw);
        Assert.Null(result.WinnerPlayerId);
        Assert.Equal(CapsuleMatchState.RoundComplete, series.State);
    }

    [Fact]
    public void RematchAdvancesRoundAndPreservesCumulativeWins()
    {
        using var series = CreateStartedSeries();
        series.CompleteRound(["alice"]);
        Assert.True(series.StartNextRound());

        Assert.Equal(2, series.RoundNumber);
        Assert.Equal(1, Assert.Single(series.GetStandings(), p => p.PlayerId == "alice").RoundsWon);
        Assert.Equal(CapsuleMatchState.InRound, series.State);
    }

    [Fact]
    public void SeriesLocksRosterAfterStartAndRejectsUnknownRoundParticipants()
    {
        using var series = CreateStartedSeries();
        Assert.False(series.JoinPlayer("carol"));
        Assert.Throws<ArgumentException>(() => series.CompleteRound(["not-a-player"]));
        Assert.Equal(CapsuleMatchState.InRound, series.State);
    }

    public static IEnumerable<object[]> DrawSurvivorSets()
    {
        yield return [Array.Empty<string>()];
        yield return [new[] { "alice", "bob" }];
    }

    private static CapsuleMatchSeries CreateStartedSeries()
    {
        var series = new CapsuleMatchSeries();
        Assert.True(series.JoinPlayer("alice"));
        Assert.True(series.JoinPlayer("bob"));
        Assert.True(series.StartMatch());
        return series;
    }
}
