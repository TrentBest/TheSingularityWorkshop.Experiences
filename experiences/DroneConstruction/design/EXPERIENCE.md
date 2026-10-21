# Drone Construction — Experience Contract

**Status:** authored design contract. This document defines intended semantics; it is not evidence that a runtime exists.

## Purpose

Reconstruct the lost large-scale construction demo using the Workshop's renderer-independent tooling. A population of simulated quadcopters converts a height-field target into a block structure by repeatedly acquiring a block from a resource emitter and placing it at an unfinished target location.

The first demonstrator is a discrete simulation. Physical flight dynamics are a separate, later capability and must not be implied by cell-based movement.

## Domain objects

### Height field

A height field contains a width, height, channel-to-height mapping, maximum target height, coordinate convention, and source digest. Mapping must be explicit and versioned. For an RGB source, a mapping may treat channels as separate contributions or combine them into a scalar; the choice must be visible in configuration and reproducible in a saved run.

For every build cell, the target is a non-negative integer number of blocks. Validate dimensions, maximum height, overflow, and unsupported channel/mapping values before creating tasks.

### Build grid

The build grid tracks required block count and placed block count per cell. Invariant: placed count never exceeds required count. A task refers to one specific cell and one remaining block slot; retries must not duplicate a placement.

### Resource emitter

The initial scene has ten emitters. Each has a stable ID, finite or explicitly replenished inventory, pickup capacity, and queue policy. Inventory decrements only when a pickup is committed. Failed or cancelled pickups must not silently consume inventory.

### Agent

An agent has a stable ID, current discrete position, movement state, assigned task, carried-block flag, and lifecycle state. Agent state changes must be explicit events. The initial model need not simulate aerodynamic forces.

### Traffic cells and volumes

A skylane and a hover lane are 2D cellular grids. A cell can have at most one occupying agent unless a future, explicitly tested capacity contract says otherwise. A slipstream is a bounded transition volume with explicit reservations and occupancy/conflict checks.

The original five-cell lookahead is a named parameter. The model must not treat it as a proven safety distance. Transition clearance must account for the configured discrete movement model, cell occupancy, and transition capacity; a later continuous model must independently validate braking/acceleration distances.

## Core invariants

1. Stable IDs are unique within their declared domain.
2. One agent cannot occupy two exclusive cells in the same simulation tick.
3. A cell cannot be assigned to two exclusive occupants in the same tick.
4. A transition is admitted only when its required reservation and conflict checks pass.
5. An agent waiting for clearance does not occupy a transition cell.
6. A block is either in emitter inventory, carried by one agent, or committed to one build cell; it cannot be in two places at once.
7. A placement task is committed at most once.
8. The target structure is complete only when every build cell's placed count equals its required count.
9. Given the same initial state, configuration, and event ordering, a deterministic run produces the same state/event digest.
10. Telemetry must distinguish modeled state from rendered interpolation.

## Tick outline

Each simulation tick is a bounded transaction:

1. Read the prior committed state.
2. Generate candidate state changes (task assignment, pickup, movement, admission, placement).
3. Validate resource, occupancy, and transition invariants.
4. Commit a conflict-free set in a deterministic order.
5. Publish events and a snapshot for renderers/diagnostics.

The first implementation may use a simpler single-threaded deterministic scheduler. Parallelism is an optimization to measure later, not a requirement for correctness.

## Evidence required

- Unit tests for image mapping, height bounds, inventory conservation, task idempotency, exclusive occupancy, reservation expiry, and deterministic replay.
- Integration tests for an agent completing the full pickup → transit → placement loop.
- Stress tests that sweep agent count and record throughput, allocations, memory, and correctness.
- Renderer evidence that visualized occupancy and status are read from simulation snapshots rather than invented by the renderer.
- An explicit distinction between logical-cell simulation and physical flight simulation.

## Deferred decisions

- Exact image-channel mapping and gamma/linearization policy.
- World units and physical dimensions.
- Continuous flight dynamics and physics provider.
- Multi-agent scheduling strategy beyond deterministic first implementation.
- GPU acceleration, batching, and renderer-specific level of detail.
