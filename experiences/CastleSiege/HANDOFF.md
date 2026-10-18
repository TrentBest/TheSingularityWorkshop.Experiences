# Castle Siege — Session Handoff

**Handoff date:** 2026-10-18  
**Working branch:** `development`  
**Status:** authored preforge and battlespace contract are documented and contract-tested; the deterministic abstract tabletop rules core, immutable session-report snapshot, plain-text report presenter, and focused tests are committed. The latest observed workflow for commit `4c470304` completed: Castle Siege build, source-integration, and focused tests passed; repository-wide status failed because the separate DominoStudio test and build jobs failed. No live Physics integration or rendered playable scene is claimed.

## Executive summary

Castle Siege is an authored Experience under `experiences/CastleSiege/`, not a physics solver or presentation engine. Its first authored battlespace describes terrain, a castle wall, a target region, and one siege apparatus using stable semantic IDs and declared coordinates.

The West Wall Demonstration now has an executable, renderer-independent tabletop rules core in [CastleSiegeTabletopSession.cs](../../../src/CastleSiege/CastleSiegeTabletopSession.cs). It is a small deterministic abstract rules exercise, independent of Physics: a session driver can choose an active role's legal action, inspect a transition, and replay the accepted action sequence. Outcomes are classified as `abstract-tabletop`, never as calculated impacts.

## Where the work landed

### Authored Experience and tabletop design

- [Castle Siege preforge README](../README.md) — folder and authority boundaries, intended vertical slice, and current maturity.
- [Experience design](design/EXPERIENCE.md) — authored purpose, journey, and acceptance intent.
- [Tabletop vertical slice](design/TABLETOP_VERTICAL_SLICE.md) — first bounded playable-loop brief, rules contract checklist, event/evidence requirements, acceptance gates, contribution boundaries, and the implemented session/report snapshot.
- [Manifest contract](../manifest/MANIFEST.md) and [manifest schema](../contracts/castle-siege-experience-manifest.schema.json) — Experience identity and declared capability requirements.
- [MicroBundle growth plan](design/MICROBUNDLE_GROWTH_PLAN.md) — proposed semantic capability boundaries and maturity gates. These are not numeric bundle IDs and are not claims that all listed capabilities exist.
- [Battlespace content contract](design/BATTLESPACE_CONTENT_CONTRACT.md) — object identity, geometry representation, coordinate frame, validation boundary, and ownership.
- [Battlespace JSON Schema](../contracts/castle-siege-battlespace.schema.json) and [illustrative example](../contracts/castle-siege-battlespace.example.json) — first authored content shape and sample world.

### Deterministic tabletop rules core

[CastleSiegeTabletopSession.cs](../../../src/CastleSiege/CastleSiegeTabletopSession.cs) adds a versioned scenario, immutable state records, explicit legal actions, role/turn enforcement, deterministic transition rules, terminal outcomes, and canonical ordered event evidence.

The initial tabletop rules use wall integrity 3, attacker shots 3, and defender repair tokens 2. Attacks spend a shot and remove one abstract wall point; repairs spend a token and restore one point up to the maximum; defenders may hold. The attacker wins at zero integrity; the defender wins when shots are exhausted with the wall standing. These are game rules, not physical constants. The core has no Physics, renderer, host-loop, random, or new MicroBundle dependencies.

[CastleSiegeSessionReport.cs](../../../src/CastleSiege/CastleSiegeSessionReport.cs) captures scenario/rules identity, current immutable state, legal actions, and ordered events as a stable read-only snapshot. Its outcome is derived from the captured state, avoiding contradictory duplicated outcome data. [CastleSiegeSessionReportText.cs](../../../src/CastleSiege/CastleSiegeSessionReportText.cs) formats that snapshot for host-side inspection, including state transitions and an explicit evidence-boundary statement. The presenter is pure text formatting: it does not mutate the session, run a second rules engine, or depend on rendering libraries.

### Buildable entry surface and evidence adapter

The current implementation remains under [src/CastleSiege](../../../src/CastleSiege/). It includes the Experience manifest model, a MicroBundle shell, and a projectile-trajectory evidence adapter.

The trajectory adapter currently parses a representative Physics-shaped fixture into neutral evidence. It does **not** invoke the actual Physics experiment, solve a trajectory, detect target intersections, or establish damage. The shell explicitly reports that a Physics run is not connected. Treat this as a boundary/prototype, not an integrated gameplay slice.

### Contract tests and CI

[CastleSiegeBattlespaceContentTests.cs](../../../tests/CastleSiege.Tests/CastleSiegeBattlespaceContentTests.cs) checks the authored example's schema/version identity, coordinate frame, roles, stable ID syntax and uniqueness, bounds shape, finite values, and positive dimensions. Negative cases cover duplicate IDs and zero/negative dimensions. The tests also assert key parts of the declared schema.

These are targeted contract tests, **not** a full JSON Schema Draft 2020-12 validator. The schema and example are staged as test inputs by the test project.

- [Contract-test workflow run — success](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/actions/runs/38978054035)
- [Battlespace-validation documentation workflow — success](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/actions/runs/38978105694)
- [Session-handoff workflow](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/actions/runs/38983748546)

Those historical runs completed successfully, but do not establish that later commits or the current branch head passed CI. Check current workflow status before making a CI claim.

## Architecture decisions to preserve

1. **Experiences owns authored meaning.** Stable IDs, authored object roles, content revision, game rules, and player journey belong here.
2. **Physics owns physical truth.** Solver outputs, trajectories, contact, intersection, impact, and any physically supported quantities must come from Physics and its tested contracts.
3. **Presentation is not evidence.** WebPage/WebApp, Visualize, and Renderer can display objects and results but cannot authoritatively declare a hit, damage, or score based on pixels or effects.
4. **Gestures owns choreography when integrated.** Crew motion should respond to explicit, versioned events rather than inventing simulation outcomes.
5. **FSM_COS composes; it is not the game loop.** MicroBundleDomain describes capability contracts; MicroBundleRepository delivers immutable versioned artifacts; FSM_COS composes compatible artifacts.
6. **Do not duplicate the Physics solver.** The tabletop rules exercise must not pretend to be physical simulation.
7. **Do not mint IDs prematurely.** The growth plan's semantic keys are proposals. Numeric MicroBundle IDs, versions, provider IDs, parameter IDs, and artifact hashes should be assigned only against actual contracts and artifacts.
8. **Do not confuse authored bounds with collision geometry.** Current axis-aligned boxes are descriptive authored bounds. No target-intersection result can be claimed until Physics exposes a tested supported geometry/intersection contract.
9. **Keep outcome authority deterministic and renderer-independent.** Animation, frame rate, and host presentation cannot change accepted actions or rule results.

## Recommended next steps

1. **Add the diegetic Wargaming Hall manifestation.** Keep the presentation as a consumer of the report and rules evidence; do not make visual effects authoritative.
2. **Exercise the report consumer in a real host.** The plain-text formatter is a first inspection surface, not a rendered or interactive scene.
4. **Continue the renderer-independent authored-content consumer.** Read caller-provided JSON and validate runtime invariants without claiming full JSON Schema validation.
5. **Close the Physics contract gap.** Inspect Physics directly, agree on launch/run-evidence contracts and coordinate conventions, and wait for a tested target-intersection contract before reporting hit/miss/indeterminate or damage.
6. **Expand only after the first proof.** Add supported impact observation, Experience-owned bounded damage rules, construction/attack/reset phases, optional Gestures choreography, and later campaign depth as independently testable increments.

## Previous implementation sequence (superseded)

### 1. Implement the tabletop rules loop

Use [Tabletop Vertical Slice](design/TABLETOP_VERTICAL_SLICE.md) as the acceptance brief.

- Inspect the existing project and test layout before selecting implementation paths; follow the repository's actual conventions.
- Define a small immutable/versioned scenario and rules model for the West Wall Demonstration, with explicit roles, legal actions, initial state, transition rules, terminal conditions, and canonical event order.
- Make action validation reject unknown roles/actions, malformed payloads, and out-of-turn requests with stable, inspectable reasons.
- Keep resolution deterministic and free of renderer, host, Physics, and frame-time dependencies.
- Record before/after state and the rule applied for each accepted action. Make replay from the initial state and action sequence testable.
- Test legal transitions, rejected actions, role/turn enforcement, terminal conditions, stable event ordering, and replay equivalence.
- Label every result as an **abstract tabletop rule result**. Do not label it a hit, impact, physical damage, or Physics result.
- Keep the implementation small enough that an independent contributor can replace or extend the rules without taking over the Experience.

### 2. Continue the renderer-independent authored-content consumer

A separate useful increment is a small reader/model in the existing entry-surface project that accepts caller-provided JSON text or a stream, preserves the authored semantic IDs, and validates selected runtime invariants. Test successful loading, unsupported schema versions, duplicate IDs, invalid vectors, and empty/malformed input. Do not describe selected checks as a full JSON Schema validator.

### 3. Close the Physics contract gap

Before connecting trajectories to authored targets:

1. Inspect the current Physics API/experiment and coordinate conventions directly in the Physics repository.
2. Coordinate with the Physics owner rather than changing or duplicating its solver.
3. Agree on an explicit launch input contract and output/run-evidence shape, including units, coordinate frame, model identity, deterministic assumptions, and limitations.
4. Establish a tested coordinate conversion from the Experience's right-handed Y-up frame to the actual Physics frame.
5. Require Physics to expose or approve a tested target-intersection contract and supported geometry/tolerances before reporting hit, miss, or indeterminate.
6. Connect actual experiment output to the evidence adapter only after those contracts exist; test against real output, not a hand-authored fixture alone.

### 4. Expand only after a real first proof

Proceed incrementally: supported impact observation; Experience-owned bounded damage rules; explicit build/attack/reset phases; event-driven crew choreography through Gestures; WebPage/WebApp controls and Visualize/Renderer presentation; eventually construction tools, alternating attacks, provenance-rich run evidence, and multiplayer authority.

## Known gaps / things not to imply

- Castle Siege is **not yet playable**.
- The tabletop rules core is implemented; the diegetic UI and full host-runnable Experience remain unfinished.
- The projectile adapter is **fixture-tested only**; no live Physics call is connected.
- No validated target-intersection, impact, or damage contract is established by this work.
- The authored example's boxes are not proof of a collision-ready Physics representation.
- The targeted battlespace tests do not perform complete JSON Schema validation.
- No new standalone MicroBundles or numeric IDs were minted.
- No Physics repository files were changed.
- No package was published, no merge to `master` was performed, and no branches were deleted.

## Session / branch policy

All work described here is on `development`. Preserve the repository's Sith policy: only `master` and `development` should remain long-term. Do not publish or merge to `master` without an explicit request.

## Suggested first read for the next agent

1. This handoff.
2. [Tabletop vertical slice](design/TABLETOP_VERTICAL_SLICE.md).
3. [Battlespace content contract](design/BATTLESPACE_CONTENT_CONTRACT.md).
4. [Battlespace schema](../contracts/castle-siege-battlespace.schema.json) and [example](../contracts/castle-siege-battlespace.example.json).
5. [Contract tests](../../../tests/CastleSiege.Tests/CastleSiegeBattlespaceContentTests.cs).
6. [MicroBundle growth plan](design/MICROBUNDLE_GROWTH_PLAN.md).
7. [Entry-surface project](../../../src/CastleSiege/).

**The next proof is a small host-facing presentation consumer of the implemented immutable session report. Keep it inspectable and testable without claiming Physics integration or taking ownership of adjacent repositories.**
