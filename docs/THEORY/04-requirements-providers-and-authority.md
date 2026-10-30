![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# 04 — Requirements, Providers, and Authority

## 01 What

An Experience can say what semantic capabilities it needs without naming the particular host control or implementation that will provide them.

A **requirement** describes a need. A **provider** declares that an artifact implements a capability contract. A **selection** records which compatible provider artifact satisfies a requirement. **Authorization** decides whether a particular operation is allowed now.

These are four different questions. Treating them as one is a common source of accidental authority.

## 02 Why

If a manifest says “WPF button” or “DOM click,” it describes an implementation detail rather than the semantic ability the Experience needs. If a provider match is treated as permission, a host may perform an operation that the current domain state or actor is not allowed to perform.

The Experience therefore describes semantic capability IDs and compatible contract versions. The host discovers providers. A deterministic resolver can match declarations, but the domain still validates every request.

## 03 How

The Experiences contract layer is intentionally pure: no host APIs, filesystem access, network calls, or package dependencies are required for the matching logic.

The inspected `SemanticRequirementResolver` performs two distinct operations:

1. **Provider matching:** compare exact capability IDs and contract version ranges; reject missing required providers and unresolved ambiguity; return explicit selections in deterministic order.
2. **Artifact closure resolution:** validate immutable artifact identities and catalog descriptors; resolve dependencies; reject duplicate identities, missing dependencies, and cycles; return a dependency-ordered closure.

The resolver does not load code, fetch artifacts, verify downloaded bytes against a repository, compose a runtime, or grant domain authority. Those responsibilities belong to the repository/host/composition and domain layers respectively.

### A failure-first model

| Condition | Correct result |
|---|---|
| Required capability has no compatible provider | Fail resolution |
| Several artifact providers match and no deterministic policy selects one | Fail as ambiguous |
| Artifact identity is malformed | Reject descriptor |
| Required dependency is missing | Fail closure resolution |
| Dependency graph contains a cycle | Fail closure resolution |
| Provider matches but actor/domain request is unauthorized | Reject the operation at the domain boundary |

A valid provider declaration is metadata, not proof that its implementation behaves correctly. Contract tests and host integration tests are still needed.

## 04 See it in a minute

A semantic requirement contains a capability ID, a version range, whether it is required, and an optional rationale:

```csharp
public sealed record SemanticInteractionRequirement(
    string CapabilityId,
    ContractVersionRange ContractVersions,
    bool Required = true,
    string? Rationale = null);
```

A provider separately declares its provider ID, immutable artifact identity, and supported capabilities. Matching is exact and version-aware. If more than one distinct artifact satisfies the same requirement, the current resolver reports ambiguity instead of silently picking by display name or platform.

**Source-checked:** `src/Contracts/SemanticRequirements.cs` and [Semantic Interaction Requirements Contract](../contracts/SEMANTIC_INTERACTION_REQUIREMENTS.md). The contract document explicitly says that host provider catalogs and end-to-end repository/FSM_COS integration are not yet implemented by this layer.

## 05 Choose your next step

- Read the [Semantic Interaction Requirements Contract](../contracts/SEMANTIC_INTERACTION_REQUIREMENTS.md) for normative details.
- Read [FSM_COS](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS) for composition/arbitration.
- Read [FSM_UserIO](https://github.com/TrentBest/TheSingularityWorkshop.FSM_UserIO) for the semantic input/provider boundary as it evolves.

### Practice

1. Can a capability provider grant a visitor permission to submit a command? Why not?
2. Why should an ambiguous provider match fail closed?
3. Name one claim a pure resolver can make and one it cannot make without repository or host integration.

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
