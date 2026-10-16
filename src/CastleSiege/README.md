# Castle Siege Experience

This project is the independently buildable semantic entry surface for the Castle Siege Experience.

## Current status

**Entry surface only.** The MicroBundle provides an honest introduction and explicitly reports that the Physics adapter is not connected. It does not simulate a projectile, claim target hits or damage, provide a playable scene, or implement multiplayer.

## Planned vertical slice

1. Consume a versioned Physics trajectory result through a small adapter.
2. Show actual sampled trajectory and model assumptions.
3. Query a stable target region and report hit, miss, or indeterminate.
4. Present impact observations supported by Physics.
5. Add bounded wall response only after the corresponding Physics model and tests exist.
6. Build the equal-kit construction and alternating-attack match loop as a separate, tested increment.

The Experience owns entry semantics and player progression. Physics owns physical calculations and evidence. The host owns scene presentation and input. See [Castle Siege design contract](../../docs/CASTLE_SIEGE_EXPERIENCE.md).