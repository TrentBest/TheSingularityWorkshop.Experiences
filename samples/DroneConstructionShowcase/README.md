# Drone Construction browser showcase

This executable runs the real deterministic `ConstructionSimulation`, captures committed snapshots, and writes a self-contained HTML artifact that can be opened in a browser or attached to a web experience.

## Run locally

```sh
dotnet run --project samples/DroneConstructionShowcase/DroneConstructionShowcase.csproj --configuration Release -- ./artifacts/drone-construction-showcase.html
```

The process exits unsuccessfully if the simulation does not complete its required build. The generated page contains the captured simulation frames and run summary; no Unity runtime or external browser library is required.

## CI proof

The `drone-construction-showcase` job in the repository build workflow executes the scenario and uploads `drone-construction-browser-showcase` as a downloadable workflow artifact.

## Honest boundary

This is a functional show-and-tell slice driven by actual committed simulation snapshots. It is **not yet** the full Renderer → Visualize → GUI.SVG observer-relative integration, nor a deployed WebPage route. The next integration step is to map selected construction agents/aggregates into Renderer representation decisions, submit the resulting frame through Visualize, and then host the resulting live view in WebPage. Simulation population and presentation workload must continue to be measured separately.