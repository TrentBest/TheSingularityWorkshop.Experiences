# Experience Vertical Slice Roadmap

The roadmap is ordered around a working path rather than repository completeness.

## Phase 0 — Theory

- [x] Establish Experience repository.
- [x] Define manifestation model.
- [x] Define WebApp to AnyApp relationship.
- [x] Define initial security boundary.
- [x] Define VR as another manifestation.
- [x] Define creator-owned compute requirements and capability negotiation.
- [ ] Review theory against existing WebApp, AnyApp, FSM_COS, and MicroBundleRepository implementations.

## Phase 1 — Independent artifacts

- [ ] Create independent Forge MicroBundle project.
- [ ] Create independent Moniker MicroBundle project.
- [ ] Ensure they do not depend on AnyApp source projects.
- [ ] Build deterministic assembly artifacts.
- [ ] Wrap artifacts in the established MicroBundle binary envelope.
- [ ] Calculate immutable content hashes.

## Phase 2 — Immutable Experience manifests

- [ ] Extend Experience bundle requests with artifact version and content hash.
- [ ] Define canonical serialized representation.
- [ ] Define creator-owned compute/capability requirements.
- [ ] Validate identity and requirements during parsing.
- [ ] Remove the need for host-compiled MicroBundle discovery.

## Phase 3 — Capability-driven repository-backed composition

- [ ] Retrieve MicroBundle artifacts from MicroBundleRepository.
- [ ] Verify artifact identity.
- [ ] Materialize verified assemblies.
- [ ] Bind them to FSM_COS on the composition side.
- [ ] Evaluate Experience requirements against available manifestation capabilities.
- [ ] Keep repository storage semantics independent of FSM_COS.

## Phase 4 — WebApp to AnyApp

- [ ] Present an explicit Open in AnyApp action in the MVP WebApp.
- [ ] Launch AnyApp through the registered protocol.
- [ ] Resolve the immutable Experience identity.
- [ ] Establish the bridge session.
- [ ] Exchange capabilities and requirements.
- [ ] Publish Experience lifecycle/state.
- [ ] Record explicit connection events.

## Phase 5 — Co-entangled Experience

- [ ] WebApp, AnyApp, and MyVR can share the same Experience identity.
- [ ] Define the first shared state field.
- [ ] Define one browser-originated event.
- [ ] Define one AnyApp-originated result.
- [ ] Demonstrate disconnect/reconnect behavior.
- [ ] Demonstrate creator-defined required vs optional capabilities.
- [ ] Keep the MVP functional when optional manifestations are unavailable.

## Phase 6 — VR pathway

- [ ] Add WebXR capability discovery where appropriate.
- [ ] Demonstrate a user-gesture XR session request.
- [ ] Reuse Experience identity/session concepts.
- [ ] Establish VR to AnyApp communication requirements.
- [ ] Measure latency-sensitive work.
- [ ] Keep frame-critical operations on the VR device.
- [ ] Move suitable heavy/local computation to AnyApp.
- [ ] Demonstrate an Experience that intentionally uses WebApp + MyVR + AnyApp simultaneously.

The first demonstration is complete when one Experience can be opened from the WebApp, run by AnyApp, and visibly remain the same logical Experience across the manifestations.
