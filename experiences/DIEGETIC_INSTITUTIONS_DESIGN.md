# Diegetic Institutions — Experience Architecture

**Status:** architecture and content-authoring proposal. This document does not claim that these institutions, game modes, or their runtime integrations are implemented.

## Purpose

A capability becomes easier to discover when the world gives the visitor a believable reason to use it. The History Museum gives history and siege engineering a home; a War College can give tactics and operational reasoning a home; a Wargaming Hall can host turn-based scenarios; a Strategy Chamber can host civilization-scale decisions.

This is the **diegetic institution pattern**: represent capabilities as places, roles, services, and activities that make sense inside the Experience's world, rather than exposing every capability as a disconnected technical menu.

An institution is not merely a decorative room or a new application. It is an authored entry point into a coherent set of activities, content, rules, and optional simulation capabilities.

## A shared pattern

Each institution should define:

- **In-world reason for being:** why this place or organization exists in the setting.
- **Visitor role:** who the visitor is here—student, curator, commander, ruler, engineer, observer, or another declared role.
- **Activities:** what the visitor can meaningfully do here.
- **Entry and exit:** how the visitor discovers the institution, starts an activity, pauses or leaves, and returns to the wider world.
- **Authority and rules:** which content, scenario, game rules, or simulation contract governs the activity.
- **Progress and consequences:** what persists, what is only a session, and what the visitor learns or changes.
- **Evidence and feedback:** how the Experience explains an outcome, its provenance, and the limits of any model.
- **Accessible alternatives:** a useful text, diagram, or guided path when 3D, audio, networking, or simulation is unavailable.

The institution owns the visitor journey and composition. It does not absorb every underlying capability into one monolith.

## Candidate institutions

| Institution | Diegetic purpose | Activities it can host |
|---|---|---|
| **History Museum** | Preserve and interpret the past | Exhibits, source inspection, timelines, reconstructions, guided learning |
| **War College** | Teach command and operational reasoning | Tactical lessons, planning exercises, logistics, decision reviews, staff problems |
| **Wargaming Hall** | Let participants test decisions under explicit rules | Tabletop scenarios, hex/grid games, turn-based battles, adjudicated campaigns |
| **Strategy Chamber** | Explore the long arc of a society | Civilization-scale choices involving cities, resources, diplomacy, technology, and generations |
| **Campaign Headquarters** | Coordinate a particular ongoing operation | Orders, maps, force organization, intelligence summaries, supply and campaign state |
| **Engineering Yard** | Design and test material systems | Fortifications, siege engines, bridges, vehicles, structural experiments, repair and deconstruction |
| **After-action Archive** | Preserve the reasoning and outcome of completed activities | Replay, event timelines, decision comparisons, source/model inspection, lessons learned |

These are candidate Experience destinations, not a demand that every world contain every building. An institution should exist when it provides a useful, coherent journey.

## One subject, several legitimate experiences

A castle siege can appear in multiple institutions without duplicating or confusing its underlying content:

1. **Museum:** inspect the historical case, its sources, uncertainties, people, place, and chronology.
2. **War College:** study objectives, terrain, supply, defensive options, and command decisions.
3. **Wargaming Hall:** play a scenario with declared units, turn order, action economy, victory conditions, and adjudication rules.
4. **Engineering Yard:** configure an engine or fortification and run only the physical tests the available Physics contract supports.
5. **Strategy Chamber:** explore how a siege affects a wider society over a longer timescale, using explicit strategic abstractions.
6. **After-action Archive:** review the actual choices and results from a specific run.

The same source material, authored place, unit definitions, or historical context may be reused by reference. Each activity must still declare its own rules and abstraction. A tabletop token is not a full physical body; a strategic combat value is not a measured physical quantity; a physics result is not proof that a historical event unfolded that way.

## Wargames: tabletop to civilization scale

“War game” describes a family of decision experiences, not one simulation fidelity or one time scale.

### Tabletop and tactical scenarios

- Explicit board, grid, map, or declared spatial model.
- Units with scenario-specific capabilities, costs, status, and action rules.
- Turns or phases, movement, line of sight, range, morale, supply, and terrain effects as required by the rules.
- Clear action resolution: deterministic rules, stochastic resolution with a declared random seed, or a named adjudication procedure.
- Scenario objectives, victory/defeat conditions, and optional referee/observer role.
- Event log that can explain each meaningful outcome.

### Operational and campaign games

- Multiple engagements connected by persistent objectives and resources.
- Orders, readiness, reinforcement, transport, intelligence uncertainty, and time.
- Explicit distinction between known information, reported information, and hidden state.
- Decisions can have consequences beyond a single encounter.

### Civilization and grand-strategy experiences

- Longer time steps and larger abstractions.
- Cities, populations, production, resources, research, diplomacy, governance, and conflict.
- Rules for how local events aggregate into regional or civilization-level consequences.
- Time-scale and abstraction disclosures: an economy turn or strategic combat resolution is a model, not a claim to simulate every individual.
- Historical or fictional starting conditions remain distinct from player-created alternate histories.

### A common scenario envelope

The exact schema must be validated against the real Experience manifest and MicroBundleDomain contracts before implementation. At the conceptual level, a scenario should identify:

- stable scenario ID, version, and content hash when published;
- setting, scope, and whether it is historical, fictional, educational, or invented for play;
- map/battlespace and coordinate conventions;
- factions, sides, units, roles, and initial state;
- ruleset/version and resolution authority;
- turn/phase or time-step model;
- objectives, termination, and scoring;
- allowed player actions and information boundaries;
- optional model dependencies, inputs, units, and declared limits;
- event/result records and replay requirements;
- attribution, source, and rights metadata for borrowed content.

Do not label these conceptual fields as published MicroBundle IDs or implemented contracts until they have been checked against the actual schemas.

## Keep evidence, rules, and simulation distinct

A well-composed institution can connect three different authorities without merging them:

- **Content authority:** sources, historical claims, literary or fictional canon, interpretation, and uncertainty.
- **Game authority:** scenario rules, turns, player actions, adjudication, victory conditions, and strategic abstractions.
- **Simulation authority:** the model that computes physical or other modeled outcomes, including version, inputs, units, assumptions, and outputs.

A historical exhibit may inform a game scenario, but the scenario is still an authored model. A game outcome can be compared with historical evidence, but it does not validate itself as history. Fictional canon should name the work/version; alternate-history play should be labeled as such. A physical simulation may supply an input or result only through a real, tested contract.

## Composition and ownership

- **Experiences** owns institution identity, visitor journey, scenario identity, learning/play goals, and composition requirements.
- **Domain content artifacts** own source-backed claims, fictional canon, rules definitions, unit descriptions, and other authored domain meaning.
- **Wargame/strategy rules packages** own game-state transitions and adjudication contracts, where such packages exist.
- **Physics** owns physical equations, units, simulation behavior, and run evidence.
- **FSM_API** supplies state-machine behavior at the appropriate boundary; it does not define the meaning of a historical claim or the rules of every game.
- **FSM_COS** resolves and composes declared dependencies; it is not the game rules engine or source-of-truth database.
- **MicroBundleRepository** stores and retrieves immutable artifacts.
- **WebPage/WebApp, AnyApp, VR, Visualize/Renderer** provide manifestations, input, accessibility, and presentation according to their own boundaries.

Do not add dependencies upward from lower-level packages merely to make an institution convenient.

## Persistent world, scoped sessions

The world may provide a persistent location and institutional identity while each activity has its own lifecycle. Define which data belongs to which scope:

- **Institution:** identity, navigation, available activities, authored décor and services.
- **Scenario definition:** immutable or explicitly versioned setup and rules reference.
- **Session:** participants, current state, turn/phase, and session-specific choices.
- **Run evidence:** ordered events, resolution inputs, outputs, and replay metadata.
- **Campaign:** deliberate carry-over between scenarios, with explicit transfer rules.
- **World state:** changes that persist beyond the institution or session, if the host supports them.

Leaving a room must not silently erase a campaign; replaying a scenario must not silently mutate its published definition; completing a game must not change the persistent world unless a declared rule and host authority permit it.

## Visitor journey

1. Encounter an institution naturally in the world or through a world-consistent invitation.
2. Understand its purpose and the role offered to the visitor.
3. Browse activities by subject, scale, duration, complexity, and required capabilities.
4. Inspect the scenario, rules, provenance, and any simulation limitations before starting.
5. Play, learn, design, or observe with visible feedback appropriate to the activity.
6. Pause, leave, resume, or replay without losing track of session/campaign scope.
7. Review an after-action account with enough information to explain or reproduce the result where supported.
8. Return to the wider world with the right things persisted—and no hidden consequences.

## Implementation sequence

1. Treat the History Museum and Siege Wing as the first concrete institution pattern already documented in this repository.
2. Define a minimal shared institution/activity/manifest vocabulary only after checking the actual Experience and MicroBundleDomain contracts.
3. Design one small tabletop siege scenario with explicit turn rules and a text-first event log; keep it playable without Physics.
4. Reuse Castle Siege authored content only through stable references and only where its current contract supports the scenario.
5. Add an optional Physics-backed experiment when Physics exposes a real, testable integration contract.
6. Extend to operational campaigns and civilization-scale strategy only after persistence, time-step, information, and event-history semantics are explicit.
7. Add visual/VR presentation as manifestations of the same activity contract rather than new, duplicated scenario authorities.

## Acceptance gates

- [ ] The institution has an in-world purpose and a coherent visitor journey.
- [ ] Each activity declares its scale, role, rules, lifecycle, and required capabilities.
- [ ] Content provenance, fictional canon, authored interpretation, game rules, and simulation output remain distinct.
- [ ] Scenario identity and rules version are inspectable; published artifacts are immutable and hash-addressed where required.
- [ ] Outcomes are explainable through event or adjudication records appropriate to the rules.
- [ ] Session, campaign, and persistent-world state have explicit boundaries.
- [ ] Missing 3D or Physics support does not block a useful text/diagram/tabletop experience.
- [ ] Optional simulations disclose actual model versions, inputs, units, assumptions, and outputs.
- [ ] Cross-institution reuse references stable authored content rather than cloning competing sources of truth.
- [ ] Implementation and tests are verified before documentation claims a feature is operational.

## Related designs

- [History Museum preforge](HistoryMuseum/README.md)
- [Siege Wing design](HistoryMuseum/SIEGE_WING_DESIGN.md)
- [Castle Sieges booth design](HistoryMuseum/exhibits/CastleSieges/EXHIBIT_DESIGN.md)
- [Castle Siege Experience](CastleSiege/README.md)
- [Experience manifestation architecture](../docs/MANIFESTATION_ARCHITECTURE.md)
