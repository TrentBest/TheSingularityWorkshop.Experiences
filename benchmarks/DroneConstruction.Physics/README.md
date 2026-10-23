# Quadrotor population benchmark

This benchmark runs the source-only quadrotor dynamics model against fixed populations of 1,000, 10,000, and 100,000 drones. It is a reproducible baseline, not a hardware-flight performance claim.

## Latest recorded run

- **Commit:** `bf2ecedf608b00baad41a1244c3a691a0d0cedf2`
- **Workflow:** [Benchmark quadrotor populations](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/actions/runs/40039787133)
- **Runtime:** .NET 8.0.32
- **Runner:** Ubuntu 24.04, Linux 6.17, 4 logical CPUs
- **Fixed step:** 0.002 seconds (500 Hz)
- **Warm-up:** 5 ticks per population
- **Measured:** 20 ticks per population
- **Motor commands:** fixed `[0.68, 0.66, 0.67, 0.65]`
- **World seed:** `0x51A7E123`

| Drones | Measured ticks | Elapsed | ns / drone-step | Drone-steps / second | Allocated bytes total | Bytes / tick |
|---:|---:|---:|---:|---:|---:|---:|
| 1,000 | 20 | 69.070 ms | 3,453.5 | 289,560 | 376 | 18.8 |
| 10,000 | 20 | 282.704 ms | 1,413.5 | 707,453 | 376 | 18.8 |
| 100,000 | 20 | 2,326.572 ms | 1,163.3 | 859,634 | 40 | 2.0 |

## How to interpret this

- The measured 100,000-drone run completed 2 million drone-steps in about 2.33 seconds on this hosted runner.
- Reported throughput is for this model, runtime, runner, command pattern, and short measurement window only. It is not a prediction of a full simulation with sensors, collision/ground contact, control loops, rendering, or networking.
- The 1,000-drone result is slower per step than the larger populations. This short run is susceptible to timer/runtime noise and is not sufficient evidence for a scaling curve.
- Allocation is measured process-wide with `GC.GetTotalAllocatedBytes(precise: true)` during each timed window. The tiny totals are consistent with a low-allocation step path, but can include unrelated runtime allocations and should not be treated as a per-drone allocation guarantee.
- Each population is warmed up once and measured for only 20 ticks. For comparative performance work, repeat runs, longer windows, multiple runner types, and statistically summarized results should be added before making optimization decisions.

## Reproduce

From the repository root, with the Physics repository checked out beside this repository at `../TheSingularityWorkshop.Physics`:

```sh
dotnet run --project benchmarks/DroneConstruction.Physics/TheSingularityWorkshop.Experiences.DroneConstruction.Physics.Benchmarks.csproj --configuration Release -p:WorkshopUseRepositorySources=true
```

The benchmark is source-integrated and does not pack or publish the physics project. The workflow is restricted to pushes to this repository's `development` branch; the package job remains separate.

## Scope

This benchmark exercises the deterministic rigid-body/rotor dynamics step. It does not establish hardware calibration or complete flight-simulation readiness. The model still needs additional systems such as ground contact, blade-resolved aerodynamics, voltage sag, rotor gyroscopic coupling, sensor/localization fusion, and a closed-loop flight controller before it represents a broader flight stack.
