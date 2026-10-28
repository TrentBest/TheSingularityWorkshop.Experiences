using TheSingularityWorkshop.Experiences.CapsuleClinic;
using Xunit;

namespace TheSingularityWorkshop.Experiences.CapsuleClinic.Tests;

public sealed class CapsuleArenaGuiAdapterTests
{
    [Fact]
    public void BuildProjectsAuthoritativeSnapshotAndDeclaresStableIntentIds()
    {
        using var session = new CapsuleClinicLocalSession("gui-adapter", "local", 731, "opponent", 732);
        var snapshot = session.CaptureSnapshot();
        var tree = CapsuleArenaGuiAdapter.Build(snapshot);

        Assert.Equal("capsule-clinic-arena", tree.Id);
        Assert.Equal("local", tree.Properties["localPlayerId"]);
        Assert.Equal(snapshot.BoardWidth.ToString(System.Globalization.CultureInfo.InvariantCulture), tree.Properties["boardWidth"]);

        var localPane = tree.Find("capsule-pane-local");
        Assert.Equal("local", localPane.Properties["playerId"]);
        Assert.Equal("local", localPane.Find("capsule-pane-local-player").Text);

        Assert.Equal(CapsuleClinicIntents.MoveLeft, tree.Find("capsule-move-left").Properties["onClickCommand"]);
        Assert.Equal(CapsuleClinicIntents.MoveRight, tree.Find("capsule-move-right").Properties["onClickCommand"]);
        Assert.Equal(CapsuleClinicIntents.Rotate, tree.Find("capsule-rotate").Properties["onClickCommand"]);
        Assert.Equal(CapsuleClinicIntents.SoftDrop, tree.Find("capsule-soft-drop").Properties["onClickCommand"]);
        Assert.Equal(CapsuleClinicIntents.HardDrop, tree.Find("capsule-hard-drop").Properties["onClickCommand"]);
    }

    [Fact]
    public void BuildReflectsOnlyTheSelectedPanesFromTheSuppliedSnapshot()
    {
        using var room = new AuthoritativePuzzleRoom("gui-adapter");
        for (var index = 1; index <= 8; index++)
            Assert.True(room.JoinPlayer($"p{index}", index));
        Assert.True(room.StartMatch());

        var snapshot = CapsuleArenaSnapshot.Capture(room, "p1");
        var tree = CapsuleArenaGuiAdapter.Build(snapshot);

        Assert.Equal(new[] { "p1", "p2", "p3", "p4" },
            new[]
            {
                tree.Find("capsule-pane-local").Properties["playerId"],
                tree.Find("capsule-pane-comparison-1").Properties["playerId"],
                tree.Find("capsule-pane-comparison-2").Properties["playerId"],
                tree.Find("capsule-pane-comparison-3").Properties["playerId"]
            });
        Assert.False(tree.TryFind("p8", out _));
    }
}
