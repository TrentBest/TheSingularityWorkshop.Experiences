# Verified Experience → MicroBundleRepository binding

## Purpose

This integration layer turns the dependency-first \`ExperienceArtifactBindingPlan\` into a complete repository-local binding plan by consuming the repository-owned \`IMicroBundleIdentityRepository\` contract.

For every artifact, it:

1. Resolves the exact portable \`(ontology, version, SHA-256)\` identity.
2. Requires the returned repository-local address to be non-zero and to match the handle already represented by the source plan.
3. Retrieves the artifact through \`IMicroBundleRepository.GetAsync\`.
4. Verifies the returned address and recomputes SHA-256 over the retrieved bytes.
5. Preserves the source plan's dependency-first ordering and portable identity.

Any missing, duplicate, aliased, unresolved, mismatched, or unverifiable artifact fails the entire plan. No partial ordered binding list is returned.

## Identity boundaries

- The authored Experience manifest remains portable: ontology string + exact version + SHA-256.
- MicroBundleRepository owns durable identity allocation, persistence, repository-local addresses, and artifact-byte integrity.
- This adapter emits only a repository-scoped artifact binding plan.
- A repository-local numeric handle is **not** a \`MicroBundleDescriptor.Id\`, \`MicroBundleDependencyRequest.BundleId\`, or \`MicroBundleReference.BundleId\`.

The separate portable-to-runtime/domain mapping and dependency-closure proof remain gated by [MicroBundleDomain issue #28](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain/issues/28). This integration does not create an FSM_COS runtime manifest, materialize runtime MicroBundles, compose an Experience, or claim playability.

## Validation

The source-integration test uses the real filesystem repository adapter. It ingests two immutable artifacts, resolves the persisted bindings, retrieves their bytes, verifies SHA-256, and asserts that the final plan retains dependency-first order and exact repository addresses. Additional tests prove fail-closed behavior for missing identities, aliased handles, and unsuccessful source plans.
