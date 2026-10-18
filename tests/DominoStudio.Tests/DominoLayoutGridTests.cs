using TheSingularityWorkshop.Experiences.DominoStudio;

namespace TheSingularityWorkshop.Experiences.DominoStudio.Tests;

public sealed class DominoLayoutGridTests
{
    [Fact]
    public void StackedLevelsHaveIndependentOrientationsAndEmptySlotsUseZero()
    {
        var grid = new DominoLayoutGrid(1, 1, 3, [0d, Math.PI / 2], [2], [1, 2, 0]);
        Assert.Equal(2, grid.DominoCount);
        Assert.Equal(0d, grid.OrientationRadiansAt(0, 0, 0));
        Assert.Equal(Math.PI / 2, grid.OrientationRadiansAt(0, 0, 1));
        Assert.Null(grid.OrientationRadiansAt(0, 0, 2));
    }

    [Fact]
    public void RejectsMissingOccupiedOrientationAndNonzeroEmptyOrientation()
    {
        Assert.Throws<ArgumentException>(() => new DominoLayoutGrid(1, 1, 2, [0d], [1], [0, 0]));
        Assert.Throws<ArgumentException>(() => new DominoLayoutGrid(1, 1, 2, [0d], [0], [1, 0]));
    }

    [Fact]
    public void CopiesInputBuffers()
    {
        var heights = new[] { 1 };
        var rotations = new[] { 1, 0 };
        var grid = new DominoLayoutGrid(1, 1, 2, [0d], heights, rotations);
        heights[0] = 0; rotations[0] = 0;
        Assert.Equal(1, grid.HeightAt(0, 0));
        Assert.Equal(1, grid.OrientationCodeAt(0, 0, 0));
    }

    [Fact]
    public void ValidatorEnforcesHeightAndReportsSpacingConstraints()
    {
        var grid = new DominoLayoutGrid(1, 1, 3, [0d], [3], [1, 1, 1]);
        var constraints = new DominoLayoutConstraints(0.1, 0.1, 0.02, 0.05, 0.04, 0.08, 0.01);
        var issues = DominoLayoutValidator.Validate(grid, constraints);
        Assert.Contains(issues, x => x.Code == "maximum-height-exceeded");
        Assert.Contains(issues, x => x.Code == "spacing-below-footprint");
    }

    [Fact]
    public void RejectsInvalidConstraints()
    {
        var grid = new DominoLayoutGrid(1, 1, 1, [0d], [0], [0]);
        Assert.Throws<ArgumentException>(() => DominoLayoutValidator.Validate(grid,
            new DominoLayoutConstraints(0, 1, 1, 1, 1, 1, 0)));
    }
}