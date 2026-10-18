# History Museum — Experience preforge

**Status:** authored experience plan; not a claim that a museum venue, exhibit runtime, or historical-content bundles are implemented.

The History Museum is a persistent Singularity Universe destination for exploring human history through curated, inspectable exhibits. An exhibit is a **booth within the museum**: a focused encounter with a subject, supported by independently versioned MicroBundles and composed into the visitor's current Experience.

The first planned booth is **Castle Sieges**. It connects historical context to the existing Castle Siege authored scene and, when available, the Physics-owned experiment. It must not merge those authorities into one monolithic “history bundle.”

## Ownership boundaries

| Concern | Owner |
|---|---|
| Museum identity, visitor journey, exhibit placement, curatorial sequencing | Experiences / History Museum |
| Historical claims, source references, chronology, uncertainty and interpretation | Versioned historical-content bundles, authored and reviewed in the Experience domain |
| Castle, terrain, apparatus, stable authored object identities | Castle Siege Experience content |
| Trajectory, target intersection, impact and physical outcomes | TheSingularityWorkshop.Physics |
| Visitor controls, narration, camera, accessible presentation | WebPage / WebApp manifestation |
| Drawing and visual representation | Visualize / Renderer or host presentation |
| Artifact retrieval and immutable version/hash | MicroBundleRepository |
| Dependency resolution and runtime composition | FSM_COS |
| Stateful behavior and lifecycle coordination | FSM_API at the appropriate boundary |

The museum is an Experience, not a new engine, global knowledge database, physics solver, or composition host. The host composes capabilities; it does not decide whether a historical claim is true.

## First vertical slice

1. Establish a museum destination with an exhibit index and a clear “enter booth / return to museum” journey.
2. Present one Castle Sieges booth using versioned historical context, a timeline, a glossary, source notes, and an inspectable scene.
3. Distinguish **documented evidence**, **scholarly interpretation**, **uncertain or disputed claims**, and **illustrative reconstruction** in both content and UI.
4. Link historical objects to the Castle Siege authored content through stable semantic IDs, not duplicate scene definitions.
5. Offer a clearly labeled experiment only when it consumes actual Physics outputs. A historical explanation can exist before the solver integration is complete.
6. Preserve exhibit/content versions and provenance so a visitor's result or lesson can be reproduced against the exact material shown.

## Initial booth contract

See [Castle Sieges exhibit design](exhibits/CastleSieges/EXHIBIT_DESIGN.md) for the proposed content-bundle map, visitor flow, learning objectives, provenance rules, and implementation gates.

## Authoring rules

- A booth is a composition of focused capabilities, not a single all-purpose bundle.
- Historical context is independent of visual assets and simulation. The same content should support a text exhibit, guided tour, diagram, classroom activity, or future interactive reconstruction.
- Every factual claim should carry source/provenance metadata appropriate to its strength. Do not invent citations or present a plausible reconstruction as direct historical evidence.
- Dates and regional labels must make scope explicit; “medieval” is not one uniform culture, technology, or century.
- Explain siege warfare through logistics, engineering, fortification, disease, diplomacy, civilian experience, and consequences—not just weapons and spectacle.
- Use stable semantic IDs for people, places, events, objects, claims, and sources. Display names may change without breaking references.
- Do not claim a historical-content bundle, persistent museum, guided narration, or Physics-backed exhibit is implemented until its artifacts and runtime path exist and are tested.

## Related source of truth

- [Castle Siege Experience](../CastleSiege/README.md)
- [Castle Siege MicroBundle growth plan](../CastleSiege/design/MICROBUNDLE_GROWTH_PLAN.md)
- [Singularity Universe model in WebPage](https://github.com/TrentBest/WebPage/blob/development/docs/SINGULARITY_UNIVERSE.md)
- [MicroBundleDomain](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain)
- [MicroBundleRepository](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleRepository)
