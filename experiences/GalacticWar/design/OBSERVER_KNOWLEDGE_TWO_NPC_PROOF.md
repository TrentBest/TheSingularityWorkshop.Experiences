# Observer Knowledge Proof: Two NPCs, One Signal

**Status:** source-grounded authoring/test fixture for the Galactic War first-contact proof. This is a design-side fixture, not a claim that Galactic War currently executes StoryBuilder or that a runtime news system exists.

## Question

Can two characters refer to the same world event without being granted the same knowledge or the same right to narrate it?

The proof uses the current StoryBuilder `development` contracts directly:

- `StoryWorkspace`, `CharacterProfile`, `StoryEvent`, `SourceRecord`, and `ObserverKnowledgeRecord`
- `ObserverKnowledgeEvaluator.Evaluate`
- `ObserverKnowledgeProjectionService.Project`
- optionally, `NarratorAgent.SelectObserver` and `NarratorAgent.EvaluateObserverClaim`

No new narrative record type is required for this proof.

## Scenario

A signal is detected beyond the frontier.

- **Ilyan Voss**, a station signal analyst, witnesses the signal event directly.
- **Mara Quill**, a dockside courier, does not witness the signal. She hears an attributed account in a station bulletin.
- **Teren**, a mechanic in an isolated berth, has not received either account.

The same underlying event is therefore known differently:

| Observer | Acquisition | Permitted narration |
|---|---|---|
| Ilyan Voss | Witnessed the signal event | Direct fact |
| Mara Quill | Told through a sourced bulletin | Attributed report |
| Teren | No accepted knowledge record | No claim |

The bulletin is a separate later `StoryEvent` whose `CauseEventIds` includes the signal event. This gives the evaluator a causal path on which Mara's knowledge becomes available.

## Authoring fixture

The following C# is intentionally limited to public model/service contracts visible on StoryBuilder's `development` branch. It is a compact fixture for a StoryBuilder test or authoring harness; it is not presented as a compiled Galactic War implementation.

```csharp
using StoryBuilder.Core.Models;
using StoryBuilder.Core.Services;

var workspace = new StoryWorkspace { Name = "Galactic War — Signal Nobody Owns" };

var analyst = new CharacterProfile
{
    Name = "Ilyan Voss",
    Role = "Station signal analyst",
    Status = CanonStatus.Accepted
};
var courier = new CharacterProfile
{
    Name = "Mara Quill",
    Role = "Independent courier",
    Status = CanonStatus.Accepted
};
var mechanic = new CharacterProfile
{
    Name = "Teren",
    Role = "Berth mechanic",
    Status = CanonStatus.Accepted
};

workspace.AddCharacter(analyst);
workspace.AddCharacter(courier);
workspace.AddCharacter(mechanic);

var bulletinSource = new SourceRecord
{
    Title = "Vesper Reach public signal bulletin",
    SourceType = "Public station bulletin",
    Perspective = "Station communications desk",
    ReliabilityNote = "An attributed summary; not a direct sensor record."
};
workspace.AddSource(bulletinSource);

var signal = new StoryEvent
{
    Title = "Unidentified signal detected beyond the frontier",
    Summary = "A remote structure emits a repeating signal.",
    InWorldDateOrPeriod = "Frontier Cycle 18, day 42",
    Status = CanonStatus.Accepted
};
workspace.AddEvent(signal);

var bulletin = new StoryEvent
{
    Title = "Vesper Reach publishes the signal bulletin",
    Summary = "The station bulletin reports the signal and attributes the account to its communications desk.",
    InWorldDateOrPeriod = "Frontier Cycle 18, day 43",
    CauseEventIds = [signal.Id],
    SourceIds = [bulletinSource.Id],
    Status = CanonStatus.Accepted
};
workspace.AddEvent(bulletin);

// Ilyan personally witnessed the original signal.
workspace.AddObserverKnowledge(new ObserverKnowledgeRecord
{
    ObserverId = analyst.Id,
    EventId = signal.Id,
    Kind = ObserverKnowledgeKind.Witnessed,
    AcquiredAtEventId = signal.Id,
    Status = CanonStatus.Accepted,
    Note = "Observed the signal on the station's receiver array."
});

// Mara learned of that same event later, through an attributed report.
workspace.AddObserverKnowledge(new ObserverKnowledgeRecord
{
    ObserverId = courier.Id,
    EventId = signal.Id,
    Kind = ObserverKnowledgeKind.Told,
    AcquiredAtEventId = bulletin.Id,
    SourceId = bulletinSource.Id,
    Attribution = "Vesper Reach public signal bulletin",
    Status = CanonStatus.Accepted,
    Note = "Heard the bulletin in the public concourse."
});

var evaluator = new ObserverKnowledgeEvaluator();

// Evaluate at the bulletin event: the signal is in its causal history.
var ilyanDirect = evaluator.Evaluate(
    workspace, analyst.Id, signal.Id, bulletin.Id, NarrationClaimMode.DirectFact);

var maraAttributed = evaluator.Evaluate(
    workspace, courier.Id, signal.Id, bulletin.Id, NarrationClaimMode.AttributedReport);

var maraDirect = evaluator.Evaluate(
    workspace, courier.Id, signal.Id, bulletin.Id, NarrationClaimMode.DirectFact);

var terenBelief = evaluator.Evaluate(
    workspace, mechanic.Id, signal.Id, bulletin.Id, NarrationClaimMode.Belief);

var projectionService = new ObserverKnowledgeProjectionService();
var ilyanProjection = projectionService.Project(workspace, analyst.Id, bulletin.Id);
var maraProjection = projectionService.Project(workspace, courier.Id, bulletin.Id);
var terenProjection = projectionService.Project(workspace, mechanic.Id, bulletin.Id);
```

## Required assertions

A test harness should assert the following outcomes, rather than merely checking that the methods return:

1. `ilyanDirect.IsAllowed == true`.
2. `maraAttributed.IsAllowed == true`.
3. `maraDirect.IsAllowed == false`: being told about an event must not become witnessing it.
4. `terenBelief.IsAllowed == false`: an observer with no record must not acquire knowledge by proximity or because another character knows.
5. Ilyan's projection contains the signal under his observer ID and permits a direct-fact claim.
6. Mara's projection contains the signal under her observer ID and permits an attributed-report claim, not a direct-fact claim.
7. Teren's projection contains no permitted signal claim.
8. If Mara's knowledge record is changed to `Proposed`, the evaluator must return unresolved rather than allow the report.
9. If the bulletin is not in the causal history of the requested narration anchor, Mara's acquired knowledge must not be available at that anchor.
10. If a `Forgotten` record is accepted for an observer/event at the narration point, the evaluator must fail closed for that event.

The projection API exposes permitted claims, including claim mode, knowledge-record identity, kind, attribution, note, source ID, and confidence. Tests should assert those values as well as the event identity.

## What this proves—and what it does not

**Proves, if the assertions pass against the current StoryBuilder development build:**

- stable character identity is already the observer boundary;
- one event can have distinct observer-specific knowledge records;
- direct witnessing and attributed reporting remain distinct;
- causal acquisition timing can prevent knowledge from appearing too early;
- the projection service can prepare observer-limited event knowledge for narration.

**Does not prove:**

- that a broadcast was physically delivered to a particular device or person;
- that an in-world receipt timestamp exists;
- that a station bulletin has immutable issue/revision/distribution semantics;
- that a player-facing runtime currently distributes news;
- that a character's interpretation is objectively true.

The current `AcquiredAtEventId` is a causal acquisition anchor, not a dedicated in-world receipt timestamp. `CreatedAtUtc` is record metadata, not fictional chronology. Keep that distinction explicit.

## Failure cases worth preserving

- **No observer record:** deny; do not fall back to global event knowledge.
- **Proposed-only record:** unresolved; do not narrate as accepted knowledge.
- **Wrong observer ID:** deny; do not leak another character's records.
- **Too-early narration anchor:** deny when acquisition is not causally available.
- **Told without source or attribution:** do not permit an attributed report.
- **Forgotten:** deny rather than resurrect old knowledge from another record.
- **Contradictory accepted accounts:** preserve their attribution and confidence; do not silently collapse them into one omniscient truth.

## Galactic War integration boundary

StoryBuilder owns the authoring records and observer-knowledge evaluation. Galactic War owns the fictional broadcast channel, audience selection, delivery events, local media presentation, and the consequences of missed or delayed delivery.

For the first playable slice, use a single bulletin source and two NPCs. Do not build a general broadcast platform before this proof is exercised. If later work demonstrates a real need for first-class issue/revision/receipt semantics, specify that narrow gap separately rather than overloading book publication or mislabeling causal acquisition as delivery.

## Acceptance

The proof is ready to move from design to an executable StoryBuilder test when the StoryBuilder development owner can run the fixture against the current branch and record the assertion results. Until then, this document is a source-grounded test specification, not a passing test report.
