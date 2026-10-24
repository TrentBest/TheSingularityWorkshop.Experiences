using Xunit;
using TheSingularityWorkshop.Experiences.Catalog;
using TheSingularityWorkshop.FSM_Serialization;

namespace TheSingularityWorkshop.Experiences.Catalog.Tests;

public sealed class ExperienceRoomCatalogTests
{
    [Fact]
    public void FluidDynamicsCatalog_HasValidUniqueRoomsAndResolvableLinks()
    {
        var catalog = FluidDynamicsLaboratoryCatalog.Create();

        Assert.Empty(catalog.Validate());
        Assert.Equal(7, catalog.Rooms.Count);
        Assert.Equal(Enumerable.Range(1, 7), catalog.Rooms.Select(room => room.Sequence));
    }

    [Fact]
    public void BinaryRoundTrip_PreservesCatalogSemantics()
    {
        var original = FluidDynamicsLaboratoryCatalog.Create();
        using var stream = new MemoryBinaryStream();

        original.Pack(stream);
        stream.Position = 0;

        var restored = new ExperienceRoomCatalog();
        restored.Unpack(stream);

        Assert.Empty(restored.Validate());
        Assert.Equal(original.ExperienceKey, restored.ExperienceKey);
        Assert.Equal(original.Status, restored.Status);
        Assert.Equal(original.Rooms.Count, restored.Rooms.Count);
        for (var i = 0; i < original.Rooms.Count; i++)
        {
            Assert.Equal(original.Rooms[i].Key, restored.Rooms[i].Key);
            Assert.Equal(original.Rooms[i].Sequence, restored.Rooms[i].Sequence);
            Assert.Equal(original.Rooms[i].Title, restored.Rooms[i].Title);
            Assert.Equal(original.Rooms[i].EvidenceRule, restored.Rooms[i].EvidenceRule);
            Assert.Equal(original.Rooms[i].NextRooms, restored.Rooms[i].NextRooms);
        }
    }

    [Fact]
    public void Pack_RejectsNavigationToMissingRoom()
    {
        var catalog = FluidDynamicsLaboratoryCatalog.Create();
        catalog.Rooms[0].NextRooms.Add("missing-room");

        using var stream = new MemoryBinaryStream();
        Assert.Throws<InvalidOperationException>(() => catalog.Pack(stream));
    }

    [Fact]
    public void Unpack_RejectsTruncatedPayload()
    {
        using var stream = new MemoryBinaryStream([0x45, 0x43, 0x41, 0x54]);
        var catalog = new ExperienceRoomCatalog();

        Assert.Throws<EndOfStreamException>(() => catalog.Unpack(stream));
    }
}
