using TheSingularityWorkshop.Experiences.Warlords;
using TheSingularityWorkshop.Experiences.Warlords.Runtime;
using Xunit;

namespace TheSingularityWorkshop.Experiences.Warlords.Runtime.Tests;

public sealed class WarlordsMatchRuntimeTests
{
    [Fact]
    public void RuntimeStartsInCommandAndExposesDomainSnapshot()
    {
        using var runtime = new WarlordsMatchRuntime(WarlordsMap.CreateStarterMap(), seed: 17);

        Assert.Equal(WarlordsMatchRuntime.CommandState, runtime.LifecycleState);
        Assert.Equal(WarlordsPhase.Command, runtime.Snapshot.Phase);
        Assert.Equal(WarlordsKingdom.Azure, runtime.Snapshot.ActiveKingdom);
        Assert.NotEmpty(runtime.GetLegalMarches());
    }

    [Fact]
    public void DomainVictoryTransitionsFsmApiLifecycleToFinished()
    {
        using var runtime = new WarlordsMatchRuntime(CreateAzureVictoryMap(), seed: 17);

        var entry = runtime.March(new WarlordsMarch("azure-a", "crimson", 4));

        Assert.Equal(WarlordsBattleOutcome.AttackerVictory, entry.BattleOutcome);
        Assert.Equal(WarlordsKingdom.Azure, runtime.Snapshot.Winner);
        Assert.Equal(WarlordsPhase.Finished, runtime.Snapshot.Phase);
        Assert.Equal(WarlordsMatchRuntime.FinishedState, runtime.LifecycleState);
        Assert.Empty(runtime.GetLegalMarches());
        Assert.Throws<InvalidOperationException>(() => runtime.EndTurn());
    }

    [Fact]
    public void DisposeInvalidatesContextAndStopsFurtherUpdates()
    {
        var runtime = new WarlordsMatchRuntime(WarlordsMap.CreateStarterMap(), seed: 17);

        runtime.Dispose();
        runtime.Update();

        Assert.False(runtime.IsValid);
        Assert.Throws<ObjectDisposedException>(() =>
            runtime.March(new WarlordsMarch("azure-keep", "frostmarch", 1)));
    }

    private static WarlordsMap CreateAzureVictoryMap() => new("azure-victory", 1,
    [
        new("azure-a", "Azure A", ["azure-b", "azure-c", "crimson", "verdant", "amber"], WarlordsKingdom.Azure, true, 4),
        new("azure-b", "Azure B", ["azure-a"], WarlordsKingdom.Azure, true, 4),
        new("azure-c", "Azure C", ["azure-a"], WarlordsKingdom.Azure, true, 4),
        new("crimson", "Crimson", ["azure-a"], WarlordsKingdom.Crimson, true, 1),
        new("verdant", "Verdant", ["azure-a"], WarlordsKingdom.Verdant, true, 1),
        new("amber", "Amber", ["azure-a"], WarlordsKingdom.Amber, true, 1)
    ]);
}
