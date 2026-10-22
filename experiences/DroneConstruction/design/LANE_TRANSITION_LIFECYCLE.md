# Drone Construction — Lane Transition Lifecycle Contract

**Status:** controlled slipstream entry/traversal/exit and policy-driven skylane movement are integrated on `development`. CI has passed same-grid round trips, blocked-exit reservation retention, snapshot/location consistency, direction/speed enforcement, occupied intermediate cells, rejection of mismatched lane identity/grid or out-of-bounds targets, independent ownership of two registered lifecycle instances, simulation-level failed-entry rollback/retry, and lane-resident `Step()` isolation. Duplicate-admission and explicit intent-validation tests have passed CI. An explicit intent admission policy is now implemented on development; its ownership/admission regression tests are awaiting CI. This is a discrete lifecycle, not a physical flight model.
**Scope:** renderer-independent, deterministic, single-threaded discrete simulation.

## Why this contract comes first

`ConstructionSimulation` currently treats each agent position as a coordinate in one authoritative simulation traffic grid. `SlipstreamLifecycle` transfers occupancy between distinct `TrafficCellGrid` instances and keeps a single-agent reservation until successful exit. Integrating those pieces without a location model would make `ConstructionAgent.Position` ambiguous: the same `GridCell` could refer to different grids, and the current movement loop could attempt to move an agent in the wrong occupancy grid.

## Required location model

An agent's committed location must identify both its space and its cell. A location is one of:

- **Simulation grid:** ordinary construction movement and emitter/build-target coordinates.
- **Hover lane:** waiting or staging area with exclusive cell occupancy.
- **Slipstream:** bounded transition cells; reservation ownership and cell occupancy are separate facts.
- **Skylane:** directed cells subject to the lane's direction and cells-per-tick policy.

Coordinates are local to their named spaces. Equal `GridCell` values in different spaces do not mean the same physical location. Renderer-facing snapshots must expose the space identity along with the coordinate; they must not infer a space from coordinates.

`ConstructionSpaceCatalog` resolves registered grids from occupancy indexes rather than duplicating occupancy state. `ConstructionSimulation` registers its primary grid, supports explicit registration of external grids, and exposes controlled slipstream entry, advance, and exit methods. Each operation verifies the qualified committed location and transition-grid identity; the agent's local `Position` is synchronized only after a successful occupancy transfer or move. Snapshots derive their location from authoritative occupancy.

## Lifecycle

1. **Approach / stage.** The agent remains in its current grid and keeps its source occupancy while waiting for a transition.
2. **Request admission.** Validate the source owner and source location, destination lane cell, transition cells, bounds, and lookahead policy. A rejected request must leave source occupancy and agent location unchanged.
3. **Commit entry.** Reserve the transition for the agent and transfer occupancy from the source space to the transition's entry cell as one logical operation. After commit, the agent is present in exactly one occupancy grid. If transfer fails or throws, release the newly acquired reservation and preserve the original location/occupancy.
4. **Traverse.** Transition cells advance one orthogonal cell at a time. Skylane movement uses `Skylane.Direction` and `CellsPerTick`; the simulation validates every intermediate cell before committing the move, so speed cannot skip through an occupied cell. A blocked step preserves the previous cell and occupancy.
5. **Commit exit.** Validate the exact destination grid and cell bound to the reservation, transfer occupancy to that destination, and release the transition reservation only after the exit transfer succeeds.
6. **Resume simulation.** Ordinary construction movement resumes only after the agent is back in the simulation grid. The movement loop must never move a lane-resident agent using simulation-grid coordinates.

## Invariants to test

- **Single occupancy:** every active agent is registered in exactly one occupancy grid matching its qualified location.
- **Stable identity:** an agent ID cannot be registered twice, including in a destination grid it is entering.
- **Failure atomicity:** rejected/blocked entry or exit leaves source/destination occupancy and existing reservation state unchanged.
- **Reservation ownership:** only the owner can release an active transition reservation; a reservation cannot be overwritten by a repeated request.
- **Reservation target:** a reservation cannot be used to exit through a different grid or cell.
- **Reservation lifetime:** capacity remains reserved through traversal and is released only after successful exit, not merely because a tick or admission call returned.
- **Directional movement:** skylane traversal never moves against the declared direction and respects the configured cells-per-tick rule.
- **Determinism:** stable agent order and fixed inputs produce identical location, occupancy, reservation, and snapshot sequences.
- **Observable truth:** snapshots report the committed location space and cell; they do not portray a lane-resident agent as occupying its former simulation cell.

## Integration sequence

1. **Completed on development:** add `ConstructionSpaceKind` and `QualifiedGridLocation`; expose `ConstructionSimulation.SpaceId`, `GetAgentLocation(agentId)`, and qualified `ConstructionAgentSnapshot.Location`. Tests cover validation, movement updates, and agreement between simulation queries and snapshots. Locations now reflect the authoritative occupied grid, including registered slipstreams and skylanes.
2. **Completed as a standalone component on development:** `SlipstreamLifecycle` coordinates reservation, source-to-transition entry, one-cell orthogonal traversal, and transition-to-destination exit across distinct grids. Regression tests cover entry rollback, blocked traversal, blocked exit retention, successful exit, exact reserved destination enforcement, and reservation release.
3. **Completed as a standalone component on development:** `ConstructionSpaceCatalog` resolves an agent's unique occupied grid to a `QualifiedGridLocation`, validates qualified grid resolution, rejects duplicate registration aliases, and fails closed for missing or ambiguous occupancy.
4. **Integrated on development:** `ConstructionSimulation` supports controlled registration of external traffic grids and controlled slipstream entry, one-cell traversal, and exit. It validates that supplied qualified locations resolve to the expected grid instances and that the agent currently occupies the declared source/transition location. Successful operations synchronize `ConstructionAgent.Position`; failed operations preserve the committed location and reservation.
5. **Covered by integration tests:** successful transition and return to the main grid, main-grid movement resuming on subsequent ticks, blocked exit retaining transition occupancy and reservation, and snapshots matching the authoritative qualified location.
6. **Completed on development:** `ConstructionSimulation.TryAdvanceSkylane` validates the registered lane identity and grid instance, enforces `Skylane.Direction` and `CellsPerTick`, checks every traversed cell, and preserves position/occupancy on failure. Tests cover wrong lane identity, a different grid reusing the same lane ID, out-of-bounds targets, reverse travel, and occupied intermediate cells.
7. **Completed on development:** independent lifecycle ownership across multiple transition resources and simulation-level failed-entry rollback/retry are covered by CI. A duplicate-admission test now verifies that a resident agent cannot be admitted a second time and that neither lifecycle owner is disturbed; its CI run is pending.
8. **Completed on development:** `LaneTransitionIntent` captures the exact qualified source, slipstream entry, destination, and copied lookahead cells. Its constructor rejects invalid space roles, empty lookahead, and duplicate lookahead cells. Focused validation tests passed CI. This is immutable request data only; it does not reserve resources, choose a route, or execute movement.
9. **In progress on development:** `LaneTransitionAdmissionPolicy` owns pending intents for one simulation. Submission requires an agent to be in `AwaitingTransition` at the exact qualified source and refuses to replace an existing pending request. Admission is a separate explicit call; it delegates to the simulation's registered slipstream lifecycle, retains intent on failure for retry, and consumes it only after successful entry. Cancellation removes pending intent without moving the agent. Focused policy tests are awaiting CI.
10. **Next architectural milestone:** integrate the explicit policy with simulation scheduling so `Step()` cannot bypass pending lane intent or prematurely advance a waiting agent. Then add route/admission policy coverage for resource identity and lifecycle-state changes before scheduling lane entry, traversal, skylane movement, and exit. Add GUI.SVG observation only after that model contract is stable.

## Explicit non-claims

This contract does not prove that the five-cell lookahead is physically safe, that the model is thread-safe, that the simulation supports 100,000 agents, or that GUI.SVG/Renderer consumes live simulation snapshots. Those require separate parameters, tests, and measurements.
