# Warlords Experience Manifest — authoring record

**Status:** authored identity record only; not yet a runtime manifest or executable artifact.

- Experience key: `warlords`
- Scenario key: `four-kingdoms`
- Scenario version: `1`
- Rules version: `warlords-four-kingdoms/1`
- Domain project: `TheSingularityWorkshop.Experiences.Warlords`
- Package version: `0.1.0-alpha.1`
- Bundle ID / artifact hash: **not assigned / not produced**

The first rules project is a plain .NET 10 library with no external package dependencies. Before it becomes a MicroBundle, the global MicroBundle ID policy must be checked and a stable ID allocated. The runtime manifest must then reference actual immutable artifact identities; this document must not be mistaken for that manifest.

## Planned capability requirements

| Semantic capability | Owner | Requirement |
|---|---|---|
| `warlords.territory-map` | Experiences | Required |
| `warlords.turn-rules` | Experiences | Required |
| `warlords.combat-rules` | Experiences | Required |
| `warlords.event-history` | Experiences | Required |
| `fsm-api.lifecycle` | FSM_API | Preferred for host lifecycle integration |
| `fsm-serialization.canonical-state` | FSM_Serialization | Preferred for save/load once contract is established |
| `gui.semantic-board` | GUI | Required for the planned interactive board manifestation |
| `visualize.map-presentation` | Visualize / Renderer | Optional presentation capability |

These are proposed semantic requirements, not evidence that the corresponding artifacts have already been composed. FSM_COS remains the composition authority; FSM_Execution remains the workflow executor. Neither is a Warlords rules dependency.
