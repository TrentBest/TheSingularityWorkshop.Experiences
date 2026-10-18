namespace TheSingularityWorkshop.Experiences.DominoStudio;

/// <summary>Compact editable grid for stacked domino layouts.</summary>
/// <remarks>Height stores the number of occupied levels. Orientation slots are flattened as cellIndex * MaximumLevels + level.
/// Zero means empty; occupied slots use one-based codes into OrientationRadians, so a zero-radian orientation is unambiguous.</remarks>
public sealed class DominoLayoutGrid
{
    private readonly int[] _heights;
    private readonly int[] _orientations;
    private readonly double[] _angles;

    public DominoLayoutGrid(int width, int depth, int maximumLevels, IReadOnlyList<double> orientationRadians,
        IEnumerable<int> heights, IEnumerable<int> orientations)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (depth <= 0) throw new ArgumentOutOfRangeException(nameof(depth));
        if (maximumLevels <= 0) throw new ArgumentOutOfRangeException(nameof(maximumLevels));
        ArgumentNullException.ThrowIfNull(orientationRadians);
        ArgumentNullException.ThrowIfNull(heights);
        ArgumentNullException.ThrowIfNull(orientations);
        if (orientationRadians.Count == 0 || orientationRadians.Any(a => !double.IsFinite(a)))
            throw new ArgumentException("At least one finite orientation angle is required.", nameof(orientationRadians));

        Width = width; Depth = depth; MaximumLevels = maximumLevels;
        _angles = orientationRadians.ToArray();
        _heights = heights.ToArray();
        _orientations = orientations.ToArray();
        var cells = checked(width * depth);
        if (_heights.Length != cells) throw new ArgumentException("Height texture must contain width × depth values.", nameof(heights));
        if (_orientations.Length != checked(cells * maximumLevels))
            throw new ArgumentException("Orientation texture must contain width × depth × maximumLevels values.", nameof(orientations));

        for (var cell = 0; cell < cells; cell++)
        {
            if (_heights[cell] < 0 || _heights[cell] > maximumLevels)
                throw new ArgumentOutOfRangeException(nameof(heights), "Cell height is outside the configured level range.");
            for (var level = 0; level < maximumLevels; level++)
            {
                var code = _orientations[cell * maximumLevels + level];
                if (level < _heights[cell] && (code < 1 || code > _angles.Length))
                    throw new ArgumentException("Every occupied slot needs a valid one-based orientation code.", nameof(orientations));
                if (level >= _heights[cell] && code != 0)
                    throw new ArgumentException("Empty slots must use orientation code zero.", nameof(orientations));
            }
        }
    }

    public int Width { get; }
    public int Depth { get; }
    public int MaximumLevels { get; }
    public int DominoCount => _heights.Sum();
    public IReadOnlyList<double> OrientationRadians => Array.AsReadOnly((double[])_angles.Clone());
    public int HeightAt(int x, int z) => _heights[Index(x, z)];
    public int OrientationCodeAt(int x, int z, int level)
    {
        if ((uint)level >= (uint)MaximumLevels) throw new ArgumentOutOfRangeException(nameof(level));
        return _orientations[Index(x, z) * MaximumLevels + level];
    }
    public double? OrientationRadiansAt(int x, int z, int level)
    {
        var code = OrientationCodeAt(x, z, level);
        return code == 0 ? null : _angles[code - 1];
    }
    public int[] CopyHeights() => (int[])_heights.Clone();
    public int[] CopyOrientations() => (int[])_orientations.Clone();

    private int Index(int x, int z)
    {
        if ((uint)x >= (uint)Width) throw new ArgumentOutOfRangeException(nameof(x));
        if ((uint)z >= (uint)Depth) throw new ArgumentOutOfRangeException(nameof(z));
        return checked(z * Width + x);
    }
}

/// <summary>Declared scale and height constraints for a proposed layout.</summary>
public sealed record DominoLayoutConstraints(double CellWidthMeters, double CellDepthMeters,
    double DominoWidthMeters, double DominoDepthMeters, double DominoHeightMeters,
    double MaximumBuildHeightMeters, double MinimumCenterSpacingMeters)
{
    public bool IsValid =>
        double.IsFinite(CellWidthMeters) && CellWidthMeters > 0 &&
        double.IsFinite(CellDepthMeters) && CellDepthMeters > 0 &&
        double.IsFinite(DominoWidthMeters) && DominoWidthMeters > 0 &&
        double.IsFinite(DominoDepthMeters) && DominoDepthMeters > 0 &&
        double.IsFinite(DominoHeightMeters) && DominoHeightMeters > 0 &&
        double.IsFinite(MaximumBuildHeightMeters) && MaximumBuildHeightMeters > 0 &&
        double.IsFinite(MinimumCenterSpacingMeters) && MinimumCenterSpacingMeters >= 0;
}

public sealed record DominoLayoutIssue(string Code, int X, int Z, string Message);

/// <summary>Performs inexpensive grid checks, not 3D collision or toppling simulation.</summary>
public static class DominoLayoutValidator
{
    public static IReadOnlyList<DominoLayoutIssue> Validate(DominoLayoutGrid layout, DominoLayoutConstraints constraints)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(constraints);
        if (!constraints.IsValid) throw new ArgumentException("Layout constraints must be finite and positive (spacing may be zero).", nameof(constraints));

        var issues = new List<DominoLayoutIssue>();
        var spacing = constraints.MinimumCenterSpacingMeters;
        if (spacing > Math.Min(constraints.CellWidthMeters, constraints.CellDepthMeters))
            issues.Add(new("spacing-exceeds-cell", -1, -1, "Minimum spacing exceeds the smaller grid-cell dimension."));
        if (spacing < Math.Min(constraints.DominoWidthMeters, constraints.DominoDepthMeters))
            issues.Add(new("spacing-below-footprint", -1, -1, "Spacing is below the minimum domino footprint; overlap needs 3D validation."));

        for (var z = 0; z < layout.Depth; z++)
        for (var x = 0; x < layout.Width; x++)
        {
            var height = layout.HeightAt(x, z) * constraints.DominoHeightMeters;
            if (height > constraints.MaximumBuildHeightMeters)
                issues.Add(new("maximum-height-exceeded", x, z, $"Stack height {height:R} m exceeds maximum {constraints.MaximumBuildHeightMeters:R} m."));
        }
        return issues.AsReadOnly();
    }
}