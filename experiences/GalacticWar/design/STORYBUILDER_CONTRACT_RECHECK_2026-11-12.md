# StoryBuilder Contract Recheck — Galactic War First Playable

**Checked:** 2026-11-12  
**Target:** StoryBuilder `development`  
**Scope:** The Signal Nobody Owns. This is a source-level contract review, not a claim that Galactic War runtime behavior exists.

## Current StoryBuilder primitives confirmed in source

The current `StoryWorkspace` owns collections for timelines, planning points, observer knowledge, manuscripts, published editions, characters, places, scenes, historical claims, events, campaigns, participant actions, sources, and continuity issues.

- `StoryEvent`: stable event identity, title/summary, optional in-world date/period, timeline placement, participants, cause/consequence IDs, source IDs, and canon status.
- `SourceRecord`: title, source type, location/URI, perspective, and reliability note.
- `HistoricalClaim`: statement, optional in-world date/period, attribution, confidence, canon status, source IDs, and `RelatedEntityIds`.
- `ObserverKnowledgeRecord`: `EventId`, knowledge kind, optional `AcquiredAtEventId`, optional `SourceId`, attribution, note, confidence, review status, and `CreatedAtUtc`. **There is no dedicated observer/character identity field in this record.**
- `ManuscriptEdition` / `ManuscriptSection`: editable ordered prose; sections can reference scenes and events.
- `PublishedBookEdition`: immutable Markdown snapshot with content hash, publication timestamp, source-update timestamp, predecessor edition ID, correction notes, and section/scene/event provenance.

## Corrections and important distinctions

The previous pass incorrectly described `HistoricalClaim` as lacking a direct event association. The source shows that its `RelatedEntityIds` can reference the event ID, and `StoryWorkspaceValidator` verifies those IDs against all known workspace records. Use that supported relationship; do not invent an `EventId` property on the claim.

The more significant gap is observer identity: `ObserverKnowledgeRecord` has an `EventId` and acquisition-event reference, but no `ObserverId` or `CharacterId`. Attribution and note are text, not stable observer identity. The existing evaluator and projection may apply a host-level observer context, but the record itself does not persist a first-class observer key. Inspect those APIs before deciding whether the first proof can represent multiple named NPC knowledge states without an external mapping.

StoryBuilder's immutable `PublishedBookEdition` is a book publication snapshot. It is not automatically a news bulletin or broadcast issue. A broadcast needs explicit outlet/channel, in-world publication time, intended audience, distribution scope, issue-specific claim/evidence snapshot, and append-only correction/supersession lineage. A `SourceRecord` describes provenance and perspective; it does not prove that a specific observer received a particular issue.

## Contract-level constraints for the first proof

1. **One event identity:** the signal event remains one `StoryEvent`; reports and portrayals reference it rather than cloning it.
2. **Attributed claims:** `HistoricalClaim.RelatedEntityIds` links the claim to the event; `SourceIds` identifies supporting accounts. Validator checks both source and related-record references.
3. **Observer knowledge:** `ObserverKnowledgeRecord.EventId` identifies the subject event; `AcquiredAtEventId` identifies a causal acquisition event; `SourceId` identifies the account. It does not currently store a stable observer ID or an in-world receipt timestamp.
4. **Causal availability:** StoryBuilder's observer evaluator requires accepted knowledge and checks acquisition-event reachability through cause links. Proposed knowledge remains unresolved; missing knowledge fails closed.
5. **Separate portrayal:** `ManuscriptSection.EventIds` and `SceneBeat.EventIds` link prose/scene beats to the event. Neither is a broadcast schedule or receipt ledger.
6. **Book snapshots stay books:** use `PublishedBookEdition` for immutable manuscript publication and correction lineage only.
7. **Authored possibility is not runtime occurrence:** StoryBuilder records and campaign proposals do not establish what happened in a particular running Galactic War campaign.

## Four clocks: what is and is not represented

| Clock | Current support | Remaining boundary |
|---|---|---|
| World-event time | `StoryEvent.InWorldDateOrPeriod`, timeline placement, and causal links | Date may be approximate or unresolved |
| Account publication time | `PublishedBookEdition.PublishedAtUtc` for a book snapshot | No in-world publication timestamp for a broadcast issue |
| Observer acquisition | `AcquiredAtEventId` plus causal-history evaluation | No first-class observer key or dedicated in-world receipt timestamp on `ObserverKnowledgeRecord` |
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

The proof must not fake named-observer identity by stuffing names into free text and then claiming the domain has stable observer identity. If the current host projection supports explicit observer context, demonstrate that separately and record the limitation.

## Decision

**Proceed with the first proof using current StoryBuilder primitives for events, sources, claims, causal knowledge, scenes, and manuscript portrayal.** Treat stable observer identity and broadcast publication/delivery as explicit gaps to validate in the host/API workflow. Only propose narrow StoryBuilder extensions after concrete proof friction is recorded.

Do not add a Galactic War-specific parallel narrative database. Do not overload book publication records into broadcast issues.

## Sources inspected on StoryBuilder `development`

- [Story domain contract](https://github.com/TrentBest/StoryBuilder/blob/development/docs/STORY_DOMAIN_CONTRACT.md)
- [Timeline authoring and observer model](https://github.com/TrentBest/StoryBuilder/blob/development/docs/TIMELINE_AUTHORING_MODEL.md)
- [StoryWorkspace and records](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.Core/Models/StoryWorkspace.cs)
- [Observer knowledge record](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.Core/Models/ObserverKnowledgeRecord.cs)
- [Manuscript edition](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.Core/Models/ManuscriptEdition.cs)
- [Timeline planning point](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.Core/Models/TimelinePlanningPoint.cs)
- [Workspace validator](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.Core/Services/StoryWorkspaceValidator.cs)

Re-verify branch state before implementing against these contracts or describing them as released behavior.
