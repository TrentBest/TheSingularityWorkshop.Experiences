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

    /// <summary>Gets the current space-qualified location of a registered agent.</summary>
    public QualifiedGridLocation GetAgentLocation(ulong agentId)
    {
        if (!_agentsById.TryGetValue(agentId, out var agent))
            throw new KeyNotFoundException($"Agent {agentId} is not registered in this simulation.");
        return new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, _spaceId, agent.Position);
    }

    /// <summary>Gets the authoritative target grid.</summary>
    public BuildGrid BuildGrid => _buildGrid;

    /// <summary>Advances each agent once in stable identifier order and commits resulting state.</summary>
    public ConstructionSimulationSnapshot Step()
    {
        foreach (var agent in _agents)
        {
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
            Array.AsReadOnly(_agents.Select(agent => new ConstructionAgentSnapshot(agent.AgentId, agent.Position, agent.State,
                agent.IsCarryingBlock, agent.AssignedTaskId,
                new QualifiedGridLocation(ConstructionSpaceKind.SimulationGrid, _spaceId, agent.Position))).ToArray()),
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
