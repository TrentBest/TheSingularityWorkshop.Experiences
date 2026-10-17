# WebPage Runtime-Host Handoff

**Target:** WebPage development  
**Owner of this document:** Experiences repository  
**Purpose:** coordinate the migration from a page-owned showcase to a repository-owned, manifest-selected Experience.

## Decision

WebPage is a browser host, not the authoring home or compiled implementation of the Workshop showcase. The Experience definition and its content belong here. WebPage loads the selected Experience at runtime, asks FSM_COS to compose its versioned MicroBundle roots, and presents the resulting capabilities through the appropriate UI/rendering packages.

## Initial artifact

- Experience: `workshop-showcase`
- Version: `0.1.0`
- Manifest: [manifest.json](manifest.json)
- Demonstration catalog: `demonstrations.json` now contains all six records from the current WebPage host manifest.
- Remaining content migration: visitor journey declarations and the host's runtime integration; the legacy WebPage copy remains until the new source is exercised.

The MicroBundle IDs and versions are references to existing declared roots, not proof that arbitrary bundles can currently be fetched or loaded dynamically. The Experience has a stable source key but no numeric Experience ID yet; the current MicroBundleRepository ExperienceArtifactAddress requires one before immutable publication.

## Host contract

The WebPage host must provide:

1. a default Experience identity/version and a user-selectable active manifest;
2. a repository-backed manifest/artifact resolver with a clearly defined local cache/bootstrap path; the current MicroBundleRepository REST host exposes MicroBundle artifact routes but does not yet expose the ExperienceArtifact/IExperienceCatalog contract as HTTP endpoints;
3. schema, identity, version, digest, dependency, and host-capability validation;
4. candidate composition through FSM_COS before active-state mutation;
5. atomic activation and rollback to the prior active Experience on failure;
6. reset/restart semantics that really reload and recompose the selected manifest;
7. explicit switch/reload status and diagnostics;
8. presentation of manifest-selected content rather than a compiled page-name switch;
9. truthful GUI.SVG/renderer-backed living proof for concepts that have observable behavior.

## Safety and boundaries

- A manifest is declarative; it must not be treated as arbitrary executable code.
- Executable behavior is supplied through trusted, versioned MicroBundle artifacts.
- Browser-provided URLs and content are untrusted. Repository resolution must enforce allowed origins, immutable identity/digest verification, size limits, compatibility checks, and host capability policy.
- FSM_COS owns dependency closure and composition. WebPage must not implement a parallel dependency resolver.
- Do not remove an existing visitor journey until a replacement has passed relevant tests.
- Keep changes on `development`; do not promote or publish without explicit authorization.

## Status

This is the first repository-owned Experience manifest contract. WebPage now has an initial same-origin host-manifest switch/reset API, candidate composition and activation, stable session-scoped Hub activation that preserves PageFSM's Hub reference, and Hub-lab controls; that API still reads the legacy WebPage host-manifest schema. It does **not yet** resolve this Experience-owned manifest or its content through MicroBundleRepository/cache. The next handoff is to implement that repository boundary and migrate the remaining showcase definitions without deleting the working fallback prematurely.

WebPage implementation tracking: [WebPageLLM.md](https://github.com/TrentBest/WebPage/blob/development/docs/WebPageLLM.md). Tests/build status must be checked in the linked workflow before the initial host switch slice is called green.
