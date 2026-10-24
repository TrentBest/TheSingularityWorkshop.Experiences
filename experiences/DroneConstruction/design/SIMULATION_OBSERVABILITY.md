# Drone Construction — Snapshot and Living-View Contract

**Status:** immutable snapshots, the standalone SVG renderer, and a maintained pull-based SVG view are implemented with focused tests. The optional GUI.SVG source-graph integration test is CI-verified. A hosted GUI.SVG application and telemetry stream remain open.

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

The current snapshot surface is a useful foundation. Explicit-path slipstream traversal, blocked intermediate/exit retry, downstream skylane-target chaining, and rejection of invalid downstream targets are now wired into `ConstructionSimulation.Step()` and covered by focused CI regressions (see [READINESS.md](READINESS.md)). This does not establish general itinerary planning, starvation-freedom, or population-scale behavior; keep those claims explicitly open.

## Observer rules

1. **Read-only:** no observer writes to agents, grids, reservations, tasks, or inventories.
2. **Tick alignment:** every visual frame states which committed tick it represents. Interpolation may smooth display but cannot alter the underlying tick record.
3. **Truthful status:** show simulation tick, authoritative agent count, placed/required blocks, state distribution, active transitions, and update time only when supplied by real data.
4. **Backpressure:** rendering must not block simulation progress indefinitely. Define whether the observer receives every snapshot, sampled snapshots, or coalesced latest-state snapshots.
5. **No hidden loss:** if snapshots are dropped or coalesced, mark that behavior and preserve final state and aggregate counters.
6. **Reproducibility:** retain run metadata and input/configuration identity alongside snapshots.
7. **Failure visibility:** surface stale observer state, parse errors, dropped frames, and simulation faults.
8. **Layer separation:** domain simulation must not reference WPF, SVG, browser, or Unity types.

## Implemented visual proof

`ConstructionSnapshotSvgRenderer` consumes a committed `ConstructionSimulationSnapshot` and renders tick/progress, build cells, emitter inventory, and agents assigned to the selected simulation-grid identity. It does not advance or mutate simulation state. Focused tests check snapshot-derived values and invalid dimensions. CI run [39851568716](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/actions/runs/39851568716) produced the standalone SVG artifact.

The optional source-graph test [ConstructionSnapshotGuiSvgIntegrationTests.cs](../../../tests/DroneConstruction.Tests/ConstructionSnapshotGuiSvgIntegrationTests.cs) feeds initial and completed committed snapshots into `GuiBuilder` and `SvgGuiRenderer`. It checks tick/progress labels, agent identity, placed-block identity, and stale-tick exclusion. Its CI run [39853079623](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/actions/runs/39853079623) succeeded. GUI.SVG remains a test-side optional boundary; this test renders two snapshots in a test and does **not** establish a continuously updated application view, telemetry, or 100k-scale performance.

## First useful maintained visual vertical slice

- Draw the target build grid from snapshot dimensions and placed-block counts.
- Draw emitters and their actual remaining inventory.
- Draw agents at qualified locations with a visual style derived from lifecycle/movement state.
- Show tick and placed/required progress from the same snapshot.
- Highlight occupied cells and transition reservations only when exposed by an authoritative observation contract.
- Caption the snapshot tick so screenshots are auditable.
- Keep static storyboards separately labeled as concept images.

## Adapter boundary

Keep the domain snapshot as stable input. A renderer adapter may transform it into a renderer-neutral semantic scene. GUI.SVG manifests that scene as SVG; Renderer may provide text-oriented or other portrayals. ConstructionSimulation must not reference either adapter.

`ConstructionSnapshotSvgView` is the first maintained polling component. The host publishes each committed snapshot to `ConstructionSnapshotObserver` and calls `TryRefresh` on its own cadence. The view renders only a new accepted sequence and exposes the corresponding run ID/tick; because the observer retains only the latest value, intermediate snapshots can be coalesced. A failed render leaves the cursor unchanged for retry. The component does not schedule ticks, run a timer, host a GUI.SVG manifest, or collect telemetry. An event stream can follow only if ordering, replay, and recovery semantics are specified. Do not add a second source of truth.

## Observer-relative Renderer → Visualize integration target

The existing cross-repository proof in [Visualize's RendererAdapter guide](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/blob/development/integrations/RendererAdapter/Proof/README.md) establishes the downstream contract: a Renderer `RenderFrame` is adapted by `RendererFrameAdapter` into a Visualize frame, then `Visualizer.RenderAsync` submits it to a backend. That is a CPU-side integration proof, not a GPU backend or a complete observer/bucket/scheduling pipeline.

Drone Construction does **not** yet have a production adapter that maps its committed snapshot into that pipeline. Implement this as a separate optional presentation layer; keep the construction domain and physics projects independent of Renderer, Visualize, GUI.SVG, and graphics APIs.

### Required data flow

1. Read one immutable `ConstructionSimulationSnapshot` and retain its run ID, committed tick, and acceptance sequence as frame provenance.
2. Build a bounded observer input from that snapshot. Supply an explicit observer pose/intent and policy configuration; do not infer observer state from the drone population.
3. Select presentation candidates and representations before constructing expensive geometry or draw commands. Use Renderer policy contracts where the actual API supports the required observer and representation inputs; do not assume that the current proof's illustrative distance rules are a complete production policy.
4. Map selected representations into stable semantic/resource identities and a platform-neutral `RenderFrame`; adapt through Visualize's `RendererFrameAdapter`.
5. Submit through the existing Visualize API. Keep GPU/backend resource state disposable and downstream of authoritative simulation state.
6. Preserve a separate focused GUI.SVG path for inspectable living documentation and deterministic CI artifacts. SVG node count is not a proxy for the future GPU workload.

### Acceptance gates before calling it integrated

- The same committed snapshot and observer/configuration produce deterministic selected identities and representation decisions.
- Every emitted representation traces to a stable source identity and the snapshot tick/run; aggregation records the identities/count it summarizes.
- Changing observer pose or representation policy changes selection only; it never mutates simulation state or changes physics results.
- Tests distinguish total simulated population from selected representation count, submitted draw-command count, and resource count.
- A crowd/aggregation case demonstrates fewer presentation commands than individual-agent presentation without losing authoritative agent counts or identity recoverability.
- A near/interactive case promotes selected agents to richer representations while distant agents remain coarser or omitted according to policy.
- Frame failures, missing resources, stale snapshots, and empty selections are explicit and bounded; rendering cannot stall simulation ticks.
- CI proves the actual Drone Construction snapshot → adapter → Renderer frame → Visualize submission path. A CPU/SVG proof must be labelled as such; production GPU output and performance remain separate gates.

**Scale reporting:** report simulation throughput and observer-facing presentation workload independently. At minimum record total population, selected individual representations, aggregate representations, submitted commands, resource count, frame build/submit duration, allocations, and snapshot tick age. Do not publish a “100k rendered drones” claim from a 100k physics-kernel benchmark.

## Review checklist

- [x] Can a consumer render a snapshot without a reference to the simulation object? The standalone SVG renderer takes only a snapshot, dimensions, and space identity.
- [x] Can the optional source graph render initial and completed committed snapshots through GUI.SVG? Verified by CI run 39853079623.
- [x] Is there a maintained snapshot-polling SVG view rather than only a test helper? `ConstructionSnapshotSvgView` refreshes on demand and coalesces to the newest observer sequence; hosted GUI.SVG integration remains open.
- [x] Does the standalone SVG consumer show the exact tick and snapshot agent count? Yes; a hosted application still needs to surface refresh failures and its own cadence.
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


## First executable Renderer handoff — implementation contract

The next vertical slice belongs in an optional integration/proof project, not in `src/DroneConstruction`. Keep the Experiences core free of Renderer, Visualize, GUI.SVG, GPU, and Unity references.

### Input and provenance

- Poll `ConstructionSnapshotObserver.TryGetLatest` once per frame attempt. Treat the returned snapshot and sequence as one observation; never combine agent data from different ticks.
- Carry `RunId`, `snapshot.Tick`, and observer `sequence` in a sidecar frame-provenance record until the renderer frame contract has a first-class metadata slot. Do not encode provenance only in SVG labels.
- Use `ConstructionAgentSnapshot.AgentId` as source identity and `QualifiedGridLocation` as the space-qualified location. Grid coordinates alone are not globally unique.
- Reject/skip a stale sequence before building the frame; an empty selected set is a valid frame and must not be confused with a missing snapshot.

### First slice mapping

1. Introduce an explicit observer input (pose/space, intent, selection budget, and LOD policy); make defaults deterministic and bounded.
2. Select a bounded set of individual agents and deterministic cohorts from the one committed snapshot. Sort by stable agent ID before applying any budget or emitting output.
3. For individual representations, preserve the agent ID in a semantic/source map associated with each draw command. For aggregates, preserve the snapshot tick, cohort key, member count, and recoverable member IDs or a stable membership reference.
4. Convert selected representation decisions to existing Renderer contracts and pass the resulting `RenderFrame` through Visualize's existing `RendererFrameAdapter` and GUI.SVG backend.
5. Emit independent counters for snapshot population, individual selections, aggregate count, omitted/off-screen count, draw commands, resources, frame duration, allocations, and snapshot age. Do not treat draw-command count as simulation population.
6. Add deterministic tests for same-input/same-output, observer change without simulation mutation, aggregate count reconciliation, bounded selection, stale sequence handling, and empty frames.

### Integration boundary and proof label

Visualize already has a CPU-side Renderer-to-GUI.SVG proof using distance selection, FSM-backed representation state, `RenderFrameAdapter`, resource validation, and SVG submission. That proof is a reusable downstream boundary, not evidence that Drone Construction is connected to it. The new proof is complete only when a real `ConstructionSimulationSnapshot` is the upstream source and CI runs the entire snapshot → observer selection → Renderer frame → Visualize → GUI.SVG path.

Do not claim a hosted live app, GPU execution, production Event Horizon/bucket coordination, starvation-freedom, or 100,000-agent construction scale from this slice. Measure presentation cost separately from simulation throughput.
