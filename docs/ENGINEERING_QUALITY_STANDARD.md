# Engineering Quality Standard

This document defines the acceptance bar for code and documentation in Experiences. It is a working engineering contract, not a claim that every existing project already satisfies every gate.

## The three records of truth

Every capability should have three mutually consistent records:

1. **Implementation** — what the code and executable artifacts actually do.
2. **Engineering documentation** — how to build, configure, use, test, diagnose, and integrate the capability.
3. **Theory and rationale** — why the boundary exists, which alternatives were considered, what trade-offs are accepted, and which invariants the design is meant to preserve.

XML comments support API discovery; they do not replace the other two records. A README supports adoption; it does not replace a precise contract. A theory essay explains reasoning; it must not be presented as proof that the implementation already fulfills the idea.

## Evidence labels

Use explicit status language throughout docs:

- **Implemented** — present in the checked-in code and covered by relevant tests.
- **Verified** — exercised by a named, reproducible check whose result is available.
- **Planned** — intended but not implemented.
- **Proposed** — under consideration; no commitment implied.
- **Blocked** — progress depends on a named unresolved prerequisite.

Do not describe planned behavior in the present tense. Do not call a package publish-ready solely because it builds.

## Definition of done

A code change is ready for review only when the applicable items below are satisfied or clearly listed as a blocker.

### Correctness and compatibility

- [ ] The intended behavior is stated before implementation.
- [ ] Existing public contracts and downstream consumers have been checked.
- [ ] Boundary conditions, invalid input, empty state, and failure behavior are explicit.
- [ ] Deterministic tests cover logic that can be deterministic.
- [ ] Tests exercise observable behavior, not merely implementation details.
- [ ] No unrelated behavior is silently changed to make a check green.

### Build and diagnostics

- [ ] Restore, build, and relevant tests pass on the supported target framework.
- [ ] No new compiler warnings are introduced; the goal for maintained projects is zero warnings.
- [ ] New or touched projects treat warnings as errors where practical.
- [ ] CI failures are reported by root cause, not hidden, skipped, or reclassified.
- [ ] Package output is inspected when packaging behavior is part of the change.
- [ ] Publishing remains a separate, explicitly authorized action.

A green unit-test job is not equivalent to a green repository workflow. Report each relevant result separately.

### Documentation

- [ ] Public APIs explain purpose, parameters, return values, exceptions, mutability, and lifecycle where applicable.
- [ ] The README gives a newcomer a truthful overview and a runnable or verifiable first step.
- [ ] Detailed engineering documentation records contracts, examples, integration steps, limitations, and troubleshooting.
- [ ] Diagrams distinguish implemented paths from conceptual or future paths.
- [ ] Examples compile or are clearly marked as illustrative.
- [ ] Package and repository metadata agree with actual project behavior.
- [ ] Accessibility, security, determinism, and versioning implications are documented when relevant.

### Theory

- [ ] The design problem is stated independently of the chosen implementation.
- [ ] The proposed principle or model is defined precisely enough to challenge.
- [ ] Assumptions, scope, trade-offs, counterexamples, and failure modes are acknowledged.
- [ ] Claims are supported by code evidence, tests, measurements, references, or clearly labeled reasoning.
- [ ] The theory points to the contracts that could falsify or validate it.
- [ ] Terminology remains consistent across related Workshop repositories.

Theory is strongest when it is falsifiable: state what observation would show that the proposed boundary or principle is inadequate.

## Change-specific review record

For substantial changes, the PR description should include:

- **Intent:** the user-visible or architectural problem.
- **Boundary:** which repository/layer owns the behavior, and which layers must not own it.
- **Evidence:** exact build/test commands and observed results.
- **Documentation:** which implementation guide and theory record changed.
- **Known gaps:** unresolved warnings, compatibility risks, missing tests, or future work.
- **Release status:** whether the artifact is merely buildable, tested, publishable, or actually published.

Do not use “fully documented” as a synonym for “XML comments added.” The goal is that a reader can understand what the capability does, why it exists, how to verify it, and where its limits are without having to infer missing architecture from source code alone.
