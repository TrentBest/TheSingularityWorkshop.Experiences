# Sound Experiences — MicroBundle Design

**Status:** proposal and authored taxonomy; no sound-producing or sound-consuming MicroBundle is claimed implemented by this document.

**Purpose:** define reusable sound-related Experience ingredients in this repository so future WebPage, AnyApp, and other manifestations can assemble the same concepts instead of inventing separate browser and desktop versions.

An Experience defines *what* is being experienced. A MicroBundle is a reusable, versioned capability/content unit that can be composed into an Experience. This document is an authored design contract, not a MicroBundleDomain descriptor, executable provider, audio engine, or Physics result.

**Contract mapping:** [Map the design to the existing MicroBundleDomain and repository contracts](MICROBUNDLE_CONTRACT_MAPPING.md). This is still a design document; no executable sound bundle is claimed.

## Composition examples

- **Instrument → section → ensemble → performance:** a cello instrument can belong to a cello section, which participates in an orchestra performance.
- **Voice → choir → performance:** individual voice parts and singers can be composed into a choir; a choir can perform alone or with an orchestra.
- **Band → venue → audience:** a band performance can be placed in a room, club, outdoor stage, or arena and observed by different listeners.
- **Instrument → microphone → recording/mix:** a microphone consumes a source signal at a defined position; recording and mixing are separate downstream capabilities.
- **Physics event → acoustic source → propagation → listener:** a supported physical event may provide a source excitation; Physics models only the propagation/reception capabilities that are actually implemented, and the manifestation chooses an output backend.

These are compositional relationships, not requirements that every bundle depend directly on every other bundle.

## Candidate bundle families

| Family | Example bundles | Responsibility |
|---|---|---|
| Source / instrument | strings, brass, woodwind, percussion, keyboard, plucked/electronic instrument, human voice | Source identity, controllable parameters, and a declared signal-generation or signal-asset contract |
| Performer / section | musician, instrument section, voice part, choir section | Performer identity, role, arrangement membership, and optional Gestures choreography |
| Ensemble | band, orchestra, choir, chamber group, mixed ensemble | Membership, role/section layout, ensemble configuration, and performance requirements |
| Musical content | score, part, arrangement, tempo map, tuning, set list | Authored content and timing; not physical propagation or host playback |
| Performance | rehearsal, concert, solo, ensemble performance, conducting | Time-bounded orchestration and performer/event relationships; not a new audio engine |
| Acoustic environment | room, hall, stage, outdoor venue, acoustic boundary/material profile | Geometry/material references and declared acoustic capabilities; Physics owns validated physical calculations |
| Capture / playback | microphone, pickup, recording session, monitor, speaker, mix bus | Signal capture/routing/output contract; host-specific devices stay in the manifestation or adapter |
| Receiver / audience | listener, audience group, monitoring position, hearing profile | Receiver location/orientation and supported observation/perception configuration |
| Production | recording, mixing, mastering, live sound, spatial-audio layout | Production graph and authored settings, with signal processing delegated to the appropriate implementation |
| Event-driven sound | impact, collision, fracture, motor, alarm, footsteps | Maps a supported event into a declared excitation source; an event label alone is not a waveform |

Treat these as candidate semantic families, not a mandate to create one package for every noun. Split a family into separate MicroBundles when it has a reusable contract, independent versioning, or a meaningful capability boundary.

## Authority and data boundaries

- **Experiences:** authored identity, bundle selection, ensemble membership, score/arrangement, performance plan, and user journey.
- **MicroBundleDomain / repository:** descriptor/provider contracts, dependency declarations, versioning, artifact identity, and delivery. The Experience manifest is not itself a MicroBundle descriptor.
- **Physics:** physical source/propagation/receiver models and reproducible results only where implemented and validated. Do not depend on Experiences, WebPage, AnyApp, audio devices, Visualize, or Renderer.
- **Gestures:** reusable performer and crew motion procedures; it does not become the performance scheduler or audio synthesizer.
- **WebPage / AnyApp / future VR:** user interaction, manifestation-specific device integration, rendering, playback, and controls. They should consume the same authored Experience and versioned data.
- **FSM_COS:** composition and capability arbitration, not a replacement for the host's experience lifecycle or an audio callback loop.

A performance plan may declare intent and timing, but must not claim a performer actually completed an action unless the owning runtime supplies that state. Likewise, a physical event can trigger an acoustic source only through a declared event-to-source mapping; the mapping does not prove that a waveform was synthesized.

## Minimum candidate contracts

Before any bundle is considered executable or publishable, define as appropriate:

1. Stable semantic ID, schema/version, provenance, and dependencies.
2. Explicit role and ownership (source, performer, ensemble, content, environment, receiver, capture, or output).
3. Timebase and units for tempo, event timestamps, positions, orientation, levels, and signal sample rate where applicable.
4. Whether it supplies authored media, procedural signal generation, control metadata, or only a semantic description.
5. Input/output schema, determinism expectations, capability requirements, and behavior when a provider or device is unavailable.
6. Clear separation between authored intent, generated waveform, Physics observations, and device playback.
7. A small reproducible example plus schema/contract tests before integration into a host.

Do not force every instrument to be procedurally synthesized. A source may use a licensed/authored sample, a synthesis provider, or a live device input; the contract must say which. Do not put raw audio blobs into a manifest when the artifact repository is the proper delivery mechanism.

## First vertical slice recommendation

Start with an authored, renderer/audio-backend-independent Experience definition for a small chamber performance:

1. One instrument source or source asset, one performer role, a short score/phrase, and a declared tempo/timebase.
2. One small ensemble composition that can later scale to a band, orchestra, or choir.
3. One venue/receiver configuration that references geometry and acoustic capability by stable ID.
4. A manifestation that can preview the authored composition and label which parts are authored media, live playback, or computed Physics evidence.
5. A versioned contract fixture and tests that prove browser and desktop consumers can read the same definition.

This first slice should not wait for a complete room-acoustics solver. Conversely, playback of an authored sample must not be presented as output generated by Physics. The existing Physics first-order room-reflection model produces reflected-path metadata only; it does not synthesize, mix, or render room audio.

## Cross-repository handoff

- Physics owns physical acoustic models and their evidence; see [Physics acoustic world model](https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/docs/ACOUSTIC_WORLD_MODEL.md) and [Physics coordination log](https://github.com/TrentBest/TheSingularityWorkshop.Physics/blob/development/docs/COORDINATION.md).
- WebPage owns the browser manifestation and current Singularity Studios presentation; it should eventually consume the shared Experience definition rather than privately defining an orchestra/choir schema.
- AnyApp is the desktop counterpart and should consume the same versioned Experience/data contracts.
- Gestures may supply performer choreography but does not own ensemble semantics, score content, or acoustic calculations.

**Next design gate:** check the existing Experiences bundle-ID inventory, then build the smallest real source/content provider against the current MicroBundleDomain contract. Keep this proposal in design status until concrete implementations, tests, immutable artifact identities, and a browser/desktop integration path exist.
