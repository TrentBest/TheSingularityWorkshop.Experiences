# Galactic War — First Playable Proof: The Signal Nobody Owns

**Status:** implementation-ready vertical-slice specification. This defines a proof target; it does not claim the runtime or StoryBuilder integration is implemented.

## Purpose

Prove that a player can encounter a distant first-contact story through ordinary life, investigate competing accounts, and discover that the underlying event is not identical to any report about it.

The proof succeeds only if the world can tell a coherent story without making the player its cause, without granting characters knowledge they have not acquired, and without rewriting history when an account is corrected.

## Player-facing slice

The player begins in an inhabited frontier-adjacent station. They can ignore the story and continue their ordinary activity.

If they pay attention, they can:

1. See a public display announce an unusual signal detected by a remote observatory.
2. Hear two nearby people interpret the announcement differently.
3. Inspect an official bulletin and an independent account that cite overlapping evidence but make different claims.
4. Later encounter a correction that changes the source's interpretation without erasing the original publication.
5. Compare the accounts, inspect the available evidence, and identify at least one claim that remains unresolved.
6. Decide whether to pursue a lead, save the material, share it, or walk away.

The proof ends with a credible invitation to investigate—not with a forced mission acceptance or a complete explanation of the mystery.

## Canonical event and account model

Use StoryBuilder's existing records first. Exact field names and required values must be taken from the current StoryBuilder development contracts during implementation; this document intentionally specifies semantics rather than inventing a parallel schema.

| Concern | StoryBuilder authoring record | Required meaning |
| --- | --- | --- |
| Underlying detection | `StoryEvent` | Stable event identity; unresolved origin and meaning remain unresolved |
| Temporal context | `TimelineDefinition`, `TimelinePlanningPoint` | Approximate placement and alternatives are authoring structure, not proof of occurrence |
| Official observatory | `SourceRecord` | Named institution, access, perspective, reliability limits |
| Independent outlet | `SourceRecord` | Separate provenance and editorial perspective |
| Claims about the signal | `HistoricalClaim` | Each claim attributed to a source and linked to evidence/event context |
| People who learn of reports | `ObserverKnowledgeRecord` | Observer, acquisition path/time, source, confidence, and knowledge status |
| Public display / lounge scene | `SceneRecord`, `SceneBeat` | A portrayal of information available at that place and time |
| Authored report presentation | `ManuscriptEdition`, `ManuscriptSection` | Reusable authored content; not runtime delivery evidence |
| Runtime occurrence / player action | `TimelineEventRecord`, `ParticipantAction`, `CampaignRecord` | Actual playthrough observations and choices, kept distinct from authored possibilities |

If the current StoryBuilder contract cannot represent a required semantic distinction, record the concrete friction and propose the narrowest extension there. Do not create a Galactic War-owned duplicate narrative store.

## The four clocks

Every authored and runtime representation must keep these times distinct:

- **Event time:** when the signal was emitted, detected, or materially changed.
- **Publication time:** when a source issued a particular account or revision.
- **Acquisition time:** when an individual observer received or learned that account.
- **Presentation time:** when the player encountered the content in this playthrough.

A delayed station feed may present an old report to the player after a character has already received a newer correction. Neither timestamp should be silently substituted for another.

## Required source sequence

Create at least three immutable authored account states:

### A. Initial bulletin

- Source: remote observatory or its authorized public-relations office.
- Claim: a structured signal has been detected; preliminary analysis suggests a particular origin or geometry.
- Evidence: only the instrument data and analysis available at publication time.
- Uncertainty: explicit; the source must not claim confirmed contact.
- Publication: a stable initial edition.

### B. Independent account

- Source: independent scientific journalist, research collective, or unaffiliated observatory.
- Claim: agrees that a structured signal exists but challenges at least one inference in the official bulletin.
- Evidence: independently obtained data or a clearly attributed re-analysis.
- Perspective: disclose access limitations and any uncertainty.
- Publication: separate source and edition; do not merge its claims into the official account.

### C. Correction / update

- Source: the original publisher, or a clearly identified correcting source.
- Claim: corrects or narrows at least one material statement from the initial bulletin.
- Lineage: explicitly references the account it corrects.
- Preservation: the initial edition remains retrievable with its original publication time and claims.
- Scope: state what changed, why it changed, and what remains unknown.

A correction changes the information landscape. It does not travel backward in time and make the original publication disappear.

## Observer knowledge and delivery rules

At minimum, define:

- one station resident who saw the initial public display;
- one resident who heard a second-hand retelling but did not see the original;
- one station worker whose job gives access to a delayed or restricted feed;
- the player, whose knowledge depends on actual encounters.

For each observer, author or record only the knowledge justified by the acquisition path. A rumor is not equivalent to direct observation. Seeing a report establishes knowledge that the report was made; it does not establish the report's claims as fact.

A report that has not reached a character cannot become that character's common knowledge. A report authored but not distributed cannot be treated as received. A player who has not inspected the correction must not be presumed to know it.

The Galactic War runtime owns delivery, channel availability, local presentation, and playthrough receipt. StoryBuilder owns the authored source, claim, scene, and observer-knowledge semantics.

## Minimum validation gates

The proof is not accepted until all of the following pass:

1. **Identity:** all accounts refer to the same underlying signal event without collapsing into one account.
2. **Provenance:** every material claim identifies its source; evidence and uncertainty are inspectable.
3. **Append-only correction:** the original edition remains unchanged and retrievable after correction.
4. **Temporal integrity:** event, publication, acquisition, and player-presentation times remain separately queryable.
5. **Knowledge boundary:** an observer cannot state an unreceived report or later correction as known.
6. **Attribution:** a character repeating an unverified claim frames it as a report, rumor, inference, or belief as appropriate.
7. **Possibility boundary:** an authored alternative or planned event is not presented as an occurred runtime fact.
8. **Player independence:** the signal and initial reports exist before the player's first interaction.
9. **No forced path:** ignoring the story does not break ordinary station activity or falsely mark the player as informed.
10. **Contradiction preservation:** conflicting accounts remain visible as competing claims until evidence and an explicit adjudication policy justify a change in status.

A failure must be explicit and fail closed. Missing provenance or knowledge is not permission to invent a confident narration.

## Runtime presentation acceptance

The minimum presentation can be modest: one public display, two NPC conversations, one inspectable terminal/archive, and a later correction. These are different surfaces over the same authored information—not four separate narrative truths.

The player-facing interaction should make source, publication time, and correction status discoverable without requiring a lore encyclopedia. The player should be able to compare accounts and leave.

## Implementation order

1. Inspect the exact StoryBuilder `development` contracts and existing validation/evaluation paths.
2. Author the event, sources, claims, scenes, and observer knowledge using existing records.
3. Run StoryBuilder validation against the ten gates above; document any contract gaps with reproducible examples.
4. Only then implement the thinnest Galactic War runtime adapter for feed delivery and diegetic presentation.
5. Record runtime receipts and player choices separately from authored content.
6. Re-run the proof with at least two player paths: ignores the story; investigates and encounters the correction.

## Evidence to attach to the proof

The implementation is demonstrated by:

- the authored StoryBuilder workspace or export;
- validation output for each gate;
- a timeline showing all four clocks independently;
- an observer-knowledge matrix before and after each delivery;
- the original and corrected editions side by side;
- a short capture or walkthrough of the two player paths;
- a list of any StoryBuilder API gaps, each tied to a failed step rather than speculative feature requests.

## Out of scope for this proof

- A universal news/broadcast platform.
- Full galaxy-wide media simulation.
- Automatic truth adjudication.
- Procedural journalism or generated prose at runtime.
- A complete first-contact campaign.
- Any claim that a planned event occurred merely because it was authored.

The proof is deliberately narrow. If this small story preserves provenance, time, observer knowledge, correction history, and player independence, it establishes a trustworthy foundation for larger living campaigns.
