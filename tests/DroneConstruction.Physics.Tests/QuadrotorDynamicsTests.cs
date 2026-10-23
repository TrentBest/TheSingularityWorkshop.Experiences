using TheSingularityWorkshop.Experiences.DroneConstruction.Physics;
using TheSingularityWorkshop.Physics.Spatial;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Physics.Tests;

public sealed class QuadrotorDynamicsTests
{
    [Fact]
    public void DifferentialPropellerThrustChangesAchievedOrientationThroughTorque()
    {
        var dynamics = new QuadrotorDynamics(motorEfficiencyVariationFraction: 0d);
        var state = QuadrotorDynamics.CreateInitialState("drone-17", Vector3d.Zero);
        var initialOrientation = state.Body.Pose.Orientation;

        state = dynamics.Step(state, new[] { 1d, 0d, 0d, 0d }, 0.02d);
        Assert.NotEqual(initialOrientation, state.Body.Pose.Orientation);
        Assert.NotEqual(Vector3d.Zero, state.Body.AngularVelocity);
        Assert.True(state.Body.StepIndex == 1);
    }

    [Fact]
    public void SymmetricMotorCommandsProduceNoRollOrPitchTorqueWithoutMotorVariation()
    {
        var dynamics = new QuadrotorDynamics(motorEfficiencyVariationFraction: 0d);
        var state = QuadrotorDynamics.CreateInitialState("hover-test", new Vector3d(0d, 0d, 10d));
        var commands = new[] { 0.6d, 0.6d, 0.6d, 0.6d };

        state = dynamics.Step(state, commands, 0.001d);

        Assert.Equal(0d, state.Body.AngularVelocity.X, 10);
        Assert.Equal(0d, state.Body.AngularVelocity.Y, 10);
    }

    [Fact]
    public void SameSeedAndStableDroneIdentityReplayIdentically()
    {
        var first = new QuadrotorDynamics(worldSeed: 1234u, stableDroneNumericId: 88u);
        var replay = new QuadrotorDynamics(worldSeed: 1234u, stableDroneNumericId: 88u);
        var a = QuadrotorDynamics.CreateInitialState("drone-88", new Vector3d(0d, 0d, 5d));
        var b = QuadrotorDynamics.CreateInitialState("drone-88", new Vector3d(0d, 0d, 5d));
        var commands = new[] { 0.72d, 0.61d, 0.55d, 0.68d };

        for (var tick = 0; tick < 100; tick++)
        {
            a = first.Step(a, commands, 1d / 500d);
            b = replay.Step(b, commands, 1d / 500d);
        }

        Assert.Equal(a, b);
        Assert.Equal(a.Body.Pose, b.Body.Pose);
        Assert.Equal(a.Body.LinearVelocity, b.Body.LinearVelocity);
        Assert.Equal(a.Body.AngularVelocity, b.Body.AngularVelocity);
    }

    [Fact]
    public void NonFiniteInertiaIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new QuadrotorDynamics(diagonalInertiaKgMetresSquared: new Vector3d(double.NaN, 0.018d, 0.032d)));
    }

    [Fact]
    public void InvalidCarriedRotorSpeedIsRejected()
    {
        var dynamics = new QuadrotorDynamics(motorEfficiencyVariationFraction: 0d);
        var initial = QuadrotorDynamics.CreateInitialState("bad-rotor-state", Vector3d.Zero);
        var invalid = initial with { Rotor0RadiansPerSecond = double.NaN };

        Assert.Throws<ArgumentException>(() => dynamics.Step(invalid, new[] { 0d, 0d, 0d, 0d }, 0.01d));
    }

    [Fact]
    public void InvalidMotorCommandsAreRejected()
    {
        var dynamics = new QuadrotorDynamics();
        var state = QuadrotorDynamics.CreateInitialState("invalid-command-test", Vector3d.Zero);

        Assert.Throws<ArgumentException>(() => dynamics.Step(state, new[] { 0d, 0d, 0d }, 0.01d));
        Assert.Throws<ArgumentOutOfRangeException>(() => dynamics.Step(state, new[] { 0d, 0d, 0d, double.NaN }, 0.01d));
        Assert.Throws<ArgumentOutOfRangeException>(() => dynamics.Step(state, new[] { 0d, 0d, 0d, 1.1d }, 0.01d));
    }
}