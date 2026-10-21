namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>Maps RGB source pixels to a discrete target height field.</summary>
public enum ChannelHeightMapping
{
    Red,
    Green,
    Blue,
    Maximum,
    Average
}

/// <summary>A versioned image-derived grid of required block heights.</summary>
public sealed class ImageHeightField
{
    private readonly int[] _heights;

    private ImageHeightField(int width, int height, int maximumHeight, ChannelHeightMapping mapping, int[] heights, string sourceDigest)
    {
        Width = width;
        Height = height;
        MaximumHeight = maximumHeight;
        Mapping = mapping;
        _heights = heights;
        SourceDigest = sourceDigest;
    }

    public int Width { get; }
    public int Height { get; }
    public int MaximumHeight { get; }
    public ChannelHeightMapping Mapping { get; }
    public string SourceDigest { get; }

    public int this[int x, int y]
    {
        get
        {
            ValidateCoordinate(x, y);
            return _heights[(y * Width) + x];
        }
    }

    /// <summary>Creates a height field from packed RGB bytes; one RGB triplet per pixel, row-major.</summary>
    public static ImageHeightField FromRgb(
        int width,
        int height,
        ReadOnlySpan<byte> rgb,
        int maximumHeight,
        ChannelHeightMapping mapping,
        string sourceDigest)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (maximumHeight <= 0) throw new ArgumentOutOfRangeException(nameof(maximumHeight));
        if (string.IsNullOrWhiteSpace(sourceDigest)) throw new ArgumentException("A source digest is required.", nameof(sourceDigest));
        if (!Enum.IsDefined(mapping)) throw new ArgumentOutOfRangeException(nameof(mapping));

        var pixelCount = checked(width * height);
        if (rgb.Length != checked(pixelCount * 3))
            throw new ArgumentException("RGB data must contain exactly three bytes per pixel.", nameof(rgb));

        var heights = new int[pixelCount];
        for (var pixel = 0; pixel < pixelCount; pixel++)
        {
            var r = rgb[pixel * 3];
            var g = rgb[(pixel * 3) + 1];
            var b = rgb[(pixel * 3) + 2];
            var channel = mapping switch
            {
                ChannelHeightMapping.Red => r,
                ChannelHeightMapping.Green => g,
                ChannelHeightMapping.Blue => b,
                ChannelHeightMapping.Maximum => Math.Max(r, Math.Max(g, b)),
                ChannelHeightMapping.Average => (r + g + b) / 3,
                _ => throw new ArgumentOutOfRangeException(nameof(mapping))
            };
            heights[pixel] = (int)Math.Round(channel * (maximumHeight / 255d), MidpointRounding.AwayFromZero);
        }

        return new ImageHeightField(width, height, maximumHeight, mapping, heights, sourceDigest);
    }

    private void ValidateCoordinate(int x, int y)
    {
        if ((uint)x >= (uint)Width) throw new ArgumentOutOfRangeException(nameof(x));
        if ((uint)y >= (uint)Height) throw new ArgumentOutOfRangeException(nameof(y));
    }
}

/// <summary>A cell coordinate in a discrete two-dimensional traffic grid.</summary>
public readonly record struct GridCell(int X, int Y);

/// <summary>Declared travel direction for a one-way skylane.</summary>
public enum LaneDirection
{
    East,
    West,
    North,
    South
}

/// <summary>
/// A bounded 2D occupancy grid. Occupancy is authoritative simulation state, not renderer state.
/// </summary>
public sealed class TrafficCellGrid
{
    private readonly Dictionary<GridCell, ulong> _occupants = [];

    public TrafficCellGrid(int width, int height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        Width = width;
        Height = height;
    }

    public int Width { get; }
    public int Height { get; }

    public bool Contains(GridCell cell) => (uint)cell.X < (uint)Width && (uint)cell.Y < (uint)Height;

    public bool IsOccupied(GridCell cell)
    {
        EnsureInBounds(cell);
        return _occupants.ContainsKey(cell);
    }

    public bool TryGetOccupant(GridCell cell, out ulong agentId)
    {
        EnsureInBounds(cell);
        return _occupants.TryGetValue(cell, out agentId);
    }

    public bool TryOccupy(GridCell cell, ulong agentId)
    {
        EnsureInBounds(cell);
        if (agentId == 0) throw new ArgumentOutOfRangeException(nameof(agentId), "Agent ID zero is reserved.");
        if (_occupants.ContainsKey(cell)) return false;
        if (_occupants.Values.Contains(agentId)) return false;
        _occupants.Add(cell, agentId);
        return true;
    }

    public bool Release(GridCell cell, ulong agentId)
    {
        EnsureInBounds(cell);
        return _occupants.TryGetValue(cell, out var occupant) &&
            occupant == agentId &&
            _occupants.Remove(cell);
    }

    public bool IsClear(IEnumerable<GridCell> cells)
    {
        ArgumentNullException.ThrowIfNull(cells);
        foreach (var cell in cells)
        {
            if (!Contains(cell) || _occupants.ContainsKey(cell)) return false;
        }
        return true;
    }

    private void EnsureInBounds(GridCell cell)
    {
        if (!Contains(cell)) throw new ArgumentOutOfRangeException(nameof(cell), $"Cell ({cell.X}, {cell.Y}) is outside the grid.");
    }
}

/// <summary>
/// A directed skylane: a 2D cellular grid with one declared travel direction.
/// Direction policy is metadata in this first discrete slice; movement generation is added separately.
/// </summary>
public sealed class Skylane
{
    public Skylane(string laneId, int width, int height, LaneDirection direction, int elevation, int cellsPerTick = 1)
    {
        if (string.IsNullOrWhiteSpace(laneId)) throw new ArgumentException("Lane identity is required.", nameof(laneId));
        if (!Enum.IsDefined(direction)) throw new ArgumentOutOfRangeException(nameof(direction));
        if (cellsPerTick <= 0) throw new ArgumentOutOfRangeException(nameof(cellsPerTick));
        LaneId = laneId;
        Direction = direction;
        Elevation = elevation;
        CellsPerTick = cellsPerTick;
        Cells = new TrafficCellGrid(width, height);
    }

    public string LaneId { get; }
    public LaneDirection Direction { get; }
    public int Elevation { get; }
    public int CellsPerTick { get; }
    public TrafficCellGrid Cells { get; }
}

/// <summary>A 2D cellular holding area for agents waiting for transition clearance.</summary>
public sealed class HoverLane
{
    public HoverLane(string laneId, int width, int height)
    {
        if (string.IsNullOrWhiteSpace(laneId)) throw new ArgumentException("Lane identity is required.", nameof(laneId));
        LaneId = laneId;
        Cells = new TrafficCellGrid(width, height);
    }

    public string LaneId { get; }
    public TrafficCellGrid Cells { get; }
}

/// <summary>
/// A bounded, single-admission transition resource between a hover lane and a skylane.
/// The lookahead is configurable and represents discrete occupancy checks, not a physical safety proof.
/// </summary>
public sealed class Slipstream
{
    private ulong? _reservedBy;

    public Slipstream(string slipstreamId, int transitionCellCount, int lookaheadCells = 5)
    {
        if (string.IsNullOrWhiteSpace(slipstreamId)) throw new ArgumentException("Slipstream identity is required.", nameof(slipstreamId));
        if (transitionCellCount <= 0) throw new ArgumentOutOfRangeException(nameof(transitionCellCount));
        if (lookaheadCells <= 0) throw new ArgumentOutOfRangeException(nameof(lookaheadCells));
        SlipstreamId = slipstreamId;
        TransitionCellCount = transitionCellCount;
        LookaheadCells = lookaheadCells;
    }

    public string SlipstreamId { get; }
    public int TransitionCellCount { get; }
    public int LookaheadCells { get; }
    public ulong? ReservedBy => _reservedBy;

    /// <summary>
    /// Reserves this transition only when the destination and all caller-supplied lookahead cells are clear.
    /// The caller must supply exactly the cells that the lane's movement policy defines as lookahead.
    /// </summary>
    public bool TryReserve(ulong agentId, GridCell destination, IReadOnlyList<GridCell> lookahead, TrafficCellGrid destinationLane)
    {
        if (agentId == 0) throw new ArgumentOutOfRangeException(nameof(agentId));
        ArgumentNullException.ThrowIfNull(lookahead);
        ArgumentNullException.ThrowIfNull(destinationLane);
        if (lookahead.Count != LookaheadCells) throw new ArgumentException($"Exactly {LookaheadCells} lookahead cells are required.", nameof(lookahead));
        if (_reservedBy is not null && _reservedBy != agentId) return false;
        if (!destinationLane.Contains(destination) || destinationLane.IsOccupied(destination)) return false;
        if (!destinationLane.IsClear(lookahead)) return false;
        _reservedBy = agentId;
        return true;
    }

    public bool Release(ulong agentId)
    {
        if (agentId == 0) throw new ArgumentOutOfRangeException(nameof(agentId));
        if (_reservedBy != agentId) return false;
        _reservedBy = null;
        return true;
    }
}

/// <summary>Tracks required and committed block counts for one target cell.</summary>
public sealed class BuildCell
{
    public BuildCell(int requiredBlocks)
    {
        if (requiredBlocks < 0) throw new ArgumentOutOfRangeException(nameof(requiredBlocks));
        RequiredBlocks = requiredBlocks;
    }

    public int RequiredBlocks { get; }
    public int PlacedBlocks { get; private set; }
    public bool IsComplete => PlacedBlocks == RequiredBlocks;

    public bool TryPlaceBlock()
    {
        if (PlacedBlocks >= RequiredBlocks) return false;
        PlacedBlocks++;
        return true;
    }
}

/// <summary>Finite block inventory at one emitter.</summary>
public sealed class BlockEmitter
{
    public BlockEmitter(string emitterId, int initialInventory, int pickupCapacity = 1)
    {
        if (string.IsNullOrWhiteSpace(emitterId)) throw new ArgumentException("Emitter identity is required.", nameof(emitterId));
        if (initialInventory < 0) throw new ArgumentOutOfRangeException(nameof(initialInventory));
        if (pickupCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(pickupCapacity));
        EmitterId = emitterId;
        Inventory = initialInventory;
        PickupCapacity = pickupCapacity;
    }

    public string EmitterId { get; }
    public int Inventory { get; private set; }
    public int PickupCapacity { get; }

    public bool TryTakeBlock()
    {
        if (Inventory == 0) return false;
        Inventory--;
        return true;
    }

    public void Restock(int count)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        Inventory = checked(Inventory + count);
    }
}
