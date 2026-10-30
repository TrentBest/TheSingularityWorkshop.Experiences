![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# Experiences — Theory and Learning Path

This learning path explains the domain owned by the Experiences repository: independently authored, identifiable experiences and the contracts by which they can be composed and presented by different hosts.

It is written against the `development` source revision inspected for this chapter set. It deliberately separates implemented contracts from the larger Workshop vision. It does not document Drone Construction or Capsule Clinic implementation; those areas are being handled separately.

## How to read this path

| Chapter | Question | Main idea |
|---|---|---|
| [01 — What Is an Experience?](01-what-is-an-experience.md) | What does this repository own? | An Experience is authored domain meaning, not a window or renderer |
| [02 — Identity, Artifacts, and Composition](02-identity-artifacts-and-composition.md) | How does authored content become something a host can compose? | Keep Experience identity, MicroBundle identity, artifact bytes, and composition distinct |
| [03 — One Experience, Multiple Manifestations](03-one-experience-multiple-manifestations.md) | How can the same Experience appear in different places? | Hosts provide different capabilities while preserving the Experience's identity and semantics |
| [04 — Requirements, Providers, and Authority](04-requirements-providers-and-authority.md) | How are required capabilities described and matched? | Requirements express needs; provider matching is deterministic metadata validation, not authorization |\n| [05 — Room Catalogs, Maturity, and Evidence](05-room-catalogs-maturity-and-evidence.md) | How is an authored visitor journey described and validated? | Room catalogs define meaning and evidence boundaries independently of presentation |

Each chapter is standalone and can be taught, reordered, or extended independently. The sequence moves from domain meaning to immutable identity, manifestation, and capability resolution.

## What this path covers

- Experience identity and authored domain content.
- The distinction between an Experience and its MicroBundle implementation artifacts.
- The intended artifact and composition path through MicroBundleRepository and FSM_COS.
- The separation between semantic requirements, host capabilities, and domain authorization.
- The different roles of WebApp, AnyApp, and future VR manifestations.

## What it does not claim

The repository contains useful parts of the model, but the complete author → capture → immutable artifact → manifest → repository resolution → FSM_COS composition → host presentation path is not yet demonstrated end to end. A diagram of that path is an architectural explanation, not proof that every edge is implemented.

Story Forge belongs to StoryBuilder. General FSM theory belongs to FSM_API. Composition and arbitration theory belong to FSM_COS. Presentation and observation contracts belong to GUI, Renderer, and Visualize. This path links to those owners instead of duplicating their manuals.

## Evidence convention

- **Source-checked** means the named source was inspected at the repository revision recorded below; it does not mean the complete application was run.
- **Verified** is reserved for behavior reproduced by a named test or workflow.
- **Conceptual** identifies an intended model or example that is not evidence of a running integration.

**Source revision inspected:** `e49cdca3ed4e5d0901949d882cb3587f3946646a` (Experiences `development`). Re-check source and CI before treating these chapters as current implementation evidence.

## Further reading

- [Repository overview](../../README.md)
- [Manifestation architecture](../MANIFESTATION_ARCHITECTURE.md)
- [Publication boundary](../PUBLICATION.md)
- [StoryBuilder → Experiences → AnyApp first-run contract](../STORYBUILDER_ANYAPP_FIRST_RUN.md)
- [Semantic interaction requirements](../contracts/SEMANTIC_INTERACTION_REQUIREMENTS.md)
- [Experience vertical-slice roadmap](../ROADMAP.md)
- [FSM_COS documentation and theory standard](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/DOCUMENTATION_STANDARD.md#textbook-style-theory-chapters)
- [FSM_COS](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS), [GUI](https://github.com/TrentBest/TheSingularityWorkshop.GUI), [Renderer](https://github.com/TrentBest/TheSingularityWorkshop.Renderer), and [Visualize](https://github.com/TrentBest/TheSingularityWorkshop.Visualize)

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
