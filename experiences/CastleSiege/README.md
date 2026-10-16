# Castle Siege — Preforge Experience

**Status:** preforge construct. This directory is the authored home of the Castle Siege Experience before it becomes a complete, host-runnable product.

Castle Siege is a Singularity City destination that consumes Workshop capabilities. The Experience owns the visitor journey, authored world/build content, rules, progression, and the contract that joins those pieces. It does not own the physics solver, rendering engine, host, or composition runtime.

## Folder contract

- `design/` — experience brief, player journey, rules, fairness, milestones, and acceptance criteria.
- `manifest/` — versioned Experience identity, artifact references, and declared capability requirements.
- `content/` — authored scene/build-kit definitions and stable IDs. Keep authored geometry separate from computed physical state.
- `contracts/` — cross-repository input/output contracts, schema examples, and capability status.
- `integration/` — host/Physics/Visualize/Gestures integration notes and verified integration tests.
- `artifacts/` — generated evidence and review captures only when provenance is recorded; do not commit arbitrary build output.

The directory is intentionally a design/content boundary, not a new engine or simulation loop. These subdirectories are introduced as work arrives; empty placeholder folders are not required.

## Authority boundaries

| Concern | Authority |
|---|---|
| Experience identity, rules, build phase, turn sequence, player journey, authored content | Experiences / this preforge |
| Equations, physical state, trajectory, confirmed intersections, impact and supported damage | TheSingularityWorkshop.Physics |
| Scene hosting, camera, controls, overlays and interaction feedback | WebPage / WebApp manifestation |
| Drawing, appearance, effects and LOD | Visualize / Renderer or host presentation |
| Articulated crew choreography | Gestures, when integrated |
| Verified artifact composition and capability arbitration | FSM_COS |

Presentation must not invent hits, damage, or scores. Physics must not depend on Experiences, WebPage, Visualize, Renderer, or Gestures. FSM_COS composes capabilities; it does not become the game loop.

## First vertical slice

1. Author one small terrain patch, castle silhouette, stable target identity, and one siege apparatus.
2. Accept explicit launch-state inputs supported by the current Physics model.
3. Run the real Physics experiment and display its actual samples.
4. Query the target and report hit, miss, or indeterminate only when a supported intersection contract exists.
5. Show run/model identity, units, assumptions, and limitations beside the result.
6. Add impact response, bounded damage, construction rules, alternating attacks, and multiplayer only as their contracts become executable and tested.

The first slice is not a trebuchet mechanism simulator, rigid-body castle solver, fracture system, or multiplayer game. Do not present fixtures or visual effects as computed evidence.

## Current implementation pointers

- [Experience design contract](../../docs/CASTLE_SIEGE_EXPERIENCE.md)
- [Manifest contract](../../docs/CASTLE_SIEGE_MANIFEST.md)
- [Manifest schema](../../docs/schemas/castle-siege-experience-manifest.schema.json)
- [Manifest example](../../docs/schemas/castle-siege-experience-manifest.example.json)
- [Current entry-surface project](../../src/CastleSiege/)
- [Experience roadmap](../../docs/ROADMAP.md)

The current C# project is only an independently buildable entry surface and evidence-adapter contract. It is not yet the playable Experience. The JSON adapter is fixture-tested; live integration with the actual Physics experiment remains pending.
