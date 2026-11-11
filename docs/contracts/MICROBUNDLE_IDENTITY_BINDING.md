# Experience Ontology, Repository Handles, and Runtime Manifest

## Core rule

**The string ontology is the identity. The numeric ID is a repository-local address.**

These values have different jobs and must never be conflated:

- **Ontology identity (string):** says what the MicroBundle is. This is the canonical, portable identity returned by the endpoint and preserved by every repository that stores the artifact.
- **Repository-local handle (non-zero `ulong`):** says where that repository stored the artifact in its local blob data. It is an efficient lookup/address, not a global identity. Another repository may assign a different handle to the same ontology identity.
- **Version and content hash:** identify the exact artifact revision and verify the bytes associated with that revision.

The ontology string is not replaced when a local numeric handle is assigned. The repository must retain the string-to-handle mapping.

## Intended lifecycle

1. **Resolve from the endpoint.** The endpoint returns the artifact together with its canonical string ontology identity and the version/integrity information required by the artifact contract.
2. **Ingest into the local repository.** Store the artifact in the repository's blob data. Preserve the ontology string as its canonical identity.
3. **Assign or recover the local handle.** The local repository maintains a mapping from ontology string to its own numeric handle. The mapping is persisted with repository metadata so it survives restarts. Handle allocation belongs to the repository, not to the Experience manifest and not to a global ontology authority.
4. **Build the runtime manifest.** Resolve each ontology identity to the local handle and produce the numeric references required by the prime/runtime manifest. Preserve the ontology identity and the manifest's version/integrity requirements as the portable source of truth.
5. **Load locally.** Runtime resolution uses the local handle to locate the cached blob. Normal execution should load from the local repository/cache rather than repeatedly downloading the artifact.
6. **Check version and integrity.** Compare the runtime manifest's requested version (and content hash where specified) with the locally stored artifact metadata. Reuse a matching cached artifact; otherwise retrieve/update it through the endpoint, verify it, store it, and refresh the local mapping/runtime manifest as needed.

A local handle is meaningful only in the repository namespace that assigned it. It must not be copied to another repository and assumed to address the same blob. On transfer, restore, or synchronization, preserve ontology identity and rebuild or restore that repository's own mapping.

## Manifest contract boundary

The current Experience manifest schema represents `artifacts[].bundleId` as a string. In this authored manifest, that field is the ontology identity despite the historical field name. It is not the local numeric handle.

The local integration step resolves that string through the repository's persisted ontology-to-handle map. The generated prime/runtime manifest may then carry the local numeric handle required by FSM_COS, alongside the version/integrity metadata needed to validate the cached artifact.

Keep the authored/source manifest distinct from the repository-local runtime manifest. Do not rewrite the canonical ontology string into a machine-specific number in the source manifest.

## Mapping contract

`MicroBundleIdentityMap` is a validated in-memory view of one repository's persisted bindings:

```text
canonical ontology string -> repository-local non-zero ulong handle
```

It enforces:

- Exact ordinal, case-sensitive ontology strings; no trimming, case folding, hashing, or normalization is performed.
- Ontology strings are non-empty, already trimmed, and contain no control characters.
- Local handles are non-zero.
- Each ontology identity maps to one local handle in this repository.
- Each local handle maps to at most one ontology identity in this repository.
- Forward and reverse lookups agree.
- Unknown ontology strings and unknown handles remain unresolved.

The map does not allocate handles or persist repository metadata itself. The repository owns durable allocation, persistence, blob placement, and mapping recovery.

## Ownership boundaries

- **Ontology / endpoint:** canonical string identity and artifact publication/version information.
- **Experience:** authored manifest and its ontology references.
- **Local MicroBundle repository:** blob storage, repository-local handle allocation, durable string-to-handle mapping, cache lookup, and artifact integrity verification.
- **Manifest integration / host:** resolves source ontology references into a local prime/runtime manifest.
- **FSM_COS:** consumes the runtime manifest and local numeric references for composition and execution.
- **Foundry:** executes the composed experience through its established runtime boundary.

## Explicit non-goals and evidence limits

This mapping primitive does not define endpoint transport, allocate or persist repository handles, implement blob storage/cache policy, define a new artifact envelope, materialize executable bytes, or prove that FSM_COS/Foundry can load or run an Experience.

Passing the map's unit tests proves only that one supplied mapping is internally consistent. It does not prove endpoint retrieval, durable repository behavior, hash verification, runtime-manifest generation, local cache activation, composition, execution, or playability. Those remain distinct implementation and validation gates in the CODE RED pilot.
