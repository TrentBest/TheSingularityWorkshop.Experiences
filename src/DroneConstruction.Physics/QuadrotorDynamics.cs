using TheSingularityWorkshop.Physics.Dynamics;
using TheSingularityWorkshop.Physics.Numerics;
using TheSingularityWorkshop.Physics.Spatial;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Physics;

/// <summary>Immutable achieved body state and the four propeller angular speeds.</summary>
/// <remarks>Rotor speeds are radians per second; the body state uses the Physics library's world-frame metres and seconds.</remarks>
/// <param name="Body">Achieved rigid-body state.</param>
/// <param name="Rotor0RadiansPerSecond">Rotor zero angular speed.</param>
/// <param name="Rotor1RadiansPerSecond">Rotor one angular speed.</param>
/// <param name="Rotor2RadiansPerSecond">Rotor two angular speed.</param>
/// <param name="Rotor3RadiansPerSecond">Rotor three angular speed.</param>
public readonly record struct QuadrotorState(
    RigidBodyState Body,
    double Rotor0RadiansPerSecond,
    double Rotor1RadiansPerSecond,
    double Rotor2RadiansPerSecond,
    double Rotor3RadiansPerSecond)
{
    /// <summary>Returns the angular speed of a rotor by its stable zero-based index.</summary>
    public double RotorSpeed(int index) => index switch
    {
        0 => Rotor0RadiansPerSecond,
        1 => Rotor1RadiansPerSecond,
        2 => Rotor2RadiansPerSecond,
        3 => Rotor3RadiansPerSecond,
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };
}

/// <summary>
/// A compact propeller-driven quadrotor dynamics stepper built on Workshop Physics.
/// </summary>
/// <remarks>
/// Coordinates use a right-handed, Z-up world/body convention. Position is metres,
/// time seconds, mass kilograms, thrust newtons, inertia kg m², angular speed rad/s,
/// and angular velocity rad/s. Rotor order is front-left, front-right, rear-right,
/// rear-left in the body XY plane; alternating spin directions produce reaction torque.
/// Motor commands set target rotor speed, not vehicle orientation. Propeller thrust and
/// torque, rigid-body inertia, gravity, and angular integration determine achieved motion.
/// This reference model omits ground contact, blade aerodynamics, voltage sag, gyroscopic
/// rotor coupling, and a closed-loop flight controller; it is not hardware-validation evidence.
/// </remarks>
public sealed class QuadrotorDynamics
{
    private const double GravityMetresPerSecondSquared = 9.80665;
    private readonly double _massKg;
    private readonly Vector3d _diagonalInertiaKgMetresSquared;
    private readonly double _armLengthMetres;
    private readonly double _maximumRotorSpeedRadiansPerSecond;
    private readonly double _maximumRotorThrustNewtons;
    private readonly double _maximumReactionTorqueNewtonMetres;
    private readonly double _motorTimeConstantSeconds;
    private readonly double _thrustCoefficient;
    private readonly double _reactionTorqueCoefficient;
    private readonly double[] _motorEfficiency = new double[4];

    /// <summary>Creates a validated, symmetric quadrotor model.</summary>
    public QuadrotorDynamics(
        double massKg = 1.2d,
        Vector3d? diagonalInertiaKgMetresSquared = null,
        double armLengthMetres = 0.18d,
        double maximumRotorSpeedRadiansPerSecond = 900d,
        double maximumRotorThrustNewtons = 8d,
        double maximumReactionTorqueNewtonMetres = 0.12d,
        double motorTimeConstantSeconds = 0.04d,
        uint worldSeed = 0x51A7E123u,
        uint stableDroneNumericId = 1u,
        double motorEfficiencyVariationFraction = 0.02d)
    {
        var inertia = diagonalInertiaKgMetresSquared ?? new Vector3d(0.018d, 0.018d, 0.032d);
        if (!double.IsFinite(massKg) || massKg <= 0d)
            throw new ArgumentOutOfRangeException(nameof(massKg));
        if (!double.IsFinite(inertia.X) || !double.IsFinite(inertia.Y) || !double.IsFinite(inertia.Z) ||
            inertia.X <= 0d || inertia.Y <= 0d || inertia.Z <= 0d)
            throw new ArgumentOutOfRangeException(nameof(diagonalInertiaKgMetresSquared));
        if (!double.IsFinite(armLengthMetres) || armLengthMetres <= 0d)
            throw new ArgumentOutOfRangeException(nameof(armLengthMetres));
        if (!double.IsFinite(maximumRotorSpeedRadiansPerSecond) || maximumRotorSpeedRadiansPerSecond <= 0d)
            throw new ArgumentOutOfRangeException(nameof(maximumRotorSpeedRadiansPerSecond));
        if (!double.IsFinite(maximumRotorThrustNewtons) || maximumRotorThrustNewtons <= 0d)
            throw new ArgumentOutOfRangeException(nameof(maximumRotorThrustNewtons));
        if (!double.IsFinite(maximumReactionTorqueNewtonMetres) || maximumReactionTorqueNewtonMetres < 0d)
            throw new ArgumentOutOfRangeException(nameof(maximumReactionTorqueNewtonMetres));
        if (!double.IsFinite(motorTimeConstantSeconds) || motorTimeConstantSeconds <= 0d)
            throw new ArgumentOutOfRangeException(nameof(motorTimeConstantSeconds));

        _massKg = massKg;
        _diagonalInertiaKgMetresSquared = inertia;
        _armLengthMetres = armLengthMetres;
        _maximumRotorSpeedRadiansPerSecond = maximumRotorSpeedRadiansPerSecond;
        _maximumRotorThrustNewtons = maximumRotorThrustNewtons;
        _maximumReactionTorqueNewtonMetres = maximumReactionTorqueNewtonMetres;
        _motorTimeConstantSeconds = motorTimeConstantSeconds;
        _thrustCoefficient = maximumRotorThrustNewtons / (maximumRotorSpeedRadiansPerSecond * maximumRotorSpeedRadiansPerSecond);
        if (!double.IsFinite(motorEfficiencyVariationFraction) || motorEfficiencyVariationFraction < 0d || motorEfficiencyVariationFraction >= 1d)
            throw new ArgumentOutOfRangeException(nameof(motorEfficiencyVariationFraction));

        _reactionTorqueCoefficient = maximumReactionTorqueNewtonMetres / (maximumRotorSpeedRadiansPerSecond * maximumRotorSpeedRadiansPerSecond);
        // Stable per-motor manufacturing variation comes from Workshop Physics' keyed
        // RNG. It is independent of call order and thread scheduling.
        for (uint rotor = 0; rotor < 4; rotor++)
        {
            var sample = Squirrel3Keyed.UnitDouble(worldSeed, stableDroneNumericId, 0u, 0x44524F00u + rotor);
            _motorEfficiency[rotor] = 1d + (((sample * 2d) - 1d) * motorEfficiencyVariationFraction);
        }
    }

    /// <summary>Creates a stationary state with all four motors stopped.</summary>
    public static QuadrotorState CreateInitialState(string droneId, Vector3d worldPosition) =>
        new(
            new RigidBodyState(
                droneId,
                new Pose3d(worldPosition, Quaterniond.Identity),
                Vector3d.Zero,
                Vector3d.Zero,
                0,
                0d),
            0d, 0d, 0d, 0d);

    /// <summary>Advances motors, calculates propeller forces/torques, then integrates achieved body motion.</summary>
    /// <param name="state">The previously achieved state.</param>
    /// <param name="motorCommands">Four normalized target-speed commands in rotor order, each in [0,1].</param>
    /// <param name="deltaTimeSeconds">Positive fixed simulation step.</param>
    public QuadrotorState Step(QuadrotorState state, ReadOnlySpan<double> motorCommands, double deltaTimeSeconds)
    {
        if (!state.Body.IsValid)
            throw new ArgumentException("A valid achieved body state is required.", nameof(state));
        if (motorCommands.Length != 4)
            throw new ArgumentException("Exactly four motor commands are required.", nameof(motorCommands));
        if (!double.IsFinite(deltaTimeSeconds) || deltaTimeSeconds <= 0d)
            throw new ArgumentOutOfRangeException(nameof(deltaTimeSeconds));

        Span<double> rotorSpeeds = stackalloc double[4];
        Span<double> thrusts = stackalloc double[4];
        for (var i = 0; i < 4; i++)
        {
            var command = motorCommands[i];
            if (!double.IsFinite(command) || command < 0d || command > 1d)
                throw new ArgumentOutOfRangeException(nameof(motorCommands), "Motor commands must be finite values in [0,1].");

            var previousSpeed = state.RotorSpeed(i);
            if (!double.IsFinite(previousSpeed) || previousSpeed < 0d || previousSpeed > _maximumRotorSpeedRadiansPerSecond)
                throw new ArgumentException($"State rotor {i} speed must be finite and within the configured motor range.", nameof(state));

            var targetSpeed = command * _maximumRotorSpeedRadiansPerSecond;
            var blend = 1d - Math.Exp(-deltaTimeSeconds / _motorTimeConstantSeconds);
            rotorSpeeds[i] = state.RotorSpeed(i) + ((targetSpeed - state.RotorSpeed(i)) * blend);
            thrusts[i] = _thrustCoefficient * _motorEfficiency[i] * rotorSpeeds[i] * rotorSpeeds[i];
        }

        // Body-local +Z is the thrust axis. The four force application points create
        // roll/pitch torque through r x F; alternating rotor spin creates yaw reaction torque.
        var halfArm = _armLengthMetres / Math.Sqrt(2d);
        Span<Vector3d> rotorPositions = stackalloc Vector3d[4]
        {
            new(-halfArm, halfArm, 0d),
            new(halfArm, halfArm, 0d),
            new(halfArm, -halfArm, 0d),
            new(-halfArm, -halfArm, 0d)
        };
        Span<double> spinSigns = stackalloc double[4] { 1d, -1d, 1d, -1d };
        var bodyTorque = Vector3d.Zero;
        var totalThrust = 0d;
        for (var i = 0; i < 4; i++)
        {
            var force = new Vector3d(0d, 0d, thrusts[i]);
            bodyTorque += rotorPositions[i].Cross(force);
            bodyTorque += new Vector3d(0d, 0d,
                spinSigns[i] * _reactionTorqueCoefficient * rotorSpeeds[i] * rotorSpeeds[i]);
            totalThrust += thrusts[i];
        }

        var orientation = state.Body.Pose.Orientation;
        var worldThrust = orientation.Rotate(new Vector3d(0d, 0d, totalThrust));
        var gravity = new Vector3d(0d, 0d, -_massKg * GravityMetresPerSecondSquared);
        var worldAcceleration = (worldThrust + gravity) / _massKg;

        // Euler's rigid-body equation in body coordinates:
        // I * alpha = torque - omega x (I * omega).
        var omegaBody = state.Body.Pose.TransformDirectionToLocal(state.Body.AngularVelocity);
        var angularMomentum = new Vector3d(
            _diagonalInertiaKgMetresSquared.X * omegaBody.X,
            _diagonalInertiaKgMetresSquared.Y * omegaBody.Y,
            _diagonalInertiaKgMetresSquared.Z * omegaBody.Z);
        var gyroscopicTerm = omegaBody.Cross(angularMomentum);
        var angularAccelerationBody = new Vector3d(
            (bodyTorque.X - gyroscopicTerm.X) / _diagonalInertiaKgMetresSquared.X,
            (bodyTorque.Y - gyroscopicTerm.Y) / _diagonalInertiaKgMetresSquared.Y,
            (bodyTorque.Z - gyroscopicTerm.Z) / _diagonalInertiaKgMetresSquared.Z);
        var nextOmegaBody = omegaBody + (angularAccelerationBody * deltaTimeSeconds);
        var nextOmegaWorld = orientation.Rotate(nextOmegaBody);

        // The Workshop integrator advances position and orientation from achieved
        // velocities; it does not set a target heading or teleport the body pose.
        var bodyWithAchievedAngularVelocity = new RigidBodyState(
            state.Body.BodyId,
            state.Body.Pose,
            state.Body.LinearVelocity,
            nextOmegaWorld,
            state.Body.StepIndex,
            state.Body.ElapsedTime);
        var nextBody = RigidBodyIntegrator.Advance(bodyWithAchievedAngularVelocity, worldAcceleration, deltaTimeSeconds);

        return new QuadrotorState(
            nextBody,
            rotorSpeeds[0], rotorSpeeds[1], rotorSpeeds[2], rotorSpeeds[3]);
    }
}
