using TheSingularityWorkshop.Experiences.Warlords;
using Xunit;

namespace TheSingularityWorkshop.Experiences.Warlords.Tests;

public sealed class WarlordsGameTests
{
    [Fact]
    public void StarterMapIsConnectedAndHasFourStrongholds()
    {
        var map = WarlordsMap.CreateStarterMap();
        Assert.Equal(9, map.Territories.Count);
        Assert.Equal(4, map.Territories.Count(t => t.IsStronghold));
        Assert.All(map.Territories, territory =>
            Assert.All(territory.Neighbors, neighbor =>
                Assert.Contains(territory.Id, map.GetTerritory(neighbor).Neighbors)));
    }

    [Fact]
    public void NewGameStartsWithAzureAndRecruitsAtItsStronghold()
    {
        var game = new WarlordsGame(WarlordsMap.CreateStarterMap(), seed: 17);
        var snapshot = game.Capture();
        Assert.Equal(WarlordsKingdom.Azure, snapshot.ActiveKingdom);
        Assert.Equal(1, snapshot.Round);
        Assert.Equal(17, snapshot.Seed);
        Assert.Equal(5, snapshot.Territories.Single(t => t.TerritoryId == "azure-keep").ArmyCount);
        Assert.Equal(WarlordsPhase.Command, snapshot.Phase);
    }

    [Fact]
    public void MarchCapturesAdjacentNeutralTerritoryAndRecordsCombatEvidence()
    {
        var game = new WarlordsGame(WarlordsMap.CreateStarterMap(), seed: 17);
        var entry = game.March(new WarlordsMarch("azure-keep", "frostmarch", 4));
        Assert.Equal(WarlordsBattleOutcome.AttackerVictory, entry.BattleOutcome);
        Assert.Equal(WarlordsKingdom.Azure, entry.DestinationAfter.Owner);
        Assert.Equal(3, entry.DestinationAfter.ArmyCount);
        Assert.Equal(1, entry.SourceAfter.ArmyCount);
        Assert.Equal(1, entry.Sequence);
        Assert.Equal("combat.seeded-d6-strength-comparison", entry.RuleId);
    }

    [Fact]
    public void InvalidMarchDoesNotChangeStateOrConsumeAnEvent()
    {
        var game = new WarlordsGame(WarlordsMap.CreateStarterMap(), seed: 17);
        var before = game.Capture();
        Assert.Throws<InvalidOperationException>(() =>
            game.March(new WarlordsMarch("azure-keep", "heartland", 5)));
        var after = game.Capture();
        Assert.Equal(before.Territories, after.Territories);
        Assert.Empty(after.Events);
    }

    [Fact]
    public void NonAdjacentMarchIsRejected()
    {
        var game = new WarlordsGame(WarlordsMap.CreateStarterMap(), seed: 17);
        Assert.Throws<InvalidOperationException>(() =>
            game.March(new WarlordsMarch("azure-keep", "sunfield", 1)));
        Assert.Empty(game.Events);
    }

    [Fact]
    public void OnlyOneMarchIsAcceptedPerTurn()
    {
        var game = new WarlordsGame(WarlordsMap.CreateStarterMap(), seed: 17);
        game.March(new WarlordsMarch("azure-keep", "frostmarch", 4));
        Assert.Empty(game.GetLegalMarches());
        Assert.Throws<InvalidOperationException>(() =>
            game.March(new WarlordsMarch("frostmarch", "heartland", 1)));
        Assert.Single(game.Events);
    }

    [Fact]
    public void TurnAdvancesAndRecruitsAtNextKingdomStronghold()
    {
        var game = new WarlordsGame(WarlordsMap.CreateStarterMap(), seed: 17);
        game.EndTurn();
        var snapshot = game.Capture();
        Assert.Equal(WarlordsKingdom.Crimson, snapshot.ActiveKingdom);
        Assert.Equal(5, snapshot.Territories.Single(t => t.TerritoryId == "crimson-hold").ArmyCount);
    }

    [Fact]
    public void SeededMatchesProduceIdenticalCombatAndEventHistory()
    {
        var map = WarlordsMap.CreateStarterMap();
        var first = new WarlordsGame(map, seed: 1234);
        var second = new WarlordsGame(map, seed: 1234);
        var march = new WarlordsMarch("azure-keep", "frostmarch", 3);
        Assert.Equal(first.March(march), second.March(march));
        Assert.Equal(first.Events, second.Events);
        Assert.Equal(first.Capture().Territories, second.Capture().Territories);
    }

    [Fact]
    public void LegalMarchesExcludeFriendlyTerritoriesAndPreserveOneArmy()
    {
        var game = new WarlordsGame(WarlordsMap.CreateStarterMap(), seed: 17);
        var marches = game.GetLegalMarches();
        Assert.Contains(marches, move => move.SourceTerritoryId == "azure-keep" &&
            move.DestinationTerritoryId == "frostmarch" && move.ArmyCount == 4);
        Assert.DoesNotContain(marches, move => move.DestinationTerritoryId == "crimson-hold");
        Assert.DoesNotContain(marches, move => move.ArmyCount >= 5);
    }

    [Fact]
    public void MapRejectsDisconnectedTerritoryGraph()
    {
        var territories = new[]
        {
            new WarlordsTerritoryDefinition("a", "A", ["b", "d"], WarlordsKingdom.Azure, true, 3),
            new WarlordsTerritoryDefinition("b", "B", ["a", "c"], WarlordsKingdom.Crimson, true, 3),
            new WarlordsTerritoryDefinition("c", "C", ["b", "d"], WarlordsKingdom.Verdant, true, 3),
            new WarlordsTerritoryDefinition("d", "D", ["a", "c"], WarlordsKingdom.Amber, true, 3),
            new WarlordsTerritoryDefinition("e", "E", ["f"]),
            new WarlordsTerritoryDefinition("f", "F", ["e"])
        };
        Assert.Throws<ArgumentException>(() => new WarlordsMap("disconnected", 1, territories));
    }

    [Fact]
    public void MapRejectsAsymmetricAdjacency()
    {
        var territories = new[]
        {
            new WarlordsTerritoryDefinition("a", "A", ["b"], WarlordsKingdom.Azure, true, 3),
            new WarlordsTerritoryDefinition("b", "B", ["c"], WarlordsKingdom.Crimson, true, 3),
            new WarlordsTerritoryDefinition("c", "C", ["a"])
        };
        Assert.Throws<ArgumentException>(() => new WarlordsMap("invalid", 1, territories));
    }

    [Fact]
    public void SnapshotIsDetachedFromLaterEventListChanges()
    {
        var game = new WarlordsGame(WarlordsMap.CreateStarterMap(), seed: 5);
        var first = game.Capture();
        game.March(new WarlordsMarch("azure-keep", "frostmarch", 2));
        Assert.Empty(first.Events);
        Assert.Single(game.Capture().Events);
    }
}
