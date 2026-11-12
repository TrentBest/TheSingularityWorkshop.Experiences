# Galactic War Executable Slice — Story First, Real State

**Status:** implementation plan grounded in the current Experiences, SimpleDemos, and FSM_API development repositories. This is not a claim that the slice is implemented or running.

## The story remains the product

We are making Galactic War. StoryBuilder, FSM_API, GUI, Renderer, Visualization, FSM_COS, FSM_Foundry, and DataWarehouse are tools and capabilities used to make that universe coherent and playable. Their current shapes are evidence about what we can implement today, not permission to shrink the fiction to fit a tool.

When a real story requirement exposes a tool gap:
1. preserve the story requirement and its meaning;
2. identify the smallest concrete contract gap;
3. check the owning repository's development branch;
4. coordinate through an issue or pull request with that owner;
5. keep a temporary Experience-side adapter only when it does not duplicate the owner's domain.

Do not build a second narrative database in Galactic War. Do not let an authoring model decide what happened in a running campaign.

## The first slice: the signal reaches Vesper Reach

The first playable proof should be a short, inhabited-station experience—not a trophy simulator disguised as a game.

The visitor arrives at Vesper Reach during an ordinary station shift. The lounge display interrupts its routine programming with an official report about an anomalous signal detected beyond the settled frontier.

The scene has three distinct witnesses:

- **Ilyan Voss**, a station signal analyst, directly examined the signal evidence. Ilyan can speak about what was measured, what remains uncertain, and which interpretation is only a hypothesis.
- **Mara Quill**, a courier, heard the official bulletin. Mara can repeat and attribute the bulletin, but cannot claim to have personally witnessed the distant event.
- **Teren**, a mechanic isolated in a berth workshop, has not received either account. Teren must not know about the signal merely because the author or player does.

The visitor can watch the display, ask either informed character, ignore the story, or leave. The scene does not force the visitor to investigate. Later, a correction or independent account can appear as a new publication event. It must not rewrite the first bulletin or retroactively give every character the new information.

The first playable outcome is modest and deliberate: the station feels inhabited, the signal becomes a story in the world rather than a quest marker, and the visitor can tell the difference between an event, an account, a witness, and a rumor.

## What SimpleDemos contributes

The current `TrentBest/SimpleDemos` master branch is a useful working reference:

- `QuickBrownFox/QuickBrownFox.cs` defines one behavior blueprint and creates an instance bound to a context.
- `QuickBrownFox/LazySleepingDog.cs` shows a second independent agent and a cross-FSM transition request.
- `QuickBrownFox/SimpleDemoContext.cs` owns the host loop, sensing pass, processing-group setup, and calls to `FSM_API.Interaction.Update`.
- `QuickBrownFox/QuickBrownFox.csproj` uses the published `TheSingularityWorkshop.FSM_API` package version `1.0.13`.
- The README is careful to distinguish the C# executable from the browser SVG/HTML illustrations and to say that the advanced simulation is not launched by the current console entry point.

Reuse the useful separation: definitions are shared, contexts are per-instance, the host owns update cadence, and the environment supplies observations. Do not copy the fox/dog simulation wholesale; Galactic War has different domain semantics.

Reference: [SimpleDemos README](https://github.com/TrentBest/SimpleDemos/blob/master/README.md), [QuickBrownFox](https://github.com/TrentBest/SimpleDemos/blob/master/QuickBrownFox/QuickBrownFox.cs), [LazySleepingDog](https://github.com/TrentBest/SimpleDemos/blob/master/QuickBrownFox/LazySleepingDog.cs), [SimpleDemoContext](https://github.com/TrentBest/SimpleDemos/blob/master/QuickBrownFox/SimpleDemoContext.cs).

## Critical dependency finding: stable package is not the development proof

The existing Experiences projects commonly reference FSM_API `1.0.13`. SimpleDemos does too. That is a valid released-package dependency, but it is not automatically a valid proof of the current `FSM_API/development` source contract.

The distinction matters here. The development mapping inspected for the trophy lifecycle relies on current development behavior, including the invalid-context suspension/revalidation contract. The published package's own release notes describe invalid contexts as automatically destroying instances. Those are materially different lifecycle semantics.

Therefore:
- Do not claim an executable proof against FSM_API development if the test actually restored the published `1.0.13` package.
- Do not publish a new NuGet package just to unblock this proof.
- The development integration build must consume a locally built artifact from the exact FSM_API development commit, or a source project reference in a coordinated multi-repository development checkout.
- The normal repository CI can continue to use released dependencies until the dependency-build workflow supplies the development artifact.
- Record the exact FSM_API commit/package identity used by every integration test run.

This is a dependency orchestration issue, not yet evidence that FSM_API needs a new API.

## Proposed Experience project shape

Follow the existing Experiences pattern of a focused implementation project and a focused test project:

- `src/GalacticWar/` — domain meaning and runtime-facing contracts owned by Galactic War.
- `tests/GalacticWar.Tests/` — deterministic tests for story/event rules and observer boundaries.
- `samples/GalacticWar.FirstContact/` — a small host that can run the station scene and advance the FSM processing group.

Keep the domain model independent from rendering. The sample host should be able to print a meaningful scene transcript before a GUI surface exists. Later, GUI.Core can present the same state, and Renderer/Visualization can manifest it as MicroBundles. A visual output must never become the authority for what a character knows or what happened.

Do not add a parallel StoryBuilder model. Use StoryBuilder's existing events, sources, claims, characters, observer-knowledge records, scenes, and manuscript sections for authored material. Keep actual campaign occurrence and station delivery/presentation behavior in the Galactic War runtime boundary.

## First runtime responsibilities

The first runtime proof needs only a few explicit responsibilities:

1. **Station session:** create a stable session identity and establish the visitor's arrival at Vesper Reach.
2. **Signal bulletin:** record a publication/announcement occurrence distinct from the distant signal event.
3. **Observer knowledge:** maintain separate knowledge for Ilyan, Mara, and Teren. The narrator and presentation layer must query the relevant observer rather than read a global omniscient summary.
4. **Player choice:** allow watching, asking, ignoring, or leaving without forcing a quest state.
5. **Later correction:** publish a new account linked to the same signal event; preserve the original report and each character's acquisition history.
6. **Observable trace:** emit a transcript or structured trace that can be inspected and tested. The trace is evidence of runtime behavior, not a substitute for authoritative state.

FSM_API should be used where an entity has a real lifecycle worth modeling. It should not be added to every object merely to increase the number of state machines. A station-session FSM and explicit observer/character state machines are candidates; the actual boundary should be confirmed by the first implementation rather than assumed here.

## Acceptance gates

The first vertical slice is ready to call executable only when tests and a reproducible run establish all of the following:

- The same signal event is referenced by the official report, independent account, and later correction.
- The original report remains inspectable after correction.
- Ilyan can state only evidence available to the analyst observer.
- Mara can repeat the bulletin with attribution but cannot claim direct observation.
- Teren has no signal knowledge until an explicit acquisition event occurs.
- A character does not know a correction before its acquisition event.
- Ignoring the bulletin is a valid player path; no investigation is silently forced.
- A scene transcript can be reproduced from the same authored inputs and runtime choices.
- The integration run identifies the exact FSM_API development commit or released package used.
- Build/test output is captured; no visual mock is described as live simulation.

## Work order

1. Preserve the current first-contact story and three-observer proof as the content baseline.
2. Establish the development dependency path for FSM_API without publishing a package.
3. Implement the smallest Experience-owned station/event/observer runtime contracts.
4. Prove the observer boundaries and player independence with deterministic tests.
5. Add a runnable console host and trace.
6. Only then connect GUI.Core and MicroBundle presentation.
7. Reassess whether StoryBuilder needs any extension based on actual authoring friction; coordinate any request with its owner through GitHub.

The trophy lifecycle proof remains a separate domain slice. Its FSM_API mapping is useful groundwork, but it should not displace the first-contact story as the first visitor-facing Galactic War experience.
