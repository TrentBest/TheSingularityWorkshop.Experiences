![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# 01 — What Is an Experience?

## 01 What

Imagine visiting a science museum. The museum's identity, rooms, exhibits, experiments, visitor choices, and rules do not become different things because one visitor sees them on a phone and another uses a desktop application. The presentation changes; the thing being experienced remains recognizable.

That is the central idea of an Experience in The Singularity Workshop.

An **Experience** is an independently identifiable body of authored domain meaning: what the visitor encounters, what they can do, what rules govern those actions, what capabilities are needed, and what content or computation makes the encounter meaningful.

An Experience is not synonymous with:
- a web page or desktop window;
- a GUI tree or SVG;
- a renderer or visualization backend;
- a MicroBundle assembly;
- a host process;
- a composition engine.

Those things may participate in presenting or running an Experience, but none alone defines the whole Experience.

## 02 Why

If the Experience is defined by its current window, moving it to another host tends to duplicate its meaning. The browser version accumulates one set of rules, the desktop version another, and a future immersive version a third.

Separating the Experience from its manifestation makes a useful promise possible: presentation can change without silently changing domain identity or authority.

The separation has a cost. The Experience must describe its requirements and semantics explicitly, and hosts need adapters that preserve those meanings. A shared identity does not automatically synchronize state, provide equivalent controls, or make every host capable of every feature.

## 03 How

The repository README describes the Experience as the authored unit and WebApp, AnyApp, and VR as manifestations. Its examples show different maturity levels:

- **Forge and Moniker** are independent MicroBundle projects and artifact seeds.
- **Elements** demonstrates a small domain atom: an immutable `Element` definition exposed by an `ElementMicroBundle`.
- **Chemistry** contains domain records and a deterministic reaction-balance analyzer. The analyzer checks atom and charge accounting; it does not predict whether a reaction will occur.
- **ExperienceRoomCatalog** models visitor-facing rooms with purpose, visitor action, authoritative-state description, evidence rule, maturity, capabilities, and navigation.

These examples illustrate why the repository is not simply a collection of screens. A room can describe what a visitor does and what evidence makes an outcome meaningful without embedding a specific renderer into that definition.

### A room as a semantic contract

The catalog's `ExperienceRoom` includes:
- a stable room key and sequence;
- title and purpose;
- visitor action;
- a description of authoritative state;
- an evidence rule;
- a maturity classification;
- required capabilities and next-room links.

The maturity enum distinguishes Concept, AuthoredData, Calculated, Simulated, and Validated. This is important: an authored exhibit is not automatically a calculated result, and a simulation is not automatically a validated one.

### Ownership boundaries

| Concern | Owner |
|---|---|
| Experience identity, authored content, domain rules | Experiences / the domain-specific Experience |
| General finite-state-machine semantics | FSM_API |
| MicroBundle contract and domain | MicroBundleDomain |
| Immutable artifact delivery | MicroBundleRepository |
| Dependency resolution and composition | FSM_COS |
| GUI semantics and interaction contracts | GUI |
| Observation and rendering contracts | Renderer |
| Concrete visualization backends | Visualize |
| Browser or desktop lifecycle and native presentation | The relevant host |

The Experience may use these capabilities, but should not absorb their general responsibilities.

## 04 See it in a minute

A minimal semantic example is the Elements seed:

```csharp
public static class Iron
{
    public static readonly Element Definition =
        new(26, "Fe", "Iron", 55.845);
}
```

The `Element` record owns the semantic identity and validates its basic construction inputs. `ElementMicroBundle` exposes that definition through the MicroBundle contract. Neither type needs to know whether the visitor sees a periodic table in a browser, a desktop window, or a future spatial display.

**Source-checked:** `src/Elements/Element.cs`, `Iron.cs`, and `ElementMicroBundle.cs` at the revision listed in the [theory index](README.md). This example is not a claim that a complete host presentation or publication flow for Elements has been demonstrated.

## 05 Choose your next step

Continue with [02 — Identity, Artifacts, and Composition](02-identity-artifacts-and-composition.md) to distinguish the Experience from the MicroBundles that implement parts of it. For the current cross-repository overview, see [Manifestation Architecture](../MANIFESTATION_ARCHITECTURE.md).

### Practice

1. Is a GUI tree itself an Experience? Explain what meaning it can express and what it cannot own.
2. Give an example of a room that is authored but not yet calculated. What evidence would be needed to advance its maturity?
3. Pick a feature in one of your own Experiences. Which repository owns its domain rule, and which repository only presents the result?

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
