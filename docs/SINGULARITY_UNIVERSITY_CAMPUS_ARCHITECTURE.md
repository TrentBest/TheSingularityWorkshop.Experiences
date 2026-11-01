![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# Singularity University Campus Architecture

## 1. Purpose

Singularity University is conceived as a **living, explorable campus**. The learner should experience the University from the ground: buildings, paths, people, activity, discoveries, and places where knowledge is being explored and created.

The campus is organized by **topic and department, never by age**. A Mathematics building is for mathematics. A Physics building is for physics. Learners of different ages and prior experience share the same department and can choose or reveal different representations and levels of depth without being separated into age-specific institutions.

This is an architectural and experience direction. It is not a claim that a complete campus, schedule service, navigation system, or virtual-world population is already implemented.

## 2. The learner's journey

The primary journey is:

```text
Campus Grounds
    |
    v
Department Building
    |
    v
Department Lobby
    |---- Schedule
    |---- Building / Floor Map
    |---- Department Directory
    |---- Discoveries / Events
    |
    v
Floor
    |
    v
Classroom / Laboratory / Studio
    |
    v
Learning Experience
```

### Campus grounds

The grounds establish a sense of place. Buildings are recognizable landmarks; paths and public spaces invite exploration. Ambient activity should communicate that the world is alive without requiring the learner to interact with every character or event.

### Department buildings

Each building represents a field of knowledge, such as Mathematics, Physics, Life Sciences, Engineering, Computing, Arts, or History. The department is the organizing identity; it is not a learner age group.

### Lobby

The lobby is the department's orientation and navigation hub. On arrival, the learner can understand what the department explores, see a directory and floor map, review a schedule, discover available activities, and choose where to go next.

The lobby should feel like a real place in the campus, while keeping navigation clear and immediately usable. A learner should never need to wander physically just to find essential information.

### Floors

Floors group related subjects, subdepartments, facilities, and learning spaces. Their organization should be meaningful to the discipline rather than arbitrary game levels. A floor can contain classrooms, laboratories, libraries, galleries, studios, workshops, and quiet exploration spaces.

### Classrooms, laboratories, and studios

A room is the destination for a specific Experience. It may host an interactive lesson, experiment, exhibit, creative workshop, collaborative challenge, or self-paced investigation. The room launches or manifests the actual Experience; it should not duplicate that Experience's domain logic.

## 3. Schedule, map, and free exploration

The lobby schedule and map are two views of the same navigable campus structure.

A schedule entry should resolve to a canonical destination:
- department building;
- floor;
- room;
- Experience identity;
- session/activity identity and timing, when applicable.

Selecting a schedule entry should offer direct navigation to that destination. The floor map and directory should resolve to the same room identity, avoiding duplicate or contradictory location data.

The schedule is a guide, not a gate. Learners may follow a planned session, save an activity for later, revisit a room, or freely explore outside scheduled sessions. Self-paced and asynchronous Experiences must remain first-class. Do not introduce punitive attendance, streak pressure, or manipulative engagement mechanics.

## 4. One department, many ways into the subject

Age is not the campus taxonomy. Do not create age-based buildings, floors, classrooms, accounts, or hard eligibility gates.

Within a department, the same subject can be approached through progressively revealed representations:
- concrete play, observation, sensory interaction, and cause and effect;
- comparison, pattern discovery, prediction, and guided experiments;
- measurement, diagrams, models, and formal vocabulary;
- mathematics, equations, code, data, research, and original creation.

These are not rigid age bands. A learner may want an intuitive introduction to one topic and advanced tools for another. Adaptation should follow learner choice, accessibility preferences, context, and demonstrated understanding—not age stereotypes. Adults returning to fundamentals deserve a respectful, intelligent presentation. Early explorers deserve a meaningful way to participate without reading or equation fluency.

## 5. A living world, with purposeful restraint

The campus should feel inhabited: people moving through shared spaces, activity visible through windows and doorways, events in progress, exhibits inviting participation, and signs of discovery throughout the grounds.

Ambient life is there to make the campus legible and welcoming, not to create noise or pressure. Begin with a small number of purposeful, deterministic activities and visible states. Every character, event, or moving element should have a reason to exist and a bounded behavior model. Avoid implying real social interaction where only scripted ambience exists.

## 6. Accessibility and alternate navigation

Spatial exploration is an option, not a requirement. The same campus must be navigable through:
- a map and floor directory;
- a searchable or browsable department/room list;
- direct schedule links;
- keyboard, controller, touch, and accessible focus navigation as supported by the host;
- clear text labels and screen-reader-friendly structure.

Do not make color, sound, fine motor precision, spatial orientation, reading speed, or a VR headset the only way to find a classroom. Provide adjustable motion, audio, narration, and sensory intensity. VR is one manifestation of the campus, not the University itself.

## 7. Ownership and architecture boundaries

**Experiences owns**
- campus identity, department taxonomy, and navigational semantics;
- department, floor, room, and schedule destination definitions;
- learner journeys and authored classroom/studio experiences;
- learner interaction, accessibility alternatives, and age-flexible representations;
- campus-specific ambient activity and authored world content.

**Physics owns**
- mathematical and physical quantities, models, solvers, and authoritative outcomes;
- numerical validation, assumptions, uncertainty, and reproducible run records;
- evidence used by physics-backed laboratories and living pictures.

The campus must not implement a second physics authority. A physics laboratory can author its scenario and interaction in Experiences while consuming actual versioned Physics capabilities for calculated outcomes.

The campus is an Experience and manifestation of the existing Workshop architecture—not a new host, runtime, or renderer. Reuse existing Experience manifests, artifact identities, MicroBundle contracts, FSM_COS composition, and the canonical GUI / Visualize stack. Inspect current contracts before adding new ones; do not invent a parallel manifest, scheduler, GUI, or navigation framework.

## 8. First complete vertical slice

Build one complete path before expanding the whole University:

1. Inspect existing Experience manifest, catalog, identity, and host contracts.
2. Define stable identities and relationships for campus, department, floor, room, and schedule destination.
3. Author one department building with a recognizable lobby and at least two floors.
4. Place one existing Experience in a classroom.
5. Navigate from campus grounds to building, lobby, floor, room, and the Experience.
6. Show a schedule entry that resolves to that same canonical room and supports direct navigation.
7. Provide an equivalent map/directory route that does not require spatial movement.
8. Test free exploration when no schedule is present.
9. Record what is conceptual, implemented, tested, and CI-verified.

Only after this path is complete should the campus expand to additional departments, richer ambient activity, and larger multi-floor environments.

## 9. Evidence and documentation

Keep theory before implementation. Document the learner journey, domain identities, contracts, state transitions, host responsibilities, accessibility, and limitations. Use diagrams that correspond to real contracts and implementation. Mark conceptual designs as conceptual; do not present mock navigation or ambient animation as a functioning campus.

## 10. Guardrails

- No age-based campus segregation or hard age eligibility.
- No mandatory VR hardware, paid kit, or reading ability.
- No replacement GUI or renderer.
- No duplicated Physics implementation in Experiences.
- Work on `development`; do not promote to `master`.
- No branch deletion, NuGet publication, release, or deployment without explicit authorization.

## Related work

- [Campus implementation and delivery issue #43](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/issues/43)
- [Singularity University learning philosophy](SINGULARITY_UNIVERSITY_LEARNING_PHILOSOPHY.md)
- [University-wide age-inclusive learning mission #42](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/issues/42)

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
