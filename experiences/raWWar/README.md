# raWWar — The Living Galactic War

<badge>Concept / authored Experience vision</badge>

**A civilization learns its future before it has to live it.**

raWWar is a far-future, technological-magical universe built around a single continuing galaxy-scale conflict—and the generations of people, institutions, machines, and worlds shaped by it.

This is not a collection of disconnected battle maps. It is a living civilization whose children learn how their world works, whose adults choose vocations and responsibilities, and whose soldiers inhabit sophisticated exoskeleton systems as one part of a much larger society.

## The invitation

Imagine growing up in a civilization where the extraordinary is ordinary.

A child can study a machine that moves like a second body, watch a ghost instructor demonstrate its balance and control, explore how a vehicle is assembled, or ask why a distant world needs a supply convoy. What first appears magical is made understandable through observation, experimentation, engineering, and practice.

Years later, that same person may enter a workshop, research institution, fleet, medical service, logistics corps, or military formation with a real understanding of the systems around them.

The promise is not that every child becomes a soldier. The promise is that learning opens doors into a future that feels vast, coherent, and worth exploring.

## The world

- **Far-future humanity:** a speculative history extending from Earth into a Milky Way civilization. Avoid importing present-day military, political, or cultural assumptions as if they were universal.
- **Technological wonder:** exoskeletons, vehicles, fabrication, sensors, life-support, materials science, fleets, and planetary infrastructure are part of everyday life.
- **The magical frontier:** the universe should feel wondrous and occasionally uncanny. Whether a phenomenon is technological, physical, or genuinely supernatural is a worldbuilding question to resolve deliberately—not an excuse to make systems arbitrary.
- **A living society:** education, work, research, trade, logistics, governance, family, reputation, and military service belong to the same world.
- **One continuing war:** conflict is a persistent condition with causes, consequences, changing borders, political pressure, supply requirements, and people who must live with the results.

## Experience pillars

1. **Wonder before exposition.** Let visitors see, touch, inspect, and experiment before asking them to read a technical explanation.
2. **Learning has a place in the world.** Schools, academies, workshops, laboratories, barracks, shipyards, and public institutions are diegetic places—not disconnected menu screens.
3. **People are the story.** Soldiers are people with histories, relationships, qualifications, responsibilities, equipment, and reputations; they are not disposable counters on a map.
4. **The machine is understandable.** Exoskeletons and vehicles are inspectable engineered systems, with visible components, constraints, failure modes, and upgrade consequences.
5. **The galaxy is continuous.** A local action can connect to squad readiness, supply, planetary control, faction power, and civilization-scale politics.
6. **One world, many perspectives.** The same authoritative world can be experienced as a student, engineer, enlisted soldier, commander, historian, or observer.

## The exoskeleton as a second body

Soldiers inhabit their exoskeletons; the suit is not a cosmetic skin worn over an otherwise unrelated avatar. It is a structured, inspectable interface between a person and the physical world.

The intended design language includes:

- a sealed inner life-support system and fused chassis;
- modular armor plates and engineered structural components;
- advanced materials such as nanocarbon composites;
- distributed actuation, sensing, and control;
- serviceable assemblies, obstructions, thermal behavior, and failure states;
- research variants whose differences can be inspected and tested.

A suit should be explainable down to its reusable geometry and functional assemblies. A visitor should be able to inspect a component, understand what it protects or enables, test a design hypothesis in a controlled demonstration, and observe the consequences.

## The visitor's journey

The intended scale progression is:

**Galaxy → star system → world → institution or faction → formation → vehicle or exoskeleton → individual → consequence.**

The visitor can travel in either direction. A planetary campaign should resolve into the lives and equipment of people; an individual's action should be traceable back into the larger world.

## Campaign perspectives

The same continuing war supports two complementary perspectives:

- **Commander:** begin with authority over a faction and make strategic decisions; enter the world at soldier scale when the consequences need to be experienced directly.
- **Enlisted:** begin at the lowest practical level, follow orders, learn a trade, qualify for responsibilities, and potentially rise into command. The world is first encountered from the ground up.

These are not separate universes or unrelated rule sets. They are different positions and degrees of agency within the same simulation.

## Truth and maturity

This repository currently contains the authored vision and design contracts only. It does not claim a functioning persistent galaxy, live campaign simulation, combat runtime, exoskeleton physics, or VR training system.

Design documents distinguish:

- **Canon:** decisions already established for the Experience.
- **Candidate:** a coherent design direction awaiting a concrete contract or prototype.
- **Experiment:** a testable idea that may be changed by evidence.
- **Illumination needed:** an open question that must not be silently treated as settled.

## Design documents

- [Learning, training, and vocation](design/LEARNING_AND_TRAINING.md)
- [Persistent war and campaign perspectives](design/PERSISTENT_WAR_AND_CAMPAIGNS.md)

## Intended technical boundaries

- Experiences owns the authored world, Experience identity, and domain meaning.
- Physics owns reusable physical models and math.
- GUI.Core and Visualize own observer-independent visual meaning and visualization data; SVG or VR is a manifestation, not the source of truth.
- FSM_COS composes capabilities; AnyApp and WebApp host manifestations.
- Training and combat must use the same underlying physical and domain rules where they represent the same world.

The first implementation should prove one small, inspectable vertical slice rather than pretend the whole galaxy already exists.
