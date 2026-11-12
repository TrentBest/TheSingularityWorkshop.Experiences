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


## Host and binary-package integration recheck

A second source pass checked StoryBuilder and AnyApp `development` on 2026-11-12. It clarifies three separate paths that must not be conflated:

1. **StoryBuilder authoring composition:** `StoryBuilderExperienceMicroBundle` in `src/StoryBuilder.FSM_COS` creates a named authoring session and composes the `StoryBuilderAuthoringBundle`. `StoryBuilderAuthoringSurface` projects that mutable workspace to a semantic `GuiNode` and dispatches authoring commands. This is the authoring workbench, not a Galactic War content-package importer.
2. **Captured composition package:** `CompositionPackageCompiler` and `CompositionPackageReader` in `src/StoryBuilder.Persistence/CompositionPackageBinaryArtifacts.cs` compile an already-arbitrated session into manifest, configuration, and content byte arrays, then validate hashes, requirements, capabilities, and restored workspace identity. The reader returns a restored `StoryBuilderSession` only on successful validation. It does not itself expose a Galactic War runtime contract.
3. **AnyApp host composition:** `ExperienceManifest` is the launch/composition request converted to an FSM_COS `RuntimeManifest`; `AnyAppRuntime.Compose/ComposeAsync` produces the `RuntimeAssembly`. The current `IAnyAppSurface` exposes only a semantic GUI root. AnyApp's conditional StoryBuilder catalog registration proves the authoring bundle can be composed in that build, not that a captured StoryBuilder package is loaded and handed to another Experience.

### Integration consequence

The first real content integration needs an explicit, host-neutral handoff between a successfully validated StoryBuilder package and the Galactic War Experience. Keep the responsibilities in this order:

- StoryBuilder owns its binary format, package validation, workspace restoration, and authored record semantics.
- The host or a dedicated adapter owns invoking the StoryBuilder reader and rejecting invalid packages before use.
- Galactic War owns mapping validated authored identities/records into campaign-specific runtime references, signal occurrences, broadcasts, observer receipts, and player presentation.
- AnyApp owns composition and native presentation; it must not implement Galactic War rules or become the package's semantic authority.

Do not add a StoryBuilder.Persistence project reference to the Galactic War runtime merely to read package bytes. Do not deserialize the package independently, duplicate its manifest, treat a mutable authoring session as an immutable captured package, or infer that a composed authoring bundle means a package has been validated.

### Remaining proof

The next integration proof should pass a real `CompositionPackageArtifact` through StoryBuilder's existing reader with the required MicroBundle/capability set, then pass the resulting validated content through a narrow adapter into Galactic War. The adapter must preserve stable StoryBuilder IDs and provenance while leaving campaign occurrence and delivery identities Experience-owned. Test invalid hashes, missing required capabilities, workspace identity mismatch, and valid round-trip before attempting a full AnyApp launch.

This is a concrete cross-repository integration gap, but the existing StoryBuilder contract already describes the unfinished real-host package-consumption proof. No duplicate StoryBuilder issue or speculative schema change is warranted yet.

### Additional source paths inspected

- [Composition package compiler and reader](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.Persistence/CompositionPackageBinaryArtifacts.cs)
- [StoryBuilder authoring Experience bundle](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.FSM_COS/StoryBuilderExperienceMicroBundle.cs)
- [StoryBuilder authoring surface](https://github.com/TrentBest/StoryBuilder/blob/development/src/StoryBuilder.MicroBundle/StoryBuilderAuthoringSurface.cs)
- [AnyApp Experience manifest](https://github.com/TrentBest/AnyApp/blob/development/ExperienceManifest.cs)
- [AnyApp FSM_COS composition entry point](https://github.com/TrentBest/AnyApp/blob/development/AnyAppRuntime.cs)
- [AnyApp semantic surface contract](https://github.com/TrentBest/AnyApp/blob/development/IAnyAppSurface.cs)
- [AnyApp host adapter contract](https://github.com/TrentBest/AnyApp/blob/development/docs/EXPERIENCE_HOST_ADAPTER_CONTRACT.md)

### Exact binary reader API and test boundary

The current StoryBuilder `development` `CompositionPackageBinaryArtifacts.cs` implements these concrete public contracts:

```csharp
public sealed record CompositionPackageArtifact(
    byte[] ManifestPayload,
    byte[] ConfigurationPayload,
    byte[] ContentPayload);

public sealed class CompositionPackageCompiler
{
    public CompositionPackageArtifact Compile(
        StoryBuilderSession session,
        string contentRevision,
        string outputProfile,
        Guid arbitrationRecordId,
        IEnumerable<MicroBundleRequirement> bundleRequirements,
        IEnumerable<MicroBundleRuntimeConfiguration> bundleConfigurations);
}

public sealed class CompositionPackageReader
{
    public CompositionPackageLoadResult ValidateAndRestore(
        CompositionPackageArtifact artifact,
        IEnumerable<ulong> availableBundleIds,
        IEnumerable<string> availableCapabilities);
}

public sealed record CompositionPackageLoadResult(
    bool IsValid,
    IReadOnlyList<string> Errors,
    CompositionPackageManifest? Manifest,
    CompositionPackageConfiguration? Configuration,
    StoryBuilderSession? Session);
```

The compiler requires a non-empty arbitration record ID and captures a package ID, workspace ID, content revision, output profile, exact content/configuration SHA-256 hashes, MicroBundle requirements, and per-bundle configuration. The reader validates the decoded manifest/configuration, both payload hashes, available bundle IDs/capabilities, and the restored workspace ID. A failed validation returns diagnostics and a null session; callers must gate all mapping on `IsValid` and a non-null `Session`.

The persistence project currently targets `net8.0`, references `StoryBuilder.Core`, and consumes `TheSingularityWorkshop.FSM_Serialization` 1.0.0. Galactic War also targets `net8.0`, so there is no target-framework mismatch, but a direct runtime reference would still violate the intended ownership boundary and pull persistence concerns into the Experience.

**Placement decision:** keep the binary reader in StoryBuilder.Persistence. Put any AnyApp package-loading adapter in the host integration layer, and keep Galactic War's input contract typed and host-neutral. A cross-repository integration test should live with the host adapter (or a dedicated integration test project that explicitly checks out/references StoryBuilder); the Experiences CI must not pretend its standalone build has that dependency. The narrow Galactic War mapping can then be unit-tested using validated domain inputs without knowing how the package was serialized.

The minimum reader test matrix is:
- valid artifact + complete required bundle/capability set → valid restored session with the same workspace identity;
- altered content or configuration bytes → invalid result, no session;
- absent required bundle or capability → invalid result, no session;
- restored content workspace ID inconsistent with manifest → invalid result, no session;
- malformed manifest/configuration/content → diagnostic failure, no session.

This exact API check narrows the remaining work: the open gap is not a missing StoryBuilder compiler or reader. It is a real host adapter and an integration test that proves the validated restored session is mapped into Galactic War without leaking serialization into the runtime.