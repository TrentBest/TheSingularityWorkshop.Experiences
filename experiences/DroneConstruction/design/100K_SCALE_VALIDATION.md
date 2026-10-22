# 100,000-Agent Scale Validation Plan

**Owner:** Trent Best · **Branch:** development  
**Status:** protocol defined; measured 100,000-agent result not yet established.

## Acceptance rule

A run counts as a successful 100,000-agent demonstration only when the exact source revision, build configuration, input scenario, seed, agent count, tick count, elapsed time, memory/allocation observations, invariant results, and output artifact are recorded together. A rendered crowd by itself is not evidence of 100,000 authoritative agents.

The simulation is the source of truth. Renderer and GUI.SVG consume committed snapshots and must not create agent motion, progress, counts, or completion claims.

## Population ramp

| Stage | Population | Purpose | Required outcome |
|---|---:|---|---|
| P0 | 1–100 | semantics | deterministic completion and invariants hold |
| P1 | 1,000 | baseline | no duplicate occupancy, lost agents, or inventory drift |
| P2 | 10,000 | scaling shape | throughput, memory, allocation, snapshot cost recorded |
| P3 | 25,000 | pressure | hot paths and queue/traffic contention identified |
| P4 | 50,000 | large run | stable execution and explainable performance |
| P5 | 100,000 | target | full correctness gate plus published raw results |

Use at least three repetitions per stage after warm-up. Report median and range; do not select only the fastest run. If a stage fails, preserve failure evidence and stop the ramp until the cause is understood.

## Required run metadata

- Git commit SHA and whether the worktree was clean.
- .NET SDK/runtime, OS, CPU model/core count, memory, power mode, and build configuration.
- Scenario dimensions, height-field mapping/version, target block count, emitter count/inventory, lane topology, occupancy rules.
- Seed, agent count, tick limit, completed tasks, placed blocks, and stop reason.
- Warm-up count, repetition number, wall-clock duration, ticks/second, agent-ticks/second, blocks/second.
- Peak working set or allocated-memory measurement method; GC counts and allocation bytes where available.
- Snapshot/event capture enabled or disabled and its overhead. Never compare unlike configurations without labeling them.
- Agent state distribution, pending transition intents, occupied cells, reservations, emitter inventory, placement completion.
- Invariant failures, exceptions, deadlock/stall detection, and replay digest when implemented.
- Artifact links: machine-readable run record, raw log, summary, and snapshot samples.

## Correctness gates

1. **Identity:** every registered agent has a stable unique ID; no silent loss or duplication.
2. **Occupancy:** each agent occupies at most one cell in one qualified space; no conflicting occupants.
3. **Location:** agent position, qualified location, and grid reverse index agree at every committed tick.
4. **Resources:** emitter inventory never becomes negative; each pickup consumes exactly one block.
5. **Construction:** each placement increments exactly one valid target cell; placed blocks never exceed required blocks.
6. **Transitions:** reservations have one owner; failed entry/exit leaves authoritative state intact; blocked exit remains retryable.
7. **Determinism:** same build, seed, scenario, and inputs produce the same authoritative state/replay digest. If determinism is not implemented for a subsystem, mark that gate open.
8. **Completion:** all required tasks are committed, or the run records an explicit incomplete/timeout/stall result.
9. **Observation:** observer output agrees with the snapshot for the same tick and does not mutate the model.

## Performance reporting

Publish throughput (ticks/second, agent-ticks/second, blocks/second), median and p95 tick duration, slowest tick, peak working set, managed heap, bytes allocated per tick and per agent-tick, and a population-versus-throughput/memory/latency plot. Compare simulation-only, snapshot-only, and full visualization separately. Report oldest waiting age, maximum queue depth, and agents with no progress over the stall window.

A simulation that advances faster than real time and a renderer that displays 60 frames per second are separate claims. Do not promise a frame rate before defining hardware and frame budget.

## Five-cell slipstream lookahead

Five cells is the initial remembered prototype parameter, not a universal safety guarantee. Store it in versioned scenario/simulation configuration. Record cell units, tick duration, lane cells-per-tick, direction, transition length, occupancy model, and admission/exit reservation semantics. Test occupied lookahead, destinations that become blocked after admission, and agents that cannot exit. A lookahead check is not equivalent to reserving every future cell.

## Current evidence boundary

The repository has tests for the discrete construction loop, qualified locations, occupancy-index consistency, skylane direction/speed policy, and separate slipstream lifecycle/admission contracts. The latest observed development workflow run, [39725809829](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/actions/runs/39725809829), completed successfully, including the Drone Construction test job. This validates tests on that commit; it does **not** establish a 100,000-agent benchmark, live GUI.SVG observation, or fully integrated autonomous lane routing.

## Release gate for a scale claim

Do not describe the 100k goal as demonstrated until P5 has a committed result record, all correctness gates are green, at least three measured repetitions are attached, and a reviewer can reproduce the run from the documented command and configuration.
