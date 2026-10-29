![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# Capsule Clinic browser slice

Run a small playable Capsule Clinic session in a browser. This host uses a fixed seed, accepts keyboard and button input, submits semantic intents through CapsuleClinicLocalSession, and refreshes its SVG arena from the authoritative snapshot.

## Run it

From the Experiences repository root, run:

    dotnet run --project samples/CapsuleClinic.Web/CapsuleClinic.Web.csproj --urls http://127.0.0.1:5088

Open http://127.0.0.1:5088. Use arrow keys to move/rotate/drop and Space to hard-drop. The five on-screen controls provide the same inputs.

## What this demonstrates

- Real browser keyboard and button events.
- Key mapping to the stable capsule.* intent identifiers.
- Authoritative command validation and monotonically sequenced accepted commands.
- A fresh authoritative snapshot after each submitted intent.
- A readable SVG arena generated from that snapshot, including a seeded local player and practice opponent.
- Accessible control labels, keyboard focus indication, and live status feedback.

## Boundary

This is a runnable local host and SVG presentation slice. It does not claim that the GUI semantic tree is wired into a live Blazor event runtime, nor that Renderer or Visualize is in this rendering path. Those are separate integration contracts and remain follow-up work. The room is in-process; this is not network multiplayer. The separate Petri-dish Lab Mode is not part of this puzzle.

## Build and test

    dotnet build samples/CapsuleClinic.Web/CapsuleClinic.Web.csproj --configuration Release
    dotnet test tests/CapsuleClinic.Tests/TheSingularityWorkshop.Experiences.CapsuleClinic.Tests.csproj --configuration Release

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/Avatar.jpg" alt="The Singularity Workshop avatar" width="180" /></p>
