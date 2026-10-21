namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>A rectangular target grid whose cells track required and committed block counts.</summary>
public sealed class BuildGrid
{
    private readonly BuildCell[] _cells;

    /// <summary>Creates a rectangular grid from a required-height function.: BuildGrid.</summary>
    public BuildGrid(int width, int height, Func<int, int, int> requiredHeight)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        ArgumentNullException.ThrowIfNull(requiredHeight);

        Width = width;
        Height = height;
        _cells = new BuildCell[checked(width * height)];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var required = requiredHeight(x, y);
            if (required < 0) throw new ArgumentOutOfRangeException(nameof(requiredHeight), "Required height cannot be negative.");
            _cells[(y * width) + x] = new BuildCell(required);
        }
    }

    /// <summary>Gets the number of cells across the grid.: Width.</summary>
    public int Width { get; }
    /// <summary>Gets the number of cells down the grid.: Height.</summary>
    public int Height { get; }
    /// <summary>Gets whether every cell has its required number of blocks.: IsComplete.</summary>
    public bool IsComplete => _cells.All(cell => cell.IsComplete);
    /// <summary>Gets the total number of blocks required by all cells.: RequiredBlockCount.</summary>
    public int RequiredBlockCount => _cells.Sum(cell => cell.RequiredBlocks);
    /// <summary>Gets the total number of blocks committed to cells.: PlacedBlockCount.</summary>
    public int PlacedBlockCount => _cells.Sum(cell => cell.PlacedBlocks);

    /// <summary>Gets the build cell at the specified grid coordinates.: this.</summary>
    public BuildCell this[int x, int y]
    {
        get
        {
            ValidateCoordinate(x, y);
            return _cells[(y * Width) + x];
        }
    }

    /// <summary>Attempts to commit one block to the specified cell without exceeding its target.: TryPlaceBlock.</summary>
    public bool TryPlaceBlock(int x, int y) => this[x, y].TryPlaceBlock();

    private void ValidateCoordinate(int x, int y)
    {
        if ((uint)x >= (uint)Width) throw new ArgumentOutOfRangeException(nameof(x));
        if ((uint)y >= (uint)Height) throw new ArgumentOutOfRangeException(nameof(y));
    }
}
