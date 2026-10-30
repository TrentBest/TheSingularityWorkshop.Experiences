# Drone Construction simulation benchmark

This measures the authoritative discrete `ConstructionSimulation.Step()` path, including its committed immutable snapshot creation. It is separate from the quadrotor physics-kernel benchmark and excludes presentation/rendering.

- Fixed 256 × 256 grid, one required block per cell (65,536 tasks).
- One finite-inventory emitter; stable agent IDs and deterministic starting positions.
- Populations: 10, 100, and 1,000 agents.
- 10 warm-up ticks and 100 measured ticks per population.
- Reports elapsed time, ns/agent-tick, agent-ticks/second, process-wide allocated bytes, final tick, and placed/required blocks.

Run:
```sh
dotnet run --project benchmarks/DroneConstruction.Simulation/TheSingularityWorkshop.Experiences.DroneConstruction.Simulation.Benchmarks.csproj --configuration Release
```

This is a bounded baseline, not proof of a complete, starvation-free, collision-free, or 100k-agent construction scenario. It does not measure Visualize, GPU execution, or quadrotor dynamics. The agent-tick denominator is population × measured ticks, not a claim that every agent performs equal work. Allocation includes snapshots and process-wide runtime activity. Repeat equivalent runs before making performance claims.
