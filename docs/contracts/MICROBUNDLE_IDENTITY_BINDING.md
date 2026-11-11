# Experience-to-MicroBundle Identity Binding

## Status

This document defines the Experiences-side contract boundary for resolving authored manifest bundle keys to the numeric identities required by MicroBundleDomain, MicroBundleRepository, and FSM_COS.

The contract is implemented as a validated in-memory mapping primitive in `TheSingularityWorkshop.Experiences.Contracts`. It does **not** establish an ID allocation authority, publish a registry, or make a manifest loadable by itself.

## The two identities are intentionally different

An Experience manifest uses a readable string key for each artifact root:

```json
{
  "bundleId": "workshop-minesweeper-rules",
  "version": "1.0.0",
  "sha256": "<64 hexadecimal characters>"
}
```

The string is an Experience-authored lookup key. It is not the runtime MicroBundle ID.

The platform contracts use a non-zero `ulong` MicroBundle identity. The immutable repository address is the tuple:

```text
(BundleId: ulong, Version: string, ContentHash: SHA-256)
```

Do not conflate any of these with the Experience's own numeric identity or the hash of its serialized manifest/artifact.

## Binding contract

Before a manifest artifact can be translated into a MicroBundle repository address, the host/integration boundary must supply an explicit binding:

```text
Experience bundle key -> canonical non-zero MicroBundle ulong ID
```

The mapping primitive enforces these invariants:

- Keys are exact, ordinal, case-sensitive identifiers. No trimming, case folding, hashing, or normalization is performed.
- Keys must be non-empty, already trimmed, and free of control characters.
- Numeric IDs must be non-zero.
- Each key maps to exactly one numeric ID.
- Each numeric ID maps to at most one key in the same mapping. Aliases are rejected.
- Forward and reverse lookups must agree.
- Unknown keys and unknown numeric IDs remain unresolved; no ID is inferred or allocated.

A caller must obtain the numeric ID from an authoritative owner-controlled source and pass it as an explicit binding. This library deliberately does not define that source or assign values.

## Resolution sequence

For each manifest artifact, the integration layer must:

1. Validate the Experience manifest using the existing manifest contract.
2. Resolve its exact string `bundleId` through an explicitly supplied identity map.
3. Combine the resolved numeric ID with the manifest's exact version and SHA-256 to form the immutable MicroBundle artifact address.
4. Retrieve the artifact through the existing repository contract, which verifies the content hash.
5. Hand verified opaque bytes to a host-owned materializer/catalog implementation.
6. Allow FSM_COS to compose the resulting `IMicroBundle` instances through its existing numeric-ID runtime manifest.

Experiences owns steps 1–3 as authored identity and resolution semantics. Repository retrieval, byte verification, executable interpretation/materialization, composition, execution, and presentation remain owned by their existing platform/host boundaries.

## Deliberate non-goals

This contract does not:

- derive numeric IDs from strings, hashes, project names, or assembly metadata;
- reserve or allocate numeric IDs;
- change the current manifest schema's string-valued `artifacts[].bundleId`;
- define a universal executable bundle envelope or serialization format;
- deserialize assemblies or implement an `IMicroBundleArtifactMaterializer`;
- create a repository catalog, FSM_COS loader, Foundry executor, or local game host;
- prove that a manifest's referenced artifact exists, is loadable, or is playable.

The existing AnyApp assembly materializer is a host-owned reference, not a format or implementation to copy into Experiences. The repository's opaque-byte boundary remains authoritative until a host explicitly supplies a supported materializer contract.

## Validation and evidence

The mapping primitive's unit tests cover bidirectional resolution, exact-key behavior, unknown values, zero IDs, duplicate keys, duplicate numeric IDs, and malformed keys.

A successful mapping test is only evidence that the identity binding is internally valid. It is not evidence that the bundle artifact exists, that its SHA-256 matches, that it can be materialized, that FSM_COS can compose it, or that Foundry can execute it.

Those are separate gates and must be reported separately in the CODE RED pilot evidence.
