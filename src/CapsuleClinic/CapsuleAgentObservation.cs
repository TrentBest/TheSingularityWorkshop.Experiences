namespace TheSingularityWorkshop.Experiences.CapsuleClinic;

/// <summary>
/// Immutable, renderer-independent view of the game state exposed to a local decision policy.
/// The observation is a snapshot: later game steps cannot mutate its board cells or column heights.
/// </summary>
public sealed class CapsuleAgentObservation
{
    private readonly IReadOnlyList<PuzzleCell?> _cells;
    private readonly IReadOnlyList<int> _columnHeights;

    private CapsuleAgentObservation(
        int width,
        int height,
        IReadOnlyList<PuzzleCell?> cells,
        IReadOnlyList<int> columnHeights,
        ActiveCapsule? active,
        int score,
        bool gameOver)
    {
        Width = width;
        Height = height;
        _cells = cells;
        _columnHeights = columnHeights;
        Active = active;
        Score = score;
        GameOver = gameOver;
    }

    /// <summary>Board width in cells.</summary>
    public int Width { get; }

    /// <summary>Board height in cells.</summary>
    public int Height { get; }

    /// <summary>Row-major board cells, from bottom row to top row.</summary>
    public IReadOnlyList<PuzzleCell?> Cells => _cells;

    /// <summary>Occupied-cell height for each column.</summary>
    public IReadOnlyList<int> ColumnHeights => _columnHeights;

    /// <summary>Current active capsule, or null when no capsule is falling.</summary>
    public ActiveCapsule? Active { get; }

    /// <summary>Cumulative game score.</summary>
    public int Score { get; }

    /// <summary>Whether the game has ended.</summary>
    public bool GameOver { get; }

    /// <summary>Captures an independent observation from the authoritative game.</summary>
    public static CapsuleAgentObservation Capture(CapsuleGame game)
    {
        ArgumentNullException.ThrowIfNull(game);
        var cells = game.Board.Snapshot();
        var heights = new int[game.Board.Width];

        for (var x = 0; x < game.Board.Width; x++)
        {
            for (var y = game.Board.Height - 1; y >= 0; y--)
            {
                if (!cells[y * game.Board.Width + x].HasValue)
                    continue;

                heights[x] = y + 1;
                break;
            }
        }

        return new CapsuleAgentObservation(
            game.Board.Width,
            game.Board.Height,
            Array.AsReadOnly(cells.ToArray()),
            Array.AsReadOnly(heights),
            game.Active,
            game.Score,
            game.GameOver);
    }

    /// <summary>Gets a cell from the snapshot using board coordinates.</summary>
    public PuzzleCell? GetCell(int x, int y)
    {
        if ((uint)x >= (uint)Width) throw new ArgumentOutOfRangeException(nameof(x));
        if ((uint)y >= (uint)Height) throw new ArgumentOutOfRangeException(nameof(y));
        return _cells[y * Width + x];
    }
}
