using TheSingularityWorkshop.Experiences.StoryForge;
using Xunit;

namespace TheSingularityWorkshop.Experiences.StoryForge.Tests;

public sealed class TemporalQueryEngineTests
{
    private static readonly StoryTimeline World = new("world", "World History", "year", 1);

    [Fact]
    public void QueryAtReturnsEventsOnlyFromTheSelectedTimelineAndCoordinate()
    {
        var events = new[]
        {
            new StoryEvent("e1", "First Contact", World.At(42), ["human", "visitor"]),
            new StoryEvent("e2", "Aftermath", World.At(43), ["human"]),
            new StoryEvent("e3", "Publication", new StoryTime("publication", 42), ["human"])
        };

        var result = new TemporalQueryEngine(events).QueryAt(new TemporalQuery(World.At(42)));

        Assert.Single(result.Events);
        Assert.Equal("e1", result.Events[0].Id);
    }

    [Fact]
    public void QueryAtReturnsStateFactsValidAtTheSelectedCoordinate()
    {
        var facts = new[]
        {
            new EntityStateFact("f1", "captain", "location", "bridge",
                new StoryTimeRange(World.At(10), World.At(20))),
            new EntityStateFact("f2", "captain", "location", "medbay",
                new StoryTimeRange(World.At(20), World.At(30)))
        };

        var engine = new TemporalQueryEngine(stateFacts: facts);

        Assert.Equal("bridge", Assert.Single(engine.QueryAt(new TemporalQuery(World.At(19))).StateFacts).Value);
        Assert.Equal("medbay", Assert.Single(engine.QueryAt(new TemporalQuery(World.At(20))).StateFacts).Value);
    }

    [Fact]
    public void QueryCanFocusOnOneEntity()
    {
        var events = new[]
        {
            new StoryEvent("e1", "First Contact", World.At(42), ["human", "visitor"]),
            new StoryEvent("e2", "Signal detected", World.At(42), ["observer"])
        };

        var result = new TemporalQueryEngine(events).QueryAt(new TemporalQuery(World.At(42), "visitor"));

        Assert.Single(result.Events);
        Assert.Equal("e1", result.Events[0].Id);
    }

    [Fact]
    public void QueryExcludesProposalsWhenRequested()
    {
        var events = new[]
        {
            new StoryEvent("confirmed", "Confirmed", World.At(42)),
            new StoryEvent("proposal", "Maybe", World.At(42), certainty: StoryCertainty.Proposed)
        };

        var result = new TemporalQueryEngine(events).QueryAt(new TemporalQuery(World.At(42), includeProposals: false));

        Assert.Single(result.Events);
        Assert.Equal("confirmed", result.Events[0].Id);
    }

    [Fact]
    public void QueryReportsConflictingValuesInsteadOfChoosingCanonSilently()
    {
        var facts = new[]
        {
            new EntityStateFact("f1", "captain", "location", "bridge",
                new StoryTimeRange(World.At(10), World.At(20))),
            new EntityStateFact("f2", "captain", "location", "medbay",
                new StoryTimeRange(World.At(10), World.At(20)))
        };

        var result = new TemporalQueryEngine(stateFacts: facts).QueryAt(new TemporalQuery(World.At(15)));

        Assert.Equal(2, result.StateFacts.Count);
        Assert.Contains("captain/location", result.ContestedProperties);
    }

    [Fact]
    public void QueryRetainsProvenanceAndCopiesInputCollections()
    {
        var provenance = new StoryProvenance("chapter-1", "Established in the opening chapter", "paragraph-8");
        var source = new List<StoryEvent>
        {
            new("e1", "First Contact", World.At(42), provenance: [provenance])
        };
        var engine = new TemporalQueryEngine(source);
        source.Clear();

        var result = engine.QueryAt(new TemporalQuery(World.At(42)));

        Assert.Single(result.Events);
        Assert.Equal("chapter-1", Assert.Single(result.Events[0].Provenance).SourceId);
        Assert.Equal("paragraph-8", result.Events[0].Provenance[0].Locator);
    }

    [Fact]
    public void DuplicateIdentifiersAreRejected()
    {
        var events = new[]
        {
            new StoryEvent("same", "First", World.At(1)),
            new StoryEvent("same", "Second", World.At(2))
        };

        Assert.Throws<ArgumentException>(() => new TemporalQueryEngine(events));
    }
}
