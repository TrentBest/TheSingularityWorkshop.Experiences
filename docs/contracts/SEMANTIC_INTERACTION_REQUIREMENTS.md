# Semantic Interaction Requirements Contract

## Status

This document defines the Experience-side contract for declaring semantic interaction requirements. It is a dependency-free contract proposal for implementation and tests in this repository; it does **not** claim that AnyApp, WebApp, FSM_UserIO, or FSM_COS already implements capability negotiation.

No package reference to FSM_UserIO is required to author or validate this declarative contract. Do not add that dependency until the exact candidate commit passes the dependency-policy gate in [CiAi issue #21](https://github.com/TrentBest/TheSingularityWorkshop.CiAi/issues/21).

## Boundary

An Experience declares **what semantic interaction contract it requires**. A host chooses **which compatible implementation artifact provides it**.

Keep these distinct:

- **Requirement** — stable semantic capability identity and compatible contract version range, authored with the Experience.
- **Provider** — a host-discovered implementation that declares the capability contract(s) it implements.
- **Artifact identity** — the concrete immutable MicroBundle identity selected for a provider: bundle ID, version, and content hash, plus immutable configuration identity when required by the existing publication path.
- **Authorization** — whether a particular request is allowed in the current domain state. A capability match never grants authorization.
- **Composition** — the already-resolved concrete bundle roots and dependency closure passed through the existing repository and FSM_COS path.

The published Experience manifest must not contain WPF controls, DOM event names, keyboard bindings, browser-specific implementation details, or a host-selected bundle ID as the identity of a semantic requirement.

## Requirement shape

Use a stable, namespaced, case-sensitive capability ID and a version range for the semantic contract. A requirement may also carry a human-readable rationale. Do not use a general-purpose Experience intent as a substitute for a list of capabilities.

Example (illustrative declarative fragment, not yet a runtime manifest schema):

```json
{
  "interactionRequirements": [
    {
      "capabilityId": "workshop.userio.target-selection",
      "contractVersion": "1.x",
      "required": true,
      "rationale": "The player must be able to select a target independently of the host's input devices."
    },
    {
      "capabilityId": "workshop.userio.gantry-motion",
      "contractVersion": "1.x",
      "required": true,
      "rationale": "The player must be able to request a gantry movement."
    },
    {
      "capabilityId": "workshop.userio.capsule-rotation",
      "contractVersion": "1.x",
      "required": true,
      "rationale": "The player must be able to request capsule rotation."
    },
    {
      "capabilityId": "workshop.userio.drop-execution",
      "contractVersion": "1.x",
      "required": true,
      "rationale": "The player must be able to request a drop operation."
    }
  ]
}
```

The identifiers above are proposed Experience-side semantic names for the Capsule Clinic proof, not claims that providers already exist. Before these identifiers become a published compatibility promise, define the request payloads, validation outcomes, and deterministic domain effects they represent.

## Host resolution contract

Resolution must happen before activation and produce a concrete, auditable result:

1. Read the immutable Experience manifest and its interaction requirements.
2. Discover providers available to this host through its supported catalog/repository path.
3. Match by exact capability ID and a contract version compatible with the declared range. A provider's display name or platform name is not a match.
4. Reject missing required capabilities, multiple unresolved candidates, incompatible contract versions, invalid artifact hashes, and missing/incompatible dependencies. Do not silently choose a different semantic contract.
5. Pin each selected provider to an immutable artifact identity and resolve its dependencies through the existing MicroBundle repository/catalog.
6. Pass only the resolved concrete roots and their verified dependency closure to the existing FSM_COS composition path.
7. Activate/present the Experience only after resolution and composition succeed. Failure must report the missing or incompatible capability and the host's available candidates without claiming that the Experience is running.

Optional requirements may be omitted only according to an explicit, deterministic policy. Preferred requirements may influence selection only when the policy defines a deterministic tie-break. Do not treat a required capability as satisfied by a preferred or optional provider.

## Provider declaration

A provider declaration must be inspectable before loading or executing the implementation. At minimum it needs:

- stable provider identity;
- one or more exact capability IDs and their implemented contract versions;
- immutable MicroBundle artifact identity (bundle ID, version, SHA-256 content hash);
- dependency identities resolved through the existing dependency contract;
- any immutable configuration artifact identity required by the existing host/publication path.

The provider declaration is metadata, not proof that the implementation behaves correctly. Contract tests must verify that the implementation emits valid semantic requests and that the Experience validates those requests.

## Semantic request and authority

A host adapter translates device- or browser-specific actions into the same host-neutral semantic request. The Experience/domain validates the request against current state, actor authority, legal phase, and domain rules before applying a transition. Invalid or unauthorized requests must not mutate authoritative state.

The semantic request must not contain assumptions such as "left mouse button", "WPF command", or "DOM click". Those belong in the host adapter. The semantic operation and its validated result belong to the Experience/domain contract.

## Capsule Clinic proof and failure matrix

The first proof should exercise target selection, gantry movement, capsule rotation, and drop execution through the same domain-facing request contract on Windows and browser hosts.

| Case | Required result |
| --- | --- |
| All required capabilities resolve to compatible, immutable providers | Composition may proceed through the normal repository/FSM_COS path |
| A required capability is absent | Fail before activation; name the missing capability |
| Only an incompatible contract version is available | Fail before activation; report required and available versions |
| More than one provider matches and policy cannot choose deterministically | Fail before activation; report ambiguity |
| Provider artifact hash/version is missing or invalid | Reject the provider; do not fall back silently |
| A selected provider dependency is absent or incompatible | Fail resolution before activation |
| A semantic request is malformed or invalid for current domain state | Reject it without an authoritative state transition |
| Equivalent valid requests arrive through Windows and browser adapters | Produce the same deterministic domain transition |

Tests should separate pure requirement validation and deterministic matching from host/repository integration tests. The integration proof must exercise the real host provider catalog and repository/FSM_COS path when those host changes are allowed by the dependency-policy gate.

## Scope and non-claims

This contract does not add a capability registry to FSM_COS, reuse WebPage's separate Hub ontology contracts, or make MicroBundleDomain ontology-aware. It does not implement a WPF adapter, browser adapter, provider catalog, or end-to-end launch flow by itself. Those require host-side implementation and tests in their owning repositories after the dependency gate is satisfied.

No package publication or deployment is authorized by this work.
