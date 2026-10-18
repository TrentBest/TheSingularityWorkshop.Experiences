# Experience Directory — Domino Studio and Marble Run

**Purpose:** make the Workshop's playable and investigable Experiences discoverable from one human-facing front door.

This directory is the authored discovery contract for the two Experiences below. It is not itself a running WebApp, a claim that either Experience is deployed, or proof of physics that has not yet been implemented.

## The visitor's front door

The intended journey is:

1. A visitor enters the Workshop through the WebApp's **Experiences** directory (also reachable from the Workshop/City's public institutions or discovery area).
2. Each entry explains what can be done, what is currently real, and what remains experimental.
3. **Open Experience** launches the best available manifestation: browser first when a browser host exists, otherwise an honest preview or a documented local run path.
4. **How it works** links to the authored contract, implementation, tests, and physics boundary.
5. A visitor can move between related Experiences without needing to know repository names.

The WebApp is the distribution and discovery surface; the Experiences repository remains the authored source of identity and intent. FSM_COS composes compatible capabilities. AnyApp may provide a desktop manifestation for heavier local work. None of these layers should be collapsed into a single implementation.

## The two entries

### Domino Studio

**Explore:** layered domino layouts, per-cell occupancy masks, per-level orientations, and the rules and constraints of layout authoring.

**Why visit:** design a structure, inspect how the levels are encoded, and learn how discrete arrangements become candidate physical scenes.

**Current truth:** the layout-data and proposal contracts exist. Current validation checks authoring constraints; it does **not** establish collision-free 3D placement, stable support, or a successful toppling chain. The physics runtime must determine subsequent falls from actual contact and motion; do not script a predetermined fall sequence and call it simulation.

- [Experience home](DominoStudio/README.md)
- [Implementation and tests](../src/DominoStudio/)
- [Physics/MicroBundle boundary and milestones](../docs/MARBLE_RUN_AND_DOMINO_MICROBUNDLE_BOUNDARY.md)

### Marble Run

**Explore:** author a track and terrain, configure gravity and marble properties, release marbles, and observe how changes to the course affect the run.

**Why visit:** make continuous motion, contact, and energy behavior visible through an intuitive playground.

**Current truth:** the scene-configuration contract exists; a complete rolling-marble solver and high-count performance result are not yet established. Thousands of marbles is a measured performance goal, not a present capability claim. Start with a small reference scene, validate free fall and contact, then run reproducible scale sweeps before advertising higher counts.

- [Experience home](MarbleRun/README.md)
- [Implementation and tests](../src/MarbleRun/)
- [Physics/MicroBundle boundary and milestones](../docs/MARBLE_RUN_AND_DOMINO_MICROBUNDLE_BOUNDARY.md)

## A useful shared design

Give both entries the same visitor-facing structure:

- **Play / Explore:** a working interactive manifestation when available.
- **Change something:** one safe, understandable intervention (layout/orientation for dominoes; ramp, gravity, or release count for marbles).
- **Observe:** show state and event/contact evidence that the underlying implementation actually supplies.
- **Understand:** link to the model, assumptions, limitations, and tests.
- **Build your own:** point to authoring and contribution instructions.

Use the same Experience identity across browser, desktop, and future VR manifestations. The host may change; the meaning and provenance of the Experience should not.

## Release gates for the public directory

An entry may appear in the directory before it is fully simulated, but its status and primary action must match reality:

- **Design preview:** authored design or static illustration; clearly labelled as such.
- **Interactive authoring:** user can change data, with validation limits stated.
- **Simulation preview:** the real physics implementation is connected; assumptions and measured limitations are visible.
- **Validated showcase:** repeatable tests cover the advertised behavior and benchmark numbers are reproducible.

Do not present a static image as a live scene, a fixture as a deployed experience, an abstract event as physical evidence, or a target body count as a benchmark result.

## Next implementation steps

1. Add these entries to the WebApp's actual Experience discovery/catalog surface, with clear status labels and stable Experience IDs.
2. Give each entry a browser-accessible **Open** route or an explicitly labelled preview until a live route exists.
3. Connect both Experiences to the same typed Physics run boundary only after the relevant solver APIs and provider resolution are real and tested.
4. Capture reproducible run settings, model/version identity, and truthful event/contact diagnostics.
5. Benchmark Marble Run at increasing counts (for example 100, 1,000, then 10,000 when feasible); publish only measured results.
6. Let Domino Studio author the initial arrangement and first push; subsequent toppling must emerge from the validated physical model.

**Guiding principle:** make the Experience easy to find, easy to try, and easy to inspect—without overstating what the implementation proves.
