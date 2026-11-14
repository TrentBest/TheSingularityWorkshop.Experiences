# Galactic War — StoryBuilder Capability Crosswalk

**Status:** design-to-implementation mapping, checked against StoryBuilder `development` on 2026-11-12. This document does not claim that Galactic War runtime systems exist.

## Why this crosswalk exists

Galactic War should use StoryBuilder as its authoring workbench rather than inventing a second narrative database inside Experiences. Before asking StoryBuilder to grow, map the game's first vertical slice onto the records and workflows that already exist.

Important branch distinction: StoryBuilder's `development` branch contains a substantially richer implementation and documentation set than its `master` README. The capabilities below are grounded in the inspected `development` source and docs. They must not be described as released or NuGet-published capabilities.

## Existing StoryBuilder capabilities that fit

| Galactic War need | Existing StoryBuilder concept | Fit and boundary |
| --- | --- | --- |
| A distant first-contact event exists independently of the player | `StoryEvent`, `TimelineDefinition`, `TimelinePlanningPoint` | A stable world-event identity can be authored and placed approximately or left unresolved. A planning point is not an occurrence or accepted canon. |
| Different institutions describe the same event | `SourceRecord`, `HistoricalClaim`, `ObserverKnowledgeRecord` | Sources carry perspective and reliability notes; claims link source IDs; observer knowledge distinguishes witnessed, told, inferred, believed, disputed, and forgotten. |
| A government bulletin and an independent journalist disagree | Multiple source records and claims linked to one event | Preserve the source's statement separately from the event and from accepted canon. Confidence and canon status remain explicit. |
| A correction changes what the public believes | New source/account and claim records; event links remain stable | The correction should be a new attributed account, not a destructive rewrite of the original. A dedicated broadcast-revision lineage is not currently a first-class record. |
| A character learns about the signal from a broadcast | `ObserverKnowledgeRecord` | Record the observer, event, acquisition event, source, attribution, confidence, and review status. The evaluator requires accepted knowledge and appropriate narration framing. |
| The player sees a report in a station lounge | `SceneRecord`, `SceneBeat`, `ManuscriptEdition`, `ManuscriptSection` | The authored scene/section can portray the report while linking back to the world event. The portrayal is not the event itself. |
| The same event appears on a TV, ticker, and ship feed | Stable event-to-manuscript-section references | Multiple sections can portray one event. Medium-specific presentation and distribution behavior remain Experience/runtime responsibilities. |
| A rival expedition discovers an ancient site first | `StoryEvent`, participant IDs, source IDs, observer knowledge, campaign records | Author possible outcomes and their relationships. The runtime campaign instance must determine which outcome actually occurs. |
| The player finds evidence that contradicts a published account | Source records, historical claims, continuity issues, observer knowledge | Preserve the evidence and competing claims; do not overwrite the earlier account or silently promote a new interpretation to canon. |
| An author brainstorms alternate explanations | `TimelinePlanningPoint` alternatives and explicit promotion | Alternatives remain authoring possibilities. Promotion to a proposed event is an explicit author action and is persisted. |
| A discovery becomes a campaign consequence | `ParticipantAction`, `CampaignRecord`, `StoryEvent` | Participant choices remain proposals until deliberately accepted; resulting event identity can be linked to the action. |

## Existing contracts worth reusing

### 1. World event is not account, scene, or prose

The StoryBuilder domain contract explicitly separates:
- `StoryEvent`: the author's canonical identity for a world event, with status, timeline placement, participants, causes, consequences, and source references;
- `HistoricalClaim` and `SourceRecord`: a claim and the provenance of an account or artifact;
- `ObserverKnowledgeRecord`: what one observer is established to know, how, and when;
- `SceneBeat` and `ManuscriptSection`: portrayals that reference events without becoming a second source of truth;
- `TimelineEventRecord`: runtime execution observation, not automatic canon.

This is exactly the separation Galactic War needs for diegetic news. A report can be inaccurate while the event remains unchanged. An observer can believe the report without the report becoming true.

### 2. Four clocks must remain distinct

For the first-contact story, do not collapse:
1. **World event time** — when the signal was emitted, detected, or the structure changed.
2. **Account publication time** — when an outlet or institution issued a report.
3. **Observer acquisition time** — when a particular character learned of it.
4. **Player presentation time** — when the player encountered the TV segment, ticker, conversation, or archive entry.

StoryBuilder already models temporal placement, observer acquisition through an event link, and manuscript presentation order as separate concepts. A dedicated publication-time and distribution model for news reports is still a candidate extension; do not overload one timestamp or manuscript order to represent all four clocks.

### 3. Authored possibility is not a runtime occurrence

Use StoryBuilder to author:
- event seeds and prerequisites;
- possible rival/player outcomes;
- source perspectives and evidence;
- scene and broadcast drafts;
- consequences and continuity links.

Do not use an authored possibility as proof that an event occurred in a particular playthrough. The runtime owns occurrence, participating actors, actual evidence, and resulting state. Runtime observations should be retained as observations until an explicit campaign/canon policy accepts them.

### 4. Observer knowledge is a hard narrative constraint

The existing evaluator is deliberately conservative:
- missing knowledge denies a direct claim;
- proposed knowledge remains unresolved;
- accepted witnessed knowledge can support direct-fact framing;
- accepted reports require attributed-report framing;
- inference and belief require corresponding framing;
- knowledge acquired at a particular event becomes available later only through the causal history.

For Galactic War, this prevents an NPC, narrator, or news segment from casually knowing what the player has not discovered or what the source could not yet have known.

## First StoryBuilder proof: “The Signal Nobody Owns”

Use one compact workspace and prove the whole authoring chain before adding a special Galactic War data model.

1. Create a first-contact `StoryEvent` on the frontier timeline. Keep its exact origin and meaning unresolved.
2. Add a government observatory `SourceRecord` and an independent research network `SourceRecord`, each with a distinct perspective and reliability note.
3. Add separate `HistoricalClaim` records for their interpretations. Link both claims to the same event and their respective sources.
4. Create an accepted or proposed knowledge record for a station resident who heard the government bulletin. Keep the status deliberate; the existence of a source does not automatically mean every character heard it.
5. Create a second source account that corrects the first interpretation. Preserve the original account and claim. Record the correction as a new attributed record.
6. Create one station-lounge scene and manuscript sections for the TV report, the ticker update, the NPC disagreement, and the correction. Link each portrayal to the same world event where appropriate.
7. Add a player-discoverable lead as a separate event or claim, not as an automatic consequence of the broadcast.
8. Review the chronology, source links, observer knowledge, and event-to-passage links. Confirm that no report is accidentally promoted into objective truth.

**Proof condition:** an author can revise the broadcast wording, source perspective, or reveal order without changing the identity of the world event; and a character cannot state a later correction before acquiring that knowledge.

## Gaps to keep visible

The current StoryBuilder model provides enough primitives to author and review the first news-event proof. It does not yet provide a dedicated first-class **NewsReport/BroadcastEdition** record with all of the following as a single validated contract:

- publication and revision timestamps distinct from world-event time;
- outlet/channel identity and distribution reach;
- intended audience and access restrictions;
- explicit claims/omissions/evidence snapshot for each issued edition;
- append-only correction/supersession lineage between report editions;
- delivery/receipt evidence for a particular observer or location.

These are not reasons to build a parallel system in Galactic War. First prove the initial scenario with existing records. If the authoring workflow demonstrates friction, propose a narrowly scoped StoryBuilder extension with migration and tests. Keep the runtime's actual signal distribution, media scheduling, and observer delivery in Galactic War's host/runtime boundary.

The concrete authoring gap has now been recorded for cross-repository coordination: [StoryBuilder #9 — Add a genre-neutral immutable in-world publication edition contract](https://github.com/TrentBest/StoryBuilder/issues/9). The issue deliberately limits StoryBuilder to authored publication snapshots, provenance, and correction lineage; it leaves channel distribution and actual receipt in Galactic War.

## Workshop ownership

- **StoryBuilder** owns authoring records, provenance, proposal/review, timeline and manuscript relationships.
- **Experiences / Galactic War** owns the fictional semantics of news, discoveries, archaeological evidence, factions, campaign rules, and diegetic media.
- **FSM_API** owns explicit state identity and transitions where appropriate.
- **FSM_COS** composes capabilities at the host boundary.
- **FSM_Foundry** executes composed behavior; it does not decide authorial canon.
- **GUI.Core / GUI.SVG** can expose the same records through authoring and review surfaces.
- **Renderer / Visualization** are optional MicroBundle manifestations, not alternate story databases.
- **DataWarehouse** may preserve runtime observations and evidence within its own boundary; it does not replace StoryBuilder's authoring model.

## Source references inspected

StoryBuilder `development`:
- [README](https://github.com/TrentBest/StoryBuilder/blob/development/README.md)
- [Story domain contract](https://github.com/TrentBest/StoryBuilder/blob/development/docs/STORY_DOMAIN_CONTRACT.md)
- [Timeline authoring and observer model](https://github.com/TrentBest/StoryBuilder/blob/development/docs/TIMELINE_AUTHORING_MODEL.md)
- [Timelines, gestures, and multiverse history](https://github.com/TrentBest/StoryBuilder/blob/development/docs/TIMELINES_GESTURES_MULTIVERSE.md)
- [Development status](https://github.com/TrentBest/StoryBuilder/blob/development/docs/DEVELOPMENT_STATUS.md)
- [StoryWorkspace and domain records](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.Core/Models/StoryWorkspace.cs)
- [Observer knowledge record](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.Core/Models/ObserverKnowledgeRecord.cs)
- [Timeline planning point](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.Core/Models/TimelinePlanningPoint.cs)

The crosswalk was checked against source and documentation on 2026-11-12. Branch state and implementation may change; re-verify before treating this as a release contract.
