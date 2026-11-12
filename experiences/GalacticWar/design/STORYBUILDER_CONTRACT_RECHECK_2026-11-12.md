# StoryBuilder Contract Recheck — Galactic War First Playable

**Checked:** 2026-11-12  
**Target:** StoryBuilder `development`  
**Scope:** The Signal Nobody Owns. This is a source-level contract review, not a claim that Galactic War runtime behavior exists.

## Current StoryBuilder primitives confirmed in source

The current `StoryWorkspace` owns collections for timelines, planning points, observer knowledge, manuscripts, published editions, characters, places, scenes, historical claims, events, campaigns, participant actions, sources, and continuity issues.

- `StoryEvent`: stable event identity, title/summary, optional in-world date/period, timeline placement, participants, cause/consequence IDs, source IDs, and canon status.
- `SourceRecord`: title, source type, location/URI, perspective, and reliability note.
- `HistoricalClaim`: statement, optional in-world date/period, attribution, confidence, canon status, source IDs, and `RelatedEntityIds`.
- `ObserverKnowledgeRecord`: `ObserverId`, `EventId`, knowledge kind, optional `AcquiredAtEventId`, optional `SourceId`, attribution, note, confidence, review status, and `CreatedAtUtc`. The observer is a stable character ID; the projection service verifies that the character exists.
- `ManuscriptEdition` / `ManuscriptSection`: editable ordered prose; sections can reference scenes and events.
- `PublishedBookEdition`: immutable Markdown snapshot with content hash, publication timestamp, source-update timestamp, predecessor edition ID, correction notes, and section/scene/event provenance.

## Corrections and important distinctions

The previous pass incorrectly described `HistoricalClaim` as lacking a direct event association. The source shows that its `RelatedEntityIds` can reference the event ID, and `StoryWorkspaceValidator` verifies those IDs against all known workspace records. Use that supported relationship; do not invent an `EventId` property on the claim.

The observer identity gap identified in the immediately preceding revision was itself incorrect: `ObserverKnowledgeRecord.ObserverId` is present. `ObserverKnowledgeEvaluator.Evaluate` filters by both observer and event; `ObserverKnowledgeProjectionService.Project` validates that the observer ID resolves to a character and projects only that character's accepted, causally available knowledge. Multiple named NPC knowledge states can therefore be represented directly with stable character IDs. This is a useful first-proof capability, not a reason to add a parallel observer registry.

StoryBuilder's immutable `PublishedBookEdition` is a book publication snapshot. It is not automatically a news bulletin or broadcast issue. A broadcast needs explicit outlet/channel, in-world publication time, intended audience, distribution scope, issue-specific claim/evidence snapshot, and append-only correction/supersession lineage. A `SourceRecord` describes provenance and perspective; it does not prove that a specific observer received a particular issue.

## Contract-level constraints for the first proof

1. **One event identity:** the signal event remains one `StoryEvent`; reports and portrayals reference it rather than cloning it.
2. **Attributed claims:** `HistoricalClaim.RelatedEntityIds` links the claim to the event; `SourceIds` identifies supporting accounts. Validator checks both source and related-record references.
3. **Observer knowledge:** `ObserverKnowledgeRecord.ObserverId` identifies the character, `EventId` identifies the subject event, `AcquiredAtEventId` identifies a causal acquisition event, and `SourceId` identifies the account. It has no dedicated in-world receipt timestamp; `CreatedAtUtc` is record metadata.
4. **Causal availability:** StoryBuilder's observer evaluator requires accepted knowledge and checks acquisition-event reachability through cause links. Proposed knowledge remains unresolved; missing knowledge fails closed.
5. **Separate portrayal:** `ManuscriptSection.EventIds` and `SceneBeat.EventIds` link prose/scene beats to the event. Neither is a broadcast schedule or receipt ledger.
6. **Book snapshots stay books:** use `PublishedBookEdition` for immutable manuscript publication and correction lineage only.
7. **Authored possibility is not runtime occurrence:** StoryBuilder records and campaign proposals do not establish what happened in a particular running Galactic War campaign.

## Four clocks: what is and is not represented

| Clock | Current support | Remaining boundary |
|---|---|---|
| World-event time | `StoryEvent.InWorldDateOrPeriod`, timeline placement, and causal links | Date may be approximate or unresolved |
| Account publication time | `PublishedBookEdition.PublishedAtUtc` for a book snapshot | No in-world publication timestamp for a broadcast issue |
| Observer acquisition | `ObserverId`, `AcquiredAtEventId`, and causal-history evaluation | No dedicated in-world receipt timestamp on `ObserverKnowledgeRecord` |
| Player presentation | Manuscript section sequence and scene/beat order | Runtime media scheduling and actual player presentation remain Experience responsibilities |

Do not treat UTC record-creation metadata as a fictional-world clock.

## First proof: use existing records, expose friction honestly

The first StoryBuilder workspace should contain:

- one timeline and one proposed/accepted world event for the anomalous signal;
- source records for the official bulletin, independent survey account, and later correction;
- separate historical claims, each linked to the same event through `RelatedEntityIds` and to its account through `SourceIds`;
- observer-knowledge records for the knowledge states the current API can genuinely express, with acquisition events linked causally;
- a scene showing two NPCs interpreting the reports differently;
- manuscript sections that portray the initial bulletin and later correction while referencing the same event ID;
- a continuity issue documenting any observer identity or publication/delivery semantics that cannot be represented faithfully.

The proof should create stable character records for the NPC observers, then create separate accepted/proposed knowledge records keyed by each character's `ObserverId`. Exercise `ObserverKnowledgeEvaluator` and `ObserverKnowledgeProjectionService` for distinct observer IDs and causal anchor events. Record the remaining limitation: acquisition is event-causal, not an explicit fictional-world timestamp or broadcast-receipt record.

## Decision

**Proceed with the first proof using current StoryBuilder primitives for events, sources, claims, causal knowledge, scenes, and manuscript portrayal.** Treat broadcast publication/delivery and a dedicated in-world acquisition timestamp as explicit gaps to validate in the host/API workflow. Only propose narrow StoryBuilder extensions after concrete proof friction is recorded.

Do not add a Galactic War-specific parallel narrative database. Do not overload book publication records into broadcast issues.

## Sources inspected on StoryBuilder `development`

- [Story domain contract](https://github.com/TrentBest/StoryBuilder/blob/development/docs/STORY_DOMAIN_CONTRACT.md)
- [Timeline authoring and observer model](https://github.com/TrentBest/StoryBuilder/blob/development/docs/TIMELINE_AUTHORING_MODEL.md)
- [StoryWorkspace and records](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.Core/Models/StoryWorkspace.cs)
- [Observer knowledge record](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.Core/Models/ObserverKnowledgeRecord.cs)
- [Observer knowledge evaluator](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.Core/Services/ObserverKnowledgeEvaluator.cs)
- [Observer knowledge projection](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.Core/Services/ObserverKnowledgeProjectionService.cs)
- [Manuscript edition](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.Core/Models/ManuscriptEdition.cs)
- [Timeline planning point](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.Core/Models/TimelinePlanningPoint.cs)
- [Workspace validator](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.Core/Services/StoryWorkspaceValidator.cs)

Re-verify branch state before implementing against these contracts or describing them as released behavior.
