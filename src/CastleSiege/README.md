# Castle Siege Experience

This project is the independently buildable semantic entry surface for the Castle Siege Experience.

## Current status

**A deterministic, abstract tabletop rules slice now exists.** `CastleSiegeTabletopSession` models the West Wall Demonstration without a renderer, host loop, random seed, Physics solver, or new MicroBundle identity. It validates active roles and legal actions, records ordered before/after state evidence, and resolves attacker/defender terminal outcomes deterministically.

Its events are explicitly classified as `abstract-tabletop`. A tabletop wall-integrity change is not a calculated projectile impact or physical damage observation. The MicroBundle entry surface still reports that the live Physics adapter is not connected. There is no rendered scene, physical target-intersection result, live Physics run, or multiplayer authority.

## Tabletop proof

- Scenario key: `castle.west-wall`
- Scenario version: `1`
- Rules version: `castle-siege-tabletop/1`
- Initial state: 3 abstract wall-integrity points, 3 attacker shots, and 2 defender repair tokens.
- Attacker action: spend one shot to reduce abstract wall integrity by one.
- Defender actions: spend a repair token to restore one point (up to three), or hold position.
- Terminal outcomes: attacker victory when abstract wall integrity reaches zero; defender victory when attacker shots are exhausted while the wall remains standing.
- Evidence: ordered event sequence, actor/action, rule ID, explanation, and state before/after.

The fixed numbers are tabletop rules, not measurements or physical constants. See [Tabletop Vertical Slice](../../experiences/CastleSiege/design/TABLETOP_VERTICAL_SLICE.md) and [tabletop session tests](../../tests/CastleSiege.Tests/CastleSiegeTabletopSessionTests.cs).

## Planned Physics-backed work

1. Consume a versioned Physics trajectory result through a small adapter.
2. Show actual sampled trajectory and model assumptions.
3. Query a stable target region and report hit, miss, or indeterminate.
4. Present impact observations supported by Physics.
5. Add bounded wall response only after the corresponding Physics model and tests exist.
6. Build the equal-kit construction and alternating-attack match loop as a separate, tested increment.

The Experience owns entry semantics and player progression. Physics owns physical calculations and evidence. The host owns scene presentation and input. See [Castle Siege design contract](../../docs/CASTLE_SIEGE_EXPERIENCE.md).