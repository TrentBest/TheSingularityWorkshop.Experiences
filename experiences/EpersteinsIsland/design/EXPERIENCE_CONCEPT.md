# Eperstein's Island — Experience Concept

**Document type:** Preforge design contract  
**Status:** Draft for review  
**Experience name:** Eperstein's Island  
**Initial implementation target:** One navigable, browser-reviewable investigative slice

## 1. Experience thesis

Transform the experience of reading a fragmented, difficult documentary archive into a structured act of discovery.

The visitor moves through a fictional environment, encounters a question, examines a clue, tests it against other evidence, and sees both the conclusion and its limits. The world supplies emotional and spatial context; the archive remains the authority for historical claims.

The design should make careful investigation engaging—not make abuse entertaining.

## 2. Three perspectives

### 2.1 The Island — institutional systems

A fictional island contains places and institutions whose relationships reveal how influence and institutional failure can become mutually reinforcing.

The player can inspect:
- Public-facing spaces and their reputational roles.
- Financial and administrative structures at a high, non-operational level.
- Relationships between fictional institutions.
- Warning signs, conflicts of interest, and ignored accountability obligations.

This perspective is analytical. It must not provide practical methods for exploiting people, recruiting victims, moving victims, hiding evidence, or evading law enforcement. It does not reward successful abuse or concealment.

### 2.2 The Reckoning — investigation

The player takes the role of a fictional investigator or investigative team. Progress comes from:
- Finding records.
- Comparing independent accounts.
- Establishing chronology.
- Separating direct observation from inference.
- Protecting witnesses and sensitive information.
- Identifying institutional decisions and failures.
- Publishing a conclusion with confidence and limitations stated.

No real person is a combat target. The dramatic stakes come from evidence, obstruction as a fictional narrative theme, time pressure, and the responsibility to avoid harming witnesses.

### 2.3 The Archive — documentary layer

The archive is not a prop database. It is an independently navigable evidence layer with:
- Stable source identifier.
- Source title and issuing body/author, where known.
- Publication or record date, where known.
- Original public source location and retrieval metadata.
- Page/line/timecode locator where possible.
- Exact excerpt only when necessary and legally appropriate.
- A neutral summary.
- Claims supported and claims not supported.
- Privacy/redaction status.
- Editorial classification.
- Correction and review history.

A record can support a narrow statement without supporting the broader inference a player might initially make.

## 3. Fiction and historical boundaries

All initial locations, characters, institutions, dialogue, and events are fictional.

Composite characters must be built from broad roles and motivations, not assembled from distinctive personal details that identify a real person. Do not create a one-to-one mapping between a real individual and a fictional character. Do not imply that a fictional scene occurred in the real world.

When documentary sources are eventually introduced:
- Keep the original record separate from authored scene text.
- Link every documentary claim to the exact supporting source location.
- Label any dramatization as reconstructed.
- Avoid reproducing sensitive personal details that are not necessary to understand the public-interest issue.
- Do not infer guilt from association, presence in a record, or uncorroborated mention.
- Provide a route to flag errors and request review.

## 4. Evidence model

The initial conceptual model uses these entities:

| Entity | Purpose |
|---|---|
| SourceRecord | A public record or other reviewed source, with provenance and privacy metadata. |
| SourcePassage | A page, line, timestamp, or bounded portion of a source. |
| Claim | A narrowly worded proposition that can be evaluated. |
| ClaimSupport | A typed link between a source passage and a claim, including support/contradiction/context. |
| PersonReference | A privacy-governed reference; never automatically a game character. |
| Institution | A real or fictional organization, clearly typed and scoped. |
| Event | A dated or bounded occurrence with explicit certainty and source links. |
| Relationship | A typed, time-bounded relationship; association is not culpability. |
| RedactionDecision | What was withheld, why, by whom/review process, and when it must be revisited. |
| EditorialRevision | A correction, reclassification, or changed interpretation with history. |

The model should preserve uncertainty. Missing information must not be silently filled with invented facts.

### Claim classifications

- Documented
- Alleged
- Established
- Disputed
- Reconstructed
- Unknown

Classification belongs to each claim, not to an entire person or source. One document can contain multiple claims with different statuses.

## 5. First playable/reviewable sequence

**Working sequence:** The Closed Ledger

1. The visitor arrives at a fictional administrative building on the island.
2. An inspectable scene presents a missing-accountability question—not a claim about a real person.
3. The visitor discovers a fictional ledger fragment and an associated institutional memo.
4. The evidence board asks the visitor to distinguish what each item directly says from what it does not establish.
5. A second, independent clue changes the chronology or narrows the possible interpretation.
6. The visitor assembles a short finding with a confidence level and explicit unresolved questions.
7. The Archive panel explains the source-to-claim relationship using fictional sample records in this prototype.

The prototype should use unmistakably fictional sample evidence until a separately reviewed public-source dataset is approved. It should demonstrate the evidence workflow without copying survivor-sensitive material.

## 6. Interaction contract

The first slice needs only a few meaningful actions:

- InspectLocation
- DiscoverClue
- OpenEvidence
- CompareClaims
- LinkEvidenceToClaim
- MarkAsUnresolved
- SubmitFinding
- OpenSourceContext

Every action should be explicit and auditable in the local demonstration state. Do not introduce arbitrary command execution, unrestricted external navigation, or unreviewed data ingestion.

## 7. Presentation direction

The tone is tense, investigative, and serious—not lurid.

Visual language:
- A fictional coastal island and institutional architecture.
- Restrained color and typography.
- A spatial scene paired with an evidence-board interface.
- Timeline and relationship views that expose uncertainty instead of hiding it.
- Clear visual separation between in-world fictional artifacts and documentary source records.

The first presentation can be a 2D browser experience. VR is a possible later manifestation, not a requirement for the proof of concept.

## 8. Privacy, safety, and editorial gates

Before any real archive material is ingested:
1. Identify the exact public source and its access/use terms.
2. Determine whether it contains survivor, witness, minor, or unrelated private-person information.
3. Remove identifying details and combinations, not merely substitute names.
4. Preserve provenance for internal review without exposing sensitive originals in the public experience.
5. Have editorial and legal review assess the proposed use and wording.
6. Publish only the minimum necessary material, with correction mechanisms.

Never make a survivor's identity, location, or trauma a collectible, achievement, puzzle answer, or navigational reward.

No interactive grooming, trafficking, abuse, or concealment mechanics. Any warning depiction must be brief, non-graphic, non-interactive, and immediately cut away.

## 9. Acceptance criteria for the concept slice

The concept is ready for a first visual implementation when:
- The visitor can identify the three layers and their different authority.
- The fictional setting and characters cannot reasonably be mistaken for documentary depictions.
- At least three clues can be inspected and linked to claims.
- At least one clue is insufficient on its own, and the interface communicates that limitation.
- The visitor can produce a finding that preserves an unresolved question.
- Every sample source is visibly marked as fictional.
- No survivor-sensitive real-world material is present.
- The experience remains understandable without VR or a desktop companion.

## 10. Deliberately deferred

- Full archive ingestion and search.
- Real-person identity graphs.
- Public allegations or named-person profiles.
- Live multi-user sessions.
- VR manifestation.
- External fundraising or partnership integrations.
- Any public release pending editorial, privacy, and legal review.

## 11. Architectural ownership

Experiences owns this Experience definition, its fictional authored content, and its manifest requirements. StoryBuilder remains the owner of general story-authoring and Story Forge semantics. Domain repositories own their respective capabilities. Renderer and Visualization are consumed as MicroBundles; they are not redefined as NuGet dependencies here.

The Experience should remain portable across WebApp, AnyApp, and future VR manifestations without putting host-specific rendering details into its semantic model.
