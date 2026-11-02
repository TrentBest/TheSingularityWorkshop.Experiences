![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# GitHub Learning Studio

> Learn GitHub by understanding what it makes possible for your project—not by memorizing where buttons live.

## The invitation

You have made something: a website, a game, a library, a story, a tool, a collection of ideas. Now you want to keep it safe, improve it, work with other people, and let the world see it.

GitHub gives you a way to do those things. The GitHub Learning Studio makes the ideas understandable by letting you see them happen.

This is a Singularity University learning Experience. It teaches the concepts and practices represented by GitHub. It is not a recreation of GitHub's website, a screen-by-screen interface guide, or a GitHub account client.

## The learner's promise

A learner should be able to arrive with a real question:

- How do I give my project a home?
- How can I experiment without breaking the version everyone uses?
- How do I ask someone to look at a change before it becomes part of the project?
- How do I keep track of something that needs fixing?
- How do I publish a website so another person can visit it?
- How do I control who can see or change my work?
- How do I know whether an automated check succeeded?

The Studio should answer each question through a small, visible, consequential experience. The learner sees what changes for them, why it matters, and what to do next.

## Learning philosophy

Follow the University sequence:

**Play → Surprise → Discover → Understand → Create**

Start with a concrete project and a meaningful goal. Let the learner explore, make a change, and observe the result. Reveal vocabulary and deeper explanations as they become useful. Offer formal detail without requiring it as the entrance fee.

There are no age-based tracks. A first-time learner deserves a clear introduction; an experienced developer deserves accurate depth and the ability to skip what they already know.

The Studio should make invisible relationships visible. A repository is not merely a folder icon; it is a project home with files and a history of changes. A branch is not merely a name; it gives a line of work room to develop independently. A pull request is not merely a form; it is a proposal that gathers a change, its context, discussion, review, and a decision.

## The first journey: from project to published website

**Guiding question: “I have a project. How do I share it with the world?”**

The learner starts with a fictional, deliberately small project. The experience progressively introduces:

1. **The project home — repository.** Inspect the project's files, README, and shared identity.
2. **A recorded improvement — commit.** Change a small piece of content and see that change become a durable point in the project's history.
3. **A safe place to work — branch.** Explore how a proposed improvement can develop separately from the shared main line.
4. **A public destination — GitHub Pages.** Follow how repository content is selected, built or prepared, published, and made available at a website address.
5. **The ongoing loop.** Change the project again and see how a later update can flow through publication to the website.

The Pages journey is deliberately early. It gives the learner a personal reason to care about repository structure, commits, project settings, build status, and publication.

### The living picture

The central visualization is a project that changes as the learner works:

- A project home contains recognizable files and a README.
- A timeline records meaningful changes.
- A separate line of work shows experimentation without implying that the shared version has changed.
- A publication path connects the selected project content to a build/deployment state and then to a public website.
- A later change creates a new history event and a new publication cycle.

The learner can pause, inspect, replay, compare before and after, and ask what each relationship means. The visualization should show causal relationships, not decorative motion.

### Simulation honesty

The first slice may be a deterministic educational simulation. Its project, commits, branches, and Pages deployment are teaching data unless a separately authorized real GitHub integration is added.

The experience must label the difference between:
- a simulated action in the lesson;
- a concept that accurately describes GitHub;
- an action actually performed against a real GitHub repository or account.

No real account changes, repository writes, permission changes, or deployments occur as a side effect of completing a simulated lesson.

## Curriculum map

The Studio is modular. Learners can enter through the task they want to accomplish and follow links to related concepts.

| Learner question | GitHub concept | What the learner should understand |
|---|---|---|
| Where does my project live? | Repository | A shared project home, its files, identity, and history |
| How do I save a meaningful change? | Commit | A recorded change with context that can be revisited |
| How can I experiment safely? | Branch | An independent line of work |
| How do I propose an improvement? | Pull request | A reviewable proposal, not an automatic merge |
| How do we track work and questions? | Issues | A shared record of tasks, defects, ideas, and discussion |
| How can people visit what I made? | GitHub Pages | A path from repository content to a published website |
| Who can see or change my project? | Settings and permissions | Visibility, access, roles, and protective controls |
| How do I know the project is healthy? | Actions and checks | Automated work and evidence that a process passed or failed |
| How do I share a stable version? | Tags and releases | Named points and packaged milestones in project history |
| How do people coordinate? | Discussions and Projects | Conversation and organization around ongoing work |

The curriculum should grow from learner goals, not from an exhaustive inventory of GitHub features. Advanced topics can be introduced when they solve a problem the learner can recognize.

## Pull requests as a living collaboration

A dedicated lesson should let the learner watch a proposed change gather its supporting context:

1. Someone identifies a task or improvement.
2. Work is made on a separate branch.
3. A pull request explains the proposed change and connects it to the task.
4. A reviewer examines the change and asks questions or suggests revisions.
5. The author responds with additional commits.
6. Automated checks report their results.
7. An authorized person decides whether and how to merge.
8. The shared project history reflects the accepted change.

Make the key distinction visible: proposing, reviewing, approving, and merging are different events. The learner should understand how this protects their project and helps collaborators coordinate.

## Issues as the project's memory

The dedicated [Issues journey](IssuesJourney/README.md) develops this idea into a complete learning contract: turn an unclear report into an actionable issue, preserve clarification and decisions, connect the issue to implementation, and verify acceptance criteria before closure. Its deterministic interactive implementation is tracked separately in [issue #48](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/issues/48); this document is the authored theory and scenario design, not a claim that the lesson is already runnable.

## Settings as stewardship

Settings are not a miscellaneous list of switches. They express decisions about the project's audience, collaborators, safety, and operation.

Use a fictional project to explore visibility, collaborators, permissions, notifications, branch rules, integrations, and security. Explain consequences and sensible defaults. Never encourage learners to weaken protections or expose secrets merely to make a demonstration work.

## Presentation and ownership

This is an Experience authored in TheSingularityWorkshop.Experiences.

- **Experience semantics** define the project scenario, lesson goals, learner actions, concept relationships, and educational state.
- **GUI** owns interactive controls and learner interaction.
- **Visualize** makes project structure, history, proposals, reviews, checks, and publication state visible.
- **Renderer** presents explanations, terminology, decisions, and accessible text alternatives.
- **FSM_COS** composes the Experience through the existing manifest and MicroBundle contracts.
- **The host** provides the environment in which the Experience is manifested.

Reuse the existing GUI, Visualize, Renderer, Experience, and composition contracts. Do not create a parallel GUI, renderer, host, manifest, or execution pipeline for this Studio.

## Accessibility and learner agency

The same lesson must be usable without spatial navigation or a VR headset. Provide a clear text-first path, keyboard-operable controls, readable state descriptions, and alternatives to sound, color, fine motor interaction, or animation. Motion, narration, and sensory intensity should be adjustable.

Let learners pause, repeat, skip, explore in a different order, and revisit concepts. Do not use streak pressure, manipulative engagement loops, or public social features as a substitute for learning.

## Evidence and maturity

Keep a visible distinction between:
- concept/design;
- authored lesson content;
- deterministic educational simulation;
- GUI/Visualize/Renderer integration;
- host manifestation;
- tests and CI verification;
- any future real GitHub integration.

A polished animation is not proof of a functioning GitHub operation. A simulated Pages deployment is not a deployed website. State exactly what is implemented and verified.

## First implementation milestones

1. **Lesson contract:** define the learner journey, educational state, concept vocabulary, and scenario data for the project-to-Pages journey.
2. **Static living picture:** render the project, its files, change history, and publication path from deterministic scenario data.
3. **Interactive lesson:** let the learner make a small content change, record a simulated commit, and progress through the modeled Pages publication states.
4. **Explanation layer:** reveal the meaning and consequences of each step, with text-first alternatives and a short understanding check.
5. **Host integration:** compose and manifest the Experience through existing contracts; verify the actual path rather than assuming it.
6. **Expansion:** add pull requests, issues, settings, Actions, releases, and collaboration as independent lessons linked through the same project model.

Build one complete, truthful vertical slice before attempting a comprehensive GitHub academy.

## Related University direction

- [Singularity University campus architecture](../../docs/SINGULARITY_UNIVERSITY_CAMPUS_ARCHITECTURE.md)
- [Singularity University learning philosophy](../../docs/SINGULARITY_UNIVERSITY_LEARNING_PHILOSOPHY.md)
- [Build to Illuminate](../../docs/BUILD_TO_ILLUMINATE.md)
- [GitHub Learning Studio delivery issue #45](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/issues/45)

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
