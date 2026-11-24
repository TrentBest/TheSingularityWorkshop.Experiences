# Warlords Experience Manifest — portable authoring record

**Status:** authoring record only; this is not yet a runtime manifest or executable MicroBundle artifact.

## Portable Experience identity

- Experience key: `warlords`
- Canonical capability ontology: `TheSingularityWorkshop.Experiences.Warlords`
- Exact artifact version target: `0.1.0-alpha.1`
- Scenario key: `four-kingdoms`
- Scenario version: `1`
- Rules version: `warlords-four-kingdoms/1`
- Domain project: `TheSingularityWorkshop.Experiences.Warlords`
- Immutable artifact SHA-256: **not produced**
- Runtime manifest: **not published**

The portable artifact revision is the canonical ontology string + exact version + SHA-256 of the immutable artifact bytes. The final runtime manifest must carry that portable identity (and any required dependency identities). It must never contain a repository-local storage handle or an FSM_COS platform-local runtime slot.

## Runtime identity compatibility boundary

The current MicroBundleDomain API still exposes numeric `MicroBundleDescriptor.Id`, `IMicroBundle.Id`, and numeric dependency references. Those values are not the portable ontology identity, not a repository storage handle, and not an FSM_COS local slot.

[MicroBundleDomain issue #28](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain/issues/28) tracks the additive portable manifest/dependency contract and explicit materialization mapping. Until that contract and its consumer tests exist:

- Do not invent or allocate a Warlords numeric `MicroBundleDescriptor.Id`.
- Do not serialize a repository-local numeric handle or an FSM_COS platform slot into this authoring record or a future Experience manifest.
- Do not silently equate the existing numeric Domain ID with an FSM_COS local slot.
- Keep Warlords rules/runtime and the local GUI sample independently testable; defer executable MicroBundle composition until the adapter boundary is defined and proven.

## Planned capability requirements

| Portable capability | Owner | Requirement |
|---|---|---|
| `warlords.territory-map` | Experiences | Required |
| `warlords.turn-rules` | Experiences | Required |
| `warlords.combat-rules` | Experiences | Required |
| `warlords.event-history` | Experiences | Required |
| `fsm-api.lifecycle` | FSM_API 1.0.13 | Implemented by the separate `Warlords.Runtime` adapter |
| `fsm-serialization.canonical-state` | FSM_Serialization | Preferred for save/load once the exact contract is established |
| `gui.semantic-board` | GUI.Core + GUI.Blazor | Implemented by the local browser sample |
| `visualize.map-presentation` | Visualize / Renderer | Optional future presentation capability |

These are semantic capability declarations, not proof that a runtime composition has already occurred. FSM_COS remains the composition authority; FSM_Execution remains the workflow executor. Neither is a Warlords rules dependency.
