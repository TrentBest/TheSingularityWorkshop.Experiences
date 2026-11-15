# Lenny — Physics integration contract

**Status:** architecture contract; no Physics-backed movement is wired into the browser reader yet.

This contract prevents the living-world work from growing a second, Lenny-specific physics engine in JavaScript. The Experience authors what should happen; Physics owns physical state and achieved motion; the host bridges them; Renderer/Visualization present committed observations.

## Ownership

| Concern | Owner |
|---|---|
| Lenny, Dad, beachgoers, dolphin identities; story anchors; scene placements; cleanup rules; item identity; deposited-item counter | Experiences / Lenny |
| Physical state, units, coordinate frames, logical stepping, achieved pose/velocity, collision/contact evidence, solver limits | TheSingularityWorkshop.Physics |
| Gesture or actor intent and authored motion procedure | Gestures, where applicable |
| Provider discovery, binding, lifecycle, and translation between Experience intent and Physics contracts | Host / MicroBundle composition adapter |
| Drawn pose, animation interpolation, camera, hit regions, accessibility controls | Renderer / Visualization / Experience presentation |
| Story wording and progression | story.json (canonical; unchanged) |

A requested destination is not an achieved physical pose. CSS transitions and browser animation may interpolate committed Physics observations for presentation, but must never become the authority that decides where Lenny physically is.

## Existing Physics capabilities relevant to this Experience

Reuse these real Physics contracts rather than copying their equations:

- TheSingularityWorkshop.Physics.Spatial.Vector3d — immutable finite 3D vectors. The type is unit-agnostic; callers declare semantics.
- TheSingularityWorkshop.Physics.Spatial.Pose3d — world-frame position in metres and normalized body-local-to-world orientation.
- TheSingularityWorkshop.Physics.Dynamics.RigidBodyState — stable body ID, achieved pose, world linear velocity (m/s), angular velocity (rad/s), committed step index, and elapsed logical time (s).
- TheSingularityWorkshop.Physics.Dynamics.RigidBodyIntegrator.Advance — deterministic semi-implicit Euler free-body step with supplied constant world acceleration and fixed positive timestep. It does not calculate forces, detect collisions, resolve contacts, model friction, or enforce constraints.
- TheSingularityWorkshop.Physics.Kinematics.ConstantAccelerationProjectileSolver — analytical point-projectile advancement. It is not a character locomotion solver and is not appropriate for Lenny's walk/run path.
- docs/MOTION_DRIVEN_INTERACTION_PIPELINE.md — distinguishes prescribed kinematic trajectory, controller target, and physical force/torque contribution. Physics core must not depend on Gestures; the host adapter composes them.

The current Physics MicroBundle adapter assembly exposes sphere-sphere contact evidence and a frictionless translational normal-impulse response. Those adapters do not expose a character locomotion controller or complete world-step loop. The browser reader is static HTML/JavaScript and cannot directly invoke the C# Physics assembly in its current hosting model.

## Correct first-slice movement model

Lenny's walk-to-litter and run-to-receptacle are **controller-target / prescribed-kinematic actor motions**, not free fall and not projectile motion.

Before connecting browser presentation, the host/Physics integration needs a typed, versioned movement provider with:

- stable actor/body identity (lenny);
- declared world frame and units (metres, seconds; frame convention explicitly identified);
- fixed logical timestep and committed step/time identity;
- current achieved body state, target identity/pose, and motion mode;
- bounded speed/acceleration and declared arrival tolerance;
- deterministic same-input behavior and explicit interruption/cancellation semantics;
- output observations containing achieved pose and velocity, not merely the requested target;
- capability/limitation metadata, including whether contact avoidance or collision response is actually supported.

If the initial actor provider is a prescribed path, Physics and the adapter must explicitly declare that policy and its limitations. Do not imply collision avoidance or dynamic response unless a tested Physics model supplies it. If a controller-target implementation is selected, the provider must report the achieved trajectory and any deviation.

**Do not** add a second vector/kinematics implementation to living-world.mjs, drive the actor directly from wall-clock animation frames, or misuse the point-projectile solver or free-body integrator as a walking controller.

## Interaction sequence and commit boundaries

1. Reader selects a manuscript-grounded litter target. Experience reserves its stable item ID and sends a movement intent to the host adapter.
2. Physics-backed actor movement advances on logical ticks. Presentation follows achieved observations. Pickup completes only after the actor reaches the declared pickup region and the pickup action itself completes.
3. Experience records the held item; no counter change occurs.
4. Reader selects the receptacle. Experience sends the second movement intent.
5. Physics-backed movement reaches the receptacle under the same declared motion contract.
6. Deposit completes. Experience commits the item ID exactly once and derives the visible count from unique committed IDs.
7. Navigation/interruption cancels or pauses the movement operation through the adapter. It cannot fabricate a completed pickup/deposit or commit the counter.

The existing living-world.mjs reducer remains the Experience-owned interaction transaction/state machine. It does not calculate movement or claim to be Physics. Its animation-completion events must be replaced or gated by host observations/action-completion events before the browser slice is described as Physics-backed.

## Time, presentation, and accessibility

- Physics advances using fixed logical time; browser refresh rate and CSS animation duration are not simulation time.
- Rendering may interpolate between committed Physics observations; interpolation is presentation-only.
- Reduced-motion mode may render discrete state changes or a simplified transition while preserving logical outcomes.
- Pointer, touch, keyboard, and assistive-technology input all issue the same Experience command; they do not alter solver state directly.
- Story navigation remains available while optional movement is running.

## Integration gates

1. **Contract:** agree the movement-provider interface and confirm it against actual Physics source/tests. Do not present invented API names as existing.
2. **Provider:** implement and test the required actor-motion capability in Physics (or identify an exact existing provider); expose it through a MicroBundle/host adapter. Physics remains independent of Experiences and Gestures.
3. **Host:** resolve and bind the provider; translate Experience intent to typed input; return committed observations and terminal events.
4. **Browser:** bind Lenny's actor presentation to those observations; keep the cleanup reducer responsible only for Experience interaction and deposit accounting.
5. **Evidence:** test deterministic repeat runs, fixed-step behavior under different browser frame rates, interruption before/after pickup, navigation while carrying, arrival tolerance, reduced motion, keyboard/touch access, and exactly-once deposit commits.

Until gates 1–3 are complete, the reader remains an illustrated reading preview with a tested interaction-domain model. It must not advertise Physics-backed animation or simulation. A first visible beach slice may be developed against a clearly labelled presentation fixture, but the fixture must be replaceable and never confused with Physics output.

## Source references

- [Physics repository](https://github.com/TrentBest/TheSingularityWorkshop.Physics)
- [Physics motion-driven interaction pipeline](https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/docs/MOTION_DRIVEN_INTERACTION_PIPELINE.md)
- [Physics free rigid-body integrator contract](https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/docs/FREE_RIGID_BODY_INTEGRATOR.md)
- [Physics capability catalog](https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/docs/PHYSICS_CAPABILITY_CATALOG.md)
- [Experience-owned living-world model](living-world.json)
- [Experience-owned cleanup state machine](living-world.mjs)

---

*The Singularity Workshop — edify, not mystify.*
