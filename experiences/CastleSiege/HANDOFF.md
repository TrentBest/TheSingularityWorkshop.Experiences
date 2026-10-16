# Castle Siege — Session Handoff

**Handoff date:** 2026-10-16  
**Working branch:** `development`  
**Last verified branch head before this handoff:** `a78e6802bad9ae5e822de00dfcf91794a7c68547`  
**Status:** authored preforge and first content contract are documented and contract-tested; no playable simulation or live Physics integration is claimed.

## Executive summary

This session established Castle Siege as an authored Experience under `experiences/CastleSiege/`, rather than treating the C# entry-surface project as the source of truth. The first useful artifact is a small, versioned, renderer-independent description of the battlespace: terrain, a castle wall, a target region, and one siege apparatus, all with stable semantic IDs and declared coordinates.

The repository now documents the architectural boundary between authored content and physical truth. The sample uses a right-handed Y-up frame and metre-valued lengths. That is an Experience convention only; any future Physics adapter must explicitly convert to the coordinate frame supported by Physics.

## Where the work landed

### Authored Experience home

- [Castle Siege preforge README](../README.md) — folder and authority boundaries, intended vertical slice, and current maturity.
- [Experience design](design/EXPERIENCE.md) — authored purpose, journey, and acceptance intent.
- [Manifest contract](../manifest/MANIFEST.md) and [manifest schema](../contracts/castle-siege-experience-manifest.schema.json) — Experience identity and declared capability requirements.
- [MicroBundle growth plan](design/MICROBUNDLE_GROWTH_PLAN.md) — proposed semantic capability boundaries and maturity gates. These are not numeric bundle IDs and are not claims that all listed capabilities exist.
- [Battlespace content contract](design/BATTLESPACE_CONTENT_CONTRACT.md) — object identity, geometry representation, coordinate frame, validation boundary, and ownership.
- [Battlespace JSON Schema](../contracts/castle-siege-battlespace.schema.json) and [illustrative example](../contracts/castle-siege-battlespace.example.json) — first authored content shape and sample world.

### Buildable entry surface and evidence adapter

The current implementation remains under [src/CastleSiege](../../../src/CastleSiege/). It includes the Experience manifest model, a MicroBundle shell, and a projectile-trajectory evidence adapter.

The trajectory adapter currently parses a representative Physics-shaped fixture into neutral evidence. It does **not** invoke the actual Physics experiment, solve a trajectory, detect target intersections, or establish damage. The shell explicitly reports that a Physics run is not connected. Treat this as a boundary/prototype, not an integrated gameplay slice.

### Contract tests and CI

[CastleSiegeBattlespaceContentTests.cs](../../../tests/CastleSiege.Tests/CastleSiegeBattlespaceContentTests.cs) checks the authored example's schema/version identity, coordinate frame, roles, stable ID syntax and uniqueness, bounds shape, finite values, and positive dimensions. Negative cases cover duplicate IDs and zero/negative dimensions. The tests also assert key parts of the declared schema.

These are targeted contract tests, **not** a full JSON Schema Draft 2020-12 validator. The schema and example are staged as test inputs by the test project.

- [Contract-test workflow run — success](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/actions/runs/38978054035)
- [Latest documentation workflow run — success](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/actions/runs/38978105694)

Both workflows have completed successfully. The most recent commit is the documentation update at `a78e6802bad9ae5e822de00dfcf91794a7c68547`.

## Architecture decisions to preserve

1. **Experiences owns authored meaning.** Stable IDs, authored object roles, content revision, build/turn/game rules, and player journey belong here.
2. **Physics owns physical truth.** Solver outputs, trajectories, contact, intersection, impact, and any physically supported quantities must come from the Physics implementation and its tested contracts.
3. **Presentation is not evidence.** WebPage/WebApp, Visualize, and Renderer can display objects and results but cannot authoritatively declare a hit, damage, or score based on pixels or effects.
4. **Gestures owns choreography when integrated.** Crew motion should respond to explicit, versioned events rather than inventing simulation outcomes.
5. **FSM_COS composes; it is not the game loop.** MicroBundleDomain describes capability contracts; MicroBundleRepository delivers immutable versioned artifacts; FSM_COS composes compatible artifacts.
6. **Do not duplicate the Physics solver.** Use the Physics-owned projectile experiment and make the boundary explicit.
7. **Do not mint IDs prematurely.** The growth plan's semantic keys are proposals. Numeric MicroBundle IDs, versions, provider IDs, parameter IDs, and artifact hashes should be assigned only against actual contracts and artifacts.
8. **Do not confuse authored bounds with collision geometry.** Current axis-aligned boxes are descriptive authored bounds. No target-intersection result can be claimed until Physics exposes a tested supported geometry/intersection contract.

## Where I was planning to go next

### Next task: a small renderer-independent content consumer

This is the recommended next increment, not yet implemented.

- Add a minimal reader/model in the existing Castle Siege entry-surface project that accepts a caller-provided JSON string or stream; avoid hard-coding repository paths into runtime behavior.
- Read the battlespace schema's actual fields: schema version, Experience key, content revision, coordinate-frame identity/handedness/up-axis/units, and object ID/role/label/bounds.
- Preserve authored semantic IDs exactly and enumerate the four example objects without requiring Visualize, Renderer, WebPage, or Physics.
- Validate runtime-critical invariants explicitly: supported schema version and Experience key, nonempty unique IDs, supported roles/bounds kind, three finite coordinates/extents, and strictly positive extents. Fail clearly on invalid data.
- Keep the implementation small and add tests for successful load, stable ID preservation, unsupported schema version, duplicate IDs, invalid vectors, and empty/malformed input.
- State in code/docs that this validator implements selected runtime invariants; do not describe it as a full JSON Schema validator unless an actual compliant validator is adopted.
- Do not convert coordinate frames or interpret boxes as Physics geometry in this consumer.

### Then: close the Physics contract gap

Before connecting trajectories to authored targets:

1. Inspect the current Physics API/experiment and coordinate conventions directly in the Physics repository.
2. Coordinate with the Physics owner rather than changing or duplicating its solver.
3. Agree on an explicit launch input contract and output/run-evidence shape, including units, coordinate frame, model identity, deterministic assumptions, and limitations.
4. Establish a tested coordinate conversion from the Experience's right-handed Y-up frame to the actual Physics frame.
5. Require Physics to expose or approve a tested target-intersection contract and supported geometry/tolerances before reporting hit, miss, or indeterminate.
6. Only then connect actual experiment output to the evidence adapter and test against real output, not a hand-authored fixture alone.

### After the first real trajectory slice

Proceed incrementally, keeping each capability honest and independently testable:

- supported impact observation;
- Experience-owned bounded damage rules;
- explicit build/attack/reset phase and turn authority;
- event-driven crew choreography through Gestures;
- WebPage/WebApp controls and Visualize/Renderer presentation;
- eventually construction tools, alternating attacks, provenance-rich run evidence, and multiplayer authority.

The original intended first slice is deliberately small: one siege apparatus, one target, actual trajectory samples, and a defensible outcome only when the intersection contract supports it.

## Known gaps / things not to imply

- Castle Siege is **not yet playable**.
- The projectile adapter is **fixture-tested only**; no live Physics call is connected.
- No validated target-intersection, impact, or damage contract is currently established by this work.
- The authored example's boxes are not proof of a collision-ready Physics representation.
- The targeted tests do not perform complete JSON Schema validation.
- No new standalone MicroBundles or numeric IDs were minted.
- No Physics repository files were changed.
- No package was published, no merge to `master` was performed, and no branches were deleted.

## Session / branch policy

All work described here is on `development`. Preserve the repository's Sith policy: only `master` and `development` should remain long-term. This session did not delete branches or merge to `master`; branch cleanup remains a deliberate owner-controlled action.

## Suggested first read for the next agent

1. This handoff.
2. [Battlespace content contract](design/BATTLESPACE_CONTENT_CONTRACT.md).
3. [Battlespace schema](../contracts/castle-siege-battlespace.schema.json) and [example](../contracts/castle-siege-battlespace.example.json).
4. [Contract tests](../../../tests/CastleSiege.Tests/CastleSiegeBattlespaceContentTests.cs).
5. [MicroBundle growth plan](design/MICROBUNDLE_GROWTH_PLAN.md).
6. [Entry-surface project](../../../src/CastleSiege/).

**The most useful next proof is not another architecture document or another proposed bundle: it is a small tested consumer that loads the authored world without taking ownership of physics or presentation.**
