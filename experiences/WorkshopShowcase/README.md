# Workshop Showcase Experience

This folder is the Experience-owned source for the Workshop showcase.

The WebPage host must eventually select this Experience by immutable identity/version and load its manifest and content through the configured Experience/MicroBundle repository boundary. The host must not copy these definitions into compiled page components.

## Ownership

- This Experience owns its identity, startup/running selection, required capability roots, showcase content, and visitor journey declarations.
- The WebPage/WebApp host owns browser lifecycle, environment capabilities, navigation transport, manifest selection, and presentation adapters.
- FSM_COS owns dependency closure and composition.
- FSM_API owns state-machine semantics.
- GUI.SVG and the appropriate renderer/presentation package own visual representation.

## Current status

The manifest is an initial repository-owned contract, not proof of runtime artifact resolution. Its numeric Experience ID is deliberately unassigned: the current MicroBundleRepository ExperienceArtifactAddress requires a numeric ID, and no Workshop Showcase ID has yet been reserved. The current WebPage host still loads its legacy host manifest from its own static assets and uses a compiled MicroBundle catalog. Do not describe the Experience as runtime-loaded until the host fetches this manifest through the repository boundary and demonstrates switching between two valid manifests without recompilation.

## Next migration steps

1. The current six-entry demonstration catalog has been copied here; now migrate journey definitions and make WebPage consume this catalog before removing the duplicate host copy.
2. Reserve a numeric Experience ID, define the canonical published manifest/hash contract, then publish this Experience manifest and content as immutable, versioned artifacts through the Experience/MicroBundle repository path.
3. Update WebPage to choose an Experience identity/version, resolve the manifest, validate all references, and compose a candidate with FSM_COS.
4. Activate only after successful resolution/composition; retain the previous active Experience on failure.
5. Prove reset, switch, and reload against two configurations on the same host build.
6. Remove the corresponding duplicated content and behavior from WebPage only after the replacement is exercised by tests and a visitor journey.
