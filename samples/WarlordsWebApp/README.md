# Warlords browser board

This is the first local browser manifestation of the Warlords Experience.

## Run

From the Experiences repository root:

```powershell
dotnet run --project samples/WarlordsWebApp/WarlordsWebApp.csproj
```

Open the localhost URL printed by ASP.NET Core.

## Architecture

- Warlords domain owns the map, turns, legal marches, combat, victory, and event history.
- `Warlords.Runtime` uses FSM_API 1.0.13 for the Command → Finished match lifecycle.
- GUI.Core's `GuiBuilder` creates a renderer-neutral tree from the current domain snapshot.
- GUI.Blazor manifests that tree in the browser.
- The host allow-lists `end-turn` and `march` command IDs, then rechecks each march against the current domain legal-command set before applying it.

The board does not contain a second set of game rules. The browser is a command and observation surface over the same C# domain used by the console sample and golden-vector tests.

## Safety and limits

- The sample rejects non-loopback clients and is intended for local development.
- All four kingdoms are human-controlled in this first playable slice.
- There is no account system, networked multiplayer, save/load, AI opponent, or candidate-code execution.
- This sample is not yet installed through FSM_COS or exposed as a WebPage MicroBundle. Global bundle identity and the immutable artifact/manifest contract remain prerequisites.
