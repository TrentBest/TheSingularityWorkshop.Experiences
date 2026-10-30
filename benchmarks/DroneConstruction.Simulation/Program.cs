using System.Diagnostics;
using TheSingularityWorkshop.Experiences.DroneConstruction;

const int gridWidth = 256;
const int gridHeight = 256;
const int warmupTicks = 10;
const int measuredTicks = 250;
const int agentClusterWidth = 32;
Console.WriteLine("Drone Construction authoritative simulation benchmark");
Console.WriteLine($"Runtime: {Environment.Version}; OS: {Environment.OSVersion}; logical CPUs: {Environment.ProcessorCount}");
Console.WriteLine($"Grid: {gridWidth}x{gridHeight}; required height: 1 per cell; agent cluster: {agentClusterWidth}x{agentClusterWidth}; warm-up: {warmupTicks}; measured: {measuredTicks}");
Console.WriteLine("agents,ticks,elapsed_ms,ns_per_agent_tick,agent_ticks_per_second,allocated_bytes_total,allocated_bytes_per_tick,final_tick,placed_blocks,required_blocks");
foreach (var population in new[] { 10, 100, 1_000 })
{
    // Keep the initial population near the emitter and first scheduled tasks. This lets
    // the measured window include pickup, travel, placement, and snapshot work rather
    // than measuring only the long initial approach to a distant emitter.
    var agents = Enumerable.Range(0, population).Select(index => new ConstructionAgent(
        checked((ulong)index + 1UL),
        new GridCell(index % agentClusterWidth, index / agentClusterWidth))).ToArray();
    var simulation = new ConstructionSimulation(gridWidth, gridHeight, static (_, _) => 1, agents,
        [new PositionedBlockEmitter(new GridCell(0, 0),
            new BlockEmitter("benchmark-emitter", gridWidth * gridHeight))]);
    for (var i = 0; i < warmupTicks; i++) simulation.Step();
    GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
    var allocatedBefore = GC.GetTotalAllocatedBytes(precise: true);
    var stopwatch = Stopwatch.StartNew();
    ConstructionSimulationSnapshot? snapshot = null;
    for (var i = 0; i < measuredTicks; i++) snapshot = simulation.Step();
    stopwatch.Stop();
    var allocated = GC.GetTotalAllocatedBytes(precise: true) - allocatedBefore;
    var agentTicks = (long)population * measuredTicks;
    Console.WriteLine($"{population},{measuredTicks},{stopwatch.Elapsed.TotalMilliseconds:F3},{stopwatch.Elapsed.TotalNanoseconds / agentTicks:F1},{agentTicks / stopwatch.Elapsed.TotalSeconds:F0},{allocated},{(double)allocated / measuredTicks:F1},{snapshot!.Tick},{snapshot.PlacedBlocks},{snapshot.RequiredBlocks}");
}
