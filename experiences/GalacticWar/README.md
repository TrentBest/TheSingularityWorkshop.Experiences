# Galactic War — A Far-Future Science-Fantasy Experience

<badge>Concept / Experience preforge</badge>

**A universe of impossible machines, extraordinary people, and a future worth learning how to shape.**

Galactic War is a far-future technological-magical universe in which humanity and other powers contend across a vast galaxy. It is designed as a living Experience rather than a sequence of isolated combat levels: personal stories, learning, social life, engineering, missions, spacecraft, factions, and galaxy-scale consequences belong to one connected world.

Its signature promise is scale with intimacy. A visitor can begin with one person trying to survive a dangerous encounter, discover that the apparent mission is only part of a larger problem, prepare with other people, and eventually see how the outcome changes a world or the galaxy.

This is a distinct Experience preforge. **raWWar is a combat-style reference, not the identity or source of truth for this project.**

## The invitation

Imagine a civilization so far beyond our own that its technology can look like magic. People grow up surrounded by systems they can explore, understand, and eventually help operate. Learning is not a pause from the world: it is how people gain agency within it.

The visitor should be able to:
- discover extraordinary machines and learn how they work;
- inhabit a first-person role and experience danger, uncertainty, and teamwork;
- inspect and operate exoskeletons as engineered extensions of the body;
- move from ground engagements into spacecraft, ships, capital vessels, fleets, and alliances;
- meet heroes, villains, allies, rivals, and ordinary people with their own motives;
- see the consequences of missions ripple outward through a living social and political galaxy.

## Experience pillars

1. **Technological magic.** The world should feel wondrous, but its systems should remain discoverable and internally coherent. Advanced technology, fictional physics, and any genuinely magical phenomena need explicit rules.
2. **Survival before spectacle.** The visitor's first problem is often to survive and understand what is happening. Power and context are earned through discovery, preparation, and action.
3. **A mission is a changing question.** The initial objective may be incomplete, misleading, or only one part of a larger situation. Discovery changes what the visitor believes the mission to be.
4. **People make the galaxy.** Relationships, trust, institutions, cultures, factions, and conflicting accounts matter as much as weapons and vehicles.
5. **Scale is continuous.** Personal equipment, exoskeletons, spacecraft, ships, capital ships, fleets, and galaxy-wide alliances are connected layers of the same world.
6. **Learning creates agency.** Early learning introduces the systems and choices that shape the future. It should open paths, not force every learner toward combat or military service.
7. **Consequences persist.** A mission should change something meaningful: knowledge, relationships, resources, access, ownership, alliances, or the balance of power.

## The signature mission rhythm

**Survive → Discover → Solve → Prepare → Execute → Change the Galaxy**

- **Survive:** enter a situation with incomplete information and immediate constraints.
- **Discover:** learn what is really happening, who is involved, and what the apparent objective leaves out.
- **Solve:** choose an approach, recruit help, gather information, and identify what must be built, learned, or secured.
- **Prepare:** assemble people, equipment, vehicles, intelligence, and logistics.
- **Execute:** carry out the plan in an embodied, first-person experience where positioning, timing, momentum, and teamwork matter.
- **Change the Galaxy:** resolve the outcome into the shared world and make its consequences discoverable.

Not every mission needs every phase to have equal duration. The rhythm is a design pattern, not a mandatory six-screen UI.

## Scale progression

**Person → exoskeleton → craft → ship → capital ship → fleet → alliance → galaxy**

Each increase in scale should preserve meaningful relationships to the level below it. A fleet is not just a bigger icon; it is people, vessels, readiness, supply, command, and political commitments. A personal action can matter at larger scales without pretending that one individual controls the galaxy.

## Learning and social life

Learning begins early as exploration of a complex and wondrous civilization. People discover sciences, crafts, institutions, vehicles, social responsibilities, and possible vocations. Advanced training is a later, deliberate choice with prerequisites and meaningful qualifications.

The galaxy must also contain ordinary life: friendships, families, work, trade, culture, recreation, disagreement, institutions, and communities. These are not decorative downtime between battles; they give conflict something real to affect and give the visitor reasons to care about the outcome.

## Design status and truth

This repository currently captures authored intent and design boundaries. It does not claim a functioning galaxy simulation, deployed multiplayer world, combat runtime, exoskeleton physics, spacecraft flight model, or VR training system.

- **Canon:** approved decisions that future implementation must preserve.
- **Candidate:** a coherent direction awaiting a concrete contract or prototype.
- **Experiment:** a testable proposal whose result may change the design.
- **Illumination needed:** an unresolved question that must not be silently treated as settled.

## Design documents

- [Experience vision and world principles](design/EXPERIENCE_VISION.md)
- [Mission loop and consequence model](design/MISSION_LOOP.md)
- [Exoskeletons and spacecraft combat](design/EMBODIED_COMBAT_AND_SPACEFLIGHT.md)
- [Learning and the living social galaxy](design/LEARNING_AND_SOCIAL_GALAXY.md)

## Architecture boundaries

- Experiences owns the authored Experience identity, world meaning, and design contracts.
- Physics owns reusable physical models and mathematical foundations.
- GUI.Core and Visualize own neutral visual meaning; SVG, desktop, and VR are manifestations, not canonical world state.
- FSM_COS composes capabilities; AnyApp and WebApp host the Experience.
- Authoritative world state, simulation rules, and presentation must remain distinct.
- A visual prototype must be labelled as a prototype until it is connected to real domain state.

The first implementation should prove one small vertical slice end to end. It should not begin by pretending to simulate the entire galaxy.
