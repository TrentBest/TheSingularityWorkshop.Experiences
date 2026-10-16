# Castle Siege — Experience Design and Implementation Contract

**Status:** design contract; not yet a playable Experience.  
**Home:** `TheSingularityWorkshop.Experiences` owns the Experience identity, player-facing progression, match rules, authored content contract, and manifestation requirements.  
**Physical authority:** `TheSingularityWorkshop.Physics` owns deterministic physical outcomes and the evidence behind them.  
**Presentation:** WebPage/WebApp owns the hosted scene, controls, camera, explanatory overlays, and interaction feedback. Visualize/Renderer may consume presentation data later; they are not Physics dependencies.

This document is the cross-repository contract for building Castle Siege as a real Workshop Experience instead of leaving the entire concept inside a physics experiment catalog. It distinguishes intended behavior from implemented behavior.

## 1. The experience

Castle Siege is a hands-on siege-engineering experience. The player can inspect a castle, construct or configure a siege engine, aim a projectile, launch it, and inspect the measured path and impact. The scene is both a game and an experiment: every computed outcome should be attributable to explicit inputs, a named model, and a reproducible run.

The longer-term competitive mode takes inspiration from physics-based build-and-destroy games, but it is not a clone of any particular title:

1. Two players receive the same declared construction kit and the same build budget/rules.
2. Each independently constructs a castle or defensive structure from the permitted pieces.
3. Each construction is validated and locked as an immutable match snapshot.
4. Players alternate attacks using the same declared projectile/launch rules.
5. The match host validates turn ownership and accepted commands; clients do not decide their own hits, damage, or scores.
6. The match ends under a declared rule (for example, target objectives exhausted, a turn limit, or a validated structural objective destroyed).
7. The result is accompanied by an event record sufficient to explain how it was reached.

The first milestone is a credible single-player experiment. The competitive build-and-attack loop follows from reusable, testable pieces; it must not be simulated by cosmetic animation or client-side guesses.

## 2. Experience layers and ownership

| Concern | Owner | Contract |
|---|---|---|
| Experience identity, version, entry flow, content references | Experiences | Stable identity and a versioned definition; identity is not a renderer or URL |
| Player choices, scene composition, camera, controls, overlays | WebPage/WebApp manifestation | Input and presentation only; do not invent physical results |
| Physical state, trajectories, contact/impact observations, supported damage outcomes | Physics | SI units, declared coordinate frames, deterministic tests, model/version and assumptions |
| Object appearance, LOD, drawing, effects | Visualize/Renderer or host presentation | Consumes state/observations; effects must not be presented as proof of physical damage |
| Crew choreography and articulated motion | Gestures, when integrated | Consumes explicit events/observations; Physics must not depend on Gestures |
| Composition and capability arbitration | FSM_COS | Composes verified artifacts; it does not own game rules or become the simulation clock |
| Match authority, turn order, accepted command sequence | Match/session authority | Validates commands and records canonical results; rendering clients are not authoritative |

No single layer should quietly absorb another layer's responsibility. In particular, Physics must remain independent of WebPage, GUI, Visualize, and Experiences.

## 3. First playable vertical slice

The first slice should be small enough to validate and complete:

- One authored terrain patch and a readable castle silhouette.
- One selected wall/target region with stable identity and declared geometry.
- One siege engine presented from a commander-like viewpoint just behind and to one side of the apparatus.
- One projectile model and a clearly labeled launch control.
- A compact control panel for launch speed (or another explicitly supported launch-state input), elevation, azimuth, and logical simulation duration/step.
- A trajectory visualization sourced from actual Physics samples.
- A target result: hit, miss, or indeterminate, with the target identity and first supported intersection evidence.
- An impact report that exposes supported values and units, such as incoming velocity, momentum, and kinetic energy.
- A run identifier and a baseline-versus-variant comparison.
- A capability/status panel that distinguishes implemented computation from authored placeholders.

The first milestone does **not** require a complete trebuchet mechanism, full rigid-body castle, general masonry fracture, networked multiplayer, or debris simulation. If the arm/tension/counterweight controls are visual-only, they must say so. Do not label a launch-speed proxy as a validated trebuchet.

### Scene composition

Keep these concepts separate even if one view presents them together:

- **Authored scene:** terrain, castle sections, siege engine, lighting, camera, and decorative details.
- **Construction state:** the selected, validated arrangement of build pieces.
- **Physical state:** bodies, initial conditions, logical time, supported contacts, and model state.
- **Run evidence:** inputs, model/version, samples, target query, impact, and any supported response events.
- **Presentation state:** camera, selected object, open panels, animation, and visual effects.

Camera motion, UI layout, and visual effects must not change a physical run. A fixture trajectory or pre-authored damage animation must be visibly identified as illustrative, never passed off as a Physics result.

## 4. Build phase and equal-kit fairness

The competitive mode is a second milestone, designed now but implemented only after the single-player physics path is credible.

### Build contract

A match definition declares:
- kit identity and version;
- allowed piece types and per-piece quantities;
- placement coordinate frame, grid/precision policy, and permitted bounds;
- attachment/connection rules;
- resource or piece budget;
- overlap, support, and out-of-bounds validation rules;
- any symmetry or random-seed rules;
- build deadline or explicit readiness action.

Both players receive the same immutable kit definition. Fairness is based on identical rules and inputs, not on assuming that two client implementations behave identically. The host validates submitted structures against the canonical rules.

A locked build is an immutable snapshot containing the match ID, player slot, kit/version/hash, canonical piece identities, transforms, connections, and validation report. After lock, edits create a new build revision and cannot silently alter the active match snapshot.

The first structural model may use authored blocks and explicit support/connection rules. Do not claim realistic masonry, fracture, or structural stability until the corresponding Physics models and tests exist.

## 5. Attack phase and match rules

### Turn loop

1. The match authority announces the active player and turn number.
2. The active player submits an aim/launch command referencing the locked structure, allowed apparatus, and declared launch model.
3. The authority validates command bounds, turn ownership, and command idempotency.
4. Physics evaluates the accepted command from the authoritative state and returns a result tied to the run/model identity.
5. The authority records trajectory/impact/damage observations that the current model actually supports.
6. The result is published to both players; the next turn begins only after the previous result is committed.
7. A disconnect pauses, forfeits, or resumes according to an explicit match policy; clients cannot unilaterally rewrite the turn.

### Fairness and determinism

- Same kit and rule version for both players.
- Explicit match seed wherever randomness is used; no hidden client-side random advantage.
- Canonical ordering for build pieces, commands, and recorded events.
- Logical physics time is independent of render frame rate and network packet timing.
- A duplicate command ID must not apply the same shot twice.
- A stale turn number or mismatched structure hash is rejected.
- Match replay must distinguish a true deterministic rerun from playback of recorded presentation.
- Server/host authority is a planned boundary, not a claim that online multiplayer currently exists.

### Scoring

Do not score a hit merely because a client reports one. The initial scoring rules should be simple, visible, and tied to validated observations. Candidate objectives include destroying a named target section, disabling a declared objective, or reaching a turn-limited damage score. Pick one rule set for the first duel and version it with the match definition. Avoid hidden heuristics that cannot be explained from the event record.

## 6. Physics contract and progression

The Experience consumes a narrow adapter over Physics. It must not call private implementation details or reimplement the solver in the browser.

| Stage | Required evidence | Unlocks |
|---|---|---|
| A — launch | Validated initial position/velocity or launch definition, SI units, model identity, timestep | Honest trajectory experiment |
| B — trajectory | Ordered samples, logical time, deterministic result, analytical-reference tests | Plot/arc visualization and parameter comparison |
| C — target query | Stable target identity/frame/geometry and hit/miss/indeterminate result | Credible target hit reporting |
| D — impact | Impact time, position, pre-impact velocity, mass, supported energy/momentum diagnostics | Impact explanation |
| E — bounded response | Declared material/structural parameters, damage state and reason | Limited wall-damage gameplay |
| F — failure/topology | Tested failure criteria, topology ownership and debris/body initialization | Destruction and debris |
| G — match state | Canonical build snapshots, command log, run references, scoring rule/version | Repeatable local duel |
| H — shared session | Authenticated match authority, reconnect policy, idempotent commands and synchronization tests | Online competitive play |

The first Physics implementation is a constant-acceleration kernel, not a complete trajectory experiment or collision/damage system. Stage A must be completed into a launch-definition and sample-producing runner before the Experience can claim a computed trajectory. Each later stage has its own gate; do not bypass missing evidence with visual effects.

The current Physics coordination record and experiment catalog are the detailed numerical work queue:
- [Physics coordination log](https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/docs/COORDINATION.md)
- [Physics experiment catalog](https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/docs/CASTLE_SIEGE_EXPERIMENT_CATALOG.md)
- [Physics domain specification](https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/docs/CASTLE_SIEGE_PHYSICS_DOMAIN.md)
- [Physics payloads and versus play](https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/docs/CASTLE_SIEGE_PAYLOADS_AND_VERSUS.md)

## 7. Experience artifact and manifestation contract

The Experience should eventually be a versioned, independently identifiable artifact that can be composed into different manifestations. The repository's existing MicroBundle pattern is the intended starting point; do not invent a parallel scene host or a second composition framework.

Before creating the first Castle Siege MicroBundle, settle and test:
- stable Experience and bundle identity (do not reuse Forge, Moniker, or other existing IDs);
- artifact version and immutable content hash;
- explicit dependencies and optional capabilities;
- a minimal semantic entry surface compatible with the current GUI Core contract;
- event/state vocabulary between host controls and the Experience;
- a versioned adapter for Physics requests and results;
- behavior when Physics capabilities are unavailable;
- how a browser manifestation identifies the same Experience as a future desktop/VR manifestation.

The Experience may provide an informative landing/entry surface before Physics integration exists. It must not expose launch or damage controls as operational until their backing capability is present. If a capability is optional, degrade to an explicitly labeled overview or recorded sample; if it is required for a mode, block entry with a clear explanation.

## 8. Observability and living documentation

The experience should make the experiment inspectable without forcing every visitor into a research console.

The default view should expose a small set of meaningful controls and visible outcome. A separate Workshop Lab path can reveal:
- exact input values and units;
- model and rules versions;
- sample count and logical timestep;
- target geometry/coordinate frame;
- baseline-versus-variant input diff;
- impact diagnostics and uncertainty/limitations where available;
- build validation and match event history;
- deterministic rerun/replay status.

Documentation visuals should be generated from or explicitly tied to the same contracts as the Experience. Do not create decorative SVGs that imply an implementation exists when it does not. A scene-only prototype can still demonstrate composition and affordances, provided every unavailable capability is labeled.

## 9. Acceptance gates

A milestone is complete only when all applicable gates are satisfied:

- [ ] Its behavior and repository ownership are documented.
- [ ] Public contracts validate invalid, stale, and boundary inputs.
- [ ] Deterministic rules have tests for normal cases, edge cases, and repeatability.
- [ ] Build/test CI passes on the intended development commit.
- [ ] The host adapter consumes actual outputs rather than duplicating physical logic.
- [ ] The UI distinguishes authored fixtures, calculated observations, and unsupported capabilities.
- [ ] The current implementation, engineering guide, and rationale agree.
- [ ] No release, publication, or deployment occurs without separate approval.

## 10. Immediate implementation sequence

1. Keep the current Physics projectile kernel and tests as the numerical foundation; finish the trajectory experiment and target-query contracts there.
2. Add a Castle Siege Experience design/artifact surface in this repository, beginning with identity and a scene/entry manifest rather than embedding a physics engine in the UI.
3. Build the single-player presentation against a typed adapter and explicit capability status.
4. Add target intersection and impact diagnostics, then a bounded wall response in Physics.
5. Add immutable build-piece definitions, canonical validation, and locked build snapshots.
6. Add local alternating-turn match rules and deterministic event history.
7. Only then add structural failure/debris and online multiplayer authority.

This sequence deliberately separates a compelling visible scene from a trustworthy physical result. The scene can grow early; claims about physics, destruction, fairness, and replay must grow only when supported by executable contracts and tests.
