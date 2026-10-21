# UML Inventory

This inventory maps the new Drone Construction production source files to manually maintained, source-grounded diagrams. These diagrams document the current implementation; they do not imply that planned simulation capabilities exist.

## Drone Construction

| Source file | UML / living picture | Scope |
|---|---|---|
| src/DroneConstruction/ConstructionContracts.cs | [Construction contracts](assets/drone-construction-contracts.svg) | Height field, traffic cells, skylane, hover lane, slipstream, build cell, emitter |
| src/DroneConstruction/BuildGrid.cs | [Build grid](assets/drone-construction-build-grid.svg) | Required/placed counts and completion invariants |
| src/DroneConstruction.MicroBundles/DroneConstructionMicroBundles.cs | [MicroBundle providers](assets/drone-construction-microbundles.svg) | Seven implemented provider identities and typed factories |

## Grounding rules

- A diagram must correspond to the source at the same development revision.
- Solid relationships represent implemented source relationships; dashed relationships represent design-level relationships.
- No diagram may label a planned agent scheduler, full simulation, physical flight model, Renderer adapter, or SVG telemetry observer as implemented.
- Update the relevant SVG and this inventory in the same change as a source change that alters the documented public shape.
- These are manually maintained diagrams, not generated Roslyn UML. CI must not claim automatic source-to-UML verification until such a generator exists.
