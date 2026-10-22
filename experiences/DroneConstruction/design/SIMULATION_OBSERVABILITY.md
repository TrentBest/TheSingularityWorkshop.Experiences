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
