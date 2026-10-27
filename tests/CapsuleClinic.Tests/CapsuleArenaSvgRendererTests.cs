using TheSingularityWorkshop.Experiences.CapsuleClinic;
using Xunit;

namespace TheSingularityWorkshop.Experiences.CapsuleClinic.Tests;

public sealed class CapsuleArenaSvgRendererTests
{
    [Fact]
    public void RenderProducesStandaloneAccessibleSvgWithFourPanesAndWiderStandingsRail()
    {
        var room = new AuthoritativePuzzleRoom("svg-arena");
        Assert.True(room.JoinPlayer("alice", 101));
        Assert.True(room.JoinPlayer("bob", 202));
        Assert.True(room.JoinPlayer("carol", 303));
        Assert.True(room.StartMatch());

        var svg = CapsuleArenaSvgRenderer.Render(CapsuleArenaSnapshot.Capture(room, "bob"));

        Assert.Contains("<svg xmlns=\"http://www.w3.org/2000/svg\"", svg);
        Assert.Contains("<title id=\"arena-title\">Capsule Clinic arena</title>", svg);
        Assert.Contains("STANDINGS", svg);
        Assert.Contains("width=\"52\"", svg);
        Assert.Contains("YOU", svg);
        Assert.Contains("RANK #", svg);
        Assert.Contains("alice", svg);
        Assert.Contains("bob", svg);
        Assert.Contains("carol", svg);
        Assert.Contains("WAITING FOR CONTENDER", svg);
        Assert.EndsWith("</svg>", svg.TrimEnd());
    }

    [Fact]
    public void RenderEscapesPlayerControlledTextBeforeWritingSvgMarkup()
    {
        var room = new AuthoritativePuzzleRoom("svg-arena");
        Assert.True(room.JoinPlayer("alice<svg>", 101));
        Assert.True(room.JoinPlayer("bob", 202));
        Assert.True(room.StartMatch());

        var svg = CapsuleArenaSvgRenderer.Render(CapsuleArenaSnapshot.Capture(room, "bob"));

        Assert.Contains("alice&lt;svg&gt;", svg);
        Assert.DoesNotContain("alice<svg>", svg);
    }

    [Fact]
    public void RenderRejectsSnapshotsThatDoNotDescribeFourPanes()
    {
        var snapshot = new CapsuleArenaSnapshot("alice", Array.Empty<CapsuleArenaPaneView>());

        Assert.Throws<ArgumentException>(() => CapsuleArenaSvgRenderer.Render(snapshot));
    }
}
