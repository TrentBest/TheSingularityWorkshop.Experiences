# Drone Construction — Gravity Sort and Traffic Choreography

**Status:** reconstructed design intent from the original author's description, 2026-10-22. This is a target behavior specification, not a claim that the current simulation implements it.

## The point of the demonstration

The first objective was not merely to make 100,000 drones build something. It was to show 100,000 independent agents coordinate as one visible system: synchronized ascent, a collective gravity sort, orderly separation into directional skylanes, free-flight roaming, and then a controlled construction supply cycle. Building is the payoff; the traffic choreography is the proof.

## The choreography

1. **Synchronized ascent.** The population takes off together and climbs to a common staging altitude.
2. **Gravity sort.** The population drops together. As drones approach their assigned skylane elevations, each cohort decelerates. A cohort enters its lane only when the lower traffic has cleared the required conflict region. The visual effect is a set of skylanes peeling away from the descending population.
3. **Directional traffic.** Each skylane is a discrete 2D X/Z grid with a declared direction. Drones traverse in the permitted direction; a route requiring opposite travel must leave through a transition and enter a lane whose direction supports the next leg, rather than reverse inside a one-way lane.
4. **Roaming.** Before being assigned construction work, a drone selects a random X/Z destination, travels there, and selects another destination. Random destinations must come from a seeded generator if replayable runs are required.
5. **On-deck queue.** Ten drones are staged for construction. A drone assigned to the next construction cycle routes through the lane network to the lowest slipstream, which exits above the block emitter.
6. **Serialized emitter approach.** The next on-deck drone is released when the preceding drone exits that lowest slipstream. The drone descends to the emitter at a defined rate, picks up one available block, moves forward one or two cells to clear the pickup area, and climbs into the first hoverlane.
7. **Lane-by-lane routing.** The drone uses hoverlanes as staging/alignment layers, slipstreams as controlled transitions, and skylanes as directional travel corridors. It repeats these transitions until it reaches the destination, then completes the placement task.

The ten-drone on-deck capacity is a queue limit, not permission for all ten drones to occupy the emitter's descent corridor simultaneously. The exit event releases the next queued drone; the physical and occupancy rules must still prevent corridor conflicts.

## Routing heuristic: longest axis first

For a target coordinate delta `(dx, dz)`, the original heuristic chooses the axis with the larger absolute difference first, then handles the shorter axis:

- If `abs(dx) >= abs(dz)`, route along X first, then Z.
- Otherwise, route along Z first, then X.
- Every leg must be translated into legal hoverlane, slipstream, and skylane transitions. Axis order alone is not a collision-avoidance proof.

This heuristic can reduce unnecessary turns, but it can also produce awkward alignment when the first leg crosses lane elevations or when a transition's exit offset changes the coordinate at which the drone rejoins a hoverlane. The route planner must model those offsets explicitly rather than patching them with corrective loops.

## Five-cell slowdown / early-exit heuristic

The original prototype used a five-cell assumption in two related ways: begin coordinating/decelerating with roughly five cells of lead before a slipstream entry, and exit the transition roughly five cells early so the drone reaches the hoverlane at the alignment point for its next axis leg. Treat this as a remembered heuristic to calibrate—not as a universal physical constant or proof of safety.

The implementation should represent these as separate configurable quantities:

- **Entry lookahead:** distance/time needed to decelerate and reserve a safe entry.
- **Transition exit offset:** where the drone must leave the transition to arrive at the intended hoverlane alignment cell.
- **Conflict clearance:** the condition that the downstream or crossing traffic has actually left the conflict region.
- **Route alignment:** the cell and axis required by the next planned leg.

They may all happen to equal five cells in a specific configuration, but they answer different questions and must not be silently coupled.

### A safer formulation

For every transition, compute or declare:

1. the source cell and intended destination/alignment cell;
2. the permitted direction and transition geometry;
3. the entry point at which deceleration or speed-policy changes begin;
4. the conflict region and the occupancy/reservation that proves it clear;
5. the exit point and required arrival alignment;
6. the next route leg, including any offset imposed by crossing another skylane;
7. the failure behavior if the exit is blocked (retain occupancy and reservation, wait, retry; never teleport or silently release capacity).

The lookahead should eventually be based on the selected motion model, tick duration, speed and deceleration limits, cell size, and transition geometry. If the model is purely discrete, define a deterministic cell/tick policy and prove it with exhaustive edge-case tests instead of presenting it as real-world flight physics.

## Invariants for the spectacle to be trustworthy

- Each drone has exactly one authoritative location/state at a time.
- No two drones occupy the same exclusive cell.
- A transition cannot be entered without admission and downstream capacity/clearance.
- A blocked exit retains the transition occupancy and reservation until a successful exit.
- One-way skylane direction is never violated by a shortcut.
- The next on-deck drone is released by the documented slipstream-exit event; queue capacity and descent-corridor capacity remain distinct.
- Speed changes and offsets are deterministic for a fixed seed/configuration.
- Random roaming cannot bypass traffic admission or occupancy checks.
- Renderers display committed simulation snapshots and do not invent movement.
- A 100,000-agent claim requires recorded runs, correctness checks, and measured resource/tick-time data.

## Current implementation boundary

The reconstruction already has a deterministic discrete construction loop and tested lane lifecycle primitives, including admission, transition ownership, blocked-exit retry, and reservation invariants. The documented lane lifecycle is **not yet wired into autonomous execution inside `ConstructionSimulation.Step()`**. The full gravity-sort cohort choreography, seeded random roaming, ten-drone construction queue behavior, and composed axis/offset route planner remain target behavior until implemented and tested.

## Implementation order

1. Specify a deterministic movement/configuration model and named phases.
2. Implement a small-population gravity-sort scenario with lane cohorts and explicit clearance conditions.
3. Integrate transition admission, advancement, and exit into the simulation tick.
4. Add seeded random roaming and the ten-drone on-deck queue as independent policies.
5. Implement longest-axis-first route planning with explicit transition offsets; test routes that cross other skylanes and require direction changes.
6. Capture committed snapshots and render the actual sequence.
7. Only then run population ramps toward 100,000, preserving correctness and performance evidence at every stage.

**Design principle:** make the choreography emerge from explicit local rules and verified reservations. The visual impression should be “all as one”; the implementation must still account for each drone, each cell, each transition, and every clearance decision.


## Gravity-sort braking and clearance

The first departing cohort begins its lane maneuver about **5 m above its assigned skylane**. It applies thrust to arrest its downward motion; once it reaches lane altitude with the required vertical speed, it transitions into horizontal travel. The remaining population continues falling, so the departing cohort and the falling population no longer share the same trajectory.

Keep three events distinct:

- **Braking begins:** the departing cohort starts applying thrust at the configured lead height.
- **Lane capture:** the cohort reaches the lane's altitude and satisfies the vertical-speed tolerance required for a horizontal transition.
- **Following traffic is clear:** the falling population has enough measured separation from the lane-entry/conflict region for the next cohort to proceed.

A nominal delay such as half a second or one second can be a useful scheduling parameter, but it is not itself proof of clearance. Clearance depends on initial velocity, thrust acceleration, gravity, cohort spacing, lane geometry, and the dimensions of the conflict region.

For a simple vertical model, take upward as positive and let the departing drone's net vertical acceleration be `a_depart = thrust_acceleration - g`; free-falling followers have acceleration `-g`. If both start with the same vertical velocity and the departing cohort holds altitude (net acceleration zero), idealized relative separation grows as `0.5 * g * t^2`: about 1.23 m after 0.5 s and 4.9 m after 1 s. These figures are only a baseline for those assumptions, not a claim about the original prototype. If the departing cohort accelerates upward, or there is initial separation or relative velocity, separation changes accordingly.

**Velocity-aware braking rule (preferred over a fixed trigger):** let upward be positive, let the current vertical velocity be `v_y`, let current altitude be `y`, and let the assigned lane elevation be `y_lane`. While descending (`v_y < 0`), compute the remaining altitude gap `h = y - y_lane` and the available net upward braking acceleration `a_brake = a_thrust_max - g`. Braking is physically feasible only when `a_brake > 0`. Under the constant-acceleration approximation, the stopping distance is `d_stop = v_y^2 / (2 * a_brake)`. Begin applying maximum braking when the remaining gap reaches the stopping distance plus a configured safety margin for tick discretization, control latency, and model error. Re-evaluate from the current velocity and altitude every tick; do not use elapsed time as the authority.

At lane capture, require both altitude error within a configured tolerance and vertical speed within a configured capture tolerance before allowing horizontal lane travel. If the current gap is already less than the computed stopping distance, the controller has detected an overshoot-risk condition; it must not falsely report a precise capture. Define a deterministic recovery/abort policy for that case. The formula is a planning estimate for constant acceleration, not a substitute for the chosen integrator's per-tick state update.

**Important:** thrust that exactly balances gravity cancels downward acceleration but does not cancel existing downward velocity. To stop descending, thrust must exceed weight long enough to reduce the downward velocity. The planner must verify that the available braking distance and time allow lane capture; it must not assume that reaching the lane's altitude automatically means the drone can safely turn horizontal.

Model these as explicit configuration/state, not a single magic delay: initial vertical velocity, gravity, thrust/net acceleration or a discrete motion policy, braking lead height, lane-capture velocity tolerance, follower separation, and the conflict-clearance condition. If the production simulation is discrete rather than a physical integrator, define deterministic tick-based equivalents and test their invariants.
