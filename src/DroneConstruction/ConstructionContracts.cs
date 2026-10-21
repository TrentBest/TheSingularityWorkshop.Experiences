namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>Maps RGB source pixels to a discrete target height field.</summary>
public enum ChannelHeightMapping
{
    /// <summary>Maps the red image channel to target height.: Red.</summary>
    Red,
    /// <summary>Maps the green image channel to target height.: Green.</summary>
    Green,
    /// <summary>Maps the blue image channel to target height.: Blue.</summary>
    Blue,
    /// <summary>Maps the maximum RGB channel value to target height.: Maximum.</summary>
    Maximum,
    /// <summary>Maps the average RGB channel value to target height.: Average.</summary>
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

    /// <summary>Gets the width of the height field or grid.: Width.</summary>
    public int Width { get; }
    /// <summary>Gets the height of the height field or grid.: Height.</summary>
    public int Height { get; }
    /// <summary>Gets the upper bound used for mapped target heights.: MaximumHeight.</summary>
    public int MaximumHeight { get; }
    /// <summary>Gets the channel-to-height mapping applied to the source image.: Mapping.</summary>
    public ChannelHeightMapping Mapping { get; }
    /// <summary>Gets the source-image digest used to identify the input.: SourceDigest.</summary>
    public string SourceDigest { get; }

    /// <summary>Gets the target height at the specified grid coordinates.: this.</summary>
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
    /// <summary>Specifies movement toward increasing X coordinates.: East.</summary>
    East,
    /// <summary>Specifies movement toward decreasing X coordinates.: West.</summary>
    West,
    /// <summary>Specifies movement toward decreasing Y coordinates.: North.</summary>
    North,
    /// <summary>Specifies movement toward increasing Y coordinates.: South.</summary>
    South
}

/// <summary>
/// A bounded 2D occupancy grid. Occupancy is authoritative simulation state, not renderer state.
/// </summary>
public sealed class TrafficCellGrid
{
    private readonly Dictionary<GridCell, ulong> _occupants = [];
    private readonly Dictionary<ulong, GridCell> _agentCells = [];

    /// <summary>Creates an exclusive-occupancy grid with the specified dimensions.: TrafficCellGrid.</summary>
    public TrafficCellGrid(int width, int height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        Width = width;
        Height = height;
    }

    /// <summary>Gets the width of the height field or grid.: Width.</summary>
    public int Width { get; }
    /// <summary>Gets the height of the height field or grid.: Height.</summary>
    public int Height { get; }

    /// <summary>Gets the width of the height field or grid.: Width.</summary>
    public bool Contains(GridCell cell) => (uint)cell.X < (uint)Width && (uint)cell.Y < (uint)Height;

    /// <summary>Gets whether the specified cell currently has an occupant.: IsOccupied.</summary>
    public bool IsOccupied(GridCell cell)
    {
        EnsureInBounds(cell);
        return _occupants.ContainsKey(cell);
    }

    /// <summary>Attempts to retrieve the identifier occupying a cell.: TryGetOccupant.</summary>
    public bool TryGetOccupant(GridCell cell, out ulong agentId)
    {
        EnsureInBounds(cell);
        return _occupants.TryGetValue(cell, out agentId);
    }

    /// <summary>Attempts to retrieve an agent's current cell from the grid's identity index.</summary>
    public bool TryGetAgentCell(ulong agentId, out GridCell cell)
    {
        if (agentId == 0) throw new ArgumentOutOfRangeException(nameof(agentId), "Agent ID zero is reserved.");
        return _agentCells.TryGetValue(agentId, out cell);
    }

    /// <summary>Attempts to claim an unoccupied cell for an agent.: TryOccupy.</summary>
    public bool TryOccupy(GridCell cell, ulong agentId)
    {
        EnsureInBounds(cell);
        if (agentId == 0) throw new ArgumentOutOfRangeException(nameof(agentId), "Agent ID zero is reserved.");
        if (_occupants.ContainsKey(cell) || _agentCells.ContainsKey(agentId)) return false;
        _occupants.Add(cell, agentId);
        _agentCells.Add(agentId, cell);
        return true;
    }

    /// <summary>Releases a cell only when the supplied agent owns it.: Release.</summary>
    public bool Release(GridCell cell, ulong agentId)
    {
        EnsureInBounds(cell);
        if (!_occupants.TryGetValue(cell, out var occupant) || occupant != agentId) return false;
        _occupants.Remove(cell);
        _agentCells.Remove(agentId);
        return true;
    }

    /// <summary>Atomically moves an agent between cells while preserving exclusive occupancy on failure.</summary>
    public bool TryMove(GridCell current, GridCell destination, ulong agentId)
    {
        EnsureInBounds(current);
        EnsureInBounds(destination);
        if (agentId == 0) throw new ArgumentOutOfRangeException(nameof(agentId), "Agent ID zero is reserved.");
        if (current == destination)
            return _occupants.TryGetValue(current, out var sameCellOwner) && sameCellOwner == agentId
                && _agentCells.TryGetValue(agentId, out var recordedCell) && recordedCell == current;

        if (!_occupants.TryGetValue(current, out var owner) || owner != agentId) return false;
        if (!_agentCells.TryGetValue(agentId, out var indexedCell) || indexedCell != current) return false;
        if (_occupants.ContainsKey(destination)) return false;

        _occupants.Remove(current);
        _occupants.Add(destination, agentId);
        _agentCells[agentId] = destination;
        return true;
    }

    /// <summary>
    /// Transfers an agent between distinct occupancy grids without leaving it registered in both.
    /// This is a synchronous model operation; the grid type itself is not thread-safe.
    /// </summary>
    public bool TryTransferTo(GridCell current, GridCell destination, ulong agentId, TrafficCellGrid destinationGrid)
    {
        ArgumentNullException.ThrowIfNull(destinationGrid);
        EnsureInBounds(current);
        if (agentId == 0) throw new ArgumentOutOfRangeException(nameof(agentId), "Agent ID zero is reserved.");
        if (ReferenceEquals(this, destinationGrid)) return TryMove(current, destination, agentId);
        if (!destinationGrid.Contains(destination)) throw new ArgumentOutOfRangeException(nameof(destination));
        if (!_occupants.TryGetValue(current, out var owner) || owner != agentId) return false;
        if (!_agentCells.TryGetValue(agentId, out var indexedCell) || indexedCell != current) return false;
        if (destinationGrid._occupants.ContainsKey(destination) || destinationGrid._agentCells.ContainsKey(agentId)) return false;

        _occupants.Remove(current);
        _agentCells.Remove(agentId);
        destinationGrid._occupants.Add(destination, agentId);
        destinationGrid._agentCells.Add(agentId, destination);
        return true;
    }

    /// <summary>Gets whether every supplied cell is currently unoccupied.: IsClear.</summary>
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
    /// <summary>Creates a one-way cellular lane with direction, elevation, and movement policy.: Skylane.</summary>
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

    /// <summary>Gets the stable identifier of the lane.: LaneId.</summary>
    public string LaneId { get; }
    /// <summary>Gets the declared direction of travel.: Direction.</summary>
    public LaneDirection Direction { get; }
    /// <summary>Gets the logical elevation assigned to the lane.: Elevation.</summary>
    public int Elevation { get; }
    /// <summary>Gets the maximum configured cells traversed per tick.: CellsPerTick.</summary>
    public int CellsPerTick { get; }
    /// <summary>Creates an exclusive-occupancy grid with the specified dimensions.: TrafficCellGrid.</summary>
    public TrafficCellGrid Cells { get; }

    /// <summary>
    /// Computes the next cell toward a target only when it lies on this lane's declared one-way axis.
    /// This is a route-policy primitive; it does not reserve occupancy or perform a transition.
    /// </summary>
    public bool TryGetNextCell(GridCell current, GridCell target, out GridCell next)
    {
        next = current;
        if (!Cells.Contains(current) || !Cells.Contains(target)) return false;

        var dx = target.X - current.X;
        var dy = target.Y - current.Y;
        var legalAxis = Direction switch
        {
            LaneDirection.East => dy == 0 && dx > 0,
            LaneDirection.West => dy == 0 && dx < 0,
            LaneDirection.North => dx == 0 && dy < 0,
            LaneDirection.South => dx == 0 && dy > 0,
            _ => false
        };
        if (!legalAxis) return false;

        var distance = Math.Min(CellsPerTick, Math.Abs(dx != 0 ? dx : dy));
        next = Direction switch
        {
            LaneDirection.East => new GridCell(current.X + distance, current.Y),
            LaneDirection.West => new GridCell(current.X - distance, current.Y),
            LaneDirection.North => new GridCell(current.X, current.Y - distance),
            LaneDirection.South => new GridCell(current.X, current.Y + distance),
            _ => current
        };
        return true;
    }
}

/// <summary>A 2D cellular holding area for agents waiting for transition clearance.</summary>
public sealed class HoverLane
{
    /// <summary>Creates a cellular holding lane with the specified dimensions.: HoverLane.</summary>
    public HoverLane(string laneId, int width, int height)
    {
        if (string.IsNullOrWhiteSpace(laneId)) throw new ArgumentException("Lane identity is required.", nameof(laneId));
        LaneId = laneId;
        Cells = new TrafficCellGrid(width, height);
    }

    /// <summary>Gets the stable identifier of the lane.: LaneId.</summary>
    public string LaneId { get; }
    /// <summary>Creates an exclusive-occupancy grid with the specified dimensions.: TrafficCellGrid.</summary>
    public TrafficCellGrid Cells { get; }
}

/// <summary>
/// A bounded, single-admission transition resource between a hover lane and a skylane.
/// The lookahead is configurable and represents discrete occupancy checks, not a physical safety proof.
/// </summary>
public sealed class Slipstream
{
    private ulong? _reservedBy;

    /// <summary>Creates a bounded transition with a configured lookahead length.: Slipstream.</summary>
    public Slipstream(string slipstreamId, int transitionCellCount, int lookaheadCells = 5)
    {
        if (string.IsNullOrWhiteSpace(slipstreamId)) throw new ArgumentException("Slipstream identity is required.", nameof(slipstreamId));
        if (transitionCellCount <= 0) throw new ArgumentOutOfRangeException(nameof(transitionCellCount));
        if (lookaheadCells <= 0) throw new ArgumentOutOfRangeException(nameof(lookaheadCells));
        SlipstreamId = slipstreamId;
        TransitionCellCount = transitionCellCount;
        LookaheadCells = lookaheadCells;
    }

    /// <summary>Gets the stable identifier of the transition.: SlipstreamId.</summary>
    public string SlipstreamId { get; }
    /// <summary>Gets the number of cells in the transition.: TransitionCellCount.</summary>
    public int TransitionCellCount { get; }
    /// <summary>Gets the configured number of cells checked before admission.: LookaheadCells.</summary>
    public int LookaheadCells { get; }
    /// <summary>Gets the identifier of the agent holding the reservation, if any.: ReservedBy.</summary>
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
        // A live reservation is immutable until its owner releases it; even the same agent
        // cannot silently replace its destination/lookahead with a different route.
        if (_reservedBy is not null) return false;
        if (!destinationLane.Contains(destination) || destinationLane.IsOccupied(destination)) return false;

        // Lookahead represents distinct future cells beyond the transition destination.
        // Reject malformed routes rather than letting duplicates inflate the apparent clearance.
        var seen = new HashSet<GridCell>();
        foreach (var cell in lookahead)
        {
            if (!destinationLane.Contains(cell) || cell == destination || !seen.Add(cell)) return false;
        }
        if (!destinationLane.IsClear(lookahead)) return false;
        _reservedBy = agentId;
        return true;
    }

    /// <summary>
    /// Reserves this transition and transfers an agent from a source grid into the destination lane.
    /// A successful admission retains the reservation until the caller releases it after the agent exits
    /// the transition. A failed transfer releases the reservation and leaves source occupancy unchanged.
    /// </summary>
    public bool TryAdmit(
        ulong agentId,
        TrafficCellGrid sourceGrid,
        GridCell sourceCell,
        GridCell destination,
        IReadOnlyList<GridCell> lookahead,
        TrafficCellGrid destinationLane)
    {
        ArgumentNullException.ThrowIfNull(sourceGrid);
        ArgumentNullException.ThrowIfNull(destinationLane);

        // Admission is a one-shot operation. In particular, a repeated call by the same
        // agent must not reuse and then accidentally release its already-active reservation.
        if (_reservedBy is not null) return false;
        if (!TryReserve(agentId, destination, lookahead, destinationLane)) return false;

        try
        {
            if (sourceGrid.TryTransferTo(sourceCell, destination, agentId, destinationLane)) return true;
            Release(agentId);
            return false;
        }
        catch
        {
            // Transfer validates source coordinates and can throw before moving occupancy.
            // Do not strand the transition reservation when validation fails.
            Release(agentId);
            throw;
        }
    }

    /// <summary>Releases a cell only when the supplied agent owns it.: Release.</summary>
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
    /// <summary>Creates a target cell with a required block count.: BuildCell.</summary>
    public BuildCell(int requiredBlocks)
    {
        if (requiredBlocks < 0) throw new ArgumentOutOfRangeException(nameof(requiredBlocks));
        RequiredBlocks = requiredBlocks;
    }

    /// <summary>Gets the number of blocks required by the cell.: RequiredBlocks.</summary>
    public int RequiredBlocks { get; }
    /// <summary>Gets the number of blocks committed to the cell.: PlacedBlocks.</summary>
    public int PlacedBlocks { get; private set; }
    /// <summary>Gets the number of blocks required by the cell.: RequiredBlocks.</summary>
    public bool IsComplete => PlacedBlocks == RequiredBlocks;

    /// <summary>Attempts to commit one block without exceeding the required count.: TryPlaceBlock.</summary>
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
    /// <summary>Creates a block emitter with finite inventory and pickup capacity.: BlockEmitter.</summary>
    public BlockEmitter(string emitterId, int initialInventory, int pickupCapacity = 1)
    {
        if (string.IsNullOrWhiteSpace(emitterId)) throw new ArgumentException("Emitter identity is required.", nameof(emitterId));
        if (initialInventory < 0) throw new ArgumentOutOfRangeException(nameof(initialInventory));
        if (pickupCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(pickupCapacity));
        EmitterId = emitterId;
        Inventory = initialInventory;
        PickupCapacity = pickupCapacity;
    }

    /// <summary>Gets the stable identifier of the emitter.: EmitterId.</summary>
    public string EmitterId { get; }
    /// <summary>Gets the number of blocks currently available.: Inventory.</summary>
    public int Inventory { get; private set; }
    /// <summary>Gets the maximum configured pickup capacity.: PickupCapacity.</summary>
    public int PickupCapacity { get; }

    /// <summary>Attempts to remove one available block from inventory.: TryTakeBlock.</summary>
    public bool TryTakeBlock()
    {
        if (Inventory == 0) return false;
        Inventory--;
        return true;
    }

    /// <summary>Adds blocks to inventory after validating the requested amount.: Restock.</summary>
    public void Restock(int count)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        Inventory = checked(Inventory + count);
    }
}
