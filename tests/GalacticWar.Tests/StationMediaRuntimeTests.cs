using TheSingularityWorkshop.Experiences.GalacticWar;
using Xunit;

namespace TheSingularityWorkshop.Experiences.GalacticWar.Tests;

public sealed class StationMediaRuntimeTests
{
    private static readonly Guid SignalOccurrenceId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid AuthoredEventId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid InitialIssueId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    private static readonly Guid InitialDeliveryId = Guid.Parse("40000000-0000-0000-0000-000000000001");

    private static StationMediaRuntime CreateRuntime()
    {
        var runtime = new StationMediaRuntime("vesper-reach");
        runtime.RecordOccurrence(new SignalOccurrence(SignalOccurrenceId, AuthoredEventId, "frontier-observatory", new CampaignInstant(10)));
        runtime.IssueBroadcast(new BroadcastIssue(InitialIssueId, SignalOccurrenceId, "story-artifact:official-bulletin-v1", "source:frontier-observatory", new CampaignInstant(20)));
        runtime.ScheduleDelivery(new MediaDelivery(InitialDeliveryId, InitialIssueId, "lounge-display", "vesper-reach", new CampaignInstant(30)));
        return runtime;
    }

    [Fact]
    public void ChannelAvailabilityDoesNotImplyObserverReceipt()
    {
        var runtime = CreateRuntime();
        Assert.Single(runtime.GetChannelIssues("lounge-display", new CampaignInstant(30)));
        Assert.Empty(runtime.GetReceivedIssues("mara-quill", new CampaignInstant(30)));
        Assert.Empty(runtime.GetReceivedIssues("teren", new CampaignInstant(30)));
    }

    [Fact]
    public void ReceiptBelongsOnlyToTheObserverWhoActuallyAcquiredTheDelivery()
    {
        var runtime = CreateRuntime();
        runtime.RecordReceipt(new ObserverReceipt(Guid.NewGuid(), InitialDeliveryId, "mara-quill", new CampaignInstant(32), AcquisitionMode.Heard));
        Assert.Single(runtime.GetReceivedIssues("mara-quill", new CampaignInstant(32)));
        Assert.Empty(runtime.GetReceivedIssues("teren", new CampaignInstant(32)));
        Assert.Empty(runtime.GetReceivedIssues("mara-quill", new CampaignInstant(31)));
    }

    [Fact]
    public void CorrectionIsASecondIssueAndDoesNotRewriteTheOriginal()
    {
        var runtime = CreateRuntime();
        var correctionId = Guid.Parse("30000000-0000-0000-0000-000000000002");
        var correctionDeliveryId = Guid.Parse("40000000-0000-0000-0000-000000000002");
        runtime.IssueBroadcast(new BroadcastIssue(correctionId, SignalOccurrenceId, "story-artifact:official-bulletin-v2", "source:frontier-observatory", new CampaignInstant(60), InitialIssueId));
        runtime.ScheduleDelivery(new MediaDelivery(correctionDeliveryId, correctionId, "lounge-display", "vesper-reach", new CampaignInstant(65)));
        runtime.RecordReceipt(new ObserverReceipt(Guid.NewGuid(), InitialDeliveryId, "mara-quill", new CampaignInstant(32), AcquisitionMode.Heard));
        var before = runtime.GetReceivedIssues("mara-quill", new CampaignInstant(64));
        Assert.Single(before);
        Assert.Equal(InitialIssueId, before[0].IssueId);
        runtime.RecordReceipt(new ObserverReceipt(Guid.NewGuid(), correctionDeliveryId, "mara-quill", new CampaignInstant(66), AcquisitionMode.Viewed));
        var after = runtime.GetReceivedIssues("mara-quill", new CampaignInstant(66));
        Assert.Equal(2, after.Count);
        Assert.Equal(InitialIssueId, after[0].IssueId);
        Assert.Equal(correctionId, after[1].IssueId);
        Assert.Equal(InitialIssueId, after[1].CorrectsIssueId);
        Assert.Equal(2, runtime.Issues.Count);
    }

    [Fact]
    public void IndependentAccountReferencesTheSameOccurrenceWithoutSharingItsAudience()
    {
        var runtime = CreateRuntime();
        var independentIssueId = Guid.NewGuid();
        var independentDeliveryId = Guid.NewGuid();

        runtime.IssueBroadcast(new BroadcastIssue(
            independentIssueId,
            SignalOccurrenceId,
            "story-artifact:independent-account-v1",
            "source:independent-survey-ship",
            new CampaignInstant(34)));
        runtime.ScheduleDelivery(new MediaDelivery(
            independentDeliveryId,
            independentIssueId,
            "analyst-terminal",
            "vesper-reach",
            new CampaignInstant(35)));
        runtime.RecordReceipt(new ObserverReceipt(
            Guid.NewGuid(),
            independentDeliveryId,
            "ilyan-voss",
            new CampaignInstant(36),
            AcquisitionMode.Read));

        var independentAccount = Assert.Single(runtime.GetReceivedIssues("ilyan-voss", new CampaignInstant(36)));
        Assert.Equal(SignalOccurrenceId, independentAccount.SignalOccurrenceId);
        Assert.Equal("source:independent-survey-ship", independentAccount.AuthoredSourceId);
        Assert.NotEqual(runtime.Issues.Single(issue => issue.IssueId == InitialIssueId).AuthoredSourceId, independentAccount.AuthoredSourceId);
        Assert.Empty(runtime.GetReceivedIssues("mara-quill", new CampaignInstant(36)));
        Assert.Empty(runtime.GetReceivedIssues("teren", new CampaignInstant(36)));
    }

    [Fact]
    public void ReceiptBeforeChannelAvailabilityIsRejected()
    {
        var runtime = CreateRuntime();
        Assert.Throws<InvalidOperationException>(() => runtime.RecordReceipt(new ObserverReceipt(Guid.NewGuid(), InitialDeliveryId, "mara-quill", new CampaignInstant(29), AcquisitionMode.Heard)));
    }

    [Fact]
    public void BroadcastCannotBeIssuedBeforeItsSignalOccurrence()
    {
        var runtime = new StationMediaRuntime("vesper-reach");
        runtime.RecordOccurrence(new SignalOccurrence(SignalOccurrenceId, AuthoredEventId, "frontier-observatory", new CampaignInstant(10)));
        Assert.Throws<InvalidOperationException>(() => runtime.IssueBroadcast(new BroadcastIssue(InitialIssueId, SignalOccurrenceId, "story-artifact:official-bulletin-v1", "source:frontier-observatory", new CampaignInstant(9))));
    }

    [Fact]
    public void CorrectionMustBeLaterAndReferToTheSameSignalOccurrence()
    {
        var runtime = CreateRuntime();
        Assert.Throws<InvalidOperationException>(() => runtime.IssueBroadcast(new BroadcastIssue(Guid.NewGuid(), Guid.NewGuid(), "story-artifact:other-signal", "source:independent-network", new CampaignInstant(70), InitialIssueId)));
        Assert.Throws<InvalidOperationException>(() => runtime.IssueBroadcast(new BroadcastIssue(Guid.NewGuid(), SignalOccurrenceId, "story-artifact:early-correction", "source:frontier-observatory", new CampaignInstant(19), InitialIssueId)));
    }

    [Fact]
    public void DeliveryMustTargetThisStationAndAnExistingIssue()
    {
        var runtime = CreateRuntime();
        Assert.Throws<InvalidOperationException>(() => runtime.ScheduleDelivery(new MediaDelivery(Guid.NewGuid(), InitialIssueId, "ship-feed", "another-station", new CampaignInstant(30))));
        Assert.Throws<InvalidOperationException>(() => runtime.ScheduleDelivery(new MediaDelivery(Guid.NewGuid(), Guid.NewGuid(), "ship-feed", "vesper-reach", new CampaignInstant(30))));
    }

    [Fact]
    public void WatchingRequiresTheExactPriorPlayerPresentationButIgnoringDoesNotCreateOne()
    {
        var runtime = CreateRuntime();
        var presentationId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => runtime.RecordPlayerChoice(new StationStoryDecision(
            Guid.NewGuid(), "player", StationStoryChoice.Watch, new CampaignInstant(30))));
        Assert.Throws<InvalidOperationException>(() => runtime.RecordPlayerChoice(new StationStoryDecision(
            Guid.NewGuid(), "player", StationStoryChoice.Watch, new CampaignInstant(30), PresentationId: presentationId)));

        runtime.RecordReceipt(new ObserverReceipt(
            Guid.NewGuid(), InitialDeliveryId, "player", new CampaignInstant(31), AcquisitionMode.Viewed));
        runtime.RecordPresentation(new PlayerPresentation(presentationId, "player", InitialIssueId, new CampaignInstant(32)));

        Assert.Throws<InvalidOperationException>(() => runtime.RecordPlayerChoice(new StationStoryDecision(
            Guid.NewGuid(), "player", StationStoryChoice.Watch, new CampaignInstant(31), PresentationId: presentationId)));
        Assert.Throws<InvalidOperationException>(() => runtime.RecordPlayerChoice(new StationStoryDecision(
            Guid.NewGuid(), "another-player", StationStoryChoice.Watch, new CampaignInstant(33), PresentationId: presentationId)));

        runtime.RecordPlayerChoice(new StationStoryDecision(
            Guid.NewGuid(), "player", StationStoryChoice.Watch, new CampaignInstant(33), PresentationId: presentationId));

        var beforeIgnore = runtime.GetReceivedIssues("player", new CampaignInstant(33));
        runtime.RecordPlayerChoice(new StationStoryDecision(
            Guid.NewGuid(), "player", StationStoryChoice.Ignore, new CampaignInstant(34)));
        Assert.Equal(beforeIgnore, runtime.GetReceivedIssues("player", new CampaignInstant(34)));
        Assert.Equal(2, runtime.Decisions.Count);
    }

    [Fact]
    public void PlayerPresentationHasItsOwnClockAndRequiresViewedReceipt()
    {
        var runtime = CreateRuntime();
        var presentationId = Guid.NewGuid();
        Assert.Throws<InvalidOperationException>(() => runtime.RecordPresentation(new PlayerPresentation(
            presentationId, "player", InitialIssueId, new CampaignInstant(42))));

        runtime.RecordReceipt(new ObserverReceipt(
            Guid.NewGuid(), InitialDeliveryId, "player", new CampaignInstant(40), AcquisitionMode.Viewed));
        Assert.Empty(runtime.GetPresentedIssues("player", new CampaignInstant(41)));

        runtime.RecordPresentation(new PlayerPresentation(
            presentationId, "player", InitialIssueId, new CampaignInstant(42)));
        Assert.Empty(runtime.GetPresentedIssues("player", new CampaignInstant(41)));
        Assert.Single(runtime.GetPresentedIssues("player", new CampaignInstant(42)));
        runtime.RecordPlayerChoice(new StationStoryDecision(
            Guid.NewGuid(), "player", StationStoryChoice.Watch, new CampaignInstant(43)));
        Assert.Single(runtime.Decisions);
    }

    [Fact]
    public void AskingAnObserverIsRecordedAsChoiceNotAsAcquiredKnowledge()
    {
        var runtime = CreateRuntime();
        runtime.RecordPlayerChoice(new StationStoryDecision(
            Guid.NewGuid(), "player", StationStoryChoice.Ask, new CampaignInstant(25), "teren"));

        Assert.Single(runtime.Decisions);
        Assert.Equal(StationStoryChoice.Ask, runtime.Decisions.Single().Choice);
        Assert.Empty(runtime.GetReceivedIssues("player", new CampaignInstant(25)));
    }

    [Fact]
    public void StationSessionUsesItsOwnFsmHandleForArrivalAndDeparture()
    {
        using var session = new StationSessionLifecycle("vesper-shift-one");
        Assert.Equal("Arriving", session.CurrentState);

        session.MarkArrived();
        Assert.Equal("Active", session.CurrentState);

        session.Depart();
        Assert.Equal("Departed", session.CurrentState);
    }

    [Fact]
    public void StationSessionFsmInstancesDoNotShareLifecycleState()
    {
        using var first = new StationSessionLifecycle("vesper-shift-first");
        using var second = new StationSessionLifecycle("vesper-shift-second");

        first.MarkArrived();

        Assert.Equal("Active", first.CurrentState);
        Assert.Equal("Arriving", second.CurrentState);
    }

    [Fact]
    public void StationSessionRejectsInvalidLifecycleTransitions()
    {
        using var session = new StationSessionLifecycle("vesper-shift-invalid");

        Assert.Throws<InvalidOperationException>(() => session.Depart());
        session.MarkArrived();
        session.Depart();
        Assert.Throws<InvalidOperationException>(() => session.MarkArrived());
    }

    [Fact]
    public void DuplicateReceiptForSameObserverAndDeliveryIsRejected()
    {
        var runtime = CreateRuntime();
        runtime.RecordReceipt(new ObserverReceipt(Guid.NewGuid(), InitialDeliveryId, "mara-quill", new CampaignInstant(32), AcquisitionMode.Heard));
        Assert.Throws<InvalidOperationException>(() => runtime.RecordReceipt(new ObserverReceipt(Guid.NewGuid(), InitialDeliveryId, "mara-quill", new CampaignInstant(33), AcquisitionMode.Viewed)));
    }
}
