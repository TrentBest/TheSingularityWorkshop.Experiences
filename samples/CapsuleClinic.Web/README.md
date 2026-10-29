![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# Capsule Clinic browser slice

Run a small playable Capsule Clinic session in a browser. The Experiences session remains authoritative for game state, rules, keyboard mapping, intent validation, and accepted-command sequencing. The arena SVG is now manifested from its snapshot through **Renderer → Visualize → GUI.SVG**.

## Run it

The presentation bridge is source-linked while these Workshop packages are being developed together. From the Experiences repository root, check out the integration sources at these paths:

- `VisualizeSource/`: `TrentBest/TheSingularityWorkshop.Visualize`, branch `development`.
- `VisualizeSource/integrations/RendererAdapter/RendererSource/`: `TrentBest/TheSingularityWorkshop.Renderer`, branch `development`.
- `VisualizeSource/integrations/GuiSvg/GuiSource/`: `TrentBest/TheSingularityWorkshop.GUI`, branch `development`.

Then run:

    dotnet run --project samples/CapsuleClinic.Web/CapsuleClinic.Web.csproj --urls http://127.0.0.1:5088

Open http://127.0.0.1:5088. Use arrow keys to move/rotate/drop and Space to hard-drop. The five on-screen controls provide the same inputs.

## What this demonstrates

- Real browser keyboard and button events.
- Key mapping to stable `capsule.*` intent identifiers.
- Authoritative command validation and monotonically sequenced accepted commands.
- A fresh authoritative snapshot after each submitted intent.
- Four-pane arena rendering from the captured snapshot through Renderer, Visualize, and GUI.SVG.
- Accessible control labels, keyboard focus indication, and live status feedback.

## Boundary

Experiences owns the game session and authoritative state. The integration project maps an immutable snapshot into Renderer draw commands and Visualize resources; the Renderer adapter carries the frame into Visualize, and GUI.SVG produces the SVG document returned by `GET /api/arena.svg`. Rendering does not advance game rules or authorize player intents. This is a CPU/SVG presentation path, not a GPU renderer or a live Blazor GUI-event runtime. The room is in-process; this is not network multiplayer. The separate Petri-dish Lab Mode is not part of this puzzle.

## Build and test

    dotnet build samples/CapsuleClinic.Web/CapsuleClinic.Web.csproj --configuration Release
    dotnet test tests/CapsuleClinic.Tests/TheSingularityWorkshop.Experiences.CapsuleClinic.Tests.csproj --configuration Release

The browser workflow checks out the same source repositories, builds the full integration graph, and smoke-tests the state, SVG, accepted-input, and invalid-input endpoints.

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/Avatar.jpg" alt="The Singularity Workshop avatar" width="180" /></p>
