# Experience Ontology, Repository Handles, and Runtime Manifest

## Core rule

**The string ontology is the identity. The numeric ID is a repository-local address.**

An immutable artifact revision is identified by the tuple:

`(ontology string, version, SHA-256)`

These values have different jobs:

- **Ontology identity (string):** says what the MicroBundle is. It is the canonical, portable identity returned by the endpoint and preserved by every repository that stores the artifact.
- **Version:** identifies the declared artifact revision.
- **Content hash:** verifies the exact bytes associated with that revision.
- **Repository-local handle (non-zero `ulong`):** says where that exact artifact blob is stored in one repository. It is an efficient lookup/address, not a global identity. Another repository may assign a different handle to the same immutable artifact.

The ontology string is not replaced when a local numeric handle is assigned. The repository must retain the ontology mapping and the artifact revision metadata.

## Intended lifecycle

1. **Resolve from the endpoint.** The endpoint returns the artifact together with its canonical string ontology identity, version, and content-integrity information.
2. **Ingest into the local repository.** Store the artifact in the repository's blob data. Preserve the ontology string as its canonical identity and retain the exact version/hash.
3. **Assign or recover the local handle.** The local repository maintains a persisted mapping from the complete immutable artifact identity to its own numeric handle. This permits multiple cached versions of the same ontology to coexist. Handle allocation belongs to the repository, not to the Experience manifest and not to a global ontology authority.
4. **Build the runtime manifest.** Resolve each source artifact tuple to the local handle and produce the numeric references required by the prime/runtime manifest. Preserve the ontology identity and version/hash as the portable source of truth.
5. **Load locally.** Runtime resolution uses the local handle to locate the cached blob. Normal execution should load from the local repository/cache rather than repeatedly downloading the artifact.
6. **Check version and integrity.** Compare the manifest's requested ontology/version/hash with locally stored artifact metadata. Reuse a matching cached artifact; otherwise retrieve/update it through the endpoint, verify it, store it, and refresh the local mapping/runtime manifest as needed.

A local handle is meaningful only in the repository namespace that assigned it. It must not be copied to another repository and assumed to address the same blob. On transfer, restore, or synchronization, preserve the immutable artifact identity and rebuild or restore that repository's own mapping.

## Manifest contract boundary

The Experience manifest's `artifacts[].bundleId` is the ontology string, despite the historical field name. Together with `version` and `sha256`, it defines the immutable artifact identity. It is not the local numeric handle.

The local integration step resolves that complete tuple through the repository's persisted artifact-to-handle map. The generated prime/runtime manifest may then carry the local numeric handle required by FSM_COS, alongside the ontology string and version/integrity metadata needed to validate the cached artifact.

Keep the authored/source manifest distinct from the repository-local runtime manifest. Do not rewrite the canonical ontology string into a machine-specific number in the source manifest.

## Mapping contract

`MicroBundleIdentityMap` is a validated in-memory view of one repository's persisted bindings:

```text
(ontology string, version, SHA-256) -> repository-local non-zero ulong handle
```

It enforces:

- Exact ordinal, case-sensitive ontology strings; no trimming, case folding, hashing, or normalization is performed.
- Valid, non-empty ontology identity and version, plus a 64-character hexadecimal SHA-256.
- Local handles are non-zero.
- Each immutable artifact revision maps to one local handle in this repository.
- Each local handle maps to at most one immutable artifact revision in this repository.
- Different versions or hashes of the same ontology may coexist at different local handles.
- Unknown artifact identities and unknown handles remain unresolved.

The map does not allocate handles or persist repository metadata itself. The repository owns durable allocation, persistence, blob placement, cache policy, and mapping recovery.

## Ownership boundaries

- **Ontology / endpoint:** canonical string identity and artifact publication/version/integrity information.
- **Experience:** authored manifest and its ontology references.
- **Local MicroBundle repository:** blob storage, repository-local handle allocation, durable artifact-to-handle mapping, cache lookup, and artifact integrity verification.
- **Manifest integration / host:** resolves source artifact identities into a local prime/runtime manifest.
- **FSM_COS:** consumes the runtime manifest and local numeric references for composition and execution.
- **Foundry:** executes the composed experience through its established runtime boundary.

## Explicit non-goals and evidence limits

This mapping primitive does not define endpoint transport, allocate or persist repository handles, implement blob storage/cache policy, materialize executable bytes, or prove that FSM_COS/Foundry can load or run an Experience.

Passing the map's unit tests proves only that one supplied mapping is internally consistent. It does not prove endpoint retrieval, durable repository behavior, hash verification, runtime-manifest generation, local cache activation, composition, execution, or playability. Those remain distinct implementation and validation gates in the CODE RED pilot.
