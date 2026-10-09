using TheSingularityWorkshop.Experiences.StoryForge;

namespace TheSingularityWorkshop.Experiences.StoryForge.Tests;

public sealed class TemporalModelTests
{
    [Fact]
    public void TimelineCreatesCoordinatesInItsOwnDomain()
    {
        var timeline = new StoryTimeline("world", "World History", "year", 100);
        var time = timeline.At(250);

        Assert.Equal("world", time.TimelineId);
        Assert.Equal(250, time.Value);
        Assert.True(timeline.Contains(time));
        Assert.False(timeline.Contains(new StoryTime("publication", 250)));
    }

    [Fact]
    public void TimelineRequiresPositiveTickInterval()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new StoryTimeline("world", "World", "year", 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new StoryTimeline("world", "World", "year", -1));
    }

    [Fact]
    public void RangeIncludesStartAndExcludesEnd()
    {
        var timeline = new StoryTimeline("world", "World History", "year", 1);
        var range = new StoryTimeRange(timeline.At(10), timeline.At(20));

        Assert.True(range.Contains(timeline.At(10)));
        Assert.True(range.Contains(timeline.At(19.999m)));
        Assert.False(range.Contains(timeline.At(20)));
        Assert.Equal(10, range.Duration);
    }

    [Fact]
    public void RangeRejectsDifferentTimelines()
    {
        var first = new StoryTimeline("world", "World", "year", 1);
        var second = new StoryTimeline("publication", "Publication", "chapter", 1);

        Assert.Throws<ArgumentException>(() => new StoryTimeRange(first.At(0), second.At(10)));
    }

    [Fact]
    public void RangeRejectsReversedEndpoints()
    {
        var timeline = new StoryTimeline("world", "World", "year", 1);
        Assert.Throws<ArgumentOutOfRangeException>(() => new StoryTimeRange(timeline.At(20), timeline.At(10)));
    }

    [Fact]
    public void MarkerRetainsTargetAndTime()
    {
        var timeline = new StoryTimeline("world", "World", "year", 1);
        var marker = new TimelineMarker("m1", "First Contact", timeline.At(42), "event-1", "event");

        Assert.Equal("event-1", marker.TargetId);
        Assert.Equal("event", marker.TargetKind);
        Assert.Equal(timeline.At(42), marker.Time);
    }

    [Fact]
    public void PublicationCoverageSeparatesSequenceFromStoryTime()
    {
        var timeline = new StoryTimeline("world", "World", "year", 1);
        var coverage = new PublicationCoverage("saga", "chapter-4", "chapter", 3,
            new StoryTimeRange(timeline.At(5), timeline.At(10)));

        Assert.Equal(3, coverage.Sequence);
        Assert.Equal(5, coverage.StoryRange.Start.Value);
        Assert.Equal(10, coverage.StoryRange.End.Value);
    }

    [Fact]
    public void PublishedEditionCopiesCoverageAndExposesReadOnlySnapshot()
    {
        var timeline = new StoryTimeline("world", "World", "year", 1);
        var original = new List<PublicationCoverage>
        {
            new("saga", "chapter-1", "chapter", 0, new StoryTimeRange(timeline.At(0), timeline.At(10)))
        };
        var edition = new PublishedEdition("edition-1", "saga", "1.0", "sha256:abc", original);
        original.Clear();

        Assert.Single(edition.Coverage);
        Assert.Equal("sha256:abc", edition.ContentHash);
        Assert.Throws<NotSupportedException>(() => ((IList<PublicationCoverage>)edition.Coverage).Clear());
    }

    [Fact]
    public void CoverageSequenceCannotBeNegative()
    {
        var timeline = new StoryTimeline("world", "World", "year", 1);
        var range = new StoryTimeRange(timeline.At(0), timeline.At(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PublicationCoverage("saga", "c1", "chapter", -1, range));
    }
}
