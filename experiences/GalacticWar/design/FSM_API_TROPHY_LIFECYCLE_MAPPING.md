# FSM_API Mapping: Finite Trophy Lifecycle

**Status:** implementation mapping and proof plan. This document maps the Galactic War trophy contract to the current `FSM_API/development` API. It is not a claim that the runtime proof has been built or executed.

## Decision

Use one FSM_API handle per independently stateful trophy instance. The trophy's stable identity, finite-issue membership, provenance, ownership, custody, location, condition, and event history remain Experience-owned domain data. FSM_API supplies the lifecycle state machine and executes its transitions; it does not become the ownership ledger or proof that a transfer was legitimate.

Do not introduce a second Galactic War state-machine engine, identity allocator, or generic transition framework.

## What the current FSM_API contract gives us

The current `development` branch exposes the following established surface:

- `IStateContext` is implemented by the caller-owned context object. It supplies `Name` and `IsValid`.
- `FSM_API.Create.CreateFiniteStateMachine(name, processRate, processingGroup)` builds a named definition.
- `FSM_API.Create.CreateInstance(name, context, processingGroup)` creates a live `FSMHandle`.
- `FSMHandle.CurrentState` reports the live handle's current state; `FSMHandle.Context` exposes the caller-owned context.
- `FSM_API.Interaction.Update(processingGroup)` advances scheduled instances under host control.
- `FSM_API.Interaction.DestroyInstance(handle)` removes the FSM handle registration. It does not mean the fictional trophy was destroyed.
- `IStateContext.IsValid = false` suspends processing while retaining the handle; restoring validity allows the same handle to resume.

The handle constructor currently sets the supplied context's `IsValid` to `true` after successful construction. Therefore, create a dedicated context for each trophy lifecycle handle. Do not share one mutable context among unrelated trophy instances or assume that constructing a handle preserves an initially-false execution gate. If a newly created instance should begin suspended, set its context invalid after creation and before the first update.

The existing public string-backed FSM surface is sufficient for this lifecycle mapping. The integer-backed 2.0 work is not required to prove the domain boundary and must not be treated as released functionality.

## State ownership

| Concern | Authority | FSM_API's role |
|---|---|---|
| Stable trophy instance ID | Galactic War domain | Context carries the ID; FSM_API handle ID is not the trophy ID |
| Authorized issue and finite cap | Galactic War domain | No cap or mint authority |
| Provenance/event history | Append-only Galactic War domain record | State actions may react to accepted events; FSM_API is not the event store |
| Current lifecycle | Galactic War domain projection, represented in a live FSM handle | Executes the defined lifecycle transitions |
| Owner/title | Galactic War domain | Never infer ownership from `CurrentState` |
| Physical custodian | Galactic War domain | Never conflate custody with title |
| Location and condition | Galactic War domain | Context may expose these facts to guards/actions |
| Rendering and presentation | Renderer / Visualization MicroBundles | No authority over state or transfer |

The domain event history remains authoritative and reconstructs the current lifecycle. A live `FSMHandle` is an execution object, not durable history. After restart, the Experience rebuilds or reconciles the handle from the authoritative domain projection.

## Lifecycle definition

The FSM state names should match the agreed object lifecycle:

- `Minted`
- `InCirculation`
- `Secured`
- `Lost`
- `Destroyed`
- `Retired`

Allowed transitions are policy-controlled:

| From | To | Required domain decision |
|---|---|---|
| Minted | InCirculation | Valid issue, mint authorization, unique instance identity, cap reservation |
| InCirculation | Secured | Valid capture/claim or other authorized securing event |
| Secured | InCirculation | Authorized release, award, or return to circulation |
| InCirculation | Lost | Evidence-backed loss event |
| Secured | Lost | Evidence-backed loss while secured |
| Lost | InCirculation | Authorized recovery; preserve the prior loss event |
| Minted / InCirculation / Secured / Lost | Destroyed | Authorized, evidenced destruction under the object's rules |
| Minted / InCirculation / Secured / Lost | Retired | Authorized retirement under the object's rules |

`Destroyed` and `Retired` are terminal by default. Any restoration must be a separately authorized domain event, not a casual FSM reset. `Exhausted` and `Closed` are issue-level states, not trophy-instance states; do not mix them into each trophy's lifecycle FSM.

## The acceptance boundary

FSM_API transition guards are useful for expressing local state eligibility. They are not a substitute for an atomic domain command.

For every proposed lifecycle operation, Galactic War must first:

1. Resolve the stable trophy ID and authoritative current projection.
2. Validate the actor's authority and the object's current state.
3. Validate the finite issue cap and any issue-level authorization.
4. Validate ownership/title, custody, consent, encounter evidence, and transfer policy as applicable.
5. Check the expected prior version/state.
6. Deduplicate by a stable operation/event ID.
7. Commit one authoritative append-only domain event under the domain's concurrency boundary.
8. Reconcile the accepted event into the trophy's FSM handle and other projections.

Rejected commands must not mutate authoritative state. A retry with the same operation ID must return the original outcome rather than create a second transfer. Two competing claims must be serialized so that at most one can commit against a given expected version.

**Important:** FSM_API's state machine does not itself provide the transaction, durable event log, idempotency store, or distributed concurrency control required by steps 1–8. Those belong to the Galactic War domain and its persistence/runtime composition.

## Ownership and custody are independent

A trophy may be owned by one party and physically held by another. A museum, quartermaster, carrier, escrow service, or battlefield recovery team can have custody without receiving title. Conversely, a bound award can have an owner but no transferable title.

The lifecycle FSM must never use `Secured` as shorthand for “owned by the securing actor.” A successful capture may create a custody event first; title transfer occurs only when the object's policy and claim requirements authorize it. A temporary disarm or dropped object is not automatically a trophy transfer.

## First executable proof

Use a deliberately tiny issue of exactly three instances. The proof is complete only when executable tests demonstrate all of the following:

1. Three unique trophy identities are minted under one authorized issue.
2. A fourth mint is rejected before an instance handle can make it appear legitimate.
3. Each instance receives its own caller-owned `IStateContext` and FSM handle.
4. A valid capture moves one instance from `InCirculation` to `Secured` only after the domain command accepts and records the event.
5. Custody can change without title changing.
6. An authorized award/release changes the title projection and lifecycle as policy requires.
7. A duplicate operation ID produces no second event or transition.
8. Two competing claims against the same expected version cannot both commit.
9. A lost trophy can be recovered without erasing the loss event.
10. A destroyed or retired trophy cannot be revived by an ordinary duplicate or replay.
11. Destroying an FSM handle unregisters execution but leaves the trophy identity and provenance intact.
12. Rebuilding the FSM handle from the domain projection reproduces the expected lifecycle state.

Every test must assert both the current projection and the append-only event history. A green state assertion alone is insufficient.

## Integration sequence

1. Keep this mapping in Experiences; do not modify FSM_API merely to make Galactic War vocabulary fit it.
2. Build the small proof in an Experience-owned test project that references the actual intended FSM_API development dependency.
3. Use the current string-backed API first; keep the proof independent of the unreleased integer-backed path.
4. Run the proof and capture the exact dependency commit, build/test result, and assertions.
5. Only then decide whether a narrowly scoped FSM_API gap exists. Do not request a new API based on hypothetical convenience.
6. Compose persistence and execution through FSM_COS / FSM_Foundry when the vertical slice requires them; do not make the FSM handle itself the persistent world.

## Contract references

The mapping was checked against the current `FSM_API/development` source:

- `FSM_API/IStateContext.cs`
- `FSM_API/FSMHandle.cs`
- `FSM_API/FSM_API.Create.cs`
- `FSM_API/FSM_API.Interaction.cs`

This is a development-branch contract check, not a claim about the published 1.0.13 package or a completed 2.0 release. No NuGet publication, merge, deployment, or branch cleanup is implied.
