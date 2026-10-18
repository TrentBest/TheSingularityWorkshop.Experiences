# Castle Siege — Wargaming Hall Host

A small interactive console host for the West Wall tabletop proof. It is deliberately a consumer of the Castle Siege rules session, not a second rules implementation.

## Run it

From the repository root:

```sh
dotnet run --project samples/CastleSiege.WargamingHall
```

Choose one of the legal actions shown for the active role. Enter `q` to leave the hall and inspect the report for the accepted events so far.

## Authority boundary

- The host reads the session's current state and legal actions.
- Every selected action is applied by `CastleSiegeTabletopSession.Apply`.
- The after-action report is captured from the session and formatted by `CastleSiegeSessionReportText`.
- The host does not implement outcome rules, alter evidence, or run a separate simulation loop.
- Wall integrity is an abstract tabletop token. This sample calculates no trajectory, target intersection, impact, or physical damage.

This is a first runnable host surface, not yet the illustrated graphical Wargaming Hall. It has no Physics, renderer, network authority, or persistence integration.
