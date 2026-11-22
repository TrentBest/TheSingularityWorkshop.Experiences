![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# Eperstein's Island

> A documentary-inspired, fictional investigative Experience about power, institutional failure, evidence, and accountability.

**Status:** Concept / preforge  
**Implementation status:** First browser interaction prototype added; not yet integrated with Workshop runtime  
**Working title:** Eperstein's Island

Eperstein's Island explores how a complicated public-record archive can become an accessible, navigable investigation without turning real survivors' experiences into spectacle or presenting fictional scenes as historical fact.

The Experience uses a fictional setting, fictional characters, and fictional connective events. Public records may inform the questions and systems explored, but the initial prototype does not reproduce real people as game characters or imply that a person's appearance in a record establishes wrongdoing.

## First visual prototype

[Open The Closed Ledger — browser prototype](../../docs/demos/epersteins-island.html)

The prototype demonstrates a fictional island map, inspectable locations, three synthetic clues, evidence-to-claim linking, editorial classifications, and a finding workflow that requires an unresolved question. Every name, institution, record, date, and event in the demo is invented. The demo is a standalone browser-side interaction proof, not yet connected to FSM_COS, FSM_Foundry, Ontology, DataWarehouse, Renderer, or Visualization.

## The central promise

Visitors should be able to explore a compelling world, follow a trail of clues, and understand why each discovery matters—then inspect the documentary basis, limitations, and uncertainty behind every historical claim.

The experience has three deliberately separate layers:

1. **The Island — systems and incentives.** A fictional institutional ecosystem viewed from the inside. The player studies how status, money, access, reputation, and silence can reinforce one another. This is a critical systems model, not a simulator or tutorial for exploitation, trafficking, concealment, or evading investigators.
2. **The Reckoning — investigation and protection.** A fictional investigative journey focused on corroboration, protecting witnesses, following institutional responsibility, and making a defensible case. The player does not assault, threaten, or punish real people.
3. **The Archive — documentary grounding.** A source-led reference layer that distinguishes what a record says from what it proves, what is alleged, what is established, what is disputed, and what remains unknown.

These layers can share an Experience identity while retaining distinct content and trust boundaries.

## First vertical slice

The first milestone is intentionally small and browser-reviewable:

- One fictional island location with a few inspectable places.
- A small cast of clearly fictional, non-identifiable composite characters.
- One short investigative sequence driven by clues and corroboration.
- An evidence board showing source, date, author/issuer where appropriate, claim, context, and limitations.
- Explicit labels for **Documented**, **Alleged**, **Established**, **Disputed**, **Reconstructed**, and **Unknown**.
- A visible distinction between documentary material and authored fiction.
- A privacy-first policy for any future source ingestion.

The first slice is a concept demonstration, not a claim that the underlying archive has been exhaustively analyzed.

## Design principles

### Evidence before drama

Every factual assertion must be traceable to a source and framed according to what that source can actually establish. A flight log, contact entry, photograph, deposition mention, or address-book entry does not, by itself, prove criminal conduct.

### Fiction must stay visibly fictional

Characters and institutions in the playable world are invented. Composite characters must not be thinly disguised depictions of identifiable real people. Do not combine unique biographical details into a recognizable stand-in. Historical records may inspire broad themes and system questions, but fictional connective scenes must never be presented as documentary events.

### Survivor dignity and privacy are non-negotiable

No graphic abuse depiction, interactive grooming, or playable exploitation. Do not expose survivor identities or reconstruct identifying combinations of names, faces, locations, metadata, or personal details. Any necessary warning depiction is brief, non-graphic, non-interactive, and immediately cuts away.

### Accountability, not vigilantism

The investigative path rewards corroboration, careful reasoning, institutional analysis, witness protection, and transparent correction. It does not direct harassment, threats, doxxing, or violence at real people.

### Corrections are part of the product

The archive layer should support visible correction history, contestation, source replacement, and clear statements of uncertainty. A disclaimer is not a substitute for editorial and legal review.

## Architecture fit

This is an Experience definition, not a new host or a replacement for existing Workshop systems.

- **Ontology:** fictional characters, institutions, locations, events, claims, relationships, and evidence classifications.
- **DataWarehouse / source store:** documentary records, provenance, redaction state, and source-to-claim links, subject to a separately approved ingestion design.
- **StoryBuilder:** authoring and chronology where its stable contracts apply; do not duplicate Story Forge ownership.
- **FSM_COS:** composition of the Experience's required capabilities.
- **FSM_Foundry:** execution and stepping.
- **Renderer / Visualization:** evidence boards, maps, timelines, scenes, and inspectable environments as MicroBundles.
- **Experience manifest:** identity, version, and immutable MicroBundle requirements.

Keep the documentary archive independently useful. It should be possible to build an investigative website, educational exhibit, documentary companion, or game manifestation from the same carefully governed evidence layer.

## Editorial classifications

| Classification | Meaning |
|---|---|
| **Documented** | The cited source directly contains the stated information. |
| **Alleged** | A person or filing makes the claim; it is not thereby established. |
| **Established** | Supported by an authoritative finding or adjudication, accurately scoped. |
| **Disputed** | A material part of the claim is contested by relevant parties or evidence. |
| **Reconstructed** | Creative interpretation or synthesis; not a historical record. |
| **Unknown** | The available material does not establish the answer. |

## Out of scope for this concept branch

- Naming or portraying real people as playable villains, suspects, or targets.
- Importing survivor-sensitive records before a privacy and provenance review.
- Graphic or interactive depictions of sexual abuse or exploitation.
- Mechanics that teach recruitment, grooming, trafficking, concealment, or evasion.
- Public accusations derived merely from proximity, association, or appearance in a document.
- Fundraising, endorsements, or use of third-party names/likenesses without separate review and permission.
- Publishing, deployment, or external promotion as part of this concept milestone.

## Next design documents

- [Experience concept and vertical-slice contract](design/EXPERIENCE_CONCEPT.md)

---

*This is a fictionalized educational and investigative design concept. It is not an assertion about any person beyond claims explicitly and carefully attributed to cited public records.*
