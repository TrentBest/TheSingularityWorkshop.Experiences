# Warlords — Implementation Roadmap

The order is chosen to get a visible, playable proof quickly without turning the Experience into a second application framework.

| Stage | Deliverable | Exit evidence |
|---|---|---|
| 1. Rules | Host-independent territory/turn/combat domain | Deterministic unit tests and stable snapshots |
| 1a. Campaign evidence | Experiences-authored versioned vectors | Exact event and final-snapshot replay tests; Gauntlet can consume the same fixture |
| 2. Board | Minimal browser manifestation consuming the C# domain | Human can select and execute a legal march, end turn, and see actual resulting ownership |
| 3. Composition | Warlords MicroBundle(s), Experience manifest, immutable artifact identities | Host resolves requirements and FSM_COS composes the verified artifacts |
| 4. Lifecycle | Separate FSM_API session adapter | Tests prove Command → Finished follows domain victory; domain rules remain explicit and authoritative |
| 5. Save/replay | FSM_Serialization adapter and replay verification | Save/load round-trip and seeded event replay equality |
| 6. Opponents | Simple AI command producer | AI only submits legal domain commands |
| 7. Depth | Friendly movement, recruitment choices, economy, supply, diplomacy, fog of war | Each capability has its own contracts and tests |
| 8. Multiplayer | Authoritative sessions and player synchronization | Server validates commands, sequencing, reconnects, and replay evidence |

## Non-goals for the first proof

- No Unity dependency or Unity-specific code.
- No separate JavaScript implementation of the strategy rules.
- No premature generic RTS/4X engine.
- No AI framework dependency before a deterministic human-playable loop exists.
- No physics simulation for a board game.
- No published package or deployed host until the artifact and host gates are met.

The first board should be deliberately plain. A clean, working loop beats a beautiful static map.
