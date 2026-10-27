using TheSingularityWorkshop.Experiences.DroneConstruction;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class LogicalCommunicationContractsTests
{
    [Fact]
    public void ChannelIdentityIsStableByRegionLayerAndLane()
    {
        var first = new LogicalCommunicationChannelKey("region-a", "altitude-2", "lane-east");
        var same = new LogicalCommunicationChannelKey("region-a", "altitude-2", "lane-east");
        var otherLane = new LogicalCommunicationChannelKey("region-a", "altitude-2", "lane-west");
        var otherLayer = new LogicalCommunicationChannelKey("region-a", "altitude-3", "lane-east");

        Assert.Equal(first, same);
        Assert.NotEqual(first, otherLane);
        Assert.NotEqual(first, otherLayer);
    }

    [Fact]
    public void MessageFreshnessIncludesCreationAndExpiryTicks()
    {
        var message = new CommunicationMessageEnvelope(
            new LogicalCommunicationChannelKey("region-a", "layer-a", "lane-a"),
            senderId: 1, sequence: 0, createdTick: 10, expiresAtTick: 12,
            messageKind: "position-changed", payloadBytes: 24, CommunicationPriority.Routine);

        Assert.False(message.IsFreshAt(9));
        Assert.True(message.IsFreshAt(10));
        Assert.True(message.IsFreshAt(12));
        Assert.False(message.IsFreshAt(13));
    }

    [Fact]
    public void EnvelopeRejectsInvalidLifetimeAndMissingChannelIdentity()
    {
        Assert.Throws<ArgumentException>(() => new LogicalCommunicationChannelKey("", "layer", "lane"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CommunicationMessageEnvelope(
            new LogicalCommunicationChannelKey("region", "layer", "lane"),
            senderId: 1, sequence: 0, createdTick: 4, expiresAtTick: 3,
            messageKind: "position-changed", payloadBytes: 1, CommunicationPriority.Routine));
    }

    [Fact]
    public void EnvelopeRejectsZeroSenderAndNegativePayloadSize()
    {
        var channel = new LogicalCommunicationChannelKey("region", "layer", "lane");
        Assert.Throws<ArgumentOutOfRangeException>(() => new CommunicationMessageEnvelope(
            channel, senderId: 0, sequence: 0, createdTick: 0, expiresAtTick: 1,
            messageKind: "state", payloadBytes: 1, CommunicationPriority.Routine));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CommunicationMessageEnvelope(
            channel, senderId: 1, sequence: 0, createdTick: 0, expiresAtTick: 1,
            messageKind: "state", payloadBytes: -1, CommunicationPriority.Routine));
    }
}
