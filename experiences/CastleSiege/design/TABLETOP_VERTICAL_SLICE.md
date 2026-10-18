# Castle Siege — Tabletop Vertical Slice

**Status:** bounded implementation brief; design only. This document does not claim that a playable tabletop session, match authority, or Physics integration exists.

## Purpose

Build one small, inspectable tabletop siege game to illuminate the Experience boundary and leave the wider system open to independent contributors.

> Build enough to shine a light on the way to do it. Leave room for others to do their bit.

The first proof should answer: can a visitor enter a diegetic wargaming table, understand the rules, make a meaningful choice, see a deterministic result, and inspect why that result occurred?

It should not attempt to become Civilization, a full castle simulator, an online multiplayer service, or a substitute for Physics.

## The scenario: West Wall Demonstration

Use the existing authored Castle Siege content as the scene reference, especially `castle.west-wall` and `target.west-wall.core` in the [battlespace example](../contracts/castle-siege-battlespace.example.json). Those IDs name authored objects; they do not prove that a physical intersection query is available.

The scenario is an **abstract tabletop rules exercise**. It may use declared game tokens and a small, versioned resolution table to make the first session playable before physical trajectories or damage models exist. Any abstract result must be labeled as a tabletop rule result, never a calculated impact.

## The first playable loop

1. **Enter the institution.** The visitor approaches a Wargaming Hall table and receives a short scenario briefing.
2. **Choose a role.** Attacker and defender roles expose different legal actions, but share the same scenario definition and rules version.
3. **Inspect the setup.** Show the map, target/objective tokens, starting resources, current turn, legal actions, and victory conditions.
4. **Choose one action.** The player selects an action from an explicit allowlist. Invalid, stale, or out-of-turn actions are rejected with a reason.
5. **Resolve deterministically.** The scenario's declared abstract rules produce the next state and an ordered event record. Rendering and animation do not decide the outcome.
6. **Explain the result.** Show the rule applied, input values, prior state, resulting state, and any limitations.
7. **Continue or finish.** Advance to the next turn or a declared terminal condition. The session can be replayed from its initial state and accepted action sequence.
8. **Leave with evidence.** Present a compact after-action report: actions taken, state transitions, outcome, rules version, and replay status.

A turn-based tabletop session is complete only when this loop can run without requiring the Physics solver. A later physical experiment can be offered as a separate, explicitly connected activity.

## Rules contract

Before implementation, define and version these items in the scenario data or a dedicated rules contract:

- stable scenario key and scenario-definition version;
- initial state and token identities;
- roles and legal actions for each role;
- action payload fields, units where relevant, and validation limits;
- turn order and transition order;
- deterministic resolution rules and any explicit seed;
- terminal conditions and victory/draw rules;
- canonical event ordering and replay inputs;
- capability status and any abstracted or unavailable systems.

Do not assign numeric MicroBundle IDs or artifact hashes in this design brief. Allocate them only when an actual capability is authored, built, and verified against the current MicroBundleDomain and MicroBundleRepository contracts.

## Authority and ownership

| Concern | Owner |
|---|---|
| Scenario identity, authored setup, legal actions, abstract rules, victory conditions, event meaning | Experiences |
| Neutral capability identity and runtime/description contracts | MicroBundleDomain |
| Immutable artifact storage, retrieval, and content verification | MicroBundleRepository |
| Dependency closure, loading, arbitration, and composition | FSM_COS |
| Actual trajectory, intersection, impact, and physical response when supported | Physics |
| Diegetic room, table, controls, camera, and visual feedback | Host manifestation / presentation |
| Optional crew movement in response to explicit events | Gestures |

The scenario rules must not depend on a renderer or on the host's frame rate. FSM_COS is not the game loop. The repository does not interpret scenario meaning. Physics must not be pulled into the dependency graph merely to make an abstract game look physical.

## Evidence record

Each accepted action should produce enough information to explain and replay the result:

- session and scenario-definition identity;
- rules version;
- monotonically ordered action/event sequence;
- actor/role and validated action;
- state identity before and after resolution;
- rule or transition identifier used;
- result classification and explanatory reason;
- whether the result is **abstract tabletop**, **Physics-computed**, **authored illustration**, or **unavailable**.

A replay of recorded events is not automatically a deterministic rerun. Label the difference. If a later Physics-backed action is introduced, record the Physics model/version, units, coordinate-frame mapping, assumptions, and run reference separately.

## Deliberate exclusions

The first slice does not include:

- online multiplayer or authentication;
- a civilization-scale economy, diplomacy, research, or persistent campaign;
- realistic construction stability, masonry fracture, or debris;
- physical projectile collision or damage unless Physics exposes a tested contract;
- AI opponents beyond a later, explicit action policy;
- fabricated MicroBundle IDs, published artifacts, or assumed repository availability.

These are extension points, not failures of the first slice.

## Acceptance gates

- [ ] A scenario definition can be inspected without executing it.
- [ ] The initial state and legal actions are explicit.
- [ ] The same accepted action sequence yields the same abstract state/event sequence.
- [ ] Invalid and out-of-turn actions are rejected with explainable reasons.
- [ ] Victory and draw conditions are deterministic and visible.
- [ ] An after-action report identifies the scenario and rules version.
- [ ] The presentation distinguishes abstract results from physical observations and authored effects.
- [ ] Tests run in CI before any claim of implementation readiness.
- [ ] No publication or deployment occurs without separate approval.

## How this grows

1. Implement and test the abstract tabletop transition loop.
2. Give the visitor a diegetic Wargaming Hall entry and after-action archive.
3. Connect the authored scene through stable references, not copied geometry or labels.
4. Add a Physics-backed experiment only when the real Physics adapter and target-query contracts pass their own gates.
5. Expand from a tabletop encounter to operational campaigns or civilization-scale strategy only when persistence, time, resource ownership, and replay semantics are explicit.

This is the lantern, not the whole city. The first useful result is a real, small, inspectable loop that another contributor can extend without having to replace the architecture.
