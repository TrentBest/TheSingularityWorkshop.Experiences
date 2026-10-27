# Singularity Exploratorium — Exhibit Directory

**Wonder first. Touch something. Ask why. Try again.**

The **Singularity Exploratorium** is the Workshop's playful, high-wow discovery floor: hands-on exhibits that make a visitor curious before asking them to study a theory. It complements, rather than replaces, the Workshop's other institutions:

- **Laboratory:** structured inquiry, controlled experiments, measurement, and reproducibility.
- **Museum:** curated collections, history, context, interpretation, and preservation.
- **Exploratorium:** playful discovery, surprising behavior, direct manipulation, and “what if?” moments.

See the [Singularity Exploratorium institution and design principles](SingularityExploratorium/README.md).

This directory is the authored discovery contract for its first exhibit families. It is not itself a running WebApp, a claim that an exhibit is deployed, or proof of physics that has not yet been implemented.

## The visitor's front door

The intended journey is:

1. A visitor discovers the **Singularity Exploratorium** from the Workshop/City's public discovery area or WebApp.
2. An exhibit starts with an inviting action and explains what is currently real and what remains experimental.
3. **Open Exhibit** launches the best available manifestation: browser first when a browser host exists, otherwise an honest preview or a documented local run path.
4. **How it works** links to the authored contract, implementation, tests, and relevant physics boundary.
5. A visitor can move from playful discovery into the Museum's context or the Laboratory's structured investigation without needing to know repository names.

The WebApp is the intended distribution and discovery surface; the Experiences repository remains the authored source of identity and intent. FSM_COS composes compatible capabilities. AnyApp may provide a desktop manifestation for heavier local work. None of these layers should be collapsed into a single implementation.

## First exhibit families

### Domino Studio — discrete causality

**Try:** author layered domino layouts, inspect per-cell occupancy masks and per-level orientations, and eventually trigger a chain with a first push.

**Why visit:** dominoes make cause and effect legible. A tiny change in spacing, orientation, or support can change the outcome of a much larger chain.

**Current truth:** the layout-data and proposal contracts exist. Current validation checks authoring constraints; it does **not** establish collision-free 3D placement, stable support, or a successful toppling chain. The physics runtime must determine subsequent falls from actual contact and motion; do not script a predetermined fall sequence and call it simulation.

- [Experience home](DominoStudio/README.md)
- [Implementation and tests](../src/DominoStudio/)
- [Physics/MicroBundle boundary and milestones](../docs/MARBLE_RUN_AND_DOMINO_MICROBUNDLE_BOUNDARY.md)


### Drone Construction — large-scale coordination

**Try:** reconstruct the image-to-block construction concept using discrete agents, ten resource emitters, and explicit cellular traffic contracts.

**Why visit:** a shared target makes large-scale coordination tangible: routine travel stays in directed skylanes, waiting is explicit in hover lanes, and slipstreams isolate the transition points where conflict checks matter.

**Current truth:** the design and proposed MicroBundle catalog are authored; no executable simulator, physical flight model, or 100,000-agent performance result is claimed yet. The five-cell lookahead is a tunable starting parameter, not a validated universal safety distance.

- [Experience home](DroneConstruction/README.md)
- [Experience contract](DroneConstruction/design/EXPERIENCE.md)
- [MicroBundle boundaries](DroneConstruction/design/MICROBUNDLE_BOUNDARIES.md)
- [Architecture SVG](DroneConstruction/assets/drone-construction-architecture.svg)

### Marble Run — continuous motion

**Try:** author a track and terrain, configure gravity and marble properties, release marbles, and observe how changes to the course affect the run.

**Why visit:** ramps, obstacles, collisions, and many moving marbles turn continuous motion into an immediate visual spectacle. The long-term signature exhibit is a large course with thousands of marbles.

**Current truth:** the scene-configuration contract exists; a complete rolling-marble solver and high-count performance result are not yet established. Thousands of marbles is a measured performance goal, not a present capability claim. Start with a small reference scene, validate free fall and contact, then run reproducible scale sweeps before advertising higher counts.

- [Experience home](MarbleRun/README.md)
- [Implementation and tests](../src/MarbleRun/)
- [Physics/MicroBundle boundary and milestones](../docs/MARBLE_RUN_AND_DOMINO_MICROBUNDLE_BOUNDARY.md)

### Capsule Clinic — arcade puzzles and multiplayer networking

**Try:** clear viruses with falling two-color capsules, then challenge another player in a versus match.

**Why visit:** a familiar puzzle becomes a hands-on lesson in deterministic simulation, authoritative multiplayer, replay, and reusable game rules.

**Current truth:** the Experience definition and acceptance plan exist, and an initial deterministic board/game-loop core with focused tests has been added. A complete playable presentation, versus rules, real network transport, and replay are not yet implemented.

- [Experience home](CapsuleClinic/README.md)
- [Game and networking contract](CapsuleClinic/design/EXPERIENCE.md)
- [Implementation milestones](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/issues/22)

## The exhibit pattern

Give every exhibit the same approachable loop:

- **Try:** one obvious, delightful interaction.
- **Change:** one or two meaningful controls, not a cockpit of settings.
- **Observe:** show the real state, events, or contact evidence the implementation supplies.
- **Understand:** explain the model, assumptions, and limitations.
- **Go deeper:** link to the Laboratory, Museum, theory, tests, and authoring tools where relevant.

Use the same Experience identity across browser, desktop, and future VR manifestations. The host may change; the meaning and provenance of the Experience should not.

## Truthful maturity labels

An exhibit may be discoverable before it is fully simulated, but its status and primary action must match reality:

- **Concept:** idea or design brief.
- **Design preview:** authored design or static illustration; clearly labelled as such.
- **Interactive authoring:** visitors can change data, with validation limits stated.
- **Simulation preview:** the real physics implementation is connected; assumptions and measured limitations are visible.
- **Validated showcase:** repeatable tests cover the advertised behavior and benchmark numbers are reproducible.

Do not present a static image as a live scene, a fixture as a deployed experience, an abstract event as physical evidence, or a target body count as a benchmark result.

## Next implementation steps

1. Add the Singularity Exploratorium and these exhibit entries to the WebApp's actual discovery/catalog surface, with clear status labels and stable Experience IDs.
2. Give each entry a browser-accessible **Open** route or an explicitly labelled preview until a live route exists.
3. Connect both Experiences to the same typed Physics run boundary only after the relevant solver APIs and provider resolution are real and tested.
4. Capture reproducible run settings, model/version identity, and truthful event/contact diagnostics.
5. Benchmark Marble Run at increasing counts (for example 100, 1,000, then 10,000 when feasible); publish only measured results.
6. Let Domino Studio author the initial arrangement and first push; subsequent toppling must emerge from the validated physical model.

**North star:** make the visitor say “Whoa—how did it do that?” and then give them the tools to find out.
