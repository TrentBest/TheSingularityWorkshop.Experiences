# Castle Siege Experience Manifest — schema v1

The Experience manifest is the authored identity/requirements contract for Castle Siege. It is **not** the MicroBundle descriptor, a package feed, a runtime composition plan, or a physics result record.

## Ownership boundaries

- **Experience manifest:** stable Experience identity, authored Experience version, artifact references, and creator-declared capabilities.
- **MicroBundle descriptor:** executable bundle/provider identity and dependencies, owned by MicroBundleDomain.
- **Artifact repository:** immutable artifact bytes and their verified content hashes.
- **Physics run record:** model identity/version, physical inputs, units, and actual solver samples, owned by Physics.
- **FSM_COS:** composes verified artifacts; it does not reinterpret Experience rules or create physical outcomes.

## Schema and identity

- Schema version is explicitly `1`; an unknown schema version must be rejected rather than guessed.
- Experience identity is reserved as numeric ID `3301` and key `castle-siege`. Package versions and URLs are not identity.
- Every artifact is referenced by stable artifact ID and version. `sha256` is nullable during development; a publishable immutable artifact must be content-pinned before trust or execution.
- Capability IDs are stable semantic names, not renderer-specific implementation details.
- Requirement levels are `Required`, `Preferred`, `Optional`, `Delegable`, and `FrameCritical`. These are authored constraints, not permissions. The host must compare them against capabilities actually available and must not infer capability presence from a declaration alone.

## Canonical representation

The C# contract validates identity and required fields and emits compact JSON with artifacts and capabilities sorted by ordinal ID. Producers should use `ToCanonicalJson()` before hashing or comparing equivalent manifests. JSON property order and whitespace from arbitrary external producers are not canonical; consumers should parse and validate before treating a manifest as trusted.

The canonical representation is deterministic for a given validated object. It is not yet a cross-language canonical-JSON standard and must not be treated as a cryptographic signature format. Before immutable publication, the ecosystem must define one cross-language canonicalization rule and hash the exact published bytes.

## Current declared requirement

The initial manifest requires `physics.projectile-trajectory.constant-acceleration`. This describes the intended minimum for the first physics-backed slice. It does **not** mean that the Experience currently connects to the Physics project or that collision, hit/miss, impact, damage, or multiplayer exists. A JSON evidence adapter now validates and projects the Physics v1 trajectory run-record shape without reimplementing its solver. The current tests use a contract fixture; live source integration that feeds an actual Physics experiment result through this adapter is still pending. The entry bundle therefore continues to report that a Physics run is not connected.

## Files

- JSON Schema: [castle-siege-experience-manifest.schema.json](schemas/castle-siege-experience-manifest.schema.json)
- Example: [castle-siege-experience-manifest.example.json](schemas/castle-siege-experience-manifest.example.json)
- Experience design: [CASTLE_SIEGE_EXPERIENCE.md](CASTLE_SIEGE_EXPERIENCE.md)
