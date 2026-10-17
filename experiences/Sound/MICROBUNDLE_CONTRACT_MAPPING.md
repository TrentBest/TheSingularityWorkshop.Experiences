# Sound Experience — MicroBundle Contract Mapping

**Status:** design mapping; not an executable MicroBundle or publishable Experience artifact.

This document maps the Sound Experience proposal to existing Workshop contracts before any sound-specific schema or runtime is introduced.

## Existing contracts to reuse

| Concern | Existing owner / contract | Sound use |
|---|---|---|
| Executable capability identity | MicroBundleDomain IMicroBundle and MicroBundleDescriptor | Each independently executable source, capture, or processing capability implements the neutral lifecycle. |
| Provider identity | MicroBundleProvider in the descriptor | Declare a stable provider key; do not overload the Experience manifest's semantic capability names. |
| Dependency and configuration | MicroBundleDependencyRequest; configuration bytes are opaque to the domain contract | Declare required capabilities and pass sound-specific configuration through a separately versioned payload contract. |
| Authoring/tooling fields | MicroBundleDefinition, MicroBundleField, MicroBundleFieldBuilder | Expose stable parameter IDs for editor-visible settings. Do not treat labels or nesting paths as durable IDs. |
| Artifact identity and delivery | MicroBundleRepository Core artifact contract | Identify a built artifact by numeric bundle ID, explicit version, and SHA-256 content identity; repository stores and verifies bytes. |
| Experience identity and composition intent | Experience-owned manifest | Name the experience, pin artifact identities, and declare semantic capabilities. This is not a MicroBundle descriptor or runtime assembly. |
| Runtime composition | FSM_COS / composition host | Resolve dependencies, load bundles, arbitrate, and build the runtime. A sound MicroBundle must not implement a second composition loop. |

Reference contracts:
- [MicroBundleDomain architecture](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain/blob/development/docs/ARCHITECTURE.md)
- [MicroBundleDomain getting started](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain/blob/development/docs/GETTING_STARTED.md)
- [Stable parameter identity](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain/blob/development/docs/STABLE_PARAMETER_IDS.md)
- [MicroBundleRepository architecture](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleRepository/blob/development/docs/ARCHITECTURE.md)
- [Experience publication boundary](../../docs/PUBLICATION.md)
- [Castle Siege manifest contract example](../CastleSiege/manifest/MANIFEST.md)

## Keep the three identities separate

1. **Semantic identity** describes what a Sound Experience ingredient means (for example, sound.instrument.cello or sound.ensemble.chamber).
2. **MicroBundle identity** is the concrete executable bundle ID and version declared by its MicroBundleDescriptor.
3. **Artifact identity** is the immutable delivery address: bundle ID + version + SHA-256 of the built bytes.

These identities are related, but are not interchangeable. Do not put a semantic string where a numeric bundle ID is required, use a DLL hash as a parameter ID, or treat an Experience manifest as a bundle descriptor.

## First vertical slice: chamber performance

The first slice should exercise composition without prematurely committing to a synthesis engine or audio device API.

### Authored Experience ingredients

- A chamber-performance Experience definition with its own stable Experience ID/key and schema version.
- A short authored phrase/score with explicit tempo and timebase.
- One source choice: an authored sample asset or a declared procedural/live source provider. The definition must state which kind it references.
- One performer role and a small ensemble membership list.
- A venue/receiver configuration that references stable geometry/acoustic capability IDs without claiming a physical result.
- A manifestation policy that says whether the current host can preview the source, display metadata only, or consume a live input.

### Executable MicroBundle candidates

Do not assume every semantic noun needs a separate executable bundle. Start with only the capability boundaries required by the slice:

1. **Sound source provider** — owns the declared source interface and reports whether it supplies authored media, procedural output, or live input. It must not claim device playback merely because it exposes a source.
2. **Sound phrase/content provider** — supplies versioned score/phrase data and timing metadata; it need not synthesize audio.
3. **Ensemble composition provider** — validates role membership and links the selected phrase/source into a performance plan; it must not become a host scheduler or audio callback.
4. **Optional output adapter** — host-specific and only added when a real browser/desktop output path is implemented and tested. Do not make the neutral Experience contract depend on Web Audio, WASAPI, WPF, or a particular device.

An instrument, band, orchestra, or choir can be represented as authored semantic composition data where that is sufficient. A separately executable MicroBundle is justified when it provides a reusable capability contract, independent versioning, or behavior—not merely because the noun exists.

## Candidate semantic identifiers

These are proposed semantic keys only, **not assigned numeric MicroBundle IDs**:

- sound.source.authored-media
- sound.source.procedural
- sound.source.live-input
- sound.content.phrase
- sound.performer.role
- sound.ensemble.chamber
- sound.ensemble.band
- sound.ensemble.orchestra
- sound.ensemble.choir
- sound.environment.venue-reference
- sound.receiver.position
- sound.capture.microphone
- sound.production.mix
- sound.output.playback

Before selecting numeric bundle IDs, inspect the current Experiences bundle-ID inventory and reserve IDs consistently with existing artifacts. Do not invent IDs from these semantic names.

## Configuration and evolution rules

- Use a separately versioned sound configuration payload; the MicroBundleDomain dependency configuration remains opaque bytes.
- If fields are exposed through MicroBundleDefinition, assign stable IDs using the existing field builder. Example namespace: com.thesingularityworkshop.sound.source.sample-rate. Keep IDs stable across label/localization and layout changes.
- Define units and clocks explicitly. Do not mix score beats, wall-clock timestamps, simulation time, and audio sample indices without an explicit mapping.
- Separate authored intent, source media, generated waveform, Physics observations, and actual host playback in data and UI.
- Keep licensed media as repository-delivered artifacts, not embedded as large blobs in the Experience manifest.
- Unknown schema versions and unsupported provider capabilities must fail explicitly rather than be guessed.
- A missing audio device or provider must degrade to a declared metadata-only/unavailable state, not silently report success.

## Acceptance gate

The first executable implementation is ready only when all of the following exist:

- The actual numeric MicroBundle IDs have been checked against the current bundle inventory.
- Concrete IMicroBundle implementations compile against the current MicroBundleDomain package.
- The manifest pins explicit artifact versions and SHA-256 values for publishable builds.
- Contract fixtures prove the same authored definition can be read by browser and desktop consumers without host-specific fields.
- Tests distinguish source availability, generated/sample content, computed Physics metadata, and device playback.
- A small end-to-end preview is backed by a real provider/output implementation, or explicitly labels itself metadata-only.

No audio engine, synthesis provider, source asset, or playback path is claimed implemented by this design document.
