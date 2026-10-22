namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>Places one finite-inventory block emitter at a discrete traffic-grid coordinate.</summary>
public sealed record PositionedBlockEmitter(GridCell Position, BlockEmitter Emitter);

/// <summary>Immutable renderer-neutral view of one agent at a committed simulation tick.</summary>
public sealed record ConstructionAgentSnapshot(
    ulong AgentId,
    GridCell Position,
    ConstructionAgentState State,
    bool IsCarryingBlock,
    ulong? AssignedTaskId,
    QualifiedGridLocation Location);

/// <summary>Immutable renderer-neutral view of one emitter at a committed simulation tick.</summary>
public sealed record ConstructionEmitterSnapshot(string EmitterId, GridCell Position, int Inventory);

/// <summary>
/// A committed snapshot of logical construction state. It contains model values, not renderer interpolation.
/// </summary>
public sealed record ConstructionSimulationSnapshot(
    long Tick,
    int RequiredBlocks,
    int PlacedBlocks,
    bool IsComplete,
    IReadOnlyList<ConstructionAgentSnapshot> Agents,
    IReadOnlyList<ConstructionEmitterSnapshot> Emitters,
    IReadOnlyList<int> CellPlacedBlocks);

/// <summary>
/// Deterministic, single-threaded vertical slice: assign a placement task, travel to a stocked emitter,
/// pick up one block, move cell-by-cell to the target, and commit placement. This is not flight physics.
/// </summary>
public sealed class ConstructionSimulation
{
    private readonly BuildGrid _buildGrid;
    private readonly ConstructionTaskScheduler _scheduler = new();
    private readonly ConstructionAgent[] _agents;
    private readonly Dictionary<ulong, ConstructionAgent> _agentsById;
    private readonly PositionedBlockEmitter[] _emitters;
    private readonly Dictionary<ulong, PlacementTask> _agentTasks = [];
    private readonly TrafficCellGrid _traffic;
    private readonly ConstructionSpaceCatalog _spaces = new();
    private readonly Dictionary<string, SlipstreamLifecycle> _slipstreamLifecycles = new(StringComparer.Ordinal);
    private readonly Dictionary<ulong, LaneTransitionIntent> _pendingLaneTransitionIntents = [];
    private readonly string _spaceId;
    private long _tick;

    /// <summary>Creates a deterministic simulation over one shared exclusive-occupancy grid.</summary>
    public ConstructionSimulation(
        int width,
        int height,
        Func<int, int, int> requiredHeight,
        IEnumerable<ConstructionAgent> agents,
        IEnumerable<PositionedBlockEmitter> emitters,
        string spaceId = "construction-main")
    {
        ArgumentNullException.ThrowIfNull(agents);
        ArgumentNullException.ThrowIfNull(emitters);
        if (string.IsNullOrWhiteSpace(spaceId)) throw new ArgumentException("A stable simulation-space identity is required.", nameof(spaceId));
        _spaceId = spaceId;
        _buildGrid = new BuildGrid(width, height, requiredHeight);
        _traffic = new TrafficCellGrid(width, height);
        if (!_spaces.TryRegister(ConstructionSpaceKind.SimulationGrid, _spaceId, _traffic))
            throw new InvalidOperationException("The simulation traffic space could not be registered.");
        var agentArray = agents.ToArray();
        if (agentArray.Any(agent => agent is null)) throw new ArgumentException("Agents cannot contain null entries.", nameof(agents));
        var emitterArray = emitters.ToArray();
        if (emitterArray.Any(emitter => emitter is null || emitter.Emitter is null))
            throw new ArgumentException("Emitters cannot contain null entries or null emitter resources.", nameof(emitters));
        _agents = agentArray.OrderBy(agent => agent.AgentId).ToArray();
        _emitters = emitterArray.OrderBy(emitter => emitter.Emitter.EmitterId, StringComparer.Ordinal).ToArray();
        if (_agents.Select(agent => agent.AgentId).Distinct().Count() != _agents.Length)
            throw new ArgumentException("Agent IDs must be unique.", nameof(agents));
        _agentsById = _agents.ToDictionary(agent => agent.AgentId);
        if (_agents.Any(agent => agent.State != ConstructionAgentState.Idle || agent.AssignedTaskId is not null || agent.IsCarryingBlock))
            throw new ArgumentException("Agents must be idle, unassigned, and not carrying a block when a simulation is created.", nameof(agents));
        if (_emitters.Select(emitter => emitter.Emitter.EmitterId).Distinct(StringComparer.Ordinal).Count() != _emitters.Length)
            throw new ArgumentException("Emitter IDs must be unique.", nameof(emitters));
        foreach (var emitter in _emitters)
            if (!_traffic.Contains(emitter.Position)) throw new ArgumentOutOfRangeException(nameof(emitters), "Emitter positions must be inside the traffic grid.");

        foreach (var agent in _agents)
        {
            if (!_traffic.Contains(agent.Position)) throw new ArgumentOutOfRangeException(nameof(agents), "Agent positions must be inside the traffic grid.");
            if (!_traffic.TryOccupy(agent.Position, agent.AgentId))
                throw new ArgumentException("Initial agent positions must be exclusive.", nameof(agents));
        }

        ulong taskId = 1;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        for (var ordinal = 1; ordinal <= _buildGrid[x, y].RequiredBlocks; ordinal++)
        {
            if (!_scheduler.Enqueue(new PlacementTask(taskId, x, y, ordinal)))
                throw new InvalidOperationException("Generated placement task IDs must be unique.");
            taskId = checked(taskId + 1);
        }
    }

    /// <summary>Gets the number of committed ticks.</summary>
    public long TickCount => _tick;
    /// <summary>Gets the stable identity of this simulation's traffic coordinate space.</summary>
    public string SpaceId => _spaceId;

    /// <summary>Gets the number of agents with an unadmitted lane-transition intent.</summary>
    internal int PendingLaneTransitionIntentCount => _pendingLaneTransitionIntents.Count;

    /// <summary>Gets a pending lane-transition intent owned by this simulation.</summary>
    internal bool TryGetPendingLaneTransitionIntent(ulong agentId, out LaneTransitionIntent? intent) =>
        _pendingLaneTransitionIntents.TryGetValue(agentId, out intent);

    /// <summary>Records one pending intent after the caller validates the agent's committed source.</summary>
    internal bool TryRegisterPendingLaneTransitionIntent(ulong agentId, LaneTransitionIntent intent) =>
        agentId != 0 && _pendingLaneTransitionIntents.TryAdd(agentId, intent);

    /// <summary>Removes a pending intent after cancellation or successful admission.</summary>
    internal bool TryRemovePendingLaneTransitionIntent(ulong agentId) =>
        _pendingLaneTransitionIntents.Remove(agentId);

    /// <summary>Gets the current space-qualified location of a registered agent.</summary>
    public QualifiedGridLocation GetAgentLocation(ulong agentId)
    {
        if (!_agentsById.ContainsKey(agentId))
            throw new KeyNotFoundException($"Agent {agentId} is not registered in this simulation.");
        return _spaces.GetAgentLocation(agentId);
    }

    /// <summary>Registers an external traffic space without exposing the internal location catalog.</summary>
    public bool TryRegisterTrafficSpace(ConstructionSpaceKind kind, string spaceId, TrafficCellGrid grid)
    {
        if (kind == ConstructionSpaceKind.SimulationGrid)
            throw new ArgumentOutOfRangeException(nameof(kind), "The simulation's primary grid identity cannot be replaced.");
        return _spaces.TryRegister(kind, spaceId, grid);
    }

    /// <summary>Registers the single lifecycle coordinator that owns a registered slipstream space.</summary>
    public bool TryRegisterSlipstreamLifecycle(string transitionSpaceId, SlipstreamLifecycle lifecycle)
    {
        ArgumentNullException.ThrowIfNull(lifecycle);
        if (string.IsNullOrWhiteSpace(transitionSpaceId)
            || !_spaces.TryResolve(new QualifiedGridLocation(ConstructionSpaceKind.Slipstream, transitionSpaceId, new GridCell(0, 0)), out var registeredGrid)
            || !ReferenceEquals(registeredGrid, lifecycle.TransitionCells)
            || _slipstreamLifecycles.ContainsKey(transitionSpaceId))
            return false;

        _slipstreamLifecycles.Add(transitionSpaceId, lifecycle);
        return true;
    }

    /// <summary>Enters using the coordinator registered for the transition space.</summary>
    public bool TryEnterSlipstream(
        ulong agentId,
        QualifiedGridLocation source,
        QualifiedGridLocation transitionEntry,
        QualifiedGridLocation destination,
        IReadOnlyList<GridCell> lookahead)
    {
        ArgumentNullException.ThrowIfNull(transitionEntry);
        return _slipstreamLifecycles.TryGetValue(transitionEntry.SpaceId, out var lifecycle)
            && TryEnterSlipstream(agentId, source, transitionEntry, lifecycle, destination, lookahead);
    }

    /// <summary>Advances using the coordinator registered for the transition space.</summary>
    public bool TryAdvanceSlipstream(ulong agentId, string transitionSpaceId, GridCell nextCell) =>
        !string.IsNullOrWhiteSpace(transitionSpaceId)
        && _slipstreamLifecycles.TryGetValue(transitionSpaceId, out var lifecycle)
        && TryAdvanceSlipstream(agentId, transitionSpaceId, lifecycle, nextCell);

    /// <summary>Exits using the coordinator registered for the transition space.</summary>
    public bool TryExitSlipstream(ulong agentId, string transitionSpaceId, QualifiedGridLocation destination) =>
        !string.IsNullOrWhiteSpace(transitionSpaceId)
        && _slipstreamLifecycles.TryGetValue(transitionSpaceId, out var lifecycle)
        && TryExitSlipstream(agentId, transitionSpaceId, lifecycle, destination);

    /// <summary>
    /// Enters a registered slipstream from the agent's exact committed source location.
    /// Position is synchronized only after occupancy transfer succeeds.
    /// </summary>
    public bool TryEnterSlipstream(
        ulong agentId,
        QualifiedGridLocation source,
        QualifiedGridLocation transitionEntry,
        SlipstreamLifecycle lifecycle,
        QualifiedGridLocation destination,
        IReadOnlyList<GridCell> lookahead)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(transitionEntry);
        ArgumentNullException.ThrowIfNull(lifecycle);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(lookahead);
        if (!_agentsById.TryGetValue(agentId, out var agent)
            || source.SpaceKind == ConstructionSpaceKind.Slipstream
            || transitionEntry.SpaceKind != ConstructionSpaceKind.Slipstream
            || destination.SpaceKind == ConstructionSpaceKind.Slipstream
            || !_spaces.TryResolve(source, out var sourceGrid)
            || !_spaces.TryResolve(transitionEntry, out var transitionGrid)
            || !_spaces.TryResolve(destination, out var destinationGrid)
            || !ReferenceEquals(transitionGrid, lifecycle.TransitionCells)
            || (_slipstreamLifecycles.TryGetValue(transitionEntry.SpaceId, out var registeredLifecycle) && !ReferenceEquals(registeredLifecycle, lifecycle))
            || !_spaces.TryGetAgentLocation(agentId, out var current)
            || current != source
            || agent.Position != source.Cell)
            return false;

        if (!lifecycle.TryEnter(agentId, sourceGrid!, source.Cell, transitionEntry.Cell,
                destinationGrid!, destination.Cell, lookahead))
            return false;

        agent.SetPosition(transitionEntry.Cell);
        return true;
    }

    /// <summary>Advances an agent within its registered slipstream and synchronizes its local position on success.</summary>
    public bool TryAdvanceSlipstream(ulong agentId, string transitionSpaceId, SlipstreamLifecycle lifecycle, GridCell nextCell)
    {
        ArgumentNullException.ThrowIfNull(lifecycle);
        if (!_agentsById.TryGetValue(agentId, out var agent)
            || string.IsNullOrWhiteSpace(transitionSpaceId))
            return false;
        if (!_spaces.TryGetAgentLocation(agentId, out var current)
            || current!.SpaceKind != ConstructionSpaceKind.Slipstream
            || !StringComparer.Ordinal.Equals(current.SpaceId, transitionSpaceId)
            || !_spaces.TryResolve(current, out var transitionGrid)
            || !ReferenceEquals(transitionGrid, lifecycle.TransitionCells)
            || agent.Position != current.Cell
            || (_slipstreamLifecycles.TryGetValue(transitionSpaceId, out var registeredLifecycle) && !ReferenceEquals(registeredLifecycle, lifecycle)))
            return false;

        if (!lifecycle.TryAdvance(agentId, nextCell)) return false;
        agent.SetPosition(nextCell);
        return true;
    }

    /// <summary>
    /// Exits a registered slipstream into the exact reserved destination. Failed exits preserve
    /// transition occupancy, the agent position, and the lifecycle reservation for retry.
    /// </summary>
    public bool TryExitSlipstream(
        ulong agentId,
        string transitionSpaceId,
        SlipstreamLifecycle lifecycle,
        QualifiedGridLocation destination)
    {
        ArgumentNullException.ThrowIfNull(lifecycle);
        ArgumentNullException.ThrowIfNull(destination);
        if (!_agentsById.TryGetValue(agentId, out var agent)
            || string.IsNullOrWhiteSpace(transitionSpaceId)
            || destination.SpaceKind == ConstructionSpaceKind.Slipstream
            || !_spaces.TryResolve(destination, out var destinationGrid))
            return false;
        if (!_spaces.TryGetAgentLocation(agentId, out var current)
            || current!.SpaceKind != ConstructionSpaceKind.Slipstream
            || !StringComparer.Ordinal.Equals(current.SpaceId, transitionSpaceId)
            || !_spaces.TryResolve(current, out var transitionGrid)
            || !ReferenceEquals(transitionGrid, lifecycle.TransitionCells)
            || agent.Position != current.Cell
            || (_slipstreamLifecycles.TryGetValue(transitionSpaceId, out var registeredLifecycle) && !ReferenceEquals(registeredLifecycle, lifecycle)))
            return false;

        if (!lifecycle.TryExit(agentId, destinationGrid!, destination.Cell)) return false;
        agent.SetPosition(destination.Cell);
        return true;
    }

    /// <summary>
    /// Advances an agent along a registered skylane toward a target using the lane's direction and
    /// cells-per-tick policy. Every traversed cell must be clear; failure preserves occupancy and position.
    /// </summary>
    public bool TryAdvanceSkylane(ulong agentId, string skylaneSpaceId, Skylane lane, GridCell target)
    {
        ArgumentNullException.ThrowIfNull(lane);
        if (!_agentsById.TryGetValue(agentId, out var agent)
            || string.IsNullOrWhiteSpace(skylaneSpaceId)
            || !StringComparer.Ordinal.Equals(skylaneSpaceId, lane.LaneId)
            || !_spaces.TryGetAgentLocation(agentId, out var current)
            || current!.SpaceKind != ConstructionSpaceKind.Skylane
            || !StringComparer.Ordinal.Equals(current.SpaceId, skylaneSpaceId)
            || !_spaces.TryResolve(current, out var registeredGrid)
            || !ReferenceEquals(registeredGrid, lane.Cells)
            || agent.Position != current.Cell
            || !lane.TryGetNextCell(current.Cell, target, out var next))
            return false;

        var distance = Math.Abs(next.X - current.Cell.X) + Math.Abs(next.Y - current.Cell.Y);
        var dx = Math.Sign(next.X - current.Cell.X);
        var dy = Math.Sign(next.Y - current.Cell.Y);
        var cell = current.Cell;
        for (var step = 0; step < distance; step++)
        {
            cell = new GridCell(cell.X + dx, cell.Y + dy);
            if (lane.Cells.IsOccupied(cell)) return false;
        }

        if (!lane.Cells.TryMove(current.Cell, next, agentId)) return false;
        agent.SetPosition(next);
        return true;
    }

    /// <summary>Gets the authoritative target grid.</summary>
    public BuildGrid BuildGrid => _buildGrid;

    /// <summary>Advances each agent once in stable identifier order and commits resulting state.</summary>
    public ConstructionSimulationSnapshot Step()
    {
        // Validate every agent that this scheduler can advance before mutating any agent, task,
        // emitter, or build-grid state. Otherwise a later mismatch could throw after earlier
        // agents had already advanced, leaving a partially applied tick with an unchanged tick ID.
        foreach (var agent in _agents)
        {
            if (!_spaces.TryGetAgentLocation(agent.AgentId, out var committedLocation)
                || committedLocation!.SpaceKind != ConstructionSpaceKind.SimulationGrid
                || !StringComparer.Ordinal.Equals(committedLocation.SpaceId, _spaceId))
                continue;

            if (agent.Position != committedLocation.Cell)
                throw new InvalidOperationException(
                    $"Agent {agent.AgentId} position {agent.Position} does not match committed location {committedLocation}.");
        }

        foreach (var agent in _agents)
        {
            // Coordinates are local to their owning grid. Until lane lifecycle integration is
            // complete, this simulation may advance only agents authoritatively occupying its grid.
            if (!_spaces.TryGetAgentLocation(agent.AgentId, out var committedLocation)
                || committedLocation!.SpaceKind != ConstructionSpaceKind.SimulationGrid
                || !StringComparer.Ordinal.Equals(committedLocation.SpaceId, _spaceId))
                continue;

            switch (agent.State)
            {
                case ConstructionAgentState.Idle:
                    if (_scheduler.TryAssignNext(agent, out var task) && task is not null)
                        _agentTasks[agent.AgentId] = task;
                    break;

                case ConstructionAgentState.QueuedAtEmitter:
                    if (!_agentTasks.TryGetValue(agent.AgentId, out var pickupTask))
                        throw new InvalidOperationException("An assigned agent must have a tracked placement task.");
                    var stocked = _emitters.FirstOrDefault(item => item.Emitter.Inventory > 0);
                    if (stocked is null)
                    {
                        _scheduler.Requeue(pickupTask.TaskId, agent);
                        _agentTasks.Remove(agent.AgentId);
                        break;
                    }
                    if (agent.Position != stocked.Position)
                    {
                        MoveOneCell(agent, stocked.Position);
                        break;
                    }
                    if (stocked.Emitter.TryTakeBlock())
                        agent.TryTransition(ConstructionAgentState.CarryingBlock);
                    break;

                case ConstructionAgentState.CarryingBlock:
                    agent.TryTransition(ConstructionAgentState.AwaitingTransition);
                    break;

                case ConstructionAgentState.AwaitingTransition:
                    // Explicit intent belongs to this simulation and must be admitted or cancelled
                    // before ordinary scheduling may advance the agent.
                    if (!_pendingLaneTransitionIntents.ContainsKey(agent.AgentId))
                        agent.TryTransition(ConstructionAgentState.Traveling);
                    break;

                case ConstructionAgentState.Traveling:
                    if (!_agentTasks.TryGetValue(agent.AgentId, out var travelTask))
                        throw new InvalidOperationException("A traveling agent must have a tracked placement task.");
                    var target = new GridCell(travelTask.X, travelTask.Y);
                    if (agent.Position != target) MoveOneCell(agent, target);
                    if (agent.Position == target) agent.TryTransition(ConstructionAgentState.PlacingBlock);
                    break;

                case ConstructionAgentState.PlacingBlock:
                    if (!_agentTasks.TryGetValue(agent.AgentId, out var placementTask))
                        throw new InvalidOperationException("A placing agent must have a tracked placement task.");
                    if (!agent.IsCarryingBlock || !_buildGrid.TryPlaceBlock(placementTask.X, placementTask.Y))
                        throw new InvalidOperationException("Placement invariant failed; block was not committed.");
                    if (!agent.TryTransition(ConstructionAgentState.Completed))
                        throw new InvalidOperationException("The agent rejected a legal completion transition.");
                    if (!_scheduler.Complete(placementTask.TaskId, agent))
                        throw new InvalidOperationException("The scheduler rejected a completed task.");
                    _agentTasks.Remove(agent.AgentId);
                    break;
            }
        }

        _tick = checked(_tick + 1);
        return Snapshot();
    }

    /// <summary>Returns a stable, immutable snapshot of the last committed simulation state.</summary>
    public ConstructionSimulationSnapshot Snapshot()
    {
        var placed = new int[checked(_buildGrid.Width * _buildGrid.Height)];
        for (var y = 0; y < _buildGrid.Height; y++)
        for (var x = 0; x < _buildGrid.Width; x++)
            placed[(y * _buildGrid.Width) + x] = _buildGrid[x, y].PlacedBlocks;

        return new ConstructionSimulationSnapshot(
            _tick,
            _buildGrid.RequiredBlockCount,
            _buildGrid.PlacedBlockCount,
            _buildGrid.IsComplete,
            Array.AsReadOnly(_agents.Select(agent =>
            {
                var location = _spaces.GetAgentLocation(agent.AgentId);
                return new ConstructionAgentSnapshot(agent.AgentId, location.Cell, agent.State,
                    agent.IsCarryingBlock, agent.AssignedTaskId, location);
            }).ToArray()),
            Array.AsReadOnly(_emitters.Select(item => new ConstructionEmitterSnapshot(item.Emitter.EmitterId, item.Position, item.Emitter.Inventory)).ToArray()),
            Array.AsReadOnly(placed));
    }

    private void MoveOneCell(ConstructionAgent agent, GridCell target)
    {
        var current = agent.Position;
        var next = current.X != target.X
            ? new GridCell(current.X + Math.Sign(target.X - current.X), current.Y)
            : new GridCell(current.X, current.Y + Math.Sign(target.Y - current.Y));
        if (!_traffic.Contains(next)) return;
        if (!_traffic.TryMove(current, next, agent.AgentId)) return;
        agent.SetPosition(next);
    }
}
