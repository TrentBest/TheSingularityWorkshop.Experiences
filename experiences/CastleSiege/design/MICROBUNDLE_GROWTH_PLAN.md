# Castle Siege — MicroBundle Growth Plan

**Status:** authored capability map; proposed boundaries, not a claim that these bundles are implemented or published.

Castle Siege should grow by composing independently meaningful capabilities from the Workshop. It should not become one large bundle that quietly owns physics, rendering, rules, choreography, storage, and runtime orchestration.

This catalog is maintained in the Experience preforge because it describes the capabilities this Experience needs and how they participate in its authored journey. Each capability's actual implementation belongs to the repository that owns that domain.

## Boundaries we must preserve

- **MicroBundleDomain** defines neutral capability identity, dependency requests, provider declarations, load/arbitration contracts, and editor-inspectable definitions.
- **MicroBundleRepository** retrieves immutable, versioned artifacts by bundle ID, version, and SHA-256. It does not decide what the bundle means or how bundles compose.
- **FSM_COS** resolves dependencies and composes a runtime. It is not the game loop or the owner of Castle Siege rules.
- **Experiences** owns the authored journey, scene/build content, rules, and the requirements joining capabilities.
- **Physics** owns physical state and supported physical outcomes.
- **WebPage/WebApp and Visualize/Renderer** own presentation and interaction, not authoritative outcomes.
- **Gestures** may own articulated crew choreography when that capability is ready.

See [MicroBundleDomain architecture](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain/blob/development/docs/ARCHITECTURE.md) and [MicroBundleRepository architecture](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleRepository/blob/development/docs/ARCHITECTURE.md).

## Capability map

Names below are stable *proposed semantic keys*, not assigned numeric MicroBundle IDs. Numeric IDs, versions, provider IDs, parameter IDs, and artifact hashes must be allocated and recorded when a capability is authored against a real contract. Do not derive stable identity from display names or folder paths.

| Proposed semantic key | Owner | Responsibility | Maturity / gate |
|---|---|---|---|
| `castle-siege.entry-surface` | Experiences | Current honest entry surface and Experience identity | Exists as a shell; not a playable simulation |
| `castle-siege.authored-battlespace` | Experiences | Stable IDs for terrain patch, castle silhouette, target region, and siege apparatus; authored geometry separate from computed state | First authored content schema and example exist; duplicate-ID validation and Physics mapping remain pending |
| `castle-siege.launch-command` | Experiences + Physics contract | Express a player's requested launch inputs with units, coordinate frame, and validation | Contract needed; no solver in this bundle |
| `physics.projectile-trajectory` | Physics | Produce deterministic trajectory samples with model/version, assumptions, units, and run provenance | Candidate dependency; use the Physics-owned experiment, never duplicate its solver |
| `physics.target-intersection` | Physics | Classify a trajectory against a declared target using supported geometry and tolerances | Not available until Physics exposes a tested intersection contract |
| `physics.impact-observation` | Physics | Report supported impact state/impulse/energy observations, with explicit limits | Depends on authoritative impact contract |
| `castle-siege.damage-rules` | Experiences rules consuming Physics evidence | Translate supported impact observations into bounded game-state changes under declared rules | Later milestone; never infer damage from a visual effect |
| `castle-siege.turn-authority` | Experiences / match authority | Validate build/attack phases, command ownership, turn sequence, and accepted event record | Later milestone; server/session authority required for competition |
| `castle-siege.run-evidence` | Experiences | Bind input, model identity, actual run record, target identity, classification, and limitations into an explainable result | Initial adapter exists as a fixture-tested contract; live Physics integration remains pending |
| `gestures.siege-crew-choreography` | Gestures | Respond to explicit events such as setup, loading, launch, impact, and reset with authored articulated motion | Candidate cross-repo capability; do not invent events before contracts exist |
| `castle-siege.presentation` | WebPage/WebApp + Visualize/Renderer | Present the authored scene, controls, actual evidence, and feedback | Host/presentation concern; must not be authoritative for physics or score |

This is a capability map, not a requirement to create eleven packages immediately. Some rows may remain manifest requirements or contracts rather than standalone bundles until they have a coherent independent boundary and a real implementation.

The first authored content contract is [BATTLESPACE_CONTENT_CONTRACT.md](BATTLESPACE_CONTENT_CONTRACT.md), with a schema and illustrative data file. It establishes content identity and declared bounds only; it is not yet a runtime capability or Physics collision contract.

## First useful composition

The first vertical slice should be deliberately small:

1. **Authored battlespace** supplies stable target and apparatus identities plus authored geometry.
2. **Launch command** validates the requested inputs and makes units/frame explicit.
3. **Physics trajectory** runs the existing Physics implementation and returns actual samples plus provenance.
4. **Target intersection** is added only after Physics exposes and tests the geometry contract.
5. **Run evidence** binds the actual run and supported classification to an explainable Experience result.
6. **Presentation** renders those facts without manufacturing hits, damage, or score.

Until steps 3 and 4 are genuinely connected, the Experience must continue to say that the Physics run is not connected. A fixture proves an adapter's behavior; it does not prove a live Physics integration.

## Authoring rules for growing the catalog

- **One coherent responsibility per capability.** Split when independent reuse, versioning, ownership, or configuration makes the boundary meaningful—not merely to maximize bundle count.
- **Keep semantics with their authority.** A Physics bundle does not own Castle Siege's victory rules. An Experience bundle does not own the equations of motion.
- **Declare requirements honestly.** A required capability is a real runtime prerequisite, not a future aspiration disguised as an installed dependency.
- **Use explicit stable identities.** Domain definitions support author-assigned parameter IDs; keep them stable across display-name or nesting changes. Do not mint IDs casually in this design document.
- **Separate definition from artifact.** A source project, a manifest entry, a NuGet package, and a content-addressed MicroBundle artifact are different things. Only record repository artifact addresses after bytes have been produced and verified.
- **Record provenance.** Computed evidence should identify inputs, capability/model version, units, assumptions, and source run. Authored content should identify its stable content IDs and revision.
- **Keep composition out of bundles.** Bundles implement capabilities; FSM_COS owns dependency closure, load ordering, arbitration, and convergence.
- **Grow through proof.** Each new capability should have a definition, contract tests, and a minimal visible or inspectable example before the Experience relies on it.

## Relationship to the current code

`src/CastleSiege/CastleSiegeMicroBundle.cs` is the current semantic entry-surface shell. It is not yet the complete composition above. Its ID `3301` is currently shared with the Experience identity; before publishing independently addressable bundles, audit and document the global ID allocation policy rather than assuming an Experience ID is automatically a safe ID for every future capability.

`CastleSiegeExperienceManifest` describes Experience identity and capability requirements. It is intentionally distinct from `MicroBundleDescriptor`, which describes one MicroBundle. The current manifest's string capability keys are Experience-level requirements, not proof that repository artifacts have been resolved or loaded.

## Next gates

1. Keep the authored Experience design and this capability map together under `experiences/CastleSiege/`.
2. Align manifest/schema examples with these semantic requirements without claiming that future capabilities exist.
3. Coordinate with the Physics owner for the real trajectory output and target-intersection contract; do not change Physics or duplicate its solver.
4. Define the smallest authored battlespace schema with stable IDs and explicit coordinate units.
5. Only then implement/register the next real MicroBundle and test its definition, artifact identity, and composition behavior.
