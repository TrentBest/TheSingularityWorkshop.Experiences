using System.Globalization;
using TheSingularityWorkshop.MicroBundleDomain;
using TheSingularityWorkshop.Physics.Kinematics;
using TheSingularityWorkshop.Physics.MicroBundles.Kinematics;
using TheSingularityWorkshop.Physics.Spatial;

// This is a host-composition contract sample, not an authored Lenny scene placement.
// The 1 m path is deliberately synthetic until artwork-relative scene coordinates are approved.
var bundle = new PrescribedKinematicMotionMicroBundle();
IMicroBundle loadedBundle = bundle;
loadedBundle.Load(new SampleLoadContext());

if (!loadedBundle.Descriptor.Providers.Any(provider =>
        provider.Id == PrescribedKinematicMotionMicroBundle.ProviderId))
{
    throw new InvalidOperationException("The prescribed-motion provider is not declared by the loaded bundle.");
}

if (loadedBundle is not IPrescribedKinematicMotionProvider motionProvider)
{
    throw new InvalidOperationException("The loaded bundle does not expose the typed prescribed-motion contract.");
}

var definition = new PrescribedKinematicMotionDefinition(
    definitionId: "lenny.host-sample.walk",
    actorId: "actor:lenny",
    startPose: new Pose3d(Vector3d.Zero, Quaterniond.Identity),
    destinationPose: new Pose3d(new Vector3d(1d, 0d, 0d), Quaterniond.Identity),
    startTimeSeconds: 0.5d,
    durationSeconds: 2d,
    maximumLinearSpeed: 1d,
    maximumAngularSpeed: 0d,
    profile: PrescribedKinematicMotionProfile.QuinticSmoothStep,
    maximumLinearAcceleration: 1.5d,
    maximumAngularAcceleration: 0d);

const double fixedTimeStepSeconds = 0.1d;
const long totalStepCount = 20;
PrescribedKinematicMotionCommit? finalCommit = null;

for (long step = 0; step <= totalStepCount; step++)
{
    var requested = motionProvider.EvaluateFixedStep(
        definition, step, fixedTimeStepSeconds, totalStepCount);

    // This sample host has no contacts, obstacles, or competing world constraints.
    // Its explicit kinematic-world policy accepts the prescribed sample as achieved.
    // A real host must supply its independently committed RigidBodyState here.
    var achieved = requested.State;
    var committed = motionProvider.Commit(
        definition,
        requested,
        achieved,
        positionToleranceMeters: 1e-9d,
        angularToleranceRadians: 1e-9d);

    finalCommit = committed;
    Console.WriteLine(
        $"step={step:D2} time={achieved.ElapsedTime.ToString("0.000", CultureInfo.InvariantCulture)} " +
        $"x={achieved.Pose.Position.X.ToString("0.000000", CultureInfo.InvariantCulture)} " +
        $"vx={achieved.LinearVelocity.X.ToString("0.000000", CultureInfo.InvariantCulture)} " +
        $"arrived={committed.IsArrived}");
}

if (finalCommit is not { IsArrived: true })
{
    throw new InvalidOperationException("The host did not commit the actor at the destination.");
}

Console.WriteLine("Lenny prescribed-motion provider host composition: PASS");

internal sealed class SampleLoadContext : IMicroBundleLoadContext
{
    public ulong RuntimeId => 1UL;

    public bool TryGetConfiguration(ulong bundleId, out ReadOnlyMemory<byte> configuration)
    {
        configuration = ReadOnlyMemory<byte>.Empty;
        return false;
    }
}
