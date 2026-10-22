namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>
/// Owns explicit lane-transition requests for one simulation. Submission records intent only;
/// admission is a separate call and delegates the atomic occupancy/reservation change to the simulation.
/// </summary>
public sealed class LaneTransitionAdmissionPolicy
{
    private readonly ConstructionSimulation _simulation;

    /// <summary>Creates a policy bound to one simulation instance.</summary>
    public LaneTransitionAdmissionPolicy(ConstructionSimulation simulation) =>
        _simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));

    /// <summary>Gets the number of agents with an unadmitted lane-transition intent.</summary>
    public int PendingCount => _simulation.PendingLaneTransitionIntentCount;

    /// <summary>Gets the currently pending intent for an agent, if one exists.</summary>
    public bool TryGetPending(ulong agentId, out LaneTransitionIntent? intent) =>
        _simulation.TryGetPendingLaneTransitionIntent(agentId, out intent);

    /// <summary>
    /// Records one intent for an agent waiting at its exact qualified source location.
    /// An existing intent is never silently replaced.
    /// </summary>
    public bool TrySubmit(ulong agentId, LaneTransitionIntent intent)
    {
        ArgumentNullException.ThrowIfNull(intent);
        if (agentId == 0 || _simulation.TryGetPendingLaneTransitionIntent(agentId, out _))
            return false;

        ConstructionSimulationSnapshot snapshot;
        try
        {
            snapshot = _simulation.Snapshot();
        }
        catch (KeyNotFoundException)
        {
            return false;
        }

        var agent = snapshot.Agents.FirstOrDefault(item => item.AgentId == agentId);
        if (agent is null
            || agent.State != ConstructionAgentState.AwaitingTransition
            || agent.Location != intent.Source)
            return false;

        return _simulation.TryRegisterPendingLaneTransitionIntent(agentId, intent);
    }

    /// <summary>
    /// Attempts to admit a pending intent through the simulation's registered lifecycle.
    /// Failed admission retains the intent for inspection or retry; success consumes it.
    /// </summary>
    public bool TryAdmit(ulong agentId)
    {
        if (!_simulation.TryGetPendingLaneTransitionIntent(agentId, out var intent) || intent is null)
            return false;

        if (!_simulation.TryEnterSlipstream(
                agentId,
                intent.Source,
                intent.TransitionEntry,
                intent.Destination,
                intent.Lookahead))
            return false;

        _simulation.TryRemovePendingLaneTransitionIntent(agentId);
        return true;
    }

    /// <summary>Cancels an unadmitted intent without changing agent location or occupancy.</summary>
    public bool TryCancel(ulong agentId) => _simulation.TryRemovePendingLaneTransitionIntent(agentId);
}
