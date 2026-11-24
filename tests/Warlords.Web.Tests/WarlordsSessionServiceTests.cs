using TheSingularityWorkshop.Experiences.Warlords;
using WarlordsWebApp;
using Xunit;

namespace TheSingularityWorkshop.Experiences.Warlords.Web.Tests;

public sealed class WarlordsSessionServiceTests
{
    [Fact]
    public void SemanticTreeProjectsTerritoriesAndAllowListedLegalCommands()
    {
        using var session = new WarlordsSessionService();

        var tree = session.BuildTree();

        Assert.Equal("Panel", tree.Kind);
        Assert.Equal("Frostmarch", tree.Find("territory-frostmarch-name").Text);
        var firstMarch = tree.Find("march-option-0");
        Assert.Equal("Button", firstMarch.Kind);
        Assert.True(firstMarch.Properties.ContainsKey("onClickCommand"));
    }

    [Fact]
    public void HostRejectsACommandThatIsNotInTheCurrentLegalMoveSet()
    {
        using var session = new WarlordsSessionService();
        var before = session.Snapshot;

        session.Dispatch("march|azure-keep|not-a-territory|4");

        Assert.Equal(0, session.Snapshot.Events.Count);
        Assert.Equal(before.Territories, session.Snapshot.Territories);
        Assert.StartsWith("Command rejected:", session.Message);
    }

    [Fact]
    public void LegalGuiCommandMutatesTheDomainAndAdvancesTheVisibleSnapshot()
    {
        using var session = new WarlordsSessionService();

        session.Dispatch("march|azure-keep|frostmarch|4");

        Assert.Single(session.Snapshot.Events);
        Assert.Equal(WarlordsKingdom.Azure, session.Snapshot.Territories
            .Single(territory => territory.TerritoryId == "frostmarch").Owner);
        Assert.Contains("AttackerVictory", session.Message);
    }

    [Fact]
    public void EndTurnCommandAdvancesTheAuthoritativeKingdom()
    {
        using var session = new WarlordsSessionService();

        session.Dispatch("end-turn");

        Assert.Equal(WarlordsKingdom.Crimson, session.Snapshot.ActiveKingdom);
        Assert.Contains("Crimson", session.Message);
    }
}
