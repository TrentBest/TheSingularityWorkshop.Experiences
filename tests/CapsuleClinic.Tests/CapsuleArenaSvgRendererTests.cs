using TheSingularityWorkshop.Experiences.CapsuleClinic;
using Xunit;
using System.Xml.Linq;

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
        Assert.Equal("svg", XDocument.Parse(svg).Root?.Name.LocalName);
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
    public void RenderSupportsCustomBoardDimensions()
    {
        var room = new AuthoritativePuzzleRoom("custom-board", width: 10, height: 20);
        Assert.True(room.JoinPlayer("alice", 101));
        Assert.True(room.JoinPlayer("bob", 202));
        Assert.True(room.StartMatch());

        var snapshot = CapsuleArenaSnapshot.Capture(room, "alice");
        var svg = CapsuleArenaSvgRenderer.Render(snapshot);

        Assert.Equal(10, snapshot.BoardWidth);
        Assert.Equal(20, snapshot.BoardHeight);
        Assert.Equal(400, CountOccurrences(svg, "stroke=\"#273949\""));
    }

    private static int CountOccurrences(string value, string fragment)
    {
        var count = 0;
        var offset = 0;
        while ((offset = value.IndexOf(fragment, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += fragment.Length;
        }
        return count;
    }

    [Fact]
    public void RenderRejectsSnapshotsThatDoNotDescribeFourPanes()
    {
        var snapshot = new CapsuleArenaSnapshot("alice", Array.Empty<CapsuleArenaPaneView>());

        Assert.Throws<ArgumentException>(() => CapsuleArenaSvgRenderer.Render(snapshot));
    }
}
