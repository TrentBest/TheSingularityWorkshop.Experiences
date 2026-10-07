# Common Workshop startup contract

WebPage, WebApp, and AnyApp are three manifestations of the same Workshop startup contract.

The host is allowed to differ in presentation capability. It is not allowed to invent a different Experience sequence.

## Runtime sequence

For a session with no assigned Experiences:

    SESSION MANIFEST
          |
          +--> MONIKER
          |
          +--> assigned Experiences = none
                        |
                        v
                      FORGE

For a session with assigned Experiences:

    SESSION MANIFEST
          |
          +--> MONIKER
          |
          +--> Experience 1
          +--> Experience 2
          +--> ...

## Living GUI boundary

The Living GUI is itself becoming a canonical Experience MicroBundle (Experiences issue #8).

Its behavior belongs to:
- FSM_API for execution;
- the canonical Experience/MicroBundle artifact for behavior and configuration;
- MicroBundleRepository for immutable artifact delivery;
- FSM_COS for composition;
- the manifestation for rendering only.

WebPage, WebApp, and AnyApp must not grow three independent Living GUI implementations.

Until issue #8 is complete, existing host implementations are transitional and must not be mistaken for the target architecture.

## Moniker boundary

The Workshop Moniker is also an Experience artifact.

The host must not compile its own WebAppMonikerMicroBundle, AnyApp MonikerMicroBundle, or another equivalent host-local copy once the published artifact is available.

The target path is:

    session manifest
          |
          v
    published Moniker Experience identity
          |
          v
    MicroBundleRepository
          |
          v
    verified immutable MicroBundle artifact
          |
          v
    FSM_COS
          |
          v
    RuntimeAssembly
          |
          v
    manifestation

The first Moniker artifact may begin as one MicroBundle so the delivery boundary can be proven. The next refinement is to decompose its letters and motion behaviors into independently addressable MicroBundles.

## WebPage-only presentation

WebPage is the public proving ground.

Only WebPage should add the explanatory labels/advisory material and the first Flex showcase that deliberately exposes the Workshop technology.

WebApp and AnyApp should not reproduce those labels.

After their common Living GUI phase, they should present the same Moniker Experience and continue through the same session-manifest decision.

This is a manifestation difference, not an Experience difference.

## Configuration

Every requested MicroBundle may carry configuration in the manifest.

No configuration means the MicroBundle's declared defaults.

The host does not interpret domain configuration merely to make its own presentation work. It passes configuration through the composition boundary.

## Artifact identity

A published MicroBundle is immutable and addressed by:

    BundleId + Version + SHA-256

An Experience manifest records the immutable identity of every required MicroBundle plus its optional configuration.

## Repository discovery

The repository must eventually expose both:
1. what immutable artifacts and published Experiences physically exist; and
2. where ontology coverage is available.

Ontology-aware discovery needs at least:

    Ontology identity
    Start address
    End address
    Repository node/server
    Storage partition
    Artifact identity

The range is an address-space capability, not a semantic dependency resolver.

The existing MicroBundleRepository issues #6, #15, and #16 track the Observatory, ontology range discovery, and server placement work.

## Non-negotiable boundary

    Experience = WHAT
    MicroBundleRepository = WHERE
    FSM_COS = HOW
    FSM_API = BEHAVIOR / EXECUTION
    Manifestation = PRESENTATION

A manifestation may optimize for its platform. It must not replace the common Experience with host-local behavior.