# Chemistry Laboratory — Experience Design

**Status:** preforge design, not yet an executable chemistry simulation.

## Intent

Create a research department inside the Singularity Laboratory that makes matter inspectable from atomic identity through molecular structure, reactions, energy transfer, materials, and combustion. It should feel spectacular because real state and causal relationships become visible—not because decoration is mistaken for evidence.

The floor is an Experience: an environment composed from focused MicroBundles. It is not a Blazor page, a physics engine, or a collection of unrelated animations.

## Spatial hierarchy

`Singularity Workshop → Laboratory → Chemistry Department → rooms / benches`

The department's Places should eventually be authoritative records connecting place identity, parent, scene ID, capability/tool, and entry point. This document and its SVG are the design seed; they do not claim that WebPage has already registered these rooms.

## Room contracts

| Room | Visitor action | Authoritative state | Required truth check |
|---|---|---|---|
| Atomic Gallery | Inspect or compare elements | Element identity and sourced properties | Identity is not a species/material model |
| Molecular Assembly | Add atoms and candidate bonds | Species graph, charge, bond order, geometry | Validate chemistry before calling a structure valid |
| Reaction Theatre | Choose reactants and products | Reaction definition, balanced coefficients, conditions | Balanced does not mean experimentally established |
| Thermal Observatory | Inspect energy movement | Named energy reservoirs, heat capacity/model assumptions | Units and sign conventions are explicit |
| Combustion Chamber | Change fuel/oxidizer/surface/initial state | Surface cell state plus selected kinetics/transport model | State storage alone is not combustion simulation |
| Materials Bench | Compare candidate materials | Material identity and versioned property set | Element properties do not imply bulk material properties |
| Evidence & Timeline | Step, replay, compare | Experiment revision, inputs, outputs, model/version | Reproducibility includes solver and configuration identity |

## Canonical experiment record

The first implementation should define a versioned, serializable experiment record containing:

- stable experiment key and revision;
- reactant/product species references and stoichiometric coefficients;
- initial conditions and explicit units;
- environmental conditions required by the selected model;
- selected model and version, including solver settings;
- declared fixed and varied parameters;
- output channels, each with quantity name, units, provenance, and validity;
- conservation checks and numeric tolerances;
- ordered event/timeline records;
- a maturity label: concept, authored data, calculated, simulated, or validated.

Keep the saved experiment editable and mutable. Published MicroBundle artifacts remain immutable and content-addressed.

## Dependency progression

1. **Element identity:** atomic number, symbol, name, atomic weight, source/version, validity checks.
2. **Species:** element/isotope counts, charge, structure representation, explicit identity.
3. **Bond and geometry:** bond order, valence rules, geometry model, validation diagnostics.
4. **Reaction:** reactants, products, coefficients, conditions, reversibility/kinetic model if supported.
5. **Stoichiometry:** balancing and conserved-quantity accounting with independently tested algorithms.
6. **Thermochemistry:** heat capacity, formation enthalpy or other selected properties with source, reference state, units, and uncertainty where known.
7. **Kinetics:** rate law, parameters, units, valid domain, and reference tests.
8. **Coupling:** explicit adapter from reaction rates/energy terms into thermal and combustion state.
9. **Transport:** diffusion/advection or surface exchange only when implemented and tested.
10. **Evidence:** reproducible runs, independent reference cases, error/conservation reports, and diagrams generated from actual output.

Do not collapse these into one giant chemistry bundle. Each capability should remain independently addressable and replaceable.

## Model and safety posture

This is a computational research/education environment. Keep the initial examples focused on benign, well-characterized reference cases and symbolic/abstract experiments. The department must not turn into instructions for performing dangerous chemical synthesis, optimizing harmful substances, or operating real combustion experiments. Physical apparatus in the visual floor is illustrative unless an actual, reviewed laboratory integration exists.

## Acceptance criteria for the first useful slice

- A newcomer can identify the rooms and follow a coherent experiment journey.
- A conceptual floor diagram is linked from the Experience home and labelled as conceptual.
- No numeric Experience or MicroBundle IDs are invented; reserve them against the current registries.
- JSON examples validate against a declared schema when that schema exists.
- Element identity, species identity, reaction balancing, and solver outputs are not conflated.
- Every displayed computed value includes units and model provenance.
- At least one reference reaction can be balanced and tested independently before kinetic/thermal claims are shown.
- A future living image is driven from experiment state and events, not a hand-authored animation pretending to be a solver.
