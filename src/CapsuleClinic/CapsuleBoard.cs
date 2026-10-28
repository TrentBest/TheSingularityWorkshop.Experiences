namespace TheSingularityWorkshop.Experiences.CapsuleClinic;

/// <summary>The four colors used by Capsule Clinic puzzle pieces.</summary>
public enum CapsuleColor
{
    /// <summary>Red pieces.</summary>
    Red = 0,
    /// <summary>Blue pieces.</summary>
    Blue = 1,
    /// <summary>Yellow pieces.</summary>
    Yellow = 2,
    /// <summary>Purple pieces.</summary>
    Purple = 3
}

/// <summary>The orientation of a two-segment capsule relative to its pivot segment.</summary>
public enum CapsuleOrientation
{
    /// <summary>The partner segment is one cell to the right of the pivot.</summary>
    Horizontal = 0,
    /// <summary>The partner segment is one cell above the pivot.</summary>
    Vertical = 1
}

/// <summary>A pair of colored capsule segments.</summary>
/// <param name="PivotColor">The color of the pivot segment.</param>
/// <param name="PartnerColor">The color of the other segment.</param>
public readonly record struct CapsulePair(CapsuleColor PivotColor, CapsuleColor PartnerColor)
{
    /// <summary>Returns whether both segment colors are defined.</summary>
    public bool IsValid => Enum.IsDefined(PivotColor) && Enum.IsDefined(PartnerColor);
}

/// <summary>A settled puzzle cell. Virus cells are targets; capsule segments are movable puzzle pieces.</summary>
/// <param name="Color">The cell color.</param>
/// <param name="IsVirus">Whether this cell is a virus target.</param>
public readonly record struct PuzzleCell(CapsuleColor Color, bool IsVirus, long CapsulePairId = 0);

/// <summary>The result of resolving all currently available matches and resulting gravity.</summary>
/// <param name="ChainCount">Number of match waves resolved.</param>
/// <param name="CellsCleared">Total cells removed.</param>
/// <param name="VirusesCleared">Number of virus targets removed.</param>
public readonly record struct ResolutionResult(int ChainCount, int CellsCleared, int VirusesCleared);

/// <summary>
/// A small deterministic, renderer-independent puzzle board for Capsule Clinic.
/// Coordinates use (0,0) at the bottom-left; gravity moves cells toward decreasing Y.
/// </summary>
/// <remarks>
/// This first core slice models placement of settled pieces, match resolution, and gravity.
/// Active falling-piece input, scoring, match lifecycle, and network transport are separate layers.
/// </remarks>
public sealed class CapsuleBoard
{
    private readonly PuzzleCell?[,] _cells;
    private long _nextCapsulePairId;

    /// <summary>Creates an empty board.</summary>
    /// <param name="width">Board width in cells.</param>
    /// <param name="height">Board height in cells.</param>
    public CapsuleBoard(int width = 8, int height = 16)
    {
        if (width < 1) throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        if (height < 1) throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
        Width = width;
        Height = height;
        _cells = new PuzzleCell?[width, height];
    }

    /// <summary>Gets the board width.</summary>
    public int Width { get; }

    /// <summary>Gets the board height.</summary>
    public int Height { get; }

    /// <summary>Gets the number of occupied cells.</summary>
    public int OccupiedCount
    {
        get
        {
            var count = 0;
            foreach (var cell in _cells)
                if (cell.HasValue) count++;
            return count;
        }
    }

    /// <summary>Gets the cell at the given coordinates, or null when it is empty.</summary>
    public PuzzleCell? GetCell(int x, int y)
    {
        ValidateCoordinates(x, y);
        return _cells[x, y];
    }

    /// <summary>Returns an independent snapshot in row-major order from bottom to top.</summary>
    public IReadOnlyList<PuzzleCell?> Snapshot()
    {
        var snapshot = new PuzzleCell?[Width * Height];
        for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
                snapshot[y * Width + x] = _cells[x, y];
        return Array.AsReadOnly(snapshot);
    }

    /// <summary>Places a virus target in an empty cell.</summary>
    public bool TryPlaceVirus(int x, int y, CapsuleColor color)
    {
        ValidateCoordinates(x, y);
        if (!Enum.IsDefined(color)) throw new ArgumentOutOfRangeException(nameof(color));
        if (_cells[x, y].HasValue) return false;
        _cells[x, y] = new PuzzleCell(color, true);
        return true;
    }

    /// <summary>
    /// Atomically settles both segments of a capsule at the supplied pivot and orientation.
    /// Returns false without changing the board if either destination is invalid or occupied.
    /// </summary>
    public bool TryPlaceCapsule(CapsulePair pair, int pivotX, int pivotY, CapsuleOrientation orientation)
    {
        if (!pair.IsValid) throw new ArgumentException("Both capsule colors must be defined.", nameof(pair));
        if (!Enum.IsDefined(orientation)) throw new ArgumentOutOfRangeException(nameof(orientation));

        var partnerX = pivotX + (orientation == CapsuleOrientation.Horizontal ? 1 : 0);
        var partnerY = pivotY + (orientation == CapsuleOrientation.Vertical ? 1 : 0);
        if (!IsInside(pivotX, pivotY) || !IsInside(partnerX, partnerY)) return false;
        if (_cells[pivotX, pivotY].HasValue || _cells[partnerX, partnerY].HasValue) return false;

        var pairId = checked(++_nextCapsulePairId);
        _cells[pivotX, pivotY] = new PuzzleCell(pair.PivotColor, false, pairId);
        _cells[partnerX, partnerY] = new PuzzleCell(pair.PartnerColor, false, pairId);
        return true;
    }

    /// <summary>
    /// Clears every horizontal or vertical run of at least four same-colored cells, then applies
    /// gravity and repeats until the board is stable.
    /// </summary>
    public ResolutionResult ResolveMatchesAndGravity()
    {
        var chains = 0;
        var cleared = 0;
        var viruses = 0;

        while (true)
        {
            var matches = FindMatches();
            if (matches.Count == 0) break;

            chains++;
            foreach (var (x, y) in matches)
            {
                if (_cells[x, y] is { IsVirus: true }) viruses++;
                _cells[x, y] = null;
            }
            cleared += matches.Count;
            ApplyGravity();
        }

        return new ResolutionResult(chains, cleared, viruses);
    }

    private HashSet<(int X, int Y)> FindMatches()
    {
        var matches = new HashSet<(int X, int Y)>();
        for (var y = 0; y < Height; y++)
        {
            var x = 0;
            while (x < Width)
            {
                if (_cells[x, y] is not { } cell) { x++; continue; }
                var end = x + 1;
                while (end < Width && _cells[end, y] is { } next && next.Color == cell.Color) end++;
                if (end - x >= 4)
                    for (var i = x; i < end; i++) matches.Add((i, y));
                x = end;
            }
        }

        for (var x = 0; x < Width; x++)
        {
            var y = 0;
            while (y < Height)
            {
                if (_cells[x, y] is not { } cell) { y++; continue; }
                var end = y + 1;
                while (end < Height && _cells[x, end] is { } next && next.Color == cell.Color) end++;
                if (end - y >= 4)
                    for (var i = y; i < end; i++) matches.Add((x, i));
                y = end;
            }
        }

        return matches;
    }

    private void ApplyGravity()
    {
        // Move one cell at a time. A surviving capsule pair behaves as one rigid group
        // while both bonded segments remain; if one segment was cleared, its partner
        // becomes a singleton and falls independently.
        while (true)
        {
            var moves = new List<(int X, int Y, PuzzleCell Cell, int TargetX, int TargetY)>();
            var visited = new HashSet<(int X, int Y)>();

            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    if (_cells[x, y] is not { } cell || visited.Contains((x, y)))
                        continue;

                    if (cell.CapsulePairId != 0 &&
                        TryFindBondedPartner(x, y, cell.CapsulePairId, out var partnerX, out var partnerY, out var partner))
                    {
                        visited.Add((x, y));
                        visited.Add((partnerX, partnerY));

                        var targetY = y - 1;
                        var partnerTargetY = partnerY - 1;
                        if (targetY < 0 || partnerTargetY < 0)
                            continue;

                        if (CanMoveGroupDown((x, y), (partnerX, partnerY), targetY, partnerTargetY))
                        {
                            moves.Add((x, y, cell, x, targetY));
                            moves.Add((partnerX, partnerY, partner, partnerX, partnerTargetY));
                        }

                        continue;
                    }

                    visited.Add((x, y));
                    if (y > 0 && !_cells[x, y - 1].HasValue)
                        moves.Add((x, y, cell, x, y - 1));
                }
            }

            if (moves.Count == 0)
                return;

            // Clear all sources first so vertical bonded pairs can move through their
            // own previous cells without overwriting either segment.
            foreach (var move in moves)
                _cells[move.X, move.Y] = null;
            foreach (var move in moves)
                _cells[move.TargetX, move.TargetY] = move.Cell;
        }
    }

    private bool TryFindBondedPartner(
        int x, int y, long pairId,
        out int partnerX, out int partnerY, out PuzzleCell partner)
    {
        foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
        {
            var candidateX = x + dx;
            var candidateY = y + dy;
            if (!IsInside(candidateX, candidateY) ||
                _cells[candidateX, candidateY] is not { } candidate ||
                candidate.CapsulePairId != pairId)
                continue;

            partnerX = candidateX;
            partnerY = candidateY;
            partner = candidate;
            return true;
        }

        partnerX = partnerY = 0;
        partner = default;
        return false;
    }

    private bool CanMoveGroupDown(
        (int X, int Y) first, (int X, int Y) second,
        int firstTargetY, int secondTargetY)
    {
        return IsAvailableForGroup(first.X, firstTargetY, first, second) &&
               IsAvailableForGroup(second.X, secondTargetY, first, second);
    }

    private bool IsAvailableForGroup(
        int x, int y, (int X, int Y) first, (int X, int Y) second)
    {
        if (!IsInside(x, y)) return false;
        if ((x == first.X && y == first.Y) || (x == second.X && y == second.Y))
            return true;
        return !_cells[x, y].HasValue;
    }

    private bool IsInside(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    private void ValidateCoordinates(int x, int y)
    {
        if (!IsInside(x, y)) throw new ArgumentOutOfRangeException(nameof(x), $"Coordinates ({x},{y}) are outside the board.");
    }
}
