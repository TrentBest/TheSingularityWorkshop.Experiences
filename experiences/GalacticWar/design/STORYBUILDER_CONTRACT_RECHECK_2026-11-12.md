# StoryBuilder Contract Recheck — Galactic War First Playable

**Checked:** 2026-11-12  
**Target:** StoryBuilder `development`  
**Scope:** The Signal Nobody Owns; this is a source-level recheck, not a claim that Galactic War runtime behavior exists.

## What changed in the evidence

The earlier Galactic War crosswalk correctly identified the core authoring primitives, but StoryBuilder `development` has continued to evolve. Its current README reports workspace format 16, immutable publication snapshots, persisted planning-point promotion, append-only visual-asset revisions and review decisions, page-composition layouts, and optional discovery-interaction settings. The branch also describes a local Blazor workbench with host-filesystem workspace persistence. These are development-branch capabilities, not a statement about released packages.

Relevant source contracts inspected:

- `StoryWorkspace` contains `StoryEvent`, `SourceRecord`, `HistoricalClaim`, `ObserverKnowledgeRecord`, `ManuscriptEdition`, and `PublishedBookEdition`.
- `StoryEvent` carries stable identity, title/summary, optional in-world date/period, timeline placement, participants, cause/consequence IDs, source IDs, and canon status.
- `SourceRecord` carries title, source type, location/URI, perspective, and reliability note.
- `HistoricalClaim` carries statement, optional in-world date/period, attribution, confidence, canon status, source IDs, and related entity IDs.
- `ObserverKnowledgeRecord` carries observer/event IDs, knowledge kind, optional acquisition-event ID, source ID, attribution, note, confidence, review status, and a UTC record-creation timestamp.
- `ManuscriptEdition` is the editable structured book draft. `PublishedBookEdition` is an immutable Markdown snapshot with content hash, publication timestamp, source-update timestamp, predecessor edition ID, correction notes, and section/scene/event provenance.

## Important refinement: publication lineage is not broadcast lineage

StoryBuilder now has an explicit immutable **book publication** record and correction chain. This is valuable and should be reused for actual manuscript/book output. It is not automatically a news bulletin or broadcast edition.

A broadcast still needs its own semantics if the first proof demonstrates the need: outlet/channel, in-world publication time, intended audience, distribution scope, claims/evidence included in that specific issue, and append-only correction/supersession links between issues. Do not overload `PublishedBookEdition` merely because it already has a predecessor link and a publication timestamp.

Likewise, a `SourceRecord` identifies provenance and perspective; it does not itself prove that a particular observer received a specific broadcast.

## Contract-level constraints for the first proof

1. **Keep one event identity.** The signal event remains one `StoryEvent`; accounts and portrayals reference it rather than cloning it.
2. **Keep claims attributed.** Claims carry source IDs and related entity IDs. The current `HistoricalClaim` shape does not expose a dedicated `EventId` property, so the authoring proof must verify the intended event association through the supported related-entity/source references and validator behavior. Do not invent a direct event-link API.
3. **Keep observer acquisition causal.** `ObserverKnowledgeRecord.AcquiredAtEventId` is an event reference, not a publication or receipt timestamp. Its `CreatedAtUtc` is record metadata, not an in-world clock.
4. **Keep manuscript portrayal separate.** `ManuscriptSection.EventIds` links a passage to event identities. Manuscript reading order is not a broadcast schedule or an observer-delivery ledger.
5. **Use book snapshots only for books.** `PublishedBookEdition` proves immutable manuscript publication and correction lineage. It does not prove distribution to a station display, ship feed, or individual resident.
6. **Preserve author review.** Proposed records remain proposed; existence is not acceptance. Observer narration continues to fail closed when accepted knowledge or causal availability cannot be established.
7. **Keep runtime occurrence outside authoring possibility.** StoryBuilder's authored event and campaign proposals do not establish what happened in a particular running campaign.

## Updated four-clock interpretation

For Galactic War, retain these distinct clocks:

| Clock | Current StoryBuilder support | Remaining boundary |
|---|---|---|
| World-event time | Event timeline and optional in-world date/period | May be approximate or unresolved |
| Account publication time | `PublishedBookEdition.PublishedAtUtc` for book snapshots | No equivalent in-world broadcast issue timestamp on `SourceRecord` or `HistoricalClaim` |
| Observer acquisition | `AcquiredAtEventId` and causal-history evaluation | Event-scoped causality exists; a dedicated in-world receipt timestamp/receipt record is not shown by this contract |
| Player presentation | Manuscript section sequence and scene ordering | Runtime media scheduling and actual player presentation remain Experience responsibilities |

The table deliberately distinguishes a real timestamp on a book publication snapshot from the still-missing in-world publication time of a broadcast. Do not treat UTC authoring metadata as a fictional-world clock.

## Decision

**Proceed with the first proof using current StoryBuilder primitives, but treat broadcast publication and delivery as an explicit gap—not as a solved feature.** The first proof should create the world event, source perspectives, competing claims, observer knowledge, and scene/manuscript portrayals. It should record the missing broadcast semantics as concrete friction if the author cannot represent the four clocks and correction history without ambiguity.

Only after that proof should StoryBuilder receive a narrow domain extension proposal. Galactic War must not grow a parallel StoryBuilder-like narrative database, and it must not claim the book publication model is a news distribution model.

## Sources inspected on StoryBuilder `development`

- [README and current development capability summary](https://github.com/TrentBest/StoryBuilder/blob/development/README.md)
- [Story domain contract](https://github.com/TrentBest/StoryBuilder/blob/development/docs/STORY_DOMAIN_CONTRACT.md)
- [Timeline authoring and observer model](https://github.com/TrentBest/StoryBuilder/blob/development/docs/TIMELINE_AUTHORING_MODEL.md)
- [StoryWorkspace and domain records](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.Core/Models/StoryWorkspace.cs)
- [Manuscript edition](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.Core/Models/ManuscriptEdition.cs)
- [Published book edition](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.Core/Models/PublishedBookEdition.cs)
- [Observer knowledge record](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.Core/Models/ObserverKnowledgeRecord.cs)
- [Timeline planning point](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.Core/Models/TimelinePlanningPoint.cs)

Branch state can change. Re-verify these contracts before implementing against them or describing them as released behavior.
