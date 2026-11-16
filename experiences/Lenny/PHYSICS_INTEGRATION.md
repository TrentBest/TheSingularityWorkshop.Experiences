# Lenny — Physics integration contract

**Status:** the Physics prescribed-kinematic provider is implemented and CI-validated. The Experience-owned cleanup reducer is present and tested. No browser/runtime bridge or Physics-driven browser animation is wired yet.

This contract prevents a second Lenny-specific kinematics implementation from appearing in JavaScript. The Experience authors what should happen; Physics evaluates declared root motion; the host owns provider resolution and authoritative commits; Renderer/Visualization present achieved observations.

## Ownership

| Concern | Owner |
|---|---|
| Lenny, Dad, beachgoers, dolphin identities; story anchors; authored actor behaviors; scene placements; litter IDs; cleanup rules; deposited-item counter | Experiences / Lenny |
| Motion definitions, units, frames, fixed-step evaluation, declared speed/acceleration ceilings, requested samples, achieved-state validation | TheSingularityWorkshop.Physics |
| Gesture intent and authored motion procedure, where applicable | Gestures |
| Provider discovery/binding, runtime lifecycle, world commit policy, translation between Experience intent and Physics contracts | Host / MicroBundle composition adapter |
| Drawn pose, animation interpolation, camera, artwork-relative hit regions, accessibility controls | Renderer / Visualization / Experience presentation |
| Story wording and progression | `story.json` (canonical and unchanged) |

The cleanup reducer is Experience-owned transaction state. It is not a movement solver. A requested destination is not an achieved pose. CSS/browser animation may interpolate committed observations for presentation, but it must not decide the authoritative physical position or declare arrival.

## Implemented Physics capability

The Physics repository now provides:

- `PrescribedKinematicMotionDefinition`: immutable, versioned motion identity, actor identity, start/destination poses, absolute logical start time, duration, and declared limits.
- `PrescribedKinematicMotionProfile.ConstantVelocity`: constant-speed root motion over the open interval; its start/stop velocity discontinuity is explicit.
- `PrescribedKinematicMotionProfile.QuinticSmoothStep`: quintic progress `s(u)=10u³−15u⁴+6u⁵`, with zero velocity and acceleration at both endpoints. Peak speed and acceleration are analytically checked against declared linear/angular ceilings.
- `PrescribedKinematicMotion.EvaluateFixedStep`: derives logical time from a fixed timestep and step identity, not browser render cadence.
- `PrescribedKinematicMotion.Commit`: pairs a requested sample with a separately supplied achieved `RigidBodyState`, verifies actor/definition/version/step/time identity, and reports arrival only when the destination sample is within both position and angular tolerances.
- `PrescribedKinematicMotionMicroBundle` and `IPrescribedKinematicMotionProvider`: a typed MicroBundle provider for evaluation and commit validation.

The provider ID is a declaration, not a global service locator. A composition host must explicitly resolve the loaded `PrescribedKinematicMotionMicroBundle` instance and bind its typed provider contract. Physics remains independent of Experiences, Gestures, Renderer, Visualization, Foundry, FSM_COS, and any host framework.

The implementation and tests are in [TheSingularityWorkshop.Physics](https://github.com/TrentBest/TheSingularityWorkshop.Physics). The focused Physics build/tests and Sith Policy both passed for the validated provider/profile implementation.

## Correct first-slice movement model

Lenny's approach-to-litter, pickup staging, carrying, and run-to-receptacle are prescribed actor root motions, not projectile motion and not free-body integration.

For a first beach slice:

1. Experience reserves a stable litter ID and sends Lenny's current achieved pose, destination pose, duration, and motion limits to the host.
2. The host creates a versioned definition using `QuinticSmoothStep` and evaluates it on fixed logical steps.
3. The host commits the achieved root-pose observation. Presentation follows that committed observation.
4. The Experience completes pickup only after the host reports arrival and the pickup action finishes; the item then becomes held. The counter remains unchanged.
5. The reader selects the receptacle. The host performs the second prescribed motion.
6. The Experience commits the item exactly once only after deposit completion. The visible count is derived from unique deposited item IDs and can be shown in the Experience's extra-pages summary.

The Physics profile provides root translation/orientation only. It does not provide gait, foot placement, limb articulation, balance, contact resolution, collision avoidance, friction, obstacle avoidance, or environmental-medium response. Do not describe those capabilities as implemented. If the scene requires Lenny to route around beachgoers or avoid collisions, that requires a separate tested capability and cannot be faked by the prescribed trajectory.

## Browser/runtime boundary

The current Lenny reader is static HTML/JavaScript published through GitHub Pages. It cannot directly call the C# Physics assembly in its current hosting model. A real integration therefore needs an explicit runtime bridge, such as a browser WebAssembly host or an application/server host that exposes committed observations to the reader.

Until that bridge exists:

- `living-world.mjs` may own the interaction transaction and reject stale/duplicate events, but it must not calculate movement.
- `PICKUP_COMPLETED` and `DEPOSIT_COMPLETED` must not be treated as Physics evidence merely because a CSS animation ended.
- Any visual-only fixture must be clearly labelled as a presentation fixture and must never be described as Physics-backed.
- The reader must remain usable without optional interactions, and story navigation must never wait for movement.

## Living-world authorship and artwork gates

The authoring model anchors actors to manuscript pages. Page 8 is the first candidate living beach scene: Lenny, Dad, and beachgoers are present, and the text describes people enjoying the beach and litter appearing again. Page 12 adds the beach community's authored reactions. Page 21 introduces the lead dolphin's litter-helping role; pages 23–25 introduce the dolphin pod's protective journey; pages 26–29 anchor the underwater scenes.

Beachgoer behaviors must stay within the story: relaxing, surfing, building sandcastles, picnicking, sunbathing, polite listening/nodding, continuing their activity, and the single shushing response. Do not invent new plot reactions or dialogue. Dolphin actions must remain tied to the manuscript's retrieval, tossing, bubble, and guiding events.

The current illustrations are provisional. Do not add visible collectible litter targets or normalized hit regions until the author approves the artwork and placements. Keep litter identity separate from artwork-relative hit regions so approved artwork can change without changing the cleanup transaction model. Never alter `story.json` to store interaction state.

## Time, presentation, and accessibility

- Physics advances using fixed logical time; browser refresh rate and CSS animation duration are not simulation time.
- Presentation may interpolate between committed Physics observations, but interpolation is presentation-only.
- Reduced-motion mode may use discrete state changes while preserving the same logical outcomes.
- Pointer, touch, keyboard, and assistive-technology inputs issue the same Experience command.
- Story navigation remains available while optional movement is running.
- Operation IDs must be unique for each pickup/deposit attempt; stale completion events must not mutate current state.

## Integration gates

1. **Experience model:** keep actor/scene/interactable definitions and cleanup state Experience-owned; preserve the canonical manuscript.
2. **Host provider binding:** load the Physics MicroBundle and explicitly bind `IPrescribedKinematicMotionProvider`.
3. **World commit:** supply achieved snapshots, enforce monotonic commit ordering, and preserve the last committed state on cancellation.
4. **Runtime bridge:** expose those observations to the static reader through an explicit supported host/runtime; do not add a parallel JavaScript solver.
5. **Browser presentation:** animate Lenny from achieved observations; keep litter targets hidden until author-approved artwork/placements exist.
6. **Evidence:** test deterministic replay, fixed-step behavior under different browser frame rates, pickup/deposit interruption, navigation while carrying, arrival tolerances, reduced motion, keyboard/touch access, and exactly-once deposit commits.

Until gates 2–4 are complete, the reader remains an illustrated reading preview with a tested Experience interaction model. It must not advertise Physics-backed animation or simulation.

## Source references

- [Physics prescribed kinematic motion](https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/docs/PRESCRIBED_KINEMATIC_MOTION.md)
- [Physics motion-driven interaction pipeline](https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/docs/MOTION_DRIVEN_INTERACTION_PIPELINE.md)
- [Experience-owned living-world model](living-world.json)
- [Experience-owned cleanup state machine](living-world.mjs)

---

*The Singularity Workshop — edify, not mystify.*
