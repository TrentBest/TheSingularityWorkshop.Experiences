namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>Explicit lifecycle states for a simulated construction agent.</summary>
public enum ConstructionAgentState
{
    Idle,
    QueuedAtEmitter,
    CarryingBlock,
    AwaitingTransition,
    Traveling,
    PlacingBlock,
    Completed,
    Faulted
}

/// <summary>One uniquely identified block-placement task.</summary>
public sealed record PlacementTask(ulong TaskId, int X, int Y, int BlockOrdinal)
{
    public bool IsValid => TaskId != 0 && X >= 0 && Y >= 0 && BlockOrdinal > 0;
}

/// <summary>
/// A discrete construction agent. This class models task and lifecycle state only; it does not claim flight dynamics.
/// </summary>
public sealed class ConstructionAgent
{
    public ConstructionAgent(ulong agentId, GridCell initialPosition)
    {
        if (agentId == 0) throw new ArgumentOutOfRangeException(nameof(agentId));
        AgentId = agentId;
        Position = initialPosition;
    }

    public ulong AgentId { get; }
    public GridCell Position { get; private set; }
    public ConstructionAgentState State { get; private set; } = ConstructionAgentState.Idle;
    public ulong? AssignedTaskId { get; private set; }
    public bool IsCarryingBlock { get; private set; }

    public bool TryAssign(PlacementTask task)
    {
        ArgumentNullException.ThrowIfNull(task);
        if (!task.IsValid || State != ConstructionAgentState.Idle || AssignedTaskId is not null || IsCarryingBlock)
            return false;
        AssignedTaskId = task.TaskId;
        State = ConstructionAgentState.QueuedAtEmitter;
        return true;
    }

    public bool TryCancelAssignment(ulong taskId)
    {
        if (taskId == 0 || AssignedTaskId != taskId || State != ConstructionAgentState.QueuedAtEmitter || IsCarryingBlock)
            return false;
        AssignedTaskId = null;
        State = ConstructionAgentState.Idle;
        return true;
    }

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

    public int PendingCount => _pending.Count;
    public int AssignedCount => _assigned.Count;
    public int CompletedCount => _completed.Count;

    public bool Enqueue(PlacementTask task)
    {
        ArgumentNullException.ThrowIfNull(task);
        if (!task.IsValid || _pending.ContainsKey(task.TaskId) || _assigned.ContainsKey(task.TaskId) || _completed.Contains(task.TaskId))
            return false;
        _pending.Add(task.TaskId, task);
        return true;
    }

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

    public bool Complete(ulong taskId, ulong agentId)
    {
        if (taskId == 0 || agentId == 0) return false;
        if (!_assigned.TryGetValue(taskId, out var assigned) || assigned.AgentId != agentId) return false;
        _assigned.Remove(taskId);
        _completed.Add(taskId);
        return true;
    }

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

    public bool IsCompleted(ulong taskId) => _completed.Contains(taskId);
}
