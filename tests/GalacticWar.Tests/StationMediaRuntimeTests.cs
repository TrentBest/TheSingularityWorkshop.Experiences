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
    public void DuplicateReceiptForSameObserverAndDeliveryIsRejected()
    {
        var runtime = CreateRuntime();
        runtime.RecordReceipt(new ObserverReceipt(Guid.NewGuid(), InitialDeliveryId, "mara-quill", new CampaignInstant(32), AcquisitionMode.Heard));
        Assert.Throws<InvalidOperationException>(() => runtime.RecordReceipt(new ObserverReceipt(Guid.NewGuid(), InitialDeliveryId, "mara-quill", new CampaignInstant(33), AcquisitionMode.Viewed)));
    }
}
