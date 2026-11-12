# Branch Reconciliation

**Audit date:** 2026-11-12  
**Repository:** `TrentBest/TheSingularityWorkshop.Experiences`  
**Canonical policy:** exactly `master` (released baseline) and `development` (active work).  
**Audit base:** `development` at `51f4a9006930975967312e3a37baf63b1d88af7e`.

This is a point-in-time owner decision aid. It does not authorize branch deletion, PR closure, merge, force-push, release, or deployment.

## Live inventory

GitHub reported eight branches at the audit time:

| Branch | Role / evidence | Compare with development | PR / CI | Disposition |
|---|---|---:|---|---|
| `master` | Released baseline | Canonical | Not applicable | **KEEP** |
| `development` | Active work | Canonical | Not applicable | **KEEP** |
| `codex/capsule-clinic-intent-smoke` | Workflow-only Capsule Clinic semantic-intent smoke test; modifies `.github/workflows/capsule-clinic-web.yml` | 1 ahead, 0 behind | Draft [PR #53](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/pull/53); browser slice and build succeeded | **PRESERVE pending review** |
| `feature/ciai-v1-provenance-producer` | Minesweeper candidate-validation workflow changes; modifies `.github/workflows/minesweeper-candidate-validation.yml` | 2 ahead, 0 behind | Draft [PR #57](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/pull/57); observed build run succeeded | **PRESERVE pending review**; directly related to CiAi/Minesweeper proof path |
| `feature/experience-microbundle-identity-map` | Adds identity-map and artifact-binding contracts, tests, and contract documentation | 14 ahead, 0 behind | Draft [PR #54](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/pull/54); observed build run succeeded | **PRESERVE pending review** |
| `feature/galactic-war-perception-and-perspective` | Adds Galactic War campaign, perception, diplomacy, provenance, StoryBuilder crosswalk, and media design/runtime files | 29 ahead, 0 behind | Draft [PR #56](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/pull/56); observed build run succeeded | **PRESERVE pending review** |
| `feature/minesweeper-playable-2d` | Branch head is exactly the current `development` SHA; compare reports identical, 0 ahead / 0 behind, no changed files | Identical | No open PR found for this branch; no branch-name matches found in repository code search | **READY FOR OWNER DELETION** |
| `codex/experiences-branch-reconciliation-2026-11-12` | Temporary audit/implementation branch for this reconciliation; contains the Sith publication gate and this audit | 1 ahead, 0 behind | Draft [PR #58](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/pull/58); exact-head Actions run #43186345179: 30 jobs succeeded, 0 failed | **PRESERVE until PR #58 is resolved; then retire** |

## Evidence and limits

- Every noncanonical branch was compared against the audit's `development` SHA using GitHub's compare endpoint.
- The four feature branches with open PRs each contain unique work and are not candidates for deletion. Their observed CI results are recorded above; this is not a substitute for final PR review.
- `feature/minesweeper-playable-2d` points to the exact same commit as `development`. No open PR was found, and repository code search returned no branch-name matches. This supports deletion readiness, but the owner should still confirm no external/local worktree depends on it before deleting.
- Code search was scoped to the repository's indexed source and is not proof that no external consumer, local clone, workflow history, or unindexed reference exists.
- The live inventory includes this audit's temporary branch. After the owner resolves PR #58 and retires its head branch, repeat the inventory check before considering the repository Sith-compliant.
- The current publication workflow gate is proposed in PR #58. Until that change is reviewed and integrated, do not treat the gate as active on `development` or `master`.

## Owner action

1. Review the four useful feature PRs independently; do not force-integrate stale or unrelated work.
2. If confirmed safe, include `TrentBest/TheSingularityWorkshop.Experiences:feature/minesweeper-playable-2d` in the owner's single branch-cleanup batch.
3. Resolve PR #58, then retire its temporary branch when appropriate.
4. Re-query GitHub. The final persistent inventory must be exactly `master` and `development`.

No branch has been deleted, no PR has been merged or closed, and no publication or deployment was performed by this audit.
