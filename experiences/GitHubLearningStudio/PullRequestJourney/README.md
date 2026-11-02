![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# Pull Request Collaboration Journey

> A pull request is a proposal for changing a shared project—not a button that automatically changes it.

## Learner goal

Help a learner understand how a project can accept improvements without treating every proposed change as immediately trusted or automatically merged.

The learner should finish able to explain these separate events:

- **Proposal:** an author opens a pull request to gather a change and its context.
- **Review:** another person examines the proposal and may comment or request changes.
- **Revision:** the author adds commits; the same proposal evolves.
- **Checks:** automated processes provide evidence about the proposed change.
- **Approval:** an authorized reviewer signals that the proposal meets the review requirements.
- **Merge:** an authorized decision-maker integrates the accepted change into the shared line.

A passing check is not an approval. An approval is not a merge. A pull request is not itself a commit.

## Scenario

Continue the fictional **Field Notes** project. A task asks for a clearer project introduction. The learner works on a separate branch, records commits, opens a proposal, receives a request for a clearer explanation, revises the same proposal, observes checks, obtains simulated approval, and makes an explicit simulated merge decision.

Use a second task or alternate starting state when learners enter this journey independently. Do not require completion of the Pages journey.

## State model

The Experience domain should own a deterministic state model containing:

- Task identity, description, and status.
- Source and target branches.
- Proposed commits and changed-file summary.
- Pull-request identity, title, description, and lifecycle state.
- Review entries, reviewer role, and requested changes.
- Check runs with explicit pending, passed, and failed outcomes.
- Approval state distinct from check state.
- Merge eligibility, authorization context, and merge outcome.
- A learner-facing explanation and the currently available semantic commands.

Snapshots should be immutable. Transitions should be deterministic and reject invalid ordering. Keep the model small enough to teach the core relationship rather than recreating every GitHub feature.

## Suggested teaching sequence

1. **Start with the task.** Show the problem in terms of the project and its users.
2. **Work separately.** Create a task branch and make a meaningful commit.
3. **Make a proposal.** Open a pull request with a clear explanation and a link to the task.
4. **Review the change.** Let the learner inspect the changed files and reviewer feedback.
5. **Revise the proposal.** Request changes, add a follow-up commit, and show the same proposal updating.
6. **Read the evidence.** Run checks; include a failed-check path that blocks merge until resolved.
7. **Separate approval from evidence.** Show that both are required when the simulated project policy says so.
8. **Make the merge decision.** Require an explicit authorized simulated actor and a valid merge state.
9. **Observe the result.** Show the target branch history after merge and connect it back to the original task.

## GUI and presentation

Use GUI.Core to project semantic state and available commands. Controls declare stable command metadata; the host owns real event handling and re-projection.

Keep the explanation text authored by the Experience. A compact branch-and-proposal timeline may help learners see how commits, reviews, checks, approval, and merge relate. Use Visualize only if its actual spatial/relational capabilities materially improve this model; do not force static SVG geometry to own lesson text or controls. Renderer remains computational visibility/detail policy, not a text renderer.

Every screen must state that the environment is simulated. No real Git commands, GitHub API operations, PR creation, review submissions, approvals, checks, merges, or repository writes occur.

## Assessment

Ask learners to reason about cases, not memorize terms:

- All checks pass, but no reviewer has approved. Can this proposal merge under the modeled policy?
- A reviewer requests changes. Does the author need a new pull request or can the existing proposal be updated?
- A reviewer approves, but a required check fails. Is the proposal mergeable?
- A proposal is approved and all required checks pass. What separate action changes the shared target branch?

The learner should be able to explain why each gate exists.

## Accessibility and agency

Provide a complete text-first route, keyboard-operable controls, explicit status words in addition to color, and reduced-motion alternatives. Let learners replay the review/revision loop and explore failed paths without penalty. No VR prerequisite or age gate.

## Implementation boundary

This design contract is implemented as a deterministic Experience-owned session in `src/GitHubLearningStudio.PullRequests/`, with a GUI.Core semantic projection and focused tests in `tests/GitHubLearningStudio.PullRequests.Tests/`. The model distinguishes proposal lifecycle, review state, check state, and merge authorization. The host must still supply real event handling and re-project snapshots; no WebApp route or live GitHub integration is claimed. CI verification is pending for this draft branch.

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
