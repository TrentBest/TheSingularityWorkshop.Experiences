![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# Drone Construction — Skylane and Airspace Doctrine

**Status:** proposed traffic doctrine for design review. This document defines the next model to test; it does not claim that sloped lanes, aircraft-class altitude assignment, protected descent redirects, jet-wash separation, or queue-stability policies are implemented.

## 1. Core model: a lane is a directed corridor, not a grid

The existing discrete construction simulation uses qualified cellular traffic spaces. That is useful for deterministic occupancy and lifecycle tests, but it is not the final geometry for every aircraft.

A **Skylane** should be modeled conceptually as a directed corridor with:

- a centerline or route curve, including horizontal and sloped segments;
- a usable width and height envelope;
- a preferred travel direction and compatible aircraft classes;
- an altitude band or altitude profile, not necessarily a constant elevation;
- entry, merge, lane-change, exit, and conflict-zone boundaries;
- speed, separation, capacity, and admission policies;
- a stable lane identity and versioned configuration.

A lane can climb diagonally through space. Its geometry is not required to be orthogonal to the ground plane or aligned to a global X/Z grid. The current cell-grid implementation remains a useful first discrete model; continuous geometry must be introduced as an explicit model, not implied by the grid.

**Co-axis, not co-grid:** aircraft assigned to a lane generally share a similar forward heading and do not expect to cross one another soon. They can occupy different positions along a corridor without each position being a separate orthogonal grid cell. This simplifies routine flow, but does not remove following-distance, merge, overtaking, or wake-separation constraints.

## 2. Lane families and aircraft classes

Lane assignment is dynamic and capability-aware. Aircraft should prefer lanes that match their dimensions, performance envelope, mission, destination, and traffic conditions.

| Lane family | Intended use | Policy |
|---|---|---|
| Low-altitude delivery | Tiny, light, low-energy delivery drones | Prefer lower bands when route, obstacles, and local density permit |
| General small-UAS | Small and medium multirotors | Use compatible corridors and explicit climb/descent transitions |
| Heavy-UAS | Larger cargo drones with larger swept volume and stronger wake/downwash | Dedicated higher or otherwise separated corridors where practical |
| Fixed-wing / non-VTOL | Aircraft that need forward airspeed and cannot safely stop and hover | Use continuous, turn-capable corridors; no assumption of stationary holding |
| VTOL approach/departure | VTOL aircraft entering or leaving a pad or service volume | Use a protected vertical corridor and a reserved approach/departure volume |

These are **policy families**, not universal altitude numbers. Actual bands and separation must be configured from aircraft envelopes, local obstacles, noise/safety rules, weather, and the chosen motion model. A larger aircraft should not automatically be assigned higher altitude if its route or performance makes that unsafe; compatibility and separation take precedence over a simplistic size ordering.

Lane assignment should consider at least: aircraft class, current state and velocity, dimensions, wake/downwash class, mission priority, destination, energy margin, lane capacity, predicted delay, and available transitions. A lane change is a planned maneuver, not an instantaneous reassignment.

## 3. Lane changes are announced, admitted, and flown

An aircraft intending to leave its current lane announces a **lane-change intent** to the relevant traffic participants or coordinator. The intent should identify:

- aircraft and current lane/segment;
- target lane/segment and intended merge point;
- predicted time window and maneuver envelope;
- current velocity/heading and required bank/turn profile;
- required vertical and lateral separation;
- reservation or admission token, once granted;
- expiry, cancellation, and fallback behavior.

The message is not itself permission to move. The target segment and merge window must be admitted against current and predicted occupancy. Once admitted, the aircraft follows a continuous banked transition that respects its turn radius and speed envelope. It releases the source-lane claim only when the transition has committed to the target lane under the configured safety model.

If admission is denied, the aircraft remains on its current valid route or follows an explicit fallback. Non-VTOL aircraft must not be told to hover or stop in the sky as a generic queueing behavior. If no safe continuation exists, the route planner must select a safe diversion or a preplanned holding pattern that the aircraft can actually fly.

## 4. Sloped climbs and non-VTOL rerouting

A non-VTOL aircraft that must change altitude follows a climb/descent corridor with a feasible gradient and turn radius. Treat that segment as its own lane or lane transition, with a defined start, end, permitted direction, and conflict envelope. It is a normal part of the network, not a vertical elevator attached to a ground-aligned grid.

The planner should check:

1. whether the aircraft can achieve the altitude change at its current speed and available climb/descent performance;
2. whether the entire sloped corridor is clear, including the aircraft's swept volume and wake envelope;
3. whether the exit merges into a compatible lane heading and speed band;
4. whether the route creates a crossing conflict with another lane or a protected VTOL volume;
5. whether there is enough downstream capacity to accept the aircraft after the maneuver.

Do not create an instantaneous altitude change in the authoritative simulation. The visualizer may interpolate between committed states, but it must not invent the flight path or imply that a physically infeasible transition succeeded.

## 5. Protected VTOL descent and perimeter redirects

A VTOL aircraft may descend or ascend vertically only inside a designated, admitted takeoff/landing volume. That volume must reserve enough space for the aircraft's footprint, rotor/wake effects, uncertainty margin, and the configured vertical path.

For an illustrative aircraft requiring a **10 m × 10 m clear footprint**, a redirect boundary could be placed **15 m outside the protected footprint** as an initial planning margin. This is a proposed scenario parameter, not a proven safety distance. The exact interpretation must be explicit: the 15 m is a perimeter offset from the protected area's boundary, not an ambiguous radius measured from its center.

At the redirect boundary, the through-lane ends for that route. An aircraft that cannot be admitted to the protected vertical volume follows a preplanned perimeter route using compatible corridors, travels around the protected area, and rejoins the original route beyond the conflict region. The route should preserve direction and avoid cutting across the reserved descent volume.

A redirect is a **network detour**, not a holding loop that may accumulate aircraft indefinitely. Before an aircraft commits to the detour, the planner must establish that the perimeter path has capacity, has a safe rejoin point, and does not feed into a blocked merge. If no safe detour is available, the aircraft needs a bounded alternate destination or operational contingency.


## 6. Permanent airspace reservations before dynamic rerouting

The simplest safe baseline may be to **make important penetrations permanent features of the airspace map**, rather than allowing ordinary traffic through them until an event forces everyone to evacuate. For example, a runway's approach and departure volumes can be modeled as persistent protected regions, with ordinary skylanes routed around them from the outset. This removes the specific failure mode in which a temporary corridor closure releases a sudden surge of displaced aircraft into neighboring lanes.

This is a design default, not a claim that every protected region must be immutable. Planned maintenance, emergencies, and unusual operations can still change availability, but the network should treat those as explicit state changes with controlled admission—not as an unexpected collapse of a route everyone was relying on.

For a protected region, the map should record:

- the protected volume and its approach/departure extensions, not just a 2D footprint;
- which aircraft classes and operations are excluded, permitted, or conditionally authorized;
- the permanent bypass corridors and their actual capacity;
- any authorized crossing points, altitude/trajectory constraints, and separation rules;
- how temporary restrictions are announced, versioned, and propagated to route planners;
- how existing reservations are handled without dumping traffic into an already saturated bypass.

**Permanent exclusion is not the same as unlimited bypass capacity.** The bypass network still needs load checks and admission metering. The design should avoid sudden reroute waves, but also measure whether the steady-state traffic that remains is supportable.

### Airport operations and legal boundaries

An airport should be represented as an operational airspace system, not merely a runway polygon. Runway approaches, departures, nearby traffic patterns, obstacle constraints, and the relevant protected airspace all matter. The system can model a future in which authorized non-civilian or autonomous aircraft use carefully engineered routes near or over airport property, but software policy alone cannot grant that authorization or make such a route safe.

Actual operations must comply with the airspace rules and clearances in force at the location and time, including restrictions that apply to airports and controlled airspace. Any future ecosystem-specific operating model that differs from current rules would require lawful authorization and coordination with the relevant aviation authorities, plus validated separation procedures. Until then, treat overflight as a scenario that may be modeled for research—not as permission to operate.

This distinction helps keep the architecture ambitious without confusing a proposed traffic model with an approved real-world operating procedure.

## 7. Crossings, merges, and right-of-way

A crossing is a conflict region shared by two or more otherwise compatible corridors. It must be represented explicitly, even when the lanes are not drawn on a common grid.

Required rules:

- **No implicit crossing:** geometric intersection means a conflict until a policy proves otherwise.
- **Reserve before entry:** admission reserves a time/space window or another tested exclusive conflict claim for the complete protected maneuver.
- **Clear before release:** the claim is released only after the aircraft's swept volume and relevant wake envelope clear the conflict region.
- **Merge compatibility:** heading, speed, turn radius, aircraft class, and downstream capacity must be compatible.
- **Stable arbitration:** equal-priority conflicts use a deterministic tie-breaker (for example, stable request sequence/aircraft ID) so replays are explainable.
- **Fairness:** deterministic priority must not become permanent starvation. Record waiting age and apply an explicit aging, reservation, or bounded-deferral rule.
- **Emergency behavior:** emergency priority may pre-empt future reservations only under a documented protocol; it cannot erase an aircraft already occupying a protected volume.
- **No crossing through a lane-change maneuver:** lane changes must be admitted as maneuvers with their own conflict envelope, not merely as destination-lane assignment.

For a first deterministic prototype, use reservation-based crossing windows and a single authoritative admission point per conflict region. More distributed negotiation can be evaluated later, once the baseline is measurable.

## 8. Close-following separation: climb, descend, or re-time

When aircraft on compatible headings approach too closely, the system can choose a small vertical offset, a controlled speed change, or a time shift. A small climb by one aircraft and a small descent by another may be preferable to forcing both into a full lane change, but only if their performance and lane envelopes allow it.

The separation controller must account for more than point-to-point distance:

- aircraft body and rotor/swept dimensions;
- relative velocity and predicted closest point of approach;
- wake/downwash class and wake persistence;
- wind and uncertainty margins, when modeled;
- vertical separation and the clearance needed to enter/leave an altitude band;
- neighboring lanes and the risk that one aircraft's maneuver creates a second conflict.

A maneuver should be chosen only when the predicted trajectory clears both aircraft and surrounding traffic. “Climb one, descend one” is a candidate resolution, not a universal rule. If vertical room is unavailable, use speed/spacing changes or an alternate route.

## 9. Turbulence, queues, and traffic waves

The likely system-level failure is not only collision. A redirect or merge can create a **queue that propagates upstream**, reducing throughput across otherwise independent lanes. Safe individual maneuvers can still create a bad traffic system.

We should measure:

- queue depth and oldest waiting age at each entry, merge, and protected volume;
- time spent waiting for admission, crossing clearance, and downstream capacity;
- throughput and delay by aircraft class and lane;
- number of redirects, failed admissions, retries, and route changes;
- utilization of conflict regions and perimeter routes;
- speed variance and minimum predicted separation;
- progress rate and maximum time without progress;
- starvation count and bounded-wait violations;
- the difference between planned and actual arrival times at lane boundaries.

Initial stability rules:

1. Never release an aircraft into a redirect unless the downstream route has reserved capacity or a bounded, tested admission strategy.
2. Do not let a queue block the through-lane unless that lane is intentionally configured as a queueing resource.
3. Keep VTOL vertical volumes separate from through-traffic and reserve the approach/departure corridor end to end.
4. Use admission metering and explicit merge windows rather than asking aircraft to negotiate at the last moment.
5. Allow rerouting before the aircraft reaches the point where stopping or turning is no longer feasible.
6. Detect persistent queues and stalled progress; report them rather than hiding them behind a smooth animation.
7. Test adverse cases: sustained arrival rates above service capacity, one blocked exit, competing redirects, and repeated high-priority traffic.

A finite redirect route cannot solve sustained overload. If the average arrival rate into a constrained resource exceeds its service rate, the queue will grow unless demand is metered, diverted to a genuinely independent route, or reduced. The model must make this visible.

## 10. Proposed route and transition lifecycle

Current committed lane → predict destination, traffic, and maneuver envelope → choose compatible lane/sloped transition/perimeter redirect → announce intent and request target/conflict reservations → if denied, retain a safe route and retry or replan → if admitted, fly the bounded transition → confirm target lane and conflict-volume clearance → commit target lane, release source claims, and publish snapshot/event.

The exact implementation may evolve, but occupancy, reservation, and route ownership must have one authoritative commit point. A render frame is only an observation of this state.

## 11. Visualization requirements

The first useful visualization should show the **traffic system**, not just drone icons:

- sloped and curved lane centerlines with direction arrows;
- lane envelopes and aircraft-class eligibility;
- lane elevation/altitude profile and the observer's view;
- protected VTOL footprints, approach/departure volumes, and redirect perimeters;
- conflict regions, reservations, merge windows, and rejected intents;
- aircraft class, current lane, intended lane, and lane-change state;
- queue depth, oldest wait, throughput, redirects, and blocked/stalled states;
- committed simulation tick and snapshot age.

Provide observer controls to switch between overview, lane-following, and local conflict views. Detail can be reduced with distance, but traffic metrics must remain available independently of the number of drawn aircraft. A visualization must never make an unsafe or blocked maneuver look successful merely to keep the animation fluid.

## 12. Lane-scoped communications and channel economy

A lane is not only a movement corridor; it is also a natural **logical communications scope**. When an aircraft is admitted into a region/layer and assigned to a lane, it should subscribe to that lane's coordination channel. Routine lane-local messages should be delivered to participants that need them, rather than broadcast indiscriminately to every aircraft in the region.

This follows the useful remote-controller analogy—controllers can select channels to reduce interference and keep unrelated traffic separate—but the model must distinguish a **logical channel** from a **physical radio frequency/channel**. The simulation can give each lane a stable logical channel identity without assuming every lane receives a unique physical frequency. Real spectrum is finite; radios, regulations, interference, range, and network topology constrain physical allocation. A communication planner may map multiple logical channels onto shared physical resources using addressed messages, scheduled time slots, frequency reuse where safe, or another explicitly modeled multiplexing strategy.

### Scope and membership

- Define a channel key from the stable region, layer, lane identity, and relevant configuration/version. Do not use a transient object reference or grid-cell coordinate as the identity.
- Lane admission establishes membership; lane exit releases it. A lane change is a handoff: the aircraft receives the target lane's required coordination messages before committing, then leaves the source scope when the transition lifecycle says it is safe.
- Region/layer changes update the set of subscriptions. Cross-lane merges, crossings, protected volumes, emergency coordination, and handoffs use narrowly scoped coordination channels or addressed messages spanning only the affected participants.
- Keep a region/layer control and emergency path available even when a lane channel is congested or a drone has not yet joined a lane. A lane channel must not become a single point of failure for safety-critical notices.
- Channel membership is not airspace admission. Hearing a channel, announcing intent, or receiving a message never grants permission to enter a lane or conflict region; authoritative reservations and admission rules still decide movement.

### Message budget: publish what matters

Prefer event-driven, bounded messages over full-state broadcasts every simulation tick. Examples include lane entry/exit, intent and admission, changed route or closure, reservation/handoff state, relevant hazard, and a position/health update when it is needed for separation or prediction. Stable lane participants can use configured update rates and thresholds; higher urgency or uncertainty can increase reporting frequency, while unchanged state need not generate repeated chatter.

Every message class should define its audience, priority, maximum rate or coalescing policy, freshness/expiry, and behavior under congestion. Coalesce superseded state updates; do not coalesce away distinct safety events or acknowledgements needed to complete a protocol. Bound queues and record drops, latency, stale-message use, retries, and channel utilization. Deterministic simulation should define ordering and delivery assumptions so results are reproducible.

### Allocation and failure behavior

1. Assign logical lane channels from the authoritative region/layer/lane configuration.
2. Let a channel planner map logical scopes to available physical resources according to radio capability, interference, range, capacity, and policy; reuse a physical channel only where the interference plan permits it.
3. Keep control, emergency, and cross-lane coordination traffic available through an explicit shared or redundant mechanism.
4. On channel loss, congestion, or stale state, do not silently proceed as if coordination succeeded. Apply a documented safe fallback, bounded retry/rejoin policy, or route/mission contingency appropriate to the aircraft model.
5. Measure messages and bytes per drone and per lane, broadcast fan-out, channel utilization, latency percentiles, drops/retries, handoff duration, and safety-relevant stale-message counts. Compare lane-scoped delivery with region-wide broadcast at increasing population sizes.

The first implementation should model **logical channel identities, subscriptions, routing scopes, and message budgets** independently of physical RF simulation. Physical frequency assignment, modulation, radio range, interference, and regulatory compliance are separate capabilities requiring their own contracts and evidence. This doctrine does not claim that a real radio channel has been allocated or that wireless safety has been validated.

## 13. Implementation sequence

1. **Doctrine and terminology:** agree on corridor, lane segment, lane family, conflict region, protected volume, redirect, reservation, and queue metrics.
2. **Discrete traffic contract:** add tests for lane-change intent, admission, reservation ownership, deterministic arbitration, and blocked retry without claiming continuous flight.
3. **Tiny scenario:** two compatible lanes, one sloped transition, one crossing, one protected VTOL volume, and one perimeter redirect.
4. **Traffic stress cases:** competing merges, blocked downstream capacity, sustained overload, and starvation checks.
5. **Snapshot and visualization:** render real committed state with lane/aircraft/queue identity and counters; observer changes remain presentation-only.
6. **Continuous geometry:** introduce centerlines, envelopes, bank/turn constraints, vertical profiles, and wake/separation policy behind explicit contracts.
7. **Scale ramp:** measure simulation-only and observer/render workloads separately before increasing population.

## Current evidence boundary

The existing Drone Construction implementation has deterministic discrete construction state, committed snapshots, skylane destination movement, and tested explicit transition traversal/retry/chaining. Those primitives do not yet demonstrate the continuous sloped-lane network described here, aircraft-class altitude assignment, broadcast lane-change negotiation, VTOL perimeter rerouting, wake-aware separation, queue stability, or a live observer-relative visualization. Keep each as an explicit design/implementation gate until tests and reproducible artifacts prove it.

**Design principle:** route around conflicts before they become emergencies; reserve space before entering it; never trade authoritative truth for a prettier frame.

