# UML Inventory

This inventory maps the new Drone Construction production source files to manually maintained, source-grounded diagrams. These diagrams document the current implementation; they do not imply that planned simulation capabilities exist.

## Drone Construction

| Source file | UML / living picture | Scope |
|---|---|---|
| src/DroneConstruction/ConstructionContracts.cs | [Construction contracts](assets/drone-construction-contracts.svg) | Height field, traffic cells, skylane, hover lane, slipstream, build cell, emitter |
| src/DroneConstruction/BuildGrid.cs | [Build grid](assets/drone-construction-build-grid.svg) | Required/placed counts and completion invariants |
| src/DroneConstruction/AgentAndTaskContracts.cs | [Agent and task contracts](assets/drone-construction-agent-task.svg) | Agent lifecycle, deterministic assignment, completion and requeue |
| src/DroneConstruction.MicroBundles/DroneConstructionMicroBundles.cs | [MicroBundle providers](assets/drone-construction-microbundles.svg) | Nine provider identities and typed factories |\n| src/DroneConstruction/ConstructionSimulation.cs | [Deterministic simulation](assets/drone-construction-simulation.svg) | Single-threaded task, pickup, cell movement, placement, and snapshot loop |

## Grounding rules

- A diagram must correspond to the source at the same development revision.
- Solid relationships represent implemented source relationships; dashed relationships represent design-level relationships.
- The simulation diagram covers the implemented single-threaded discrete loop only. It must not imply skylane/slipstream admission, physical flight, a Renderer adapter, or a live SVG telemetry observer.
- Update the relevant SVG and this inventory in the same change as a source change that alters the documented public shape.
- These are manually maintained diagrams, not generated Roslyn UML. CI must not claim automatic source-to-UML verification until such a generator exists.
