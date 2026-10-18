# Fluid Dynamics Laboratory — Experience Design

**Status:** preforge design. Existing Physics wave and shallow-water primitives inform the plan; the full department and its interactive manifestation are not implemented.

## Intent

Create a Singularity Laboratory department where visitors can explore water as a physical state that changes over space and time. Make the path from simple waves to terrain, rainfall, vessels, and structures visible without implying that every adjacent physical model already exists.

Spatial hierarchy:

`Singularity Workshop → Singularity Laboratory → Fluid Dynamics Department → rooms / basins / test tanks`

A future authoritative Place record must link stable place identity, parent, scene identity, capability/tool, and entry point. This design and its SVG do not register runtime Places.

## Room contracts

| Room | Visitor action | Authoritative state | Truth check |
|---|---|---|---|
| Water Gallery | Inspect units, domain, boundaries, and model assumptions | Selected basin definition and model identity | Do not confuse a water-level field with full 3D fluid state |
| Wave Basin | Change a supported initial wave and compare runs | Inputs and outputs from the selected wave model | Label 1D reference behavior separately from 2D shallow-water behavior |
| Bathymetry & Terrain | Change a depth/terrain field | Basin geometry and bathymetry inputs | Terrain affects water only through a connected model |
| Rainfall & Runoff | Change a rainfall-rate field | Declared rainfall input and basin state | A rainfall field alone does not establish infiltration, drainage, or watershed hydrology |
| Vessel Test Tank | Place a vessel and investigate its response | Body state plus fluid-force model, when available | No simulated buoyancy, drag, or vessel motion until the coupling is real |
| Shoreline & Structures | Compare a shoreline, barrier, or structure | Boundary geometry and any connected force/erosion model | Do not invent structural loads, erosion, or damage from the rendered waterline |
| Evidence & Timeline | Replay and compare experiment revisions | Inputs, solver/model version, outputs, units, diagnostics | Preserve provenance and distinguish authored previews from calculated results |

## Current capability boundary

The Physics repository contains:
- a 1D linear-wave basin reference;
- 2D shallow-water basin models, including bathymetry;
- a rainfall-rate field.

The repository's gap register still identifies the following as missing or not established at general-world scope:
- coupled solid/fluid forces and boat/body motion;
- general 3D flow;
- validated cross-domain mass and energy exchange;
- a complete world in which structures, terrain, and vessels respond causally to the water.

This experience must consume those capabilities through their real contracts. It must not clone solvers into the Experience layer or make presentation code authoritative.

## Shared experiment record

Each runnable exhibit should retain:
- stable experiment key and revision;
- model and solver identity/version;
- spatial domain, units, initial conditions, and boundary conditions;
- geometry and bathymetry identity/version;
- time-step and run settings actually consumed by the model;
- measured output channels and units;
- conservation, validity, and numerical diagnostics that the solver supports;
- ordered events and a maturity label: concept, authored preview, calculated, simulated, or validated.

## Living-image principle

The water-level visualization should be derived from model output. A visitor should be able to pause, scrub or replay where supported, select a cell/region, and inspect its value with units. If a host renders illustrative waves before live integration exists, label them as authored preview. Do not make a decorative animation masquerade as simulation.

## Dependency progression

1. Verify the public APIs and tests for existing wave, shallow-water, bathymetry, and rainfall models.
2. Build one deterministic basin experiment with explicit boundary/initial conditions.
3. Generate an evidence record and visual from actual output; test repeatability and the applicable water-volume accounting.
4. Expose supported controls and diagnostics through the host-neutral Experience contract.
5. Add a vessel only after a buoyancy model and fluid/body force exchange contract are implemented and tested.
6. Add structure loading, erosion, or 3D flow only as independent validated capabilities.

## Acceptance criteria

- The floor plan and room catalog agree on the seven room identities.
- The room catalog validates against its declared JSON Schema.
- Existing capabilities and missing coupling are clearly distinguished.
- No numeric Experience or MicroBundle IDs are invented.
- No room claims to be runtime registered or host-runnable before integration proves it.
- A future living image is driven from authoritative output and carries model identity, units, and maturity.
