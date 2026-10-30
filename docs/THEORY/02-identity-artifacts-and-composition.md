![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# 02 — Identity, Artifacts, and Composition

## 01 What

A visitor chooses an Experience, but a runtime may need several independently built capabilities to present it. Those capabilities can be versioned and delivered separately.

The key distinction is between four identities:

1. **Experience identity** — identifies the authored encounter and its version.
2. **MicroBundle identity** — identifies a composable capability unit.
3. **Artifact identity** — pins one concrete immutable build of a MicroBundle, including its content hash.
4. **Runtime composition** — the resolved set of concrete MicroBundles that FSM_COS composes for a host.

These are related, but they are not interchangeable. A project, assembly, NuGet package, MicroBundle, and Experience may overlap in implementation, but they answer different questions.

## 02 Why

If a manifest names only a friendly bundle name or a moving version, a host cannot reliably know which bytes it is about to compose. If a host decides the meaning of an Experience from whichever assemblies happen to be compiled into it, the host becomes an accidental source of truth.

Immutable artifact identity makes the intended selection auditable: the manifest or resolved composition can identify a bundle ID, version, and SHA-256 content hash. It still does not prove that the bytes are safe or that the implementation behaves correctly; verification and policy remain necessary at the trust boundary.

## 03 How

The intended flow is:

```text
Authored Experience
       |
       v
Experience manifest
       |
       v
Bundle identity + version + content hash
       |
       v
MicroBundleRepository
       |
       v
Retrieved bytes and identity verification
       |
       v
Host materialization / catalog
       |
       v
FSM_COS composition
       |
       v
Host-specific manifestation
```

This diagram describes the architectural path. The repository's current documentation explicitly identifies parts of this path as unfinished; do not read the arrows as proof of a complete runtime pipeline.

### What is implemented in this repository

- The publication guide describes Forge and Moniker build outputs as ordinary .NET assemblies implementing `IMicroBundle`.
- The guide distinguishes build, identity, publication, and composition as separate actions.
- The Castle Siege manifest type demonstrates a versioned Experience declaration with artifacts and capability requirements, deterministic JSON ordering, and validation. Its artifact SHA-256 may be null, which means the artifact is not yet content-pinned.
- The semantic contract layer defines `ArtifactIdentity`, `ArtifactDescriptor`, and dependency-closure resolution. It validates identity shape, detects duplicate descriptors, missing dependencies, and dependency cycles, and returns dependency-ordered descriptors.
- The neutral MicroBundle contract is owned outside this repository; this repository supplies domain-specific MicroBundles and Experience content.

These are separate examples of the evolving model, not evidence that one generic manifest/runtime pipeline already consumes every Experience.

### The repository is not the composition engine

The Experiences repository should describe and package domain capabilities. MicroBundleRepository stores and delivers opaque artifacts. The host is responsible for obtaining bytes and materializing the implementation in the supported way. FSM_COS resolves and composes the requested dependency closure; it does not become the Experience's authoring database or the host's application lifecycle manager.

## 04 See it in a minute

The Experience-owned semantic contract represents an artifact identity as:

```csharp
public sealed record ArtifactIdentity(
    string BundleId,
    string Version,
    string Sha256);
```

Its `IsValid` check requires non-empty identity components and a 64-character hexadecimal hash. Its `StableKey` length-prefixes the bundle ID and version before appending the normalized hash, avoiding delimiter ambiguity in the identity components.

The dependency resolver then:
1. validates catalog descriptors and identities;
2. rejects duplicate artifact identities;
3. finds each requested root by exact stable identity;
4. traverses dependencies in deterministic order;
5. detects missing dependencies and cycles;
6. returns the dependency closure in dependency-before-dependent order.

**Source-checked:** `src/Contracts/SemanticRequirements.cs`. This chapter describes the inspected implementation; it does not claim a repository download, byte materialization, or FSM_COS launch was executed by this documentation review.

## 05 Choose your next step

- Read [Experience publication](../PUBLICATION.md) for the current artifact boundary.
- Read the [StoryBuilder → Experiences → AnyApp first-run contract](../STORYBUILDER_ANYAPP_FIRST_RUN.md) for the detailed package-to-host integration gaps.
- Read [FSM_COS](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS) for composition and arbitration theory.

### Practice

1. Why is an Experience version not a substitute for a MicroBundle artifact hash?
2. What can a dependency-closure validator prove before loading code? What can it not prove?
3. Where should the host report an artifact that is present but has the wrong content hash?

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
