# Marble Run and Domino Studio: Experience and MicroBundle Boundaries

**Status:** executable data contracts exist for Domino layout grids/proposals and Marble Run scene configuration. Neither experience yet runs a complete rigid-body simulation or claims high-count performance.

## Experiences are not physics MicroBundles

The Marble Run and Domino Studio packages are consumer experiences. They own image import/conversion, scene authoring, layout editing, terrain/track construction, player input, cameras, persistence, and presentation. Physics owns authoritative body state, integration, collision/contact evidence and response, deterministic stepping, and diagnostics.

The experience packages do not become MicroBundles simply because they are composed by a host. Supporting MicroBundles should represent reusable physical capabilities that can be consumed by more than one experience.

## Proposed capability bundles, gated by executable contracts

The smallest useful physical capability set is:

1. **Shape and collision geometry** — stable shape identity, transform, units, geometry revision and collision proxy.
2. **Broadphase candidate generation** — scalable candidate pairs with stable IDs and no claim that a candidate is a contact.
3. **Narrowphase contact observation** — contact point, normal, signed separation, logical step/time and optional computed loads.
4. **Rigid-body stepping** — translational and rotational state integration with declared timestep and gravity.
5. **Contact response** — normal impulse, then independently tested friction, angular response and resting contacts.
6. **Material/contact profile** — explicit mass/density, restitution, friction and provenance; missing values remain unknown rather than silently becoming zero.
7. **Replay/diagnostics** — run identity, model/config versions, initial intervention, contact trace and error/residual measurements.

These are a capability plan, not seven already-implemented bundles. The current Physics repository has bounded contact observations, sphere-sphere detection, translational normal impulse response, and positional correction. That is not yet a complete rigid-body stepper, a domino toppling solver, or a rolling-friction marble solver. Do not wrap missing solvers in provider declarations and imply they exist.

## MicroBundleDomain and composition

MicroBundleDomain defines stable bundle IDs, versions, dependency IDs and provider IDs. A provider ID is metadata; it is not a typed service locator or proof that a solver can be invoked. The current load context provides runtime identity and configuration access, not a universal registration API.

Before an experience can resolve a capability provider, the composition host must own an explicit typed resolution mechanism, or a narrow integration MicroBundle must define it. Require semantic contract identity/version, units, timebase, and deterministic missing/ambiguous/incompatible outcomes. Do not add a generic service locator to Physics or MicroBundleDomain just to make this demo work.

The MicroBundleRepository stores/retrieves artifacts. It does not calculate physics or arbitrate the assembled composition. The composition host (such as FSM_COS) chooses dependencies, loads capabilities, resolves compatible providers and handles lifecycle.

## Current experience contracts

- Domino Studio: DominoLayoutGrid separates per-cell height from per-level orientation. Orientation zero means empty; occupied slots use 1-based codes so zero radians remains a valid angle. DominoLayoutProposal carries a source digest, converter identity/version, coordinate frame, interpretation, scale, confidence and grid.
- Marble Run: MarbleRunSceneDefinition declares gravity, scale/density, requested body count, geometry revision, coordinate frame and selected Physics model identity. The sphere-mass helper is explicitly a homogeneous-sphere assumption.
- The first validation methods are inexpensive authoring checks. They do not prove 3D support, collision-free construction, physical chain propagation, or throughput.
- Thousands of marbles is a benchmark target, not a current claim.
- The first push is the only authored domino impulse. Subsequent falls must emerge from the physical model. A pre-baked path may be used as a labelled playback/cache mode only when its initial state and model inputs match; it is not a substitute for live dynamics under changed conditions.

## Gesture ownership

Do not modify the Gestures repository or add a dependency to it. Physics exposes observations and outcomes; the Gestures owner determines any motion-procedure interface. Any later integration belongs at the Experience/composition boundary and requires coordination with the responsible LLM.

## Next executable milestones

1. Complete CI validation of the grid and scene data contracts.
2. Implement a CPU reference rigid-body vertical slice in Physics, with analytical free-fall, plane contact and two-body impact tests.
3. Add shape-specific collision geometry for a standing domino and a sphere; do not claim rotation or friction until solved and tested.
4. Create the smallest typed capability wrappers around real Physics implementations, then test host resolution, absent providers and incompatible contracts.
5. Connect Domino Studio and Marble Run to the same Physics run API; save layout/geometry revision and initial impulse.
6. Add repeatable scale sweeps (100, 1,000, 10,000 bodies where feasible) and publish measured results, not target numbers.
7. Only then consider a validated pre-baked trajectory cache and GPU acceleration.

No NuGet publication, release, master promotion, or deployment is authorized by this work.
