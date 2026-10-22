using TheSingularityWorkshop.Experiences.DroneConstruction;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class GravitySortBrakingPolicyTests
{
    [Fact]
    public void StoppingDistanceUsesNetThrustAccelerationAfterGravity()
    {
        // 12 m/s downward, 9.8 m/s^2 gravity, 19.8 m/s^2 thrust => 10 m/s^2 net braking.
        var distance = GravitySortBrakingPolicy.GetStoppingDistance(
            verticalVelocity: -12,
            gravity: 9.8,
            maximumThrustAcceleration: 19.8);

        Assert.Equal(7.2, distance, precision: 10);
    }

    [Fact]
    public void BrakingWindowRespondsToInstantaneousVelocity()
    {
        var slowerDrone = GravitySortBrakingPolicy.ShouldBeginBraking(
            currentAltitude: 8.1, laneAltitude: 0, verticalVelocity: -8,
            gravity: 10, maximumThrustAcceleration: 14, safetyMargin: 0);
        var fasterDrone = GravitySortBrakingPolicy.ShouldBeginBraking(
            currentAltitude: 8, laneAltitude: 0, verticalVelocity: -12,
            gravity: 10, maximumThrustAcceleration: 14, safetyMargin: 0);

        Assert.False(slowerDrone); // The 8.1 m gap is just outside the 8 m stopping distance.
        Assert.True(GravitySortBrakingPolicy.ShouldBeginBraking(
            currentAltitude: 8, laneAltitude: 0, verticalVelocity: -8,
            gravity: 10, maximumThrustAcceleration: 14, safetyMargin: 0)); // Inclusive boundary.
        Assert.True(fasterDrone);
        // Once below lane altitude, continue arresting descent; the caller must also flag overshoot
        // and must not claim successful lane capture.
        Assert.True(GravitySortBrakingPolicy.ShouldBeginBraking(
            currentAltitude: -0.5, laneAltitude: 0, verticalVelocity: -8,
            gravity: 10, maximumThrustAcceleration: 14, safetyMargin: 0));
        Assert.False(GravitySortBrakingPolicy.IsLaneCaptured(
            currentAltitude: -0.5, laneAltitude: 0, verticalVelocity: 0,
            altitudeTolerance: 0.05, verticalSpeedTolerance: 0.05));
    }

    [Fact]
    public void BrakingBeginsAtStoppingDistanceIncludingSafetyMargin()
    {
        var shouldBrake = GravitySortBrakingPolicy.ShouldBeginBraking(
            currentAltitude: 8.5, laneAltitude: 0, verticalVelocity: -8,
            gravity: 10, maximumThrustAcceleration: 14, safetyMargin: 0.5);

        Assert.True(shouldBrake);
    }

    [Fact]
    public void CannotCalculateBrakingWhenThrustDoesNotExceedGravity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GravitySortBrakingPolicy.GetStoppingDistance(
                verticalVelocity: -8, gravity: 9.8, maximumThrustAcceleration: 9.8));
    }

    [Fact]
    public void LaneCaptureRequiresBothAltitudeAndVerticalSpeedTolerance()
    {
        Assert.True(GravitySortBrakingPolicy.IsLaneCaptured(
            currentAltitude: 100.02, laneAltitude: 100, verticalVelocity: 0.03,
            altitudeTolerance: 0.05, verticalSpeedTolerance: 0.05));
        Assert.False(GravitySortBrakingPolicy.IsLaneCaptured(
            currentAltitude: 100, laneAltitude: 100, verticalVelocity: -0.2,
            altitudeTolerance: 0.05, verticalSpeedTolerance: 0.05));
        Assert.False(GravitySortBrakingPolicy.IsLaneCaptured(
            currentAltitude: 100.2, laneAltitude: 100, verticalVelocity: 0,
            altitudeTolerance: 0.05, verticalSpeedTolerance: 0.05));
    }

    [Fact]
    public void NonFiniteAndNegativeConfigurationValuesAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GravitySortBrakingPolicy.GetStoppingDistance(
                verticalVelocity: double.NaN, gravity: 9.8, maximumThrustAcceleration: 20));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GravitySortBrakingPolicy.ShouldBeginBraking(
                currentAltitude: 10, laneAltitude: 0, verticalVelocity: -1,
                gravity: 9.8, maximumThrustAcceleration: 20, safetyMargin: -1));
    }
}
