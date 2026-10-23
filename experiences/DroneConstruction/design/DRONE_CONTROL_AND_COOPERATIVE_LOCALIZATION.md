# Drone Control and Cooperative Localization

**Owner:** Trent Best · **Branch:** development  
**Status:** architecture contract for a future flight-model layer; not a claim that flight dynamics or localization are implemented.

## First principle: many small state machines, one authoritative simulation

Do not put every possible drone condition into one enormous finite-state machine (FSM). Use a small coordinator FSM per drone, and add subsystem FSMs only where a subsystem has independent behavior, faults, timing, or recovery.

```text
Drone coordinator FSM
  ├── mission / task lifecycle
  ├── navigation FSM             (when route behavior is modeled)
  ├── localization-quality FSM   (when estimator behavior is modeled)
  ├── communications FSM         (when link/relay behavior is modeled)
  └── motor controller FSMs      (one per modeled motor/ESC)
```

For a quadcopter with four independently simulated motor controllers, that means one coordinator plus four motor FSMs, with navigation/localization/comms FSMs added when their independent behavior is actually present. It does **not** mean inventing four motor FSMs in the existing grid-based construction demo: the current `ConstructionAgent` explicitly models discrete task/lifecycle state, not flight dynamics.

### Responsibilities and data ownership

| Component | Owns | Must not own |
|---|---|---|
| Drone coordinator | mission phase, task assignment, safe-mode decisions, subsystem coordination | raw motor-control loops or estimator mathematics |
| Motor FSM (per modeled motor) | disarmed, initializing, ready, running, degraded, faulted; command acceptance and fault/recovery | whole-drone mission state |
| Navigation FSM | route mode, following/avoidance/replan mode | authoritative measured position |
| Localization-quality FSM | acquiring, tracking, degraded, lost; source-quality transitions | position as a graph of enumerated states |
| State estimator | numeric pose, velocity, uncertainty/covariance, timestamp, contributing observations | mission policy |
| Swarm/localization service | anchor definitions, measurement fusion, peer observations, health/age limits | silently treating a peer's claimed position as ground truth |

The numeric state is data; FSMs describe **what the system is doing about that data**. Avoid allocating a separate heavyweight FSM per scalar or per sensor sample. The FSM_API is intended to be exercised where independent state transitions add real behavioral value; the scale benchmark must measure the cost.

## Position-estimate contract

A future estimator output should include, at minimum:

- **Identity/frame:** drone ID, coordinate-frame ID and frame/version.
- **Pose:** position in declared units; orientation if the scenario requires it.
- **Motion:** velocity, and optionally acceleration.
- **Quality:** uncertainty (preferably covariance or a documented approximation), source/method, and validity state.
- **Time:** measurement timestamp and estimate timestamp; stale observations must be detectable.
- **Provenance:** anchor/peer observation IDs or a compact, reproducible set of contributing measurement references.

Consumers must not consume a position without its frame, timestamp, and quality. Never average raw coordinates from different frames or measurement epochs as if they were equivalent. A localization FSM can move to `Degraded` or `Lost` based on age, uncertainty, anchor geometry, or sensor health, while the estimator continues to publish its best estimate with explicit uncertainty where safe.

## Cooperative optical positioning: what the light can and cannot do

A single moving or scanned light can be a synchronization cue or provide a bearing if the optical system measures angle. A plain visible pulse does not, by itself, give a drone a full 3D coordinate. Occlusion means some drones will miss the signal, and a relay drone's position claim is not a new geometric measurement.

A viable cooperative design separates **anchors**, **measurements**, and **estimates**:

1. Three or more surveyed, non-collinear anchors transmit uniquely identifiable, timestamped signals. For 3D ranging, plan for at least four geometrically suitable anchors or another independent constraint; three ranges can leave a mirror/ambiguity solution. Anchor count alone does not guarantee good geometry.
2. Each receiver records actual measurements (for example range, angle, or time-of-arrival), anchor identity, timestamp, and measurement uncertainty. A flashing LED observed by a camera is not automatically a precise range measurement; the physical method and clock synchronization must be specified.
3. A drone with usable anchor observations computes a local estimate and uncertainty, then relays the **measurements or their sufficient statistics**, not just an unqualified coordinate.
4. A drone without line-of-sight can fuse multiple independent peer-relative ranges/bearings with its own inertial/visual odometry and prior estimate. If it receives only a peer's coordinate and no independent relative measurement, it cannot triangulate a new position from that message alone.
5. A cooperative estimator corrects accumulated drift when anchor constraints return. It tracks uncertainty and observation age, rejects inconsistent/outlier measurements, and avoids counting correlated relayed observations as independent evidence.

### Failure modes to model

- line-of-sight blockage, reflections/multipath, ambient light, and receiver saturation;
- clock drift, pulse collisions, anchor identity confusion, and stale messages;
- poor anchor geometry and uncertainty growth in unobserved directions;
- correlated peer errors and error propagation through relay chains;
- loss of enough independent measurements to localize safely.

A practical research prototype should compare fixed optical anchors, UWB range measurements, and inertial/visual odometry rather than assuming light-only positioning wins. Optical systems need a safety review for eye exposure and an operational plan for occlusion. This is a simulation/research architecture, not a claim of validated physical swarm navigation.

## Scale and determinism rules

- Keep stable drone IDs and compact, bounded per-drone state.
- Batch sensor/peer observations; do not broadcast a full swarm snapshot to every drone each tick.
- Separate authoritative simulation ticks from renderer interpolation and display frame rate.
- Record estimator input ordering, timestamps, configuration, and seed for replay.
- Benchmark coordinator-only versus coordinator + motor FSMs versus full subsystem FSMs at 1k, 10k, and higher populations. Capture state-machine count, tick time distribution, allocations, memory, observation/relay volume, estimator error, and invariant failures.
- Keep the existing discrete construction simulation as a distinct model. A cell coordinate is not a physical 3D pose and a successful grid run is not flight-validation evidence.

## Implementation sequence

1. Preserve and benchmark the current discrete construction path.
2. Define a separate, typed pose/estimate contract with frame, timestamp, and uncertainty; test invalid, stale, and frame-mismatched inputs.
3. Add a deterministic synthetic-anchor scenario and known ground-truth trajectories.
4. Add observation loss, occlusion, timing drift, noisy measurements, and peer-error propagation tests.
5. Add the coordinator FSM, then subsystem FSMs only alongside tested independent behaviors.
6. Run population-ramp benchmarks and report localization error as well as throughput before making any 100k claim.
