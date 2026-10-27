using TheSingularityWorkshop.Experiences.DroneConstruction;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class LogicalCommunicationRouterTests
{
    [Fact]
    public void RoutesOnlyToFreshSubscribersOfTheExactLaneInStableOrder()
    {
        var router = new LogicalCommunicationRouter();
        var laneA = new LogicalCommunicationChannelKey("region", "layer", "lane-a");
        var laneB = new LogicalCommunicationChannelKey("region", "layer", "lane-b");
        router.TrySubscribe(laneA, 30);
        router.TrySubscribe(laneA, 10);
        router.TrySubscribe(laneA, 20);
        router.TrySubscribe(laneB, 40);

        var message = Message(laneA, senderId: 20, createdTick: 4, expiresAtTick: 8);

        Assert.Equal(new ulong[] { 10, 30 }, router.GetRecipients(message, currentTick: 5));
        Assert.Empty(router.GetRecipients(message, currentTick: 9));
        Assert.Empty(router.GetRecipients(Message(laneB, 40, 4, 8), currentTick: 5));
    }

    [Fact]
    public void SubscriptionChangesAreExplicitAndRemoveEmptyChannels()
    {
        var router = new LogicalCommunicationRouter();
        var channel = new LogicalCommunicationChannelKey("region", "layer", "lane");
        Assert.False(router.TrySubscribe(channel, 0));
        Assert.True(router.TrySubscribe(channel, 1));
        Assert.False(router.TrySubscribe(channel, 1));
        Assert.Equal(1, router.SubscriptionCount);

        Assert.True(router.TryUnsubscribe(channel, 1));
        Assert.False(router.TryUnsubscribe(channel, 1));
        Assert.Equal(0, router.SubscriptionCount);
        Assert.Empty(router.GetSubscribers(channel));
    }

    [Fact]
    public void RecipientListIsSnapshotAndSenderDoesNotReceiveItsOwnMessage()
    {
        var router = new LogicalCommunicationRouter();
        var channel = new LogicalCommunicationChannelKey("region", "layer", "lane");
        router.TrySubscribe(channel, 1);
        router.TrySubscribe(channel, 2);

        var recipients = router.GetRecipients(Message(channel, 1, 0, 5), currentTick: 1);
        router.TryUnsubscribe(channel, 2);

        Assert.Equal(new ulong[] { 2 }, recipients);
        Assert.Empty(router.GetRecipients(Message(channel, 1, 0, 5), currentTick: 1));
    }

    private static CommunicationMessageEnvelope Message(
        LogicalCommunicationChannelKey channel, ulong senderId, long createdTick, long expiresAtTick) =>
        new(channel, senderId, sequence: 0, createdTick, expiresAtTick,
            messageKind: "position-changed", payloadBytes: 24, CommunicationPriority.Routine);
}
