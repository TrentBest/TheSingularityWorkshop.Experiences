namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>
/// Coordinates one slipstream reservation and the authoritative occupancy transfer through
/// a dedicated transition grid. Operations are synchronous; callers must serialize access.
/// </summary>
public sealed class SlipstreamLifecycle
{
    private readonly Slipstream _slipstream;
    private readonly TrafficCellGrid _transitionCells;

    /// <summary>Creates a lifecycle coordinator for a slipstream and its dedicated transition grid.</summary>
    public SlipstreamLifecycle(Slipstream slipstream, TrafficCellGrid transitionCells)
    {
        ArgumentNullException.ThrowIfNull(slipstream);
        ArgumentNullException.ThrowIfNull(transitionCells);
        _slipstream = slipstream;
        _transitionCells = transitionCells;
    }

    /// <summary>Gets the agent currently occupying the transition, if any.</summary>
    public ulong? AgentId => _slipstream.ReservedBy;

    /// <summary>
    /// Reserves downstream capacity and transfers an agent into the transition entry cell.
    /// Failure leaves source occupancy intact and releases only a reservation acquired by this call.
    /// </summary>
    public bool TryEnter(
        ulong agentId,
        TrafficCellGrid sourceGrid,
        GridCell sourceCell,
        GridCell transitionEntryCell,
        TrafficCellGrid destinationLane,
        GridCell destinationCell,
        IReadOnlyList<GridCell> lookahead)
    {
        if (agentId == 0) throw new ArgumentOutOfRangeException(nameof(agentId));
        ArgumentNullException.ThrowIfNull(sourceGrid);
        ArgumentNullException.ThrowIfNull(destinationLane);
        ArgumentNullException.ThrowIfNull(lookahead);
        if (ReferenceEquals(sourceGrid, _transitionCells)
            || ReferenceEquals(destinationLane, _transitionCells)
            || ReferenceEquals(sourceGrid, destinationLane))
            return false;
        if (!_transitionCells.Contains(transitionEntryCell) || _transitionCells.IsOccupied(transitionEntryCell))
            return false;
        if (!_slipstream.TryReserve(agentId, destinationCell, lookahead, destinationLane))
            return false;

        try
        {
            if (sourceGrid.TryTransferTo(sourceCell, transitionEntryCell, agentId, _transitionCells))
                return true;
            _slipstream.Release(agentId);
            return false;
        }
        catch
        {
            _slipstream.Release(agentId);
            throw;
        }
    }

    /// <summary>Moves the reserved agent by exactly one orthogonal cell within the transition grid.</summary>
    public bool TryAdvance(ulong agentId, GridCell nextCell)
    {
        if (agentId == 0) throw new ArgumentOutOfRangeException(nameof(agentId));
        if (_slipstream.ReservedBy != agentId) return false;
        if (!_transitionCells.TryGetAgentCell(agentId, out var currentCell)) return false;
        var distance = Math.Abs(nextCell.X - currentCell.X) + Math.Abs(nextCell.Y - currentCell.Y);
        if (distance != 1) return false;
        return _transitionCells.TryMove(currentCell, nextCell, agentId);
    }

    /// <summary>
    /// Transfers the reserved agent into its destination lane. A failed exit retains both transition
    /// occupancy and reservation so the caller can retry after the destination becomes available.
    /// </summary>
    public bool TryExit(ulong agentId, TrafficCellGrid destinationLane, GridCell destinationCell)
    {
        if (agentId == 0) throw new ArgumentOutOfRangeException(nameof(agentId));
        ArgumentNullException.ThrowIfNull(destinationLane);
        if (_slipstream.ReservedBy != agentId) return false;
        if (ReferenceEquals(destinationLane, _transitionCells)) return false;
        // Exit must honor the exact destination bound by admission; otherwise a caller
        // could use one lane's clearance reservation to enter a different lane or cell.
        if (!ReferenceEquals(destinationLane, _slipstream.ReservedDestinationLane)
            || _slipstream.ReservedDestinationCell != destinationCell)
            return false;
        if (!_transitionCells.TryGetAgentCell(agentId, out var currentCell)) return false;

        if (!_transitionCells.TryTransferTo(currentCell, destinationCell, agentId, destinationLane))
            return false;

        // The agent has left the transition grid; release capacity only after transfer commits.
        return _slipstream.Release(agentId);
    }
}
