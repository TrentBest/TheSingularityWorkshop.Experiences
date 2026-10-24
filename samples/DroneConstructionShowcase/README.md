# Drone Construction browser showcase

This executable runs the real deterministic `ConstructionSimulation`, captures committed snapshots, and writes a self-contained HTML artifact that can be opened in a browser or attached to a web experience.

## Run locally

```sh
dotnet run --project samples/DroneConstructionShowcase/DroneConstructionShowcase.csproj --configuration Release -- ./artifacts/drone-construction-showcase.html
```

The process exits unsuccessfully if the simulation does not complete its required build. The generated page contains the captured simulation frames and run summary; no Unity runtime or external browser library is required.

## Open in Visual Studio

1. Open `TheSingularityWorkshop.Experiences.sln` (not just the repository folder).
2. In Solution Explorer, right-click **DroneConstructionShowcase** and choose **Set as Startup Project**.
3. Select the **Debug** or **Release** configuration and use **Build > Build Solution**.
4. Press **F5** to run with the debugger, or **Ctrl+F5** to run without it.
5. The generated HTML is written to `artifacts/drone-construction-showcase.html` relative to the process working directory. Open that file in a browser to view the showcase.

Install the **.NET 8 SDK** and the Visual Studio **.NET desktop development** workload if Visual Studio reports that the target framework or SDK is unavailable. This is a local .NET executable; it does not require choosing a GitHub Actions runner to build or run it.

## CI proof

The `drone-construction-showcase` job in the repository build workflow executes the scenario and uploads `drone-construction-browser-showcase` as a downloadable workflow artifact.

## Honest boundary

This is a functional show-and-tell slice driven by actual committed simulation snapshots. It is **not yet** the full Renderer → Visualize → GUI.SVG observer-relative integration, nor a deployed WebPage route. The next integration step is to map selected construction agents/aggregates into Renderer representation decisions, submit the resulting frame through Visualize, and then host the resulting live view in WebPage. Simulation population and presentation workload must continue to be measured separately.