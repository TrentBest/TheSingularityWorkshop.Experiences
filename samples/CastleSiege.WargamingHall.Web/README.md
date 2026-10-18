# Castle Siege Wargaming Hall — Web host

A small runnable browser surface for the Castle Siege West Wall tabletop slice. It is intentionally a host/presentation example, not a second rules engine.

## Run

From the repository root:

```sh
dotnet run --project samples/CastleSiege.WargamingHall.Web
```

Open the local URL printed by ASP.NET Core (normally `http://localhost:5000` or `https://localhost:5001`, depending on launch settings).

## What is live

- The browser requests a view projected from `CastleSiegeSessionReport` by `CastleSiegeHallViewModel`.
- Wall-integrity pips, resources, active role, outcome, legal actions, and ordered event history come from the same in-memory tabletop session.
- Selecting an offered action posts its action kind; the server derives the actor from the current session and calls `CastleSiegeTabletopSession.Apply`.
- Reset creates a new tabletop session. The server serializes state-changing and snapshot operations through one lock so simultaneous requests cannot race the demonstration session.
- The API smoke test checks the initial state, one accepted action and its transition, and reset behavior.

## Evidence boundary

The wall is an abstract tabletop token. This host does **not** simulate trajectories, target intersection, impact, or physical damage. It does not call the Physics repository. A depleted wall pip is evidence of an accepted tabletop rule only.

This is a deliberately small, inspectable proof surface. It is not a production multiplayer server: the session is in-memory and shared by visitors of this one host instance, with no identity, persistence, or multi-session isolation.
