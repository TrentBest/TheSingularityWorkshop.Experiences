![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# StoryBuilder Experience

> **An authoring Experience for discovering, constructing, inspecting, and revising the story of another Experience.**

StoryBuilder is not only a standalone authoring application. It is also an Experience that can be selected by a host such as AnyApp. The StoryBuilder repository owns the authoring workbench and authoring domain implementation; this directory describes the reusable Experience boundary that makes that capability discoverable and host-independent.

## Purpose

Help a creator capture the story behind an Experience as connected, inspectable records—not merely a manuscript or a timeline drawing.

The author can build and revise:

- characters, entities, world claims, and source/provenance records;
- scenes, events, timeline branches, and causal or temporal relationships;
- observer-specific knowledge and the difference between what happened, what a character knows, and what a portrayal reveals;
- manuscript passages and other portrayals that refer to canonical story records;
- captured, versioned story compositions that can be validated and handed to a host.

A book is the first vertical slice because it makes characters, scenes, events, chronology, and portrayal tangible. The model must not assume every future Experience is a book.

## Ownership and composition

- **StoryBuilder repository:** authoring commands, workspace/session model, composition-package compiler and reader, persistence adapters, and authoring views.
- **StoryBuilder Experience (this definition):** the user-facing capability boundary and its host-independent identity/configuration.
- **Story Forge in this repository:** temporal facts, timelines, publication coverage, immutable edition foundations, provenance, and queries about what was true at story time.
- **Target Experience:** owns its actual domain semantics and runtime behavior. StoryBuilder captures and connects authoring intent; it does not replace the target Experience.
- **FSM_COS:** composes capabilities at the host boundary.
- **AnyApp / WebApp / WebPage:** hosts and manifestations. They should consume a validated capture through the existing Experience manifest and MicroBundle contracts.
- **GUI / GUI.SVG / Renderer / Visualize:** present or inspect canonical data; they must not become its source of truth.

## Intended first user journey

1. Launch StoryBuilder as an Experience in AnyApp.
2. Create a workspace for a target Experience.
3. Add a character, a scene, and a story event.
4. Place the event on a timeline and reference it from the scene and a manuscript passage.
5. Inspect the timeline, manuscript, event source, and observer perspective without duplicating canonical records.
6. Validate and capture a versioned composition package.
7. Open the captured package through the target Experience manifest and AnyApp's normal composition path.
8. Save and reopen the workspace and capture, preserving stable identities and relationships.

## Contract rules

- Workspace/session state is mutable authoring data; a captured composition package is a validated snapshot.
- The Experience manifest and bundle configuration are distinct from the package and from immutable MicroBundle artifact identity.
- Reuse StoryBuilder's existing compiler/reader and FSM_Serialization binary stream contract. Do not invent a parallel manifest or persistence format here.
- Preserve exact bundle identity, version, and content hash where repository artifacts are used.
- Reject invalid references, mismatched artifact identities, and unsupported package versions explicitly.
- Keep StoryBuilder.Core host-neutral. Native and browser adapters belong at the host boundary.
- No Unity dependency is required.

## Implementation status

**Present:** StoryBuilder's authoring implementation and a source-mode native AnyApp launch slice; Story Forge's renderer-independent temporal model; existing Experience manifest and MicroBundle composition conventions.

**Still required before this Experience is end-to-end:** resolve StoryBuilder through AnyApp's supported catalog/materializer path; connect the validated capture to an Experience manifest/configuration; present the canonical authoring session through a supported host adapter; verify save/reopen and package validation in integration CI.

The native source-mode launch is a useful early proof, but it does not yet prove the complete package → manifest → verified artifacts → FSM_COS → AnyApp path.

## Related resources

- [StoryBuilder repository](https://github.com/TrentBest/StoryBuilder)
- [StoryBuilder architecture](https://github.com/TrentBest/StoryBuilder/blob/development/docs/ARCHITECTURE.md)
- [StoryBuilder → Experiences → AnyApp first-run milestone](../../docs/STORYBUILDER_ANYAPP_FIRST_RUN.md)
- [Story Forge temporal capability](../../src/StoryForge/README.md)
- [Experience directory](../EXPERIENCE_DIRECTORY.md)

---

<p align="center">
  <a href="https://github.com/TrentBest/TheSingularityWorkshop" aria-label="The Singularity Workshop living avatar">
    <img src="https://raw.githubusercontent.com/TrentBest/WebPage/development/TheSingularityWorkshop/wwwroot/Images/TheSingularityWorkshopLivingAvatar.svg" alt="The Singularity Workshop living avatar" width="112" />
  </a><br />
  <strong>The Singularity Workshop</strong><br />
  <em>If a picture is worth a thousand words, what is a living picture worth?</em><br />
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://www.patreon.com/TheSingularityWorkshop">Support the Workshop</a>
</p>
