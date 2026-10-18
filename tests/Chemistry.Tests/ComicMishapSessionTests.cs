using Xunit;
using TheSingularityWorkshop.Experiences.Chemistry;

namespace TheSingularityWorkshop.Experiences.Chemistry.Tests;

public sealed class ComicMishapSessionTests
{
    private static ExperimentSnapshot Snapshot() =>
        new("chemistry-tutorial-water", 3, "{\"reactants\":[\"H2\",\"O2\"],\"products\":[\"H2O\"]}");

    [Theory]
    [InlineData(MishapTriggerKind.ScriptedTutorial)]
    [InlineData(MishapTriggerKind.UnknownOutcome)]
    public void ComicMishapPreservesExperimentAndRecoversWithoutPermanentHarm(MishapTriggerKind trigger)
    {
        var snapshot = Snapshot();
        var session = new ComicMishapSession(snapshot, trigger);

        session.Present();
        Assert.Equal(MishapStage.Presented, session.Stage);
        Assert.Same(snapshot, session.Experiment);
        Assert.True(session.ExperimentPreserved);
        Assert.False(session.PlayerIsDead);
        Assert.False(session.HasPermanentInjury);

        session.Recover();
        Assert.Equal(MishapStage.Recovered, session.Stage);
        Assert.Same(snapshot, session.Experiment);
        Assert.True(session.ExperimentPreserved);
        Assert.False(session.PlayerIsDead);
        Assert.False(session.HasPermanentInjury);

        session.ReviewEvidence();
        Assert.Equal(MishapStage.EvidenceReviewed, session.Stage);
        Assert.Contains(trigger == MishapTriggerKind.ScriptedTutorial
            ? "evidence:scripted-comic-event"
            : "evidence:outcome-unknown", session.Timeline);
    }

    [Fact]
    public void ModelReportedIncidentRequiresAnExplicitModelEventKey()
    {
        Assert.Throws<ArgumentException>(() =>
            new ComicMishapSession(Snapshot(), MishapTriggerKind.ModelReported));

        var session = new ComicMishapSession(Snapshot(), MishapTriggerKind.ModelReported, "thermal-model.threshold-crossed");
        session.Present();
        session.Recover();
        session.ReviewEvidence();

        Assert.Contains("evidence:model-event:thermal-model.threshold-crossed", session.Timeline);
    }

    [Fact]
    public void LifecycleRejectsSkippingRecovery()
    {
        var session = new ComicMishapSession(Snapshot(), MishapTriggerKind.ScriptedTutorial);

        Assert.Throws<InvalidOperationException>(() => session.Recover());
        session.Present();
        Assert.Throws<InvalidOperationException>(() => session.ReviewEvidence());
    }

    [Fact]
    public void NonModelTriggersCannotClaimModelEventProvenance()
    {
        Assert.Throws<ArgumentException>(() =>
            new ComicMishapSession(Snapshot(), MishapTriggerKind.UnknownOutcome, "invented-explosion"));
    }
}
