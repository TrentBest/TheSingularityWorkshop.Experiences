![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# Experiences CODE RED Validation Profile

**Profile:** `experiences.code-red`  
**Profile version:** `0.1.0-draft`  
**Status:** Draft — not an executable authorization profile  
**Pilot task:** [Minesweeper 2D → 3D, issue #36](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/issues/36)  
**Integration owner:** TheSingularityWorkshop.Experiences  
**Control-plane dependencies:** Foundry, HeadlessAi, CiAi

This document records the repository-specific validation facts and the gates that must be closed before Foundry may authorize an Experiences LLM candidate. It deliberately does not claim that CODE RED task claiming, candidate dispatch, evidence acceptance, or autonomous repair is operational.

## 1. Ownership and candidate boundary

- Experiences owns the authored Experience under `experiences/{ExperienceKey}/`, its identity, manifest, configuration, MicroBundle/artifact declarations, and host-independent domain semantics.
- The initial authorized task is issue #36. Its first authored target is `experiences/Minesweeper/`; the agent must not substitute a project-only sample or a hard-coded host demo.
- The Experiences candidate is an immutable commit in `TrentBest/TheSingularityWorkshop.Experiences`.
- The trusted workflow definition is a separate identity. A trusted workflow ref/SHA must never be inferred from the candidate commit.
- The PR head SHA, GitHub pull-request merge SHA, and actual checkout/tested SHA are distinct provenance fields. A passing check is not exact-candidate evidence until the tested checkout is independently proven.
- No agent-authored output grants repository, CI rerun, merge, publication, deployment, or branch-management authority.

## 2. Existing manifest and artifact contracts

The current authored example is Castle Siege:

- Authored manifest: `experiences/CastleSiege/contracts/castle-siege-experience-manifest.example.json`
- Schema: `experiences/CastleSiege/contracts/castle-siege-experience-manifest.schema.json`
- Contract implementation: `src/CastleSiege/CastleSiegeExperienceManifest.cs`
- Shared artifact identity and closure: `src/Contracts/SemanticRequirements.cs`

The Castle Siege manifest is explicitly schema version 1 and has a Castle-Siege-specific fixed ID/key. It is a useful example of the separation between Experience identity, artifact references, and creator-declared capabilities; it is **not** a generic manifest contract that Minesweeper can copy unchanged.

The shared contract layer provides deterministic semantic-provider matching and immutable artifact-closure resolution. This feature branch now adds a general `ExperienceManifest` contract in `src/Contracts/ExperienceManifest.cs`, plus its JSON Schema and an illustrative Minesweeper manifest under `docs/contracts/`. The generic contract reuses the existing `ArtifactIdentity` and `SemanticInteractionRequirement` types; it requires content-pinned artifact roots, validates stable Experience identity and semantic version ranges, rejects unknown JSON properties/schema versions, and emits deterministic compact JSON. `ExperienceManifestResolver` now combines manifest roots, unambiguous capability-provider selection, and the existing artifact-closure resolver into a deterministic dependency-first resolution result. It is pure contract logic: it performs no repository IO, artifact materialization, executable loading, or host selection. Contract tests cover validation, canonical ordering, strict parsing, the checked-in example, provider selection, missing/ambiguous providers, and artifact closure.

**Observed implementation validation:** PR workflow run [41227800894](https://github.com/TrentBest/TheSingularityWorkshop.Experiences/actions/runs/41227800894), attempt 1, completed successfully on candidate SHA `b0e08d3bd65d7dd0b18fb7d114267f5e34517ba6`. The semantic-contract test job passed 30/30 tests, the generic manifest schema/example validation passed, and the full PR workflow concluded success. The package and development-only benchmark jobs were skipped as expected for a pull-request run. This is ordinary PR CI evidence for this code revision; it is not yet a CODE RED evidence packet or proof of exact checkout provenance.

This still does not provide a general Experience catalog, manifest-to-artifact materialization, or a supported manifest-to-running-host loader.

For an execution-eligible manifest, all referenced artifacts must resolve to valid immutable identities (including SHA-256), every dependency must be present in the same trusted catalog, and the dependency closure must be acyclic. A nullable development hash in the Castle Siege example is not sufficient authorization to load executable bytes.

## 3. Existing repository and runtime loading boundaries

The cross-repository inspection found useful loading components that must be reused rather than replaced:

- `MicroBundleRepository.Core` defines `IExperienceRepository` / `IExperienceCatalog` for immutable Experience bytes and publication pointers, plus `IMicroBundleRepository` for immutable MicroBundle bytes.
- `RestExperienceClient` can read an Experience publication address and retrieve an immutable Experience artifact. Its constructor path verifies the SHA-256 through `ExperienceArtifact`; it is read-only and does not parse manifests or compose bundles.
- `RestMicroBundleCatalog` uses `RestMicroBundleRepository` and `IMicroBundleArtifactMaterializer` to preload exact-version MicroBundle dependency closures, then exposes the resident bundles through FSM_COS `IMicroBundleCatalog`.
- `MicroBundleDomain.IMicroBundleLoadContext` is deliberately host-neutral and supplies runtime identity plus per-bundle configuration. It is not an artifact repository or loader.
- `FileSystemMicroBundleRepository` is an existing local filesystem implementation for MicroBundle artifacts.

**The local Experience activation path is still incomplete.** The inspected repository has no local `IExperienceRepository` / `IExperienceCatalog` implementation, no Experiences-owned adapter that turns the generic manifest into the repository's immutable MicroBundle addresses, and no local equivalent of the REST catalog/materializer path. The REST Experience client is not a local host. These are concrete gaps, not reasons to create another catalog or runtime in Experiences.

**Identity mismatch to resolve before materialization:** the current Experiences `ArtifactIdentity.BundleId` is a string, while MicroBundleRepository's `MicroBundleArtifactAddress.BundleId` and MicroBundleDomain's `MicroBundleDescriptor.Id` are `ulong`. Castle Siege's existing manifest example uses the logical string artifact ID `workshop-castle-siege`, while its MicroBundle implementation uses numeric ID `3301`. Do not silently parse, derive, or guess a numeric identity from the logical string. A reviewed cross-repository mapping contract (or a compatible shared identity contract) is required before a manifest can resolve to a repository artifact address.

The next loader slice must therefore reuse the existing repository interfaces, materializer, and FSM_COS catalog contract; first close the identity mapping and local Experience artifact repository gaps. It must not add a parallel loader, bundle catalog, or composition runtime to Experiences.

## 4. Existing CI workflow identity

The current general workflow is:

- Repository: `TrentBest/TheSingularityWorkshop.Experiences`
- Workflow path: `.github/workflows/build.yml`
- Workflow name: `build`
- Triggers: push to `master`/`development`, pull request, and `workflow_dispatch`
- Runtime: GitHub-hosted Ubuntu 24.04; .NET 8

The following job IDs and named steps exist in the checked-in workflow:

| Job ID | Required observed steps / scope |
|---|---|
| `build` | Matrix Restore → Build → Pack → Stage assembly artifact. The matrix includes the Contracts and Castle Siege projects among the repository's maintained projects. |
| `test-semantic-contracts` | Restore semantic contract tests → Run semantic contract tests. |
| `test-castle-siege` | Restore Castle Siege tests → Run Castle Siege contract tests → restore/build/smoke-test the console Wargaming Hall → restore/build/smoke-test the graphical Wargaming Hall API. |
| `source-integration` | Checkout Experiences, MicroBundleDomain, GUI, and Physics; restore and build the selected source graph. |
| `source-integration-tests` | Checkout the same source repositories; run Moniker.Motion.Wave, Drone Construction, Physics, Castle Siege, and Capsule Clinic source-graph tests. |
| `test-capsule-clinic` | Restore Capsule Clinic tests → Run Capsule Clinic rules and deterministic-loop tests. |
| `test-domino-studio` | Restore layout tests → Run layout and scene contract tests. |
| `test-marble-run` | Restore Marble Run tests → Run Marble Run scene contract tests. |
| `test-games-cards` | Restore card tests → Run card capability tests. |
| `test-drone-construction` | Restore Drone Construction tests → Run construction/occupancy/transition/inventory invariants → Upload renderer-produced snapshot SVG. |
| `validate-experience-manifest` | Install JSON Schema validator → Validate the generic Experience manifest schema/example and pinned artifact-root shape. |
| `validate-experience-room-catalogs` | Install JSON Schema validator → Validate laboratory room catalogs and navigation links. |
| `drone-construction-showcase` | Run deterministic browser showcase → Upload generated HTML. |
| `benchmark-quadrotor-populations` | Development-branch push only; runs the reproducible quadrotor population benchmark against the Physics development source. It is not a required PR candidate check in its current conditional form. |
| `test-chemistry` | Run chemistry conservation tests. |
| `test-moniker-motion` | Restore motion tests → Run motion contract tests. |

This is an inventory of existing checks, not a claim that each job is already registered as a required CODE RED check. The profile implementation must capture the actual workflow run ID, attempt, job IDs, step conclusions, and required artifacts from GitHub's observed run data. Missing, skipped, cancelled, or unobserved required checks cannot be interpreted as success.

### Explicitly excluded workflow path

The existing `package` job runs on `workflow_dispatch` and has `packages: write` permission. It restores and packs projects, then uploads an `experience-packages` workflow artifact. The current YAML does not contain a package-push step, so this is a packaging/artifact-production path rather than proof of an actual NuGet publication.

**Do not use this workflow's generic dispatch as the CODE RED candidate-validation entry point.** It is not a validation-only workflow, and a candidate agent must not receive a path that can trigger unrelated package production or artifact staging.

A dedicated validation workflow must have read-only permissions, no package publishing, no deployment, and explicit separation between trusted workflow code and the immutable candidate checkout.

The current isolated pilot is `.github/workflows/minesweeper-candidate-validation.yml`. It is dispatched only from `development`, requires the exact repository and workflow path/ref, checks that the workflow-definition SHA equals the dispatch event SHA, fetches the supplied candidate branch, and requires the fetched branch tip to equal the supplied full candidate SHA before archiving and testing that exact commit. It checks out the trusted test suite from the workflow commit, copies only the candidate's `src/Minesweeper` project into the trusted workspace, and uploads a run/attempt-scoped TRX report, provenance record, and dispatch marker. It has read-only contents permission and no package/deployment step. This is an isolated Minesweeper pilot, not yet a general CODE RED executor.

## 5. Cross-repository source reproducibility

The existing `source-integration` and `source-integration-tests` jobs checkout MicroBundleDomain, GUI, and Physics from their moving `development` refs. Therefore an Experiences candidate SHA alone does not fully identify the source graph used by those jobs.

Before these jobs can be used as reproducible CODE RED evidence, the run must also record and verify the resolved commit SHA for every external repository checkout. Alternatively, the dedicated validation workflow must pin approved dependency SHAs. A branch name is not an immutable dependency identity.

## 6. Required candidate-validation inputs

The future dedicated workflow and CiAi profile must carry, validate, and preserve these separate values:

| Field | Meaning |
|---|---|
| `taskId` | Foundry-issued task correlation for the authorized issue #36 work. |
| `repository` | Exact repository full name; must equal `TrentBest/TheSingularityWorkshop.Experiences`. |
| `candidateRef` | Authorized feature-branch ref; not a trusted workflow ref. |
| `candidateSha` | Full immutable candidate commit SHA to checkout and test. |
| `workflowRef` | Approved trusted control-plane ref containing the validation workflow. |
| `workflowSha` | Immutable commit SHA from which the workflow definition was loaded. |
| `profileId` / `profileVersion` | `experiences.code-red` / `0.1.0-draft` until an executable profile version is approved. |
| `attempt` | Monotonic attempt number assigned by the authorized coordinator. |

Reject repository mismatch, non-full or malformed SHA, unresolved candidate refs, branch movement between authorization and checkout, a checkout SHA other than `candidateSha`, unapproved workflow provenance, and dependency checkouts whose resolved SHAs are absent or disallowed.

The workflow must emit a machine-readable checkout attestation from the actual checked-out repository state. A caller-supplied SHA string or a workflow-run `head_sha` field alone is not proof of the commit tested.

## 7. CiAi evidence acceptance

The target evidence packet is the versioned contract under review in [CiAi PR #28](https://github.com/TrentBest/TheSingularityWorkshop.CiAi/pull/28), with the policy tracked in [CiAi issue #27](https://github.com/TrentBest/TheSingularityWorkshop.CiAi/issues/27).

Do not accept a packet as exact-candidate evidence until it proves all of the following:

1. Trusted workflow repository, path, ref, and workflow-definition SHA.
2. Candidate repository, authorized ref, immutable candidate SHA, and actual checkout/tested SHA.
3. Separate PR head and synthetic merge identities when the event is `pull_request`.
4. Workflow run ID and attempt number, with every retry retained.
5. Every required job and step observed with an acceptable conclusion; no missing or skipped required work.
6. External dependency checkout identities for source-graph jobs.
7. Expected artifact names, run association, digest, size, and retrieval/provenance metadata.
8. Evidence delivery receipt and Foundry correlation to the same `taskId`.

**Open provenance blocker:** the current CiAi PR #28 review identifies a factory path that can default candidate SHA from run head SHA and conflate workflow-definition identity with candidate identity. For a `pull_request` event, the default tested ref is generally the synthetic merge ref. CiAi must represent workflow-definition SHA, PR head SHA, and actual checkout/tested SHA independently, and derive the latter from trusted checkout/event evidence.

A green workflow conclusion without these proofs is ordinary CI status, not an accepted CODE RED evidence packet.

## 8. Operational gates

| Gate | Current status |
|---|---|
| Isolated Experiences feature branch from current development | Created; generic manifest contract and validation documentation are committed. |
| Existing semantic-provider and artifact-closure contracts | Implemented; contract tests exist. |
| Generic Experience manifest and deterministic resolver | Implemented on this feature branch; schema, example, strict parser, canonical serializer, provider selection, artifact closure, and tests added. PR CI run 41227800894 passed on SHA `b0e08d3bd65d7dd0b18fb7d114267f5e34517ba6`. |
| Manifest-to-artifact materialization and supported local loader | Not yet established by repository evidence. |
| Dedicated read-only candidate-validation workflow | Not implemented. |
| Exact tested-checkout provenance and external dependency pinning | Not yet proven. |
| CiAi v2 evidence acceptance for exact candidate checkout | Blocked on PR #28 provenance correction and integration. |
| Foundry task claim, authorization, budget, retry, and draft-PR lifecycle integration | Not yet demonstrated in Experiences. |
| HeadlessAi provider-neutral resumable turn integration | Not yet demonstrated in Experiences. |
| Experiences LLM authors manifest-defined Minesweeper | Not started; must be the agent pilot, not manual implementation. |
| Red → repair → green evidence cycle | Not demonstrated. |
| Local load/play of manifest-defined Minesweeper 2D | Not demonstrated. |
| Same Experience extended to first-person 3D | Not started. |
| MyVR integration/deployment | Deliberately deferred until local Experiences proof. |

## 9. Activation and authority rules

- Work only on an isolated feature branch based on current `development`; do not write to `master` or `development`.
- Keep this workflow/profile implementation on a feature branch and reviewable PR first.
- GitHub requires a `workflow_dispatch` workflow definition to exist on the default branch before remote dispatch can activate it. Do not merge to `master` merely to unlock dispatch. Stop and request explicit owner authorization at that gate.
- A green validation packet does not authorize merge, release, package publication, deployment, or branch deletion.
- Foundry—not the LLM—authorizes claims, source mutations, reruns, budgets, and PR handoff.
- Preserve red and green evidence for every attempt; never overwrite a failed attempt with a later pass.

## 10. Pilot sequence

1. Close the contract and provenance gates above without implementing Minesweeper gameplay by hand.
2. Have Foundry claim issue #36 and authorize an isolated Experiences feature branch.
3. Have the Experiences LLM author the 2D Minesweeper Experience under `experiences/Minesweeper/`.
4. Validate the exact candidate, deliver red diagnostics, resume the same task correlation, and observe a corrective candidate and green evidence packet.
5. Resolve the authored manifest and immutable artifact closure, then load and play it through a supported local Experiences host.
6. Have the same Experience identity evolve to first-person 3D and repeat validation/loading.
7. Only after the repository-local pilot is independently demonstrated should MyVR integration be attempted.

---

<p align="center">
  <img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="112" />
</p>
