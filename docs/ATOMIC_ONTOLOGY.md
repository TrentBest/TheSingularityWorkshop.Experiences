# Atomic Ontology Corpus

The Workshop ontology is not intended to become one universal monolithic database.

An atom is a semantic boundary that can be independently identified, composed, represented, published, and manifested. Richer bodies of knowledge are built by relating those atoms.

## Corpus A — Elements

An Element is an atomic semantic identity, not a periodic-table UI card. The initial seed is intentionally tiny: atomic number, symbol, name, and atomic weight. That is the identity seed, not the boundary of the knowledge.

The Element corpus should be able to carry related semantic knowledge including:

- isotopes and nuclides
- electron configuration, shells, oxidation states, valence, ionization energy, electron affinity, and electronegativity
- density, phase-change data, critical points, heat capacity, thermal conductivity, diffusivity, and expansion
- electrical, magnetic, optical, and mechanical properties
- crystal structures, allotropes, and phase diagrams
- chemical bonding, compounds, reactions, and stoichiometric relationships
- corrosion and environmental interactions
- strength, ductility, hardness, fracture behavior, fatigue, and related material behavior
- radioactive isotopes, decay modes, half-lives, and nuclear properties
- provenance, units, conditions, and temperature/pressure dependence

The boundary matters: these are related semantic facts and behaviors, not fields that must all be stuffed into Element.

Example semantic neighborhood:

    Iron
      |
      +--> isotope / nuclide relationships
      +--> phase behavior
      +--> thermal properties
      +--> mechanical properties
      +--> electrical properties
      +--> magnetic properties
      +--> chemical relationships
      +--> nuclear relationships

This gives later Experiences room to represent chemistry, materials science, thermodynamics, hydrodynamics, fracture, melting, and nuclear knowledge without coupling that knowledge to a renderer.

### Multiple manifestations

The same Element identity may be manifested as textbook/reference material, chemistry laboratory content, engineering/materials views, nuclear science, simulation, sci-fi/narrative, compact debug data, or interactive detail. The semantic data is not duplicated for each visual style.

### First implementation pressure test

    Iron -> Element semantic record -> ElementMicroBundle -> FSM_COS
         -> repository artifact -> Experience manifest -> WebApp / AnyApp / MyVR

The old WebPage Element work is architectural precedent. The new corpus keeps semantic identity independent from provider and manifestation while removing legacy host lifecycle and renderer assumptions.

## Corpus B — Digital Logic

Digital Logic applies the same test to constructed systems. The corpus should distinguish Physical, Combinational, Sequential, Modular Functional Units, Control and Timing, Programmable, and Visualization.

The important question is not merely what a NAND gate is. It is what composes it and what relationships exist between its abstraction levels:

    NAND
      +--> logical truth/function
      +--> physical realization
      |    +--> transistors / switching devices
      |    +--> materials
      |    +--> power
      |    +--> propagation characteristics
      |    +--> fabrication/process context
      +--> composition into other gates and larger units
      +--> timing / electrical behavior
      +--> implementation variants
      +--> visualization

A NAND gate therefore need not be a terminal leaf. It can expose its construction and behavior at different levels of detail.

## Hardware observation — future extension

The ontology should leave room for a hardware observation layer without making live telemetry a requirement of semantic atoms.

A future system may identify the available CPU/GPU and construct a representation of topology, cores/execution units, caches/memory hierarchy, work domains, and available telemetry. An observer can then manifest traffic/work, queue/utilization, timing/throughput, or architecture views.

The traffic-on-a-roadway metaphor is a manifestation idea: work enters, is routed through computational structures, waits, executes, and leaves. The actual telemetry available on a machine determines what can honestly be shown.

That is deliberately separate from the ontology. We should be able to understand CPU/GPU architecture without live telemetry, and observe a real machine without rewriting the semantic model.

## Arbitrary-reality test

1. Is it atomic enough to stand alone?
2. If not, what smaller semantic bundles compose it?
3. What relationships does it have to other atoms?
4. Which data is semantic and which is manifestation-specific?
5. Is a creator requiring, preferring, optionally using, delegating, or framing the atom?
6. Which information is immutable identity and which is contextual or observed?
7. Can FSM_COS compose it without knowing how a host renders it?
8. Can the same identity survive WebApp, AnyApp, MyVR, laboratory, engineering, and future VR manifestations?

## Anti-monolith rule

Do not solve increasing complexity by adding every new concept as a property of an existing atom. Prefer a small semantic atom plus related atoms, relationships, contextual observations, capabilities, and manifestations.

This is how a periodic table can grow into chemistry/materials science, digital logic can grow into processors, and hardware observation can become a manifestation over the same semantic machinery rather than a second architecture.