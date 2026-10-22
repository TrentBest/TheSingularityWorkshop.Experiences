# Drone Construction — Snapshot and Living-View Contract

**Status:** contract and implementation plan; live GUI.SVG/Renderer adapter is not yet demonstrated.

## Authority boundary

ConstructionSimulation owns authoritative state and advances only through explicit simulation ticks. A ConstructionSimulationSnapshot is a read-only observation of one committed tick. Renderer, GUI.SVG, dashboards, and recording tools may interpret snapshots but must not alter the simulation or invent state.

```text
Input image + mapping config
          |
          v
Height field -> Build grid / placement tasks
          |                ^
          v                |
Agents <-> Emitters -> authoritative Simulation.Step()
                         |
                         v
              committed immutable snapshot
                 /          |          \
                v           v           v
          Renderer       GUI.SVG     Telemetry
          portrayal      live view   run record
```

This is a responsibility diagram, not proof that all adapters currently exist.

## Snapshot semantics

Each snapshot represents one committed tick and must be internally coherent:
- Tick identifies the committed simulation step.
- RequiredBlocks, PlacedBlocks, and IsComplete come from the authoritative build grid.
- Each agent includes stable AgentId, lifecycle State, carrying/task fields, and a qualified Location.
- Each emitter reports stable identity, location, and current inventory.
- Per-cell placed-block counts represent the same committed tick.
- Collections are immutable to consumers; mutable backing collections must not leak.
- Location is qualified by space kind, stable space ID, and local cell; local coordinates alone are ambiguous across simulation, hover, skylane, and transition grids.

The current snapshot surface is a useful foundation. Because lane transitions are not yet wired into Step(), do not imply every agent autonomously traverses the hover → slipstream → skylane path.

## Observer rules

1. **Read-only:** no observer writes to agents, grids, reservations, tasks, or inventories.
2. **Tick alignment:** every visual frame states which committed tick it represents. Interpolation may smooth display but cannot alter the underlying tick record.
3. **Truthful status:** show simulation tick, authoritative agent count, placed/required blocks, state distribution, active transitions, and update time only when supplied by real data.
4. **Backpressure:** rendering must not block simulation progress indefinitely. Define whether the observer receives every snapshot, sampled snapshots, or coalesced latest-state snapshots.
5. **No hidden loss:** if snapshots are dropped or coalesced, mark that behavior and preserve final state and aggregate counters.
6. **Reproducibility:** retain run metadata and input/configuration identity alongside snapshots.
7. **Failure visibility:** surface stale observer state, parse errors, dropped frames, and simulation faults.
8. **Layer separation:** domain simulation must not reference WPF, SVG, browser, or Unity types.

## First useful visual vertical slice

- Draw the target build grid from snapshot dimensions and placed-block counts.
- Draw emitters and their actual remaining inventory.
- Draw agents at qualified locations with a visual style derived from lifecycle/movement state.
- Show tick and placed/required progress from the same snapshot.
- Highlight occupied cells and transition reservations only when exposed by an authoritative observation contract.
- Caption the snapshot tick so screenshots are auditable.
- Keep static storyboards separately labeled as concept images.

## Adapter boundary

Keep the domain snapshot as stable input. A renderer adapter may transform it into a renderer-neutral semantic scene. GUI.SVG manifests that scene as SVG; Renderer may provide text-oriented or other portrayals. ConstructionSimulation must not reference either adapter.

Before coding, choose snapshot polling (simpler first proof) or an event stream plus snapshots (requires ordering, replay, and recovery semantics). Do not add a second source of truth. Events describe committed changes; snapshots are authoritative checkpoints.

## Review checklist

- [ ] Can a consumer render a snapshot without a reference to the simulation object?
- [ ] Does the consumer show the exact tick and actual agent count?
- [ ] Are cross-space locations represented without coordinate ambiguity?
- [ ] Can a failed transition be seen as blocked/waiting rather than silently disappearing?
- [ ] Does the view remain correct when frames are dropped?
- [ ] Can the same snapshot be rendered twice with the same semantic output?
- [ ] Are visual claims backed by measured data or explicit labels?


## Scale-aware presentation and level of detail

The production showcase must not require one full visual object per simulated drone. The authoritative simulation may track every agent, while each observer chooses an appropriate representation based on camera relevance, distance, importance, and current display budget.

A useful tiered policy is:

- **Near / selected:** render individual drones with detailed state, direction, carrying status, and relevant lane/transition cues.
- **Mid-range:** render simplified individual markers or instanced glyphs, retaining meaningful direction and phase.
- **Far field:** aggregate drones into lane/cohort density, occupancy bands, or counts by committed state and region. Avoid allocating a separate visual object for every distant drone.
- **Off-screen / irrelevant:** omit individual marks from the current frame while retaining their authoritative simulation, occupancy, task, and clearance participation.

LOD is a presentation policy only. It must never remove an agent from simulation, release its cell, skip a collision/traffic check, or change task scheduling. Aggregate values must be derived from a coherent committed snapshot and should identify their tick. If the view samples or coalesces snapshots, expose that behavior instead of implying every simulation tick was rendered.

Renderer and Visualize serve complementary observer roles: Renderer can provide a compact, text-oriented portrayal for AI/diagnostics, while Visualize/GUI.SVG provides the spatial view. Both consume the same committed snapshot or renderer-neutral semantic scene; neither is a second simulation authority. The SVG path should avoid creating and updating 100,000 independently detailed DOM elements as the default strategy. Prefer cohort/group primitives, lane-level summaries, bounded near-field detail, and selective expansion of an agent or region when requested. Any optimization must preserve truthful counts and make aggregation/omission policy inspectable.

The simulation architecture should also avoid a Unity-style requirement for one heavyweight rigid body and four independently simulated rotors per drone. Use the project's deterministic aggregate/discrete motion model and validated average parameters for the population-level choreography; reserve detailed physical simulation for a smaller explicitly selected subset only if a future requirement justifies it. Do not introduce Unity types or dependencies into the domain core.

### LOD correctness checks

- [ ] Changing camera distance or display budget does not change the simulation digest.
- [ ] Agent totals and cohort totals reconcile with the authoritative snapshot.
- [ ] Hidden/aggregated agents still occupy cells and participate in lane clearance.
- [ ] A selected drone can be expanded without changing its simulation state.
- [ ] The same snapshot and LOD configuration produce deterministic semantic output.
- [ ] Observer backpressure and dropped/coalesced frames are visible in diagnostics.
