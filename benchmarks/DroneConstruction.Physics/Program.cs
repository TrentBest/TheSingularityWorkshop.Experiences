using System.Diagnostics;
using TheSingularityWorkshop.Experiences.DroneConstruction.Physics;
using TheSingularityWorkshop.Physics.Numerics;
using TheSingularityWorkshop.Physics.Spatial;

const double deltaTimeSeconds = 1d / 500d;
const int warmupTicks = 5;
const int measuredTicks = 20;
double[] commands = [0.68d, 0.66d, 0.67d, 0.65d];

Console.WriteLine("Quadrotor population benchmark");
Console.WriteLine($"Runtime: {Environment.Version}; OS: {Environment.OSVersion}; logical CPUs: {Environment.ProcessorCount}");
Console.WriteLine($"Fixed step: {deltaTimeSeconds:R}s; warm-up ticks: {warmupTicks}; measured ticks: {measuredTicks}");
Console.WriteLine("drones,ticks,elapsed_ms,ns_per_drone_step,drone_steps_per_second,allocated_bytes_total,allocated_bytes_per_tick");

foreach (var count in new[] { 1_000, 10_000, 100_000 })
{
    var dynamics = new QuadrotorDynamics[count];
    var states = new QuadrotorState[count];
    for (var i = 0; i < count; i++)
    {
        dynamics[i] = new QuadrotorDynamics(
            worldSeed: 0x51A7E123u,
            stableDroneNumericId: unchecked((uint)i + 1u));
        states[i] = QuadrotorDynamics.CreateInitialState(
            $"drone-{i + 1}",
            new Vector3d((i % 100) * 2d, (i / 100) * 2d, 10d));
    }

    for (var tick = 0; tick < warmupTicks; tick++)
        AdvanceAll(dynamics, states, commands, deltaTimeSeconds);

    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
    var allocatedBefore = GC.GetTotalAllocatedBytes(precise: true);
    var stopwatch = Stopwatch.StartNew();
    for (var tick = 0; tick < measuredTicks; tick++)
        AdvanceAll(dynamics, states, commands, deltaTimeSeconds);
    stopwatch.Stop();
    var allocatedBytes = GC.GetTotalAllocatedBytes(precise: true) - allocatedBefore;
    var steps = (long)count * measuredTicks;
    var nsPerStep = stopwatch.Elapsed.TotalNanoseconds / steps;
    var stepsPerSecond = steps / stopwatch.Elapsed.TotalSeconds;

    Console.WriteLine($"{count},{measuredTicks},{stopwatch.Elapsed.TotalMilliseconds:F3},{nsPerStep:F1},{stepsPerSecond:F0},{allocatedBytes},{(double)allocatedBytes / measuredTicks:F1}");
}

static void AdvanceAll(
    QuadrotorDynamics[] dynamics,
    QuadrotorState[] states,
    double[] commands,
    double deltaTimeSeconds)
{
    for (var i = 0; i < states.Length; i++)
        states[i] = dynamics[i].Step(states[i], commands, deltaTimeSeconds);
}
