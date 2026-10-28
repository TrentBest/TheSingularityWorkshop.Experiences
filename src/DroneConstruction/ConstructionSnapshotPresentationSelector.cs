namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>Explicit observer position in the simulation grid's coordinate space.</summary>
public readonly record struct ConstructionObserverPose(GridCell Position);

/// <summary>
/// Bounded, deterministic policy for choosing individual drone representations and aggregating the rest.
/// This policy is presentation-only and must never be used to decide simulation movement or scheduling.
/// </summary>
public sealed record ConstructionSnapshotPresentationPolicy
{
    public ConstructionSnapshotPresentationPolicy(int maximumIndividualAgents, int individualRadius)
    {
        if (maximumIndividualAgents < 0)
            throw new ArgumentOutOfRangeException(nameof(maximumIndividualAgents));
        if (individualRadius < 0)
            throw new ArgumentOutOfRangeException(nameof(individualRadius));

        MaximumIndividualAgents = maximumIndividualAgents;
        IndividualRadius = individualRadius;
    }

    /// <summary>Maximum number of agents represented individually in one selection.</summary>
    public int MaximumIndividualAgents { get; }

    /// <summary>Maximum Manhattan distance eligible for an individual representation.</summary>
    public int IndividualRadius { get; }
}

/// <summary>
/// A stable aggregate of agents that are not selected for individual representation.
/// Members are ordered by agent ID and remain associated with the source snapshot provenance.
/// </summary>
public sealed record ConstructionAgentAggregate(
    string AggregateId,
    string SpaceId,
    GridCell Cell,
    IReadOnlyList<ulong> MemberAgentIds)
{
    /// <summary>Gets the number of agents represented by this aggregate.</summary>
    public int Count => MemberAgentIds.Count;
}

/// <summary>Run/tick/sequence provenance for a presentation selection.</summary>
public readonly record struct ConstructionSnapshotProvenance(string RunId, long Tick, long Sequence);

/// <summary>Immutable presentation selection derived from one committed snapshot.</summary>
public sealed record ConstructionSnapshotPresentationSelection(
    ConstructionSnapshotProvenance Provenance,
    int TotalAgentsInSpace,
    IReadOnlyList<ConstructionAgentSnapshot> IndividualAgents,
    IReadOnlyList<ConstructionAgentAggregate> Aggregates)
{
    public int SelectedIndividualCount => IndividualAgents.Count;
    public int AggregatedAgentCount => Aggregates.Sum(aggregate => aggregate.Count);
}

/// <summary>
/// Deterministically selects near-field agents up to a fixed budget and groups all remaining
/// agents by qualified simulation-grid cell. It reads snapshots only and cannot mutate simulation state.
/// </summary>
public static class ConstructionSnapshotPresentationSelector
{
    public static ConstructionSnapshotPresentationSelection Select(
        ConstructionSimulationSnapshot snapshot,
        string runId,
        long sequence,
        string spaceId,
        ConstructionObserverPose observer,
        ConstructionSnapshotPresentationPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentException.ThrowIfNullOrWhiteSpace(runId);
        ArgumentException.ThrowIfNullOrWhiteSpace(spaceId);
        ArgumentNullException.ThrowIfNull(policy);
        if (sequence < 0)
            throw new ArgumentOutOfRangeException(nameof(sequence));

        var candidates = snapshot.Agents
            .Where(agent => agent.Location.SpaceKind == ConstructionSpaceKind.SimulationGrid
                && StringComparer.Ordinal.Equals(agent.Location.SpaceId, spaceId))
            .Select(agent => new
            {
                Agent = agent,
                Distance = Math.Abs((long)agent.Location.Cell.X - observer.Position.X)
                    + Math.Abs((long)agent.Location.Cell.Y - observer.Position.Y)
            })
            .OrderBy(candidate => candidate.Distance)
            .ThenBy(candidate => candidate.Agent.AgentId)
            .ToArray();

        var selected = candidates
            .Where(candidate => candidate.Distance <= policy.IndividualRadius)
            .Take(policy.MaximumIndividualAgents)
            .Select(candidate => candidate.Agent)
            .ToArray();
        var selectedIds = selected.Select(agent => agent.AgentId).ToHashSet();

        var aggregates = candidates
            .Where(candidate => !selectedIds.Contains(candidate.Agent.AgentId))
            .GroupBy(candidate => (candidate.Agent.Location.SpaceId, candidate.Agent.Location.Cell))
            .OrderBy(group => group.Key.SpaceId, StringComparer.Ordinal)
            .ThenBy(group => group.Key.Cell.Y)
            .ThenBy(group => group.Key.Cell.X)
            .Select(group => new ConstructionAgentAggregate(
                $"construction:{group.Key.SpaceId}:{group.Key.Cell.X}:{group.Key.Cell.Y",
                group.Key.SpaceId,
                group.Key.Cell,
                Array.AsReadOnly(group.Select(candidate => candidate.Agent.AgentId).Order().ToArray())))
            .ToArray();

        return new ConstructionSnapshotPresentationSelection(
            new ConstructionSnapshotProvenance(runId, snapshot.Tick, sequence),
            candidates.Length,
            Array.AsReadOnly(selected),
            Array.AsReadOnly(aggregates));
    }
}
