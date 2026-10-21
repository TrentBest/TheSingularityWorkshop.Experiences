namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>Explicit lifecycle states for a simulated construction agent.</summary>
public enum ConstructionAgentState
{
    /// <summary>Indicates the agent is idle and available for assignment.: Idle.</summary>
    Idle,
    /// <summary>Indicates the agent is queued for block pickup.: QueuedAtEmitter.</summary>
    QueuedAtEmitter,
    /// <summary>Indicates the agent has acquired a block.: CarryingBlock.</summary>
    CarryingBlock,
    /// <summary>Indicates the agent is waiting for traffic admission.: AwaitingTransition.</summary>
    AwaitingTransition,
    /// <summary>Indicates the agent is traveling toward its target.: Traveling.</summary>
    Traveling,
    /// <summary>Indicates the agent is attempting block placement.: PlacingBlock.</summary>
    PlacingBlock,
    /// <summary>Indicates a task completion event state.: Completed.</summary>
    Completed,
    /// <summary>Indicates the agent has encountered a modeled fault.: Faulted.</summary>
    Faulted
}

/// <summary>One uniquely identified block-placement task.</summary>
public sealed record PlacementTask(ulong TaskId, int X, int Y, int BlockOrdinal)
{
    /// <summary>Gets whether the task has a nonzero identifier and valid target coordinates and block ordinal.: IsValid.</summary>
    public bool IsValid => TaskId != 0 && X >= 0 && Y >= 0 && BlockOrdinal > 0;
}

/// <summary>
/// A discrete construction agent. This class models task and lifecycle state only; it does not claim flight dynamics.
/// </summary>
public sealed class ConstructionAgent
{
    /// <summary>Initializes an agent with a stable identifier and starting cell.: ConstructionAgent.</summary>
    public ConstructionAgent(ulong agentId, GridCell initialPosition)
    {
        if (agentId == 0) throw new ArgumentOutOfRangeException(nameof(agentId));
        AgentId = agentId;
        Position = initialPosition;
    }

    /// <summary>Gets the stable identifier of this agent.: AgentId.</summary>
    public ulong AgentId { get; }
    /// <summary>Gets the agent's current discrete grid position.: Position.</summary>
    public GridCell Position { get; private set; }
    /// <summary>Indicates the agent is idle and available for assignment.: Idle.</summary>
    public ConstructionAgentState State { get; private set; } = ConstructionAgentState.Idle;
    /// <summary>Gets the identifier of the assigned task, if any.: AssignedTaskId.</summary>
    public ulong? AssignedTaskId { get; private set; }
    /// <summary>Gets whether the agent currently carries a block.: IsCarryingBlock.</summary>
    public bool IsCarryingBlock { get; private set; }

    /// <summary>Attempts to assign a valid task to an idle, unassigned agent.: TryAssign.</summary>
    public bool TryAssign(PlacementTask task)
    {
        ArgumentNullException.ThrowIfNull(task);
        if (!task.IsValid || State != ConstructionAgentState.Idle || AssignedTaskId is not null || IsCarryingBlock)
            return false;
        AssignedTaskId = task.TaskId;
        State = ConstructionAgentState.QueuedAtEmitter;
        return true;
    }

    /// <summary>Attempts to cancel the task assignment matching the supplied identifier.: TryCancelAssignment.</summary>
    public bool TryCancelAssignment(ulong taskId)
    {
        if (taskId == 0 || AssignedTaskId != taskId || State != ConstructionAgentState.QueuedAtEmitter || IsCarryingBlock)
            return false;
        AssignedTaskId = null;
        State = ConstructionAgentState.Idle;
        return true;
    }

    /// <summary>Attempts a legal lifecycle transition.: TryTransition.</summary>
    public bool TryTransition(ConstructionAgentState next)
    {
        var allowed = (State, next) switch
        {
            (ConstructionAgentState.QueuedAtEmitter, ConstructionAgentState.CarryingBlock) => true,
            (ConstructionAgentState.CarryingBlock, ConstructionAgentState.AwaitingTransition) => true,
            (ConstructionAgentState.AwaitingTransition, ConstructionAgentState.Traveling) => true,
            (ConstructionAgentState.Traveling, ConstructionAgentState.PlacingBlock) => true,
            (ConstructionAgentState.PlacingBlock, ConstructionAgentState.Completed) => true,
            (ConstructionAgentState.Idle, ConstructionAgentState.Faulted) => true,
            (ConstructionAgentState.QueuedAtEmitter, ConstructionAgentState.Faulted) => true,
            (ConstructionAgentState.CarryingBlock, ConstructionAgentState.Faulted) => true,
            (ConstructionAgentState.AwaitingTransition, ConstructionAgentState.Faulted) => true,
            (ConstructionAgentState.Traveling, ConstructionAgentState.Faulted) => true,
            (ConstructionAgentState.PlacingBlock, ConstructionAgentState.Faulted) => true,
            _ => false
        };

        if (!allowed) return false;
        if (next == ConstructionAgentState.CarryingBlock) IsCarryingBlock = true;
        if (next == ConstructionAgentState.Completed)
        {
            IsCarryingBlock = false;
            AssignedTaskId = null;
            next = ConstructionAgentState.Idle;
        }
        State = next;
        return true;
    }

    /// <summary>Gets the agent's current discrete grid position.: Position.</summary>
    public void SetPosition(GridCell position) => Position = position;
}

/// <summary>
/// Deterministic task assignment: lowest pending task ID is assigned to the requesting idle agent.
/// This is a queue contract, not a route planner or physical movement solver.
/// </summary>
public sealed class ConstructionTaskScheduler
{
    private readonly SortedDictionary<ulong, PlacementTask> _pending = [];
    private readonly Dictionary<ulong, (ulong AgentId, PlacementTask Task)> _assigned = [];
    private readonly HashSet<ulong> _completed = [];

    /// <summary>Gets the number of queued tasks not yet assigned or completed.: PendingCount.</summary>
    public int PendingCount => _pending.Count;
    /// <summary>Gets the number of tasks currently assigned to agents.: AssignedCount.</summary>
    public int AssignedCount => _assigned.Count;
    /// <summary>Gets the number of tasks committed as completed.: CompletedCount.</summary>
    public int CompletedCount => _completed.Count;

    /// <summary>Adds a unique valid placement task to the pending queue.: Enqueue.</summary>
    public bool Enqueue(PlacementTask task)
    {
        ArgumentNullException.ThrowIfNull(task);
        if (!task.IsValid || _pending.ContainsKey(task.TaskId) || _assigned.ContainsKey(task.TaskId) || _completed.Contains(task.TaskId))
            return false;
        _pending.Add(task.TaskId, task);
        return true;
    }

    /// <summary>Initializes an agent with a stable identifier and starting cell.: ConstructionAgent.</summary>
    public bool TryAssignNext(ConstructionAgent agent, out PlacementTask? task)
    {
        ArgumentNullException.ThrowIfNull(agent);
        task = null;
        if (_pending.Count == 0 || agent.State != ConstructionAgentState.Idle) return false;

        var first = _pending.First();
        if (!agent.TryAssign(first.Value)) return false;
        _pending.Remove(first.Key);
        _assigned.Add(first.Key, (agent.AgentId, first.Value));
        task = first.Value;
        return true;
    }

    /// <summary>Initializes an agent with a stable identifier and starting cell.: ConstructionAgent.</summary>
    public bool Complete(ulong taskId, ConstructionAgent agent)
    {
        ArgumentNullException.ThrowIfNull(agent);
        if (taskId == 0 || agent.State != ConstructionAgentState.Idle || agent.AssignedTaskId is not null || agent.IsCarryingBlock)
            return false;
        if (!_assigned.TryGetValue(taskId, out var assigned) || assigned.AgentId != agent.AgentId) return false;
        _assigned.Remove(taskId);
        _completed.Add(taskId);
        return true;
    }

    /// <summary>Initializes an agent with a stable identifier and starting cell.: ConstructionAgent.</summary>
    public bool Requeue(ulong taskId, ConstructionAgent agent)
    {
        ArgumentNullException.ThrowIfNull(agent);
        if (taskId == 0 || !_assigned.TryGetValue(taskId, out var assigned) || assigned.AgentId != agent.AgentId)
            return false;
        if (!agent.TryCancelAssignment(taskId)) return false;
        _assigned.Remove(taskId);
        _pending.Add(taskId, assigned.Task);
        return true;
    }

    /// <summary>Gets whether the specified task identifier has been completed.: IsCompleted.</summary>
    public bool IsCompleted(ulong taskId) => _completed.Contains(taskId);
}
