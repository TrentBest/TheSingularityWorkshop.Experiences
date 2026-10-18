namespace TheSingularityWorkshop.Experiences.DominoStudio;

/// <summary>Compact editable grid using a per-cell bit mask for level occupancy.</summary>
/// <remarks>
/// <para>Each set bit represents a domino at that vertical level: bit 0 is the base level, bit 1 is the first overpass, and so on.</para>
/// <para>The height/occupancy field is separate from the orientation field, which stores one 1-based orientation code per cell and level.</para>
/// <para>Orientation code zero is reserved for empty levels; code 1 maps to the first angle, including a valid zero-radian angle.</para>
/// </remarks>
public sealed class DominoLayoutGrid
{
    private readonly ulong[] _occupancyMasks;
    private readonly int[] _orientations;
    private readonly double[] _angles;

    /// <summary>Creates a validated bitmask grid and its per-level orientation field.</summary>
    /// <param name="width">Number of cells on the X axis.</param>
    /// <param name="depth">Number of cells on the Z axis.</param>
    /// <param name="maximumLevels">Maximum levels per cell, from 1 through 64.</param>
    /// <param name="orientationRadians">Orientation lookup table in radians.</param>
    /// <param name="occupancyMasks">One 64-bit level occupancy mask per cell.</param>
    /// <param name="orientations">Flattened cell-major, level-minor orientation codes; zero marks an empty level.</param>
    public DominoLayoutGrid(int width, int depth, int maximumLevels, IReadOnlyList<double> orientationRadians,
        IEnumerable<ulong> occupancyMasks, IEnumerable<int> orientations)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (depth <= 0) throw new ArgumentOutOfRangeException(nameof(depth));
        if (maximumLevels <= 0 || maximumLevels > 64) throw new ArgumentOutOfRangeException(nameof(maximumLevels));
        ArgumentNullException.ThrowIfNull(orientationRadians);
        ArgumentNullException.ThrowIfNull(occupancyMasks);
        ArgumentNullException.ThrowIfNull(orientations);
        if (orientationRadians.Count == 0 || orientationRadians.Any(a => !double.IsFinite(a)))
            throw new ArgumentException("At least one finite orientation angle is required.", nameof(orientationRadians));

        Width = width;
        Depth = depth;
        MaximumLevels = maximumLevels;
        _angles = orientationRadians.ToArray();
        _occupancyMasks = occupancyMasks.ToArray();
        _orientations = orientations.ToArray();
        var cells = checked(width * depth);
        if (_occupancyMasks.Length != cells) throw new ArgumentException("Occupancy texture must contain width × depth masks.", nameof(occupancyMasks));
        if (_orientations.Length != checked(cells * maximumLevels))
            throw new ArgumentException("Orientation texture must contain width × depth × maximumLevels values.", nameof(orientations));

        var validMask = maximumLevels == 64 ? ulong.MaxValue : (1UL << maximumLevels) - 1UL;
        for (var cell = 0; cell < cells; cell++)
        {
            if ((_occupancyMasks[cell] & ~validMask) != 0)
                throw new ArgumentException("Occupancy mask sets a bit above maximumLevels.", nameof(occupancyMasks));
            for (var level = 0; level < maximumLevels; level++)
            {
                var occupied = (_occupancyMasks[cell] & (1UL << level)) != 0;
                var code = _orientations[cell * maximumLevels + level];
                if (occupied && (code < 1 || code > _angles.Length))
                    throw new ArgumentException("Every occupied level needs a valid one-based orientation code.", nameof(orientations));
                if (!occupied && code != 0)
                    throw new ArgumentException("Empty levels must use orientation code zero.", nameof(orientations));
            }
        }
    }

    /// <summary>Gets grid width in cells.</summary>
    public int Width { get; }
    /// <summary>Gets grid depth in cells.</summary>
    public int Depth { get; }
    /// <summary>Gets the configured maximum number of levels per cell.</summary>
    public int MaximumLevels { get; }
    /// <summary>Gets the total number of occupied domino slots.</summary>
    public int DominoCount => _occupancyMasks.Sum(mask => System.Numerics.BitOperations.PopCount(mask));
    /// <summary>Gets a defensive copy of the orientation lookup table in radians.</summary>
    public IReadOnlyList<double> OrientationRadians => Array.AsReadOnly((double[])_angles.Clone());
    /// <summary>Gets the occupancy bit mask for one cell.</summary>
    public ulong OccupancyMaskAt(int x, int z) => _occupancyMasks[Index(x, z)];
    /// <summary>Gets whether a particular level contains a domino.</summary>
    public bool IsOccupiedAtLevel(int x, int z, int level)
    {
        if ((uint)level >= (uint)MaximumLevels) throw new ArgumentOutOfRangeException(nameof(level));
        return (OccupancyMaskAt(x, z) & (1UL << level)) != 0;
    }
    /// <summary>Gets one plus the highest occupied level, or zero when the cell is empty.</summary>
    public int HeightAt(int x, int z)
    {
        var mask = OccupancyMaskAt(x, z);
        return mask == 0 ? 0 : 64 - System.Numerics.BitOperations.LeadingZeroCount(mask);
    }
    /// <summary>Gets the orientation code; zero means the level is empty.</summary>
    public int OrientationCodeAt(int x, int z, int level)
    {
        if ((uint)level >= (uint)MaximumLevels) throw new ArgumentOutOfRangeException(nameof(level));
        return _orientations[Index(x, z) * MaximumLevels + level];
    }
    /// <summary>Gets the angle in radians, or null for an empty level.</summary>
    public double? OrientationRadiansAt(int x, int z, int level)
    {
        var code = OrientationCodeAt(x, z, level);
        return code == 0 ? null : _angles[code - 1];
    }
    /// <summary>Copies occupancy masks.</summary>
    public ulong[] CopyOccupancyMasks() => (ulong[])_occupancyMasks.Clone();
    /// <summary>Copies orientation codes.</summary>
    public int[] CopyOrientations() => (int[])_orientations.Clone();

    private int Index(int x, int z)
    {
        if ((uint)x >= (uint)Width) throw new ArgumentOutOfRangeException(nameof(x));
        if ((uint)z >= (uint)Depth) throw new ArgumentOutOfRangeException(nameof(z));
        return checked(z * Width + x);
    }
}

/// <summary>Declared scale and height constraints for a proposed layout.</summary>
/// <param name="CellWidthMeters">Width of a grid cell.</param>
/// <param name="CellDepthMeters">Depth of a grid cell.</param>
/// <param name="DominoWidthMeters">Domino width.</param>
/// <param name="DominoDepthMeters">Domino depth.</param>
/// <param name="DominoHeightMeters">Height of one domino.</param>
/// <param name="MaximumBuildHeightMeters">Maximum allowed stack height.</param>
/// <param name="MinimumCenterSpacingMeters">Declared minimum center spacing.</param>
public sealed record DominoLayoutConstraints(double CellWidthMeters, double CellDepthMeters,
    double DominoWidthMeters, double DominoDepthMeters, double DominoHeightMeters,
    double MaximumBuildHeightMeters, double MinimumCenterSpacingMeters)
{
    /// <summary>Gets whether all constraint values are finite and in their valid ranges.</summary>
    public bool IsValid =>
        double.IsFinite(CellWidthMeters) && CellWidthMeters > 0 &&
        double.IsFinite(CellDepthMeters) && CellDepthMeters > 0 &&
        double.IsFinite(DominoWidthMeters) && DominoWidthMeters > 0 &&
        double.IsFinite(DominoDepthMeters) && DominoDepthMeters > 0 &&
        double.IsFinite(DominoHeightMeters) && DominoHeightMeters > 0 &&
        double.IsFinite(MaximumBuildHeightMeters) && MaximumBuildHeightMeters > 0 &&
        double.IsFinite(MinimumCenterSpacingMeters) && MinimumCenterSpacingMeters >= 0;
}

/// <summary>One structural issue found in a proposed layout.</summary>
/// <param name="Code">Stable machine-readable issue code.</param>
/// <param name="X">Grid X coordinate, or -1 for a global issue.</param>
/// <param name="Z">Grid Z coordinate, or -1 for a global issue.</param>
/// <param name="Message">Human-readable explanation.</param>
public sealed record DominoLayoutIssue(string Code, int X, int Z, string Message);

/// <summary>Performs inexpensive grid checks, not 3D collision or toppling simulation.</summary>
public static class DominoLayoutValidator
{
    /// <summary>Checks the layout against declared scale and spacing constraints.</summary>
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
