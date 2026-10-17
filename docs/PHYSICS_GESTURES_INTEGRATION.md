# Physics ↔ Gestures Integration Contract

**Status:** coordination contract; executable cross-domain contact/recovery arbitration is not yet implemented in this repository.

**Owners:** Physics owns physical evidence and model validity; Gestures owns reusable motion procedures and next-intent policy; Experiences owns the integration artifact and the manifested learning experiment; MicroBundleDomain owns the host-neutral load/arbitration contracts; FSM_COS owns composition and lifecycle; Renderer/Visualize owns presentation.

## The boundary

Do not add a Physics project/package reference to Gestures, or a Gestures reference to Physics. Keep both reusable cores independently consumable. If an integration artifact needs both, it belongs at this Experience/composition edge—or in a separately published integration MicroBundle—not inside either core.

The first useful cross-domain example is a foot or hand contacting a surface during a gesture:

1. **Gesture procedure** proposes/evaluates a target-agnostic motion.
2. **Physics contact provider** reports what its model observed: body/contact identities, geometry, relative velocity, signed separation, logical step/time, and optional normal force/impulse only when computed.
3. **Assessment capability** interprets the physical evidence with the relevant body roles, material, load, and model context. It may report supported, slipping, unsupported, or unknown only when its model supports that conclusion.
4. **Adapter capability** translates the assessed result into the narrower Gestures contact/recovery contract.
5. **Gesture policy** recommends the next intent; the actor/provider selects and evaluates a procedure.
6. **Physics** observes the next attempt. A recommendation is not a physical outcome.

A raw contact candidate is not proof of support, grip, friction, balance, or slip. In particular, do not translate Physics.Collision.ContactObservation.IsActive == true directly into Gestures.ContactLoadOutcome.Supported. If assessment is missing, stale, incompatible, or inconclusive, preserve uncertainty and select AwaitObservation (or a host-defined safe fallback).

## What exists today

- Physics exposes Physics.Collision.ContactObservation as immutable evidence, with SI-unit and logical-time semantics. It does not claim to solve friction or establish support.
- Gestures exposes its own ContactObservation assessment and deterministic ContactRecoveryPolicy. These types have intentionally different meanings.
- MicroBundleDomain exposes IMicroBundle, MicroBundleDescriptor, provider IDs, bundle-ID dependencies, and host-driven Arbitrate(...).
- The current MicroBundle descriptor does not describe a typed provider contract, contract-version range, unit compatibility, freshness policy, or clock conversion. A provider ID is an identity, not a service locator or proof of semantic compatibility.
- Experiences already contains a separate Castle Siege projectile-evidence JSON adapter. That adapter validates an existing Physics trajectory record; it does not calculate a trajectory, collision, hit, or damage. It is precedent for a narrow, evidence-preserving boundary, not an implementation of contact/recovery arbitration.

## First executable proof

Before adding a cross-package adapter, define the smallest host-neutral contract that the runtime can actually resolve. The proof should include:

- A Physics evidence provider and a separately selectable load/contact assessment provider.
- A Gestures recovery provider whose input is the assessed contract, not Physics implementation types.
- An adapter MicroBundle that resolves compatible providers through an explicit host capability mechanism. Do not pretend the current opaque ExperienceContext or provider IDs alone already implement that mechanism.
- Explicit semantic contract identity/version, SI units, logical timestamp/step provenance, and a declared freshness/time-mapping policy.
- Deterministic tests for active vs inactive contact, absent optional force/impulse, unknown assessment, stale evidence, mismatched contract version, missing assessment capability, and stable identical input.
- A test that proves no raw Physics contact can silently become Supported or Slipping.
- A visible, generated learning example that exposes the evidence and assessment side by side, with each quantity and assumption inspectable.

Keep this as an integration MicroBundle; do not make either core depend on the other. If the existing host has no provider-resolution contract yet, first extend or implement that host boundary with tests rather than smuggling a registry into a core package. Any extension to MicroBundleDomain must be minimal, backward-compatible where practical, and coordinated with its owner.

## Learning-first manifestation

The lesson should begin with observable atomic constructs: two bodies, a contact point and normal, relative motion, the reported load values (or explicit absence), and the resulting assessment. Reveal the math and geometry beside the evidence instead of requiring the student to begin with equations. Then let the student change one declared input and compare the next observation. A diagram is not evidence of a solver: label calculated values, measured/model-reported values, assumptions, and unknowns separately.

## Ownership and coordination

- **Physics:** physical definitions, units, model validity, evidence, and physical outcomes.
- **Gestures:** procedure definitions, actor filtering, pose evaluation, and next-intent recommendations.
- **Experiences:** the adapter artifact, composition test, learning narrative, and integration documentation.
- **MicroBundleDomain / FSM_COS:** host-neutral capability identity, resolution, arbitration, and lifecycle.
- **Renderer / Visualize:** inspectable visual presentation; no authority to infer physical truth.

No direct core dependency, package publication, or claim of executable contact/recovery arbitration is implied by this document.
