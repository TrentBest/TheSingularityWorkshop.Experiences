using Xunit;

namespace TheSingularityWorkshop.Experiences.DominoStudio.Tests;

public sealed class DominoLayoutProposalTests
{
    [Fact]
    public void RequiresImageDigestVersionFrameAndExplicitInterpretation()
    {
        var grid = new DominoLayoutGrid(1, 1, 1, [0d], [1UL], [1]);
        var valid = new DominoLayoutProposal(new string('a', 64), "threshold-grid", "1.0.0",
            "world-xy-v1", 0.04, 0.04, "top-down", 0.7, grid);
        Assert.True(valid.IsValid);
        Assert.False(valid with { Confidence = 1.1 }.IsValid);
        Assert.False(valid with { Interpretation = "" }.IsValid);
        Assert.False(valid with { SourceImageSha256 = "not-a-digest" }.IsValid);
    }
}
