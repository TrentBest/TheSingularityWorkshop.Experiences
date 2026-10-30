# Drone Construction simulation benchmark

This measures the authoritative discrete `ConstructionSimulation.Step()` path, including its committed immutable snapshot creation. It is separate from the quadrotor physics-kernel benchmark and excludes presentation/rendering.

- Fixed 256 × 256 grid, one required block per cell (65,536 tasks).
- One finite-inventory emitter at the origin; stable agent IDs; the first agent starts at the emitter while remaining agents start in a separate 32 × 32 staging cluster beyond the first construction row, leaving the initial work corridor clear.
- Populations: 10, 100, and 1,000 agents.
- 10 warm-up ticks and 250 measured ticks per population.
- Reports elapsed time, ns/agent-tick, agent-ticks/second, process-wide allocated bytes, final tick, and placed/required blocks.

The clustered starting geometry is intentional: the measured window should include pickup, movement, placement, and snapshot work rather than only the initial approach to a distant emitter. Compare the placed-block count between runs; a run with zero placements is a startup/travel measurement, not a sustained construction-throughput baseline.

Run:
```sh
dotnet run --project benchmarks/DroneConstruction.Simulation/TheSingularityWorkshop.Experiences.DroneConstruction.Simulation.Benchmarks.csproj --configuration Release
```

This is a bounded baseline, not proof of a complete, starvation-free, collision-free, or 100k-agent construction scenario. It does not measure Visualize, GPU execution, or quadrotor dynamics. The agent-tick denominator is population × measured ticks, not a claim that every agent performs equal work. Allocation includes snapshots and process-wide runtime activity. Repeat equivalent runs before making performance claims.
