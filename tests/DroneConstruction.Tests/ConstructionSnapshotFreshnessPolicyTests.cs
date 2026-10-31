using TheSingularityWorkshop.Experiences.DroneConstruction;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class ConstructionSnapshotFreshnessPolicyTests
{
    [Fact]
    public void Evaluate_UsesOnlyExplicitCallerTimestampsAndInclusiveAgeBudget()
    {
        var policy = new ConstructionSnapshotFreshnessPolicy(TimeSpan.FromSeconds(5));
        var capturedAt = new DateTimeOffset(2026, 10, 31, 6, 0, 0, TimeSpan.Zero);

        Assert.Equal(ConstructionSnapshotFreshness.Fresh,
            policy.Evaluate(capturedAt, capturedAt));
        Assert.Equal(ConstructionSnapshotFreshness.Fresh,
            policy.Evaluate(capturedAt, capturedAt.AddSeconds(5)));
        Assert.Equal(ConstructionSnapshotFreshness.Stale,
            policy.Evaluate(capturedAt, capturedAt.AddSeconds(5).AddTicks(1)));
    }

    [Fact]
    public void Evaluate_RejectsFutureDatedCaptureWithoutTreatingItAsFresh()
    {
        var policy = new ConstructionSnapshotFreshnessPolicy(TimeSpan.FromMinutes(1));
        var evaluatedAt = new DateTimeOffset(2026, 10, 31, 6, 0, 0, TimeSpan.Zero);

        Assert.Equal(ConstructionSnapshotFreshness.FutureDated,
            policy.Evaluate(evaluatedAt.AddTicks(1), evaluatedAt));
    }

    [Fact]
    public void Policy_RejectsNegativeAgeAndAllowsZeroAgeBudget()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ConstructionSnapshotFreshnessPolicy(TimeSpan.FromTicks(-1)));

        var zeroAgePolicy = new ConstructionSnapshotFreshnessPolicy(TimeSpan.Zero);
        var now = new DateTimeOffset(2026, 10, 31, 6, 0, 0, TimeSpan.Zero);

        Assert.Equal(ConstructionSnapshotFreshness.Fresh, zeroAgePolicy.Evaluate(now, now));
        Assert.Equal(ConstructionSnapshotFreshness.Stale, zeroAgePolicy.Evaluate(now, now.AddTicks(1)));
    }
}
