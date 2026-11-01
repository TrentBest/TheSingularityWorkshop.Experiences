![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# Singularity University: an open learning universe

## Founding principle

Singularity University is a learning environment, not an age-gated institution. There is no minimum age for curiosity and no maximum age for returning to fundamentals. A learner enters through an experience that meets their present understanding and can move toward deeper representations without changing the underlying truth of the subject.

The University must not tell a two- or three-year-old that they are too young to enter. It should instead offer a concrete, playful way into a subject: observe, touch, choose, predict, try, notice, and ask again. The same subject remains available to older learners through richer models, mathematics, derivations, evidence, and creation tools. Adults revisiting fundamentals should receive respectful, intelligent instruction—not a childish presentation.

## Learning philosophy

The Workshop's guiding sequence is:

**Play → Surprise → Discover → Understand → Create**

The learner is not a passive consumer. Experiences should invite them to manipulate a world, notice consequences, form questions, test ideas, explain what happened, and build something of their own.

Founding mantras:

- **We don't do this because it's easy.**
- **If a picture is worth a thousand words, imagine the worth of a living picture.**
- **Knowing is half the battle.**

A living picture is not merely an animation. It exposes the rules, variables, relationships, evidence, and changing state that make the phenomenon intelligible. When a picture represents computed science, it must be generated from the same authoritative model/run data it depicts, with its evidence level stated honestly.

## A university without an age gate

Age bands are design guidance, not eligibility gates or ceilings. Use learner agency, accessibility, prior knowledge, and observed readiness rather than assuming every learner of an age is alike.

- **Early explorers:** concrete cause and effect, sound/light/touch/movement, sorting, matching, spatial play, repetition, simple prediction, and caregiver co-exploration. No reading or equation fluency required.
- **Growing investigators:** compare outcomes, identify patterns, vary one condition, describe observations, build and revise models.
- **Makers and modelers:** measurement, geometry, algebra, diagrams, coding, engineering, simulation, and explicit experiments.
- **Advanced explorers:** formal mathematics, scientific literature, numerical methods, uncertainty, research methods, and original projects.
- **Returning learners:** respectful refreshers and diagnostic exploration that rebuild fundamentals without infantilizing adults or presuming what they do and do not know.

These are overlapping pathways. A learner may use an advanced tool in one domain and need a concrete introduction in another. Do not lock subjects, tools, or experiences behind age labels.

## The learning toy universe

The University should make knowledge buildable. A learner should be able to create a city, machine, ecosystem, instrument, laboratory, or imagined world without being constrained by the price or availability of physical construction sets.

Examples include:

- Marble tubes and tracks: assemble paths, release marbles, change slopes and junctions, compare outcomes, then explore gravity, energy, momentum, collisions, friction, and geometry.
- Shape discovery: sort familiar circles, stars, squares, and triangles, then explore irregular outlines, curves, knots, paths, surfaces, symmetry, transformations, topology, and non-Euclidean geometry as the learner is ready.
- Responsive Simon Says: follow or invent sequences involving lights, sound, motion, spatial position, rhythm, pattern, and changing rules.
- Domino worlds: arrange structures, make predictions, apply an initial push, and observe the consequences produced by the physical model.
- Construction worlds: build beyond the limits of a single toy set; move from blocks to architecture, structural mechanics, cities, transportation, and infrastructure.
- Impossible and inaccessible worlds: safely explore the very small, very large, distant, dangerous, expensive, or otherwise inaccessible through clearly identified models and simulations.

These are not a mandate to simulate every toy before proving a useful vertical slice. They form a coherent catalog of experiences that can grow from small, validated building blocks.

## Living experiences, not disposable animations

Every experience should distinguish:
- authored interaction and presentation;
- conceptual illustration;
- analytical teaching model;
- authoritative Physics model output;
- externally sourced or measured evidence.

A learner should be able to pause, inspect, change inputs, compare runs, replay, and progressively reveal the underlying explanation. A visual must never imply a solver capability or scientific certainty that has not been implemented and validated.

The experience should support multiple representations of the same concept: physical-world activity, spatial/visual interaction, spoken explanation where useful, text, diagrams, equations, data, and code. Do not make reading, hearing, color perception, fine motor precision, or a particular display device the only route to participation.

## Physical world and virtual world

Digital exploration should expand physical exploration, not replace it. Begin with a safe real-world observation or construction when practical; use the simulation to inspect, vary, magnify, slow down, or safely explore what is difficult to observe; then invite the learner to return to making and testing.

A headset or first-person environment can provide an expansive workshop, but VR is one manifestation, not a prerequisite for learning. Browser, desktop, mobile, room-scale, and physical activities should share the same experience semantics where practical. Never require Quest hardware or a paid physical kit as the gateway to the subject.

## Safety, privacy, and dignity

For experiences aimed at young children:
- design for caregiver co-exploration and age-appropriate supervision;
- provide safe-material and physical-activity guidance, including water and small-part warnings where relevant;
- do not use advertising, manipulative engagement loops, public social features, or child profiling as the learning model;
- minimize personal data; no child account should be required for basic exploration;
- do not expose unsupervised public chat or publishing;
- make controls, narration, motion, sound, and sensory intensity adjustable;
- respect a learner's choice to stop, repeat, skip, or explore in a different order.

Legal and platform-specific child privacy requirements must be reviewed before any public deployment. This document is an educational design contract, not a legal compliance certification.

## Repository ownership and coordination

**TheSingularityWorkshop.Experiences (Learning) owns:**
- Singularity University identity, departments, learning journeys, exhibit and toy concepts;
- authored Experience semantics, scenarios, goals, instructions, learner-facing interaction, and progression;
- age-flexible presentation profiles and accessible alternative representations;
- construction and creative tools, spatial interaction, and host-independent experience definitions;
- linking an experience to required capabilities without implementing those capabilities itself.

**TheSingularityWorkshop.Physics owns:**
- mathematical and physical quantities, models, solvers, state transitions, and authoritative outcomes;
- units, coordinate frames, assumptions, validity limits, uncertainty, numerical behavior, and conservation/evidence contracts;
- reproducible experiments, run records, analytical comparisons, and solver-backed living data;
- physics theory and mathematics chapters that connect derivation to implementation and validation.

**Shared integration contract:**
1. Learning authors a scenario and declares what physical capabilities it needs.
2. Physics exposes implemented, versioned, testable capabilities and explicit model assumptions.
3. Composition resolves only capabilities that actually exist and satisfy the declared contracts.
4. Learning presents controls and observations without becoming a second physics authority.
5. A run produces provenance-bearing evidence that can be inspected and replayed.
6. The resulting living picture is generated from the run or is clearly labeled conceptual/analytical.

Physics must not depend on Experiences, GUI, a renderer, a browser, Unity, or a headset. Experiences must not duplicate solver logic or imply that a requested capability is already available.

## First coordinated vertical slices

1. **Marble Run:** complete a small, reproducible marble scenario against a validated Physics contact/stepping path; compare the same run with a hand-checkable case and generate a living picture from its run record.
2. **Shape and geometry playground:** begin with editable 2D/3D primitives, transforms, sorting and spatial relations. Teach physical geometry first, then progressively introduce mathematical abstractions. Non-Euclidean geometry and topology should be explicit models, not decorative distortions.
3. **Domino Studio:** retain authored layout ownership in Experiences; use Physics for body state, contact, impulse, and emergent toppling. Never script the fall sequence as if it were physics.
4. **Sound/light/pattern playground:** author responsive sequences and sensory interaction in Experiences; consume validated wave, acoustics, optics, or signal capabilities only as they become available in Physics.
5. **Build-a-world:** let learners combine validated objects, materials, structures, and systems into cities and machines; make resource constraints a creative choice, not a paywall.

Each slice should start small, prove its complete path, then expand. The first implementation should not attempt a universal toy simulator or a universal physics engine.

## Documentation and evidence standard

For every substantial subject, connect:
1. learner-facing invitation and intuitive explanation;
2. theory and progressively revealed formal explanation;
3. authored Experience and interaction contract;
4. Physics model/solver and tests, where the subject is physical;
5. reproducible run/evidence and a living picture;
6. limitations, accessibility, and safety notes.

Mark each claim as conceptual, source-checked, tested, CI-verified, analytically validated, model-backed, or reference-compared as appropriate. Do not call a planned experience implemented. Keep Workshop documentation standards, animated moniker at the top, and canonical footer PNG.

## Not in scope / guardrails

- This initiative does not authorize NuGet publication, release, deployment, master promotion, branch deletion, or privileged actions.
- Work on development; preserve the two-branch policy. Existing extra branches are reconciliation work, not permission to delete them.
- Do not replace or fork the existing GUI package.
- Do not make Physics renderer- or host-dependent.
- Do not make a VR headset, expensive kit, paid course, reading ability, or age threshold a prerequisite for curiosity.
- Do not claim universal coverage or a complete simulation platform before the capabilities and evidence exist.

## Related Physics roadmap

- Physics umbrella mission: https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/38
- Mathematics implementation: https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/42
- Particle/nuclear/collider foundations: https://github.com/TrentBest/TheSingularityWorkshop.Physics/issues/41
- Existing Marble Run / Domino boundary: ../docs/MARBLE_RUN_AND_DOMINO_MICROBUNDLE_BOUNDARY.md


---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
