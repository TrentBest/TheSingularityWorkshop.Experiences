# Celestial Mechanics — Experience Design

**Status:** preforge design; not yet an executable simulation or published MicroBundle.

## The experience

Celestial Mechanics is an editable, replayable universe laboratory. A creator assembles stars, planets, moons, asteroids, and comets from a palette, configures their physical properties and initial conditions, then watches the system evolve. The same saved scenario supports three linked views:

1. **System view** — bodies, scale-aware orbits, trajectories, time controls, encounters, and event history.
2. **Gravity-field view** — gravitational acceleration vectors, potential contours, influence heatmaps, and an optional deformed surface used to explain the field.
3. **Impact observatory** — a selected collision becomes an inspectable event with relative velocity, momentum transfer, energy budget, contact geometry, and progressively finer impact visualization.

The core promise is that these are not three unrelated animations. They are different views of one authoritative, persistent scenario and its simulation timeline.

## Place in the Workshop

This belongs under `TheSingularityWorkshop.Experiences` as an Experience definition, not as engine code in the Workshop host.

- **Experience** owns the scenario format, user journey, physical-model selection, experiment controls, and semantic events.
- **Physics** owns reusable deterministic simulation primitives and explicitly supported physical models.
- **Visualize / Renderer** own field, trajectory, body, and impact presentation; they do not own authoritative physics state.
- **GUI / manifestation** owns palette interaction, editing affordances, camera/zoom, and presentation-specific controls.
- **FSM_COS** composes the selected capability MicroBundles.
- **MicroBundleRepository** delivers immutable, versioned published artifacts. It is not the mutable save-game/project store.
- **Workshop project persistence** owns editable scenarios, revisions, event history, and recovery. Its implementation boundary must be selected explicitly rather than pretending a published bundle repository is an editable database.

The Experience must remain independent of WebApp, AnyApp, MyVR, a particular renderer, and any single simulation implementation.

## Creator workflow

1. Create or open a scenario.
2. Place a celestial body from the palette.
3. Edit mass, radius, material/composition, position, velocity, spin, and initial-state provenance.
4. Choose a physical model and time scale.
5. Run, pause, step, scrub, or branch the timeline.
6. Inspect predicted encounters and collision events.
7. Open an event in the impact observatory.
8. Save a revision, duplicate a scenario, or compare two branches.

A body should be movable without silently changing its mass or velocity. Edits to initial conditions create a new scenario revision; simulation events append to the selected timeline. The UI should distinguish authored inputs from simulated outputs.

## Three linked views

### 1. System and orbit view

Display body identities, radii, paths, velocity vectors, close-approach markers, and collision/merge events. Support logarithmic or staged navigation across astronomical scales without implying that displayed radii and distances are to scale at every zoom level. Any visual exaggeration must be labelled.

### 2. Gravity field view

Offer selectable overlays:

- gravitational acceleration vectors;
- gravitational potential contours;
- field-strength heatmap and pairwise interaction regions;
- optional height-field gravity-well metaphor.

For Newtonian point masses, acceleration is derived from the bodies' masses and positions. A well surface is an explanatory encoding of a chosen scalar field, not a literal extra spatial dimension and not the simulation's force law. The overlay must disclose the quantity and scale it displays.

### 3. Impact observatory

Select a collision in the timeline and transition from system scale to impact scale while retaining the same event identity and pre-impact state. Show:

- incoming trajectories, impact angle, relative speed, and contact point;
- momentum before and after, with the selected collision model stated;
- kinetic energy and the fraction assigned to modeled outcomes;
- wavefront/pressure proxy, temperature proxy, ejecta, deformation, and material mixing only when supported by the selected model;
- time scrubber, scale legend, resolution indicator, and model limitations.

The impact view must not present decorative particles as a physically computed shockwave. Each field needs a named quantity, units, model provenance, and clear distinction between solved values and illustrative effects.

## Physical model tiers

The Experience should negotiate models by capability, not claim one solver handles every scale.

| Tier | Purpose | Model contract |
| --- | --- | --- |
| Orbital sandbox | Long-duration motion of stars, planets, moons, and small bodies | Deterministic fixed-step Newtonian N-body integration, explicit units and timestep, documented integrator/error behavior |
| Encounter and collision | Close approach, contact, bounce, fragmentation/merge policies | Continuous or bounded collision detection as available; explicit restitution, mass, momentum, and energy accounting |
| Impact observatory | Localized high-energy impact | A separately identified impact/hydrodynamics model with documented equations, resolution, material assumptions, and conservation checks |
| Presentation | Field surfaces, trails, particles, labels, LOD | Derived visualization only; never authoritative simulation state |

A collision does not automatically mean an orbit has become stable. Depending on the masses, velocity, and model, bodies may merge, fragment, rebound, or remain in a new unstable configuration. The default simplified model should state its policy, such as perfectly inelastic merging, and conserve total momentum within its documented numerical tolerance. More complex outcomes require an explicitly more capable model.

The initial orbital model and impact model should not be coupled by pretending the orbital solver resolves the interior of a planet. The impact event should preserve the incoming state and hand it to a separately specified local model, with an explicit mapping between scales and energy/momentum budgets.

## Authoritative scenario and persistence

A saved scenario should include:

- schema and model-contract versions;
- stable scenario and body identities;
- SI-valued mass, length, time, velocity, and material parameters;
- initial conditions and their provenance (authored, imported, or generated);
- integration settings, deterministic seed where randomness is used, and collision policy;
- timeline branches, event sequence, checkpoints, and parent revision;
- source revision, save status, and compatibility information.

Persist editable scenario state separately from derived data. Orbit trails, gravity grids, meshes, and impact visualizations are reproducible caches keyed by scenario revision, model version, and relevant parameters. They may be discarded and regenerated.

Saving should be transactional: write a complete new revision, validate it, then make it current. Reopening a scenario must not depend on a process-local cache. When a model version changes, the project must either migrate explicitly or remain reproducible with its recorded version; never silently reinterpret old results.

## Determinism, time, and inspection

- Use explicit simulation time and fixed-step configuration; presentation frame rate must not determine physical outcomes.
- Record meaningful events such as body creation, parameter edits, close approach, contact, merge/fragment decision, checkpoint, and branch creation.
- Support replay from a checkpoint and branch from a past state without overwriting the original timeline.
- Show simulation time separately from wall-clock time and render interpolation separately from authoritative state.
- Expose diagnostics: active body count, timestep, integrator/model identity, conservation residuals, event count, and current revision.

Determinism is a contract to test for a given model/version, configuration, platform scope, and input sequence—not a blanket promise across every hardware target.

## First proof slice

Do not start with a realistic galaxy or a planetary interior. Build a small, observable vertical slice:

1. Place a star and two orbiting bodies; edit one body's initial velocity.
2. Run and pause; show trajectories and the gravity-field overlay from the same state.
3. Set up a controlled asteroid/planet encounter.
4. Detect and record the collision with a stated simplified collision policy.
5. Open the impact observatory and show only quantities supported by the initial impact model; label any proxy.
6. Save, close, reopen, replay, and branch the scenario.
7. Verify repeatability, momentum accounting, event ordering, revision invalidation, and recovery after an interrupted save.

### Acceptance evidence

- A testable scenario manifest and schema.
- Unit tests for field calculations, integration, contact/collision policy, and conservation tolerances.
- A replay test proving identical event sequences for identical inputs under the declared determinism scope.
- A persistence round-trip test that reproduces the same authoritative scenario and event history.
- A living visual that reads simulation outputs rather than drawing a disconnected illustrative animation.
- A written limit statement for the impact model: solved variables, approximations, resolution, and unsupported effects.

## Deliberate non-goals for the first slice

- Full general relativity or a literal rubber-sheet model of spacetime.
- Planet-scale hydrodynamics coupled to long-duration N-body simulation.
- Claiming fragmentation, melting, crater formation, or seismic shock propagation without a solver that actually models them.
- Unbounded body counts or universal real-time performance promises.
- Storing mutable creator projects as immutable published MicroBundle artifacts.

## Current implementation status

This document defines the Experience boundary and a staged proof plan. It does not assert that an orbital integrator, gravity-field renderer, impact solver, persistence adapter, or executable MicroBundle already exists. Those capabilities should be connected only after their actual contracts and tests have been inspected.
