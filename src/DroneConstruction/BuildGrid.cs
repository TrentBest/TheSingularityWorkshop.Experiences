namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>A rectangular target grid whose cells track required and committed block counts.</summary>
public sealed class BuildGrid
{
    private readonly BuildCell[] _cells;

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

    public int Width { get; }
    public int Height { get; }
    public bool IsComplete => _cells.All(cell => cell.IsComplete);
    public int RequiredBlockCount => _cells.Sum(cell => cell.RequiredBlocks);
    public int PlacedBlockCount => _cells.Sum(cell => cell.PlacedBlocks);

    public BuildCell this[int x, int y]
    {
        get
        {
            ValidateCoordinate(x, y);
            return _cells[(y * Width) + x];
        }
    }

    public bool TryPlaceBlock(int x, int y) => this[x, y].TryPlaceBlock();

    private void ValidateCoordinate(int x, int y)
    {
        if ((uint)x >= (uint)Width) throw new ArgumentOutOfRangeException(nameof(x));
        if ((uint)y >= (uint)Height) throw new ArgumentOutOfRangeException(nameof(y));
    }
}
