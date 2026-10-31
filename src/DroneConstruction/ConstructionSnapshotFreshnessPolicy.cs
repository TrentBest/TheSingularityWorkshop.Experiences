namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>Result of evaluating a caller-timestamped snapshot against an explicit freshness budget.</summary>
public enum ConstructionSnapshotFreshness
{
    /// <summary>The capture timestamp is at or before evaluation time and within the age budget.</summary>
    Fresh,

    /// <summary>The capture timestamp is older than the configured maximum age.</summary>
    Stale,

    /// <summary>The capture timestamp is later than the caller-provided evaluation timestamp.</summary>
    FutureDated
}

/// <summary>
/// Deterministic, caller-clock-owned freshness policy for presentation consumers.
/// Capture time is supplied separately because simulation snapshots deliberately do not own wall-clock time.
/// This policy never reads a system clock and never changes simulation or observer state.
/// </summary>
public sealed record ConstructionSnapshotFreshnessPolicy
{
    /// <summary>Creates a policy with a non-negative maximum snapshot age.</summary>
    public ConstructionSnapshotFreshnessPolicy(TimeSpan maximumAge)
    {
        if (maximumAge < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(maximumAge));

        MaximumAge = maximumAge;
    }

    /// <summary>Maximum permitted elapsed time between capture and evaluation.</summary>
    public TimeSpan MaximumAge { get; }

    /// <summary>
    /// Evaluates a caller-provided capture timestamp against a caller-provided current timestamp.
    /// A capture timestamp later than the evaluation timestamp is FutureDated, never Fresh.
    /// </summary>
    public ConstructionSnapshotFreshness Evaluate(DateTimeOffset capturedAt, DateTimeOffset evaluatedAt)
    {
        if (capturedAt > evaluatedAt)
            return ConstructionSnapshotFreshness.FutureDated;

        return evaluatedAt - capturedAt <= MaximumAge
            ? ConstructionSnapshotFreshness.Fresh
            : ConstructionSnapshotFreshness.Stale;
    }
}
