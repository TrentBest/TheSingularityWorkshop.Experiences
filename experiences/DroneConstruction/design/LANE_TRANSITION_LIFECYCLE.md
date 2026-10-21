# Drone Construction — Lane Transition Lifecycle Contract

**Status:** design contract for M2; not evidence that lane transitions are integrated.
**Scope:** renderer-independent, deterministic, single-threaded discrete simulation.

## Why this contract comes first

`ConstructionSimulation` currently treats each agent position as a coordinate in one authoritative simulation traffic grid. `Slipstream.TryAdmit` transfers occupancy between separate `TrafficCellGrid` instances and keeps a single-agent reservation until explicitly released. Connecting those pieces without a location model would make `ConstructionAgent.Position` ambiguous: the same `GridCell` could refer to different grids, and the current movement loop could attempt to move an agent in the wrong occupancy grid.

## Required location model

An agent's committed location must identify both its space and its cell. A location is one of:

- **Simulation grid:** ordinary construction movement and emitter/build-target coordinates.
- **Hover lane:** waiting or staging area with exclusive cell occupancy.
- **Slipstream:** bounded transition cells; reservation ownership and cell occupancy are separate facts.
- **Skylane:** directed cells subject to the lane's direction and cells-per-tick policy.

Coordinates are local to their named space. Equal `GridCell` values in different spaces do not mean the same physical location. Renderer-facing snapshots must expose the space identity along with the coordinate; they must not infer a space from coordinates.

## Lifecycle

1. **Approach / stage.** The agent remains in its current grid and keeps its source occupancy while waiting for a transition.
2. **Request admission.** Validate the source owner and source location, destination lane cell, transition cells, bounds, and lookahead policy. A rejected request must leave source occupancy and agent location unchanged.
3. **Commit entry.** Reserve the transition for the agent and transfer occupancy from the source space to the transition's entry cell as one logical operation. After commit, the agent is present in exactly one occupancy grid. If transfer fails or throws, release the newly acquired reservation and preserve the original location/occupancy.
4. **Traverse.** Each tick, calculate the next transition/lane cell under the lane's declared direction and speed policy. Check occupancy before moving. A blocked step preserves the previous cell, reservation, and location.
5. **Commit exit.** Validate the destination space and exit cell, transfer occupancy to that space, update the agent's qualified location, and release the transition reservation only after the exit transfer succeeds.
6. **Resume simulation.** Ordinary construction movement resumes only after the agent is back in the simulation grid. The movement loop must never move a lane-resident agent using simulation-grid coordinates.

## Invariants to test

- **Single occupancy:** every active agent is registered in exactly one occupancy grid matching its qualified location.
- **Stable identity:** an agent ID cannot be registered twice, including in a destination grid it is entering.
- **Failure atomicity:** rejected/blocked entry or exit leaves location, source/destination occupancy, and existing reservation state unchanged.
- **Reservation ownership:** only the owner can release an active transition reservation; a reservation cannot be overwritten by a repeated request.
- **Reservation lifetime:** capacity remains reserved through traversal and is released only after successful exit, not merely because a tick or admission call returned.
- **Directional movement:** skylane traversal never moves against the declared direction and respects the configured cells-per-tick rule.
- **Determinism:** stable agent order and fixed inputs produce identical location, occupancy, reservation, and snapshot sequences.
- **Observable truth:** snapshots report the committed location space and cell; they do not portray a lane-resident agent as occupying its former simulation cell.

## Integration sequence

1. **Completed on development:** add `ConstructionSpaceKind` and `QualifiedGridLocation`; expose `ConstructionSimulation.SpaceId`, `GetAgentLocation(agentId)`, and qualified `ConstructionAgentSnapshot.Location`. Tests cover cross-space equality, validation, movement updates, and agreement between queries and snapshots. The simulation currently reports only `SimulationGrid` locations because lane transitions are not yet integrated.
2. Add a transition lifecycle owner that coordinates source grid, slipstream reservation, transition occupancy, and exit transfer; keep the operation synchronous unless a later design explicitly adds concurrency.
3. Test entry success/rejection, traversal blockage, exit success/rejection, rollback, and reservation lifetime against multiple grids.
4. Integrate the lifecycle into `ConstructionSimulation` only after the agent state machine can represent lane-resident states without misusing `GridCell`.
5. Extend immutable snapshots to expose qualified locations and verify every snapshot against authoritative occupancy.
6. Add GUI.SVG observation only after the model contract and tests pass.

## Explicit non-claims

This contract does not prove that the five-cell lookahead is physically safe, that the model is thread-safe, that the simulation supports 100,000 agents, or that GUI.SVG/Renderer consumes live simulation snapshots. Those require separate parameters, tests, and measurements.
