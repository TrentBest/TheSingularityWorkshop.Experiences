using TheSingularityWorkshop.Experiences.CapsuleClinic;
using TheSingularityWorkshop.Experiences.CapsuleClinic.VisualizeIntegration;
using TheSingularityWorkshop.Visualize;
using Xunit;

namespace TheSingularityWorkshop.Experiences.CapsuleClinic.VisualizeIntegration.Tests;

public sealed class CapsuleClinicRenderFrameFactoryTests
{
    [Theory]
    [InlineData(7, 12)]
    [InlineData(8, 3)]
    [InlineData(9, 4)]
    [InlineData(10, 6)]
    public void CapsuleSilhouettesHaveExpectedPolygonAndTriangleCounts(int geometryId, int sides)
    {
        using var room = new AuthoritativePuzzleRoom("shape-test");
        Assert.True(room.JoinPlayer("alice", 101));
        Assert.True(room.JoinPlayer("bob", 202));
        Assert.True(room.StartMatch());
        var snapshot = CapsuleArenaSnapshot.Capture(room, "alice");

        var presentation = CapsuleClinicRenderFrameFactory.Create(snapshot);

        Assert.True(presentation.ResourceCatalog.TryGetGeometry(new VisualizerResourceId((ulong)geometryId), out var geometry));
        Assert.NotNull(geometry);
        Assert.Equal(sides + 1, geometry!.Vertices.Count); // center plus one vertex per polygon side
        Assert.Equal(sides * 3, geometry.Indices.Count); // triangle fan
        Assert.All(geometry.Indices, index => Assert.True(index < geometry.Vertices.Count));
        Assert.Equal(presentation.BaseCellCommands + presentation.ActiveSegments, presentation.CommandCount);
    }

    [Fact]
    public void EmptyCellGeometryRemainsASquareAndDoesNotShareCapsuleSilhouettes()
    {
        using var room = new AuthoritativePuzzleRoom("empty-cell-test");
        Assert.True(room.JoinPlayer("alice", 101));
        Assert.True(room.JoinPlayer("bob", 202));
        Assert.True(room.StartMatch());
        var presentation = CapsuleClinicRenderFrameFactory.Create(CapsuleArenaSnapshot.Capture(room, "alice"));

        Assert.True(presentation.ResourceCatalog.TryGetGeometry(new VisualizerResourceId(1), out var empty));
        Assert.NotNull(empty);
        Assert.Equal(4, empty!.Vertices.Count);
        Assert.Equal(6, empty.Indices.Count);
        Assert.Equal(0, presentation.OccupiedCells);
    }

    [Fact]
    public void ActiveCapsuleDrawCommandsUseTheSilhouetteForEachSegmentColor()
    {
        using var room = new AuthoritativePuzzleRoom("active-shape-test");
        Assert.True(room.JoinPlayer("alice", 101));
        Assert.True(room.JoinPlayer("bob", 202));
        Assert.True(room.StartMatch());
        var snapshot = CapsuleArenaSnapshot.Capture(room, "alice");
        var active = Assert.IsType<ActiveCapsuleSnapshot>(snapshot.Panes[0].Player!.Active);

        var presentation = CapsuleClinicRenderFrameFactory.Create(snapshot);

        Assert.Equal(2, presentation.ActiveSegments);
        var activeCommands = presentation.Frame.Commands.Skip(presentation.BaseCellCommands).ToArray();
        Assert.Equal(2, activeCommands.Length);

        static ulong GeometryId(CapsuleColor color) => color switch
        {
            CapsuleColor.Red => 7,
            CapsuleColor.Blue => 8,
            CapsuleColor.Yellow => 9,
            CapsuleColor.Purple => 10,
            _ => throw new ArgumentOutOfRangeException(nameof(color))
        };

        Assert.Equal(GeometryId(active.Pair.PivotColor), activeCommands[0].Geometry.Value);
        Assert.Equal(GeometryId(active.Pair.PartnerColor), activeCommands[1].Geometry.Value);
    }
}
