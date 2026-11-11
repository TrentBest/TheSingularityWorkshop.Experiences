![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# Issues: the project's shared memory

> A project needs a place to remember what was noticed, what is being asked, what should change, and why the work matters.

## The learner's situation

A visitor reads a Field Notes entry and notices that its location attribution is missing. The report is real enough to deserve attention, but it is not yet a useful work item: another person needs to understand what is missing, what should have happened, and how anyone will know the problem is resolved.

The learner's task is to turn that report into a shared, actionable record.

## What an issue is

An issue is a durable project record for a problem, question, idea, or unit of work. It gives people a shared place to describe the situation, add evidence, ask questions, record decisions, and connect the work to the changes that address it.

An issue is not the implementation itself. Creating or assigning an issue does not create a branch, change a file, open a pull request, or prove that the problem has been fixed.

The core relationship is:

```text
Observation / question
        |
        v
      Issue  <---- discussion, clarification, labels, ownership
        |
        | guides work; does not perform it
        v
      Branch
        |
        v
      Commits
        |
        v
  Pull request proposal
        |
        v
 Review + checks + explicit merge decision
        |
        v
 Evidence that acceptance criteria are met
        |
        v
      Issue closed
```

The issue remains the record of *why* the work was needed. The branch, commits, and pull request record *how* the proposed change was made and reviewed.

## Build a useful issue

A good issue helps someone who was not present for the original observation understand the work without guessing.

For the Field Notes scenario, the learner should capture:

- **Title:** a concise statement of the problem, not a vague label such as "Fix this".
- **Observed behavior:** the field note appears without its location attribution.
- **Expected behavior:** the entry should identify the place associated with the note.
- **Context:** which entry is affected and how the omission was noticed.
- **Acceptance criteria:** what must be true before the work can be considered complete.

Acceptance criteria should be observable. "Make it better" is not a test. "The affected entry displays its location attribution, and the text-first view exposes the same information" is verifiable.

## Organization helps people find and own work

Labels describe useful categories such as bug, question, or enhancement. They are navigation and classification aids; they do not define the full meaning of the issue.

An assignee indicates who is taking responsibility for moving the work forward. Assignment is not proof that work has started or finished.

A milestone groups work toward a larger goal or time boundary. It is not a guarantee that every issue in it will be completed.

Discussion preserves questions, answers, evidence, and decisions. A useful conversation improves the shared record; it should not leave the essential problem statement hidden in a comment thread.

## Follow the issue into implementation

Once the issue is understandable, a contributor can create a branch and make a focused change. A pull request can then refer back to the issue so reviewers can understand the motivation and intended outcome.

These are distinct records with distinct responsibilities:

| Record | Main question it answers |
|---|---|
| Issue | What needs attention, and why? |
| Branch | Where is this line of work being developed? |
| Commit | What change was recorded? |
| Pull request | What change is being proposed for review? |
| Review | What feedback or decision did a reviewer provide? |
| Checks | What did the automated validation report? |
| Merge | Was the proposal explicitly integrated into the shared line? |

Referencing an issue from a pull request creates traceability. A closing reference can close the issue when the pull request is merged, but closure should still be checked against the issue's acceptance criteria. Workflow automation is not a substitute for verifying the outcome.

## Closure is a claim that should have evidence

An issue should close when its intended outcome has been achieved—not merely because someone created a branch, opened a pull request, or received approval.

If the change is merged but the acceptance criteria are not met, the issue should remain open or be reopened with the remaining gap made explicit. If the problem returns, reopening preserves continuity rather than hiding the history.

## Interactive lesson contract

The deterministic session now models this sequence:

1. Inspect the original, incomplete report.
2. Write a clear issue title and problem statement.
3. Add expected behavior and observable acceptance criteria.
4. Classify the issue and assign a simulated owner.
5. Ask and answer a clarifying question.
6. Create a linked working branch and record a focused change.
7. Open a linked pull request proposal and inspect its review/check/merge states.
8. Record a pass or fail for each acceptance criterion independently; a missing check or any failed check keeps the issue open.
9. Correct failed evidence only after the relevant outcome changes, then verify and close only when every criterion has recorded passing evidence.
10. Reopen the issue in an alternate path where evidence must be collected again or the defect returns.

The learner can record either pass or fail for each criterion. A failed result remains visible and blocks verification and closure; the learner can revise that result after correcting the work. Verification is a separate action available only after every criterion has recorded a pass. Reopening clears the recorded evidence so it must be collected again. A false green ending teaches the wrong lesson.

## Simulation and integration boundary

The current domain implementation is an educational simulation with deterministic in-memory state and a GUI.Core semantic projection. Its issue numbers, labels, people, branches, commits, pull requests, comments, checks, and closure events are teaching data.

It does not call Git, the GitHub API, or modify a real repository or account. It does not imply a live WebApp route or a completed host event loop. GUI.Core command metadata describes available semantic actions; the host remains responsible for dispatch and re-projecting the resulting state.

The authored explanation and a text-first, keyboard-operable route are part of the lesson—not optional fallbacks. The current GUI.Core adapter projects the lesson state, acceptance evidence, discussion, and gated commands; it does not install an event loop. Any future live GitHub integration must be a separately designed and explicitly authorized capability.

## Architectural ownership

- The Experience owns the scenario, educational state, concept relationships, and lesson progression.
- GUI owns learner interaction and semantic control presentation.
- Visualize may present a relational picture when a genuine visual model adds value; it is not required merely to decorate this lesson.
- Renderer remains an observer-relative computational representation, not a text-layout engine.
- The host owns event handling and presentation lifecycle.
- GitHub remains an external service; this simulation does not impersonate it.

## Related journeys

- [GitHub Learning Studio overview](../README.md)
- [Curriculum and delivery issue #45](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/issues/45)
- [Issue-journey implementation issue #48](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/issues/48)
- [Pull-request collaboration journey — draft PR #47](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/pull/47)
- [GitHub Pages journey — draft PR #46](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/pull/46)

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
