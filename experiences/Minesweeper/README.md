# Minesweeper Experience

This directory is the authored home for the Minesweeper Experience. The authoritative rules live in the host-independent project `src/Minesweeper`; presentation and host adapters must consume that domain rather than reimplementing game state.

## Current delivery status

- **Implemented in this branch:** deterministic seeded board generation, injectable fixed mine layouts, neighbor counts, first-reveal safety, reveal/flag actions, zero-cell flood reveal, win/loss states, reset, and focused domain tests.
- **Not yet implemented:** manifest binding, immutable MicroBundle artifact closure, local host materialization/activation, playable 2D manifestation, and first-person 3D manifestation.
- **Not yet verified:** CI on this candidate branch and local host playability.

Stable cell identity is `CellId(Row, Column)`. A manifestation may map that identity to pixels, a tabletop, or a 3D world position; it must not own a second copy of the rules.
