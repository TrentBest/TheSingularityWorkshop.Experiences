# Game design bible — initial outline

This document is the organizing spine for the universe and game design. It is intentionally an initial bible, not a claim that every system has been decided.

## 1. Universe premise

A far-future technological-magical civilization has developed exoskeletons, advanced materials, energy systems, spacecraft, gravity-field generators, and technologies that make extraordinary feats possible. The galaxy contains many civilizations, factions, allies, adversaries, and individuals whose capabilities can be wildly disproportionate.

An enemy force can overwhelm conventional defenses through numbers and scale. The conflict is therefore not solved by a single powerful character or weapon. Survival, intelligence, logistics, alliances, and mission design are essential.

## 2. Player fantasy and roles

The player may inhabit different scales and responsibilities over time: an individual operating on foot; an exoskeleton or powered armor pilot; a vehicle or spacecraft pilot; a specialist, commander, engineer, scout, or support role; and eventually a participant in capital-ship, fleet, or larger strategic operations.

These are candidate role families, not a locked class system. Roles should emerge from capabilities, equipment, training, and choices rather than arbitrary permanent labels.

## 3. Mission structure

The signature mission arc is:

**Survive → Understand → Discover → Plan → Prepare → Execute → Adapt → Consequence.**

A mission may involve incomplete or misleading initial information; survival while the situation is poorly understood; reconnaissance and evidence gathering; identifying the actual objective and success conditions; determining dependencies, risks, and required capabilities; seeking allies, equipment, intelligence, or access; executing a coordinated operation; adapting when assumptions fail; and consequences that persist in the wider universe.

Mission clarity must be earned without making the player feel randomly punished. The player needs meaningful clues, legible feedback, and ways to recover from uncertainty.

## 4. Combat and embodiment

Combat is an immersive first-person experience with a strong raWWar-style intensity. It can span personal weapons, powered suits, boarding actions, vehicle combat, and spacecraft dogfights.

The player should be able to maintain target awareness while maneuvering aggressively. Example fantasy: keep a target locked, rotate the craft through a 180-degree flip, continue firing as momentum carries the ship along its existing trajectory, then use axis thrusters to correct course.

## 5. Technology and fictional rules

Technology should be spectacular but coherent. Exoskeletons, energy weapons, sensors, targeting, shields, propulsion, artificial gravity/gravity fields, and scale-dependent systems need clear capabilities and limitations.

A gravity-field generator can protect a pilot from lethal acceleration and keep the player's body comfortable during maneuvers that would otherwise turn their innards into their outards. This is an explicit fictional convenience and world rule. It does not mean every vehicle maneuver is consequence-free: structural stress, heat, energy, control authority, visibility, reaction time, and tactical exposure can still matter.

## 6. Scale ladder

1. Personal movement, tools, and weapons.
2. Exoskeletons / powered armor.
3. Ground and atmospheric vehicles.
4. Small spacecraft and dogfights.
5. Larger ships, crews, and ship systems.
6. Capital ships and coordinated formations.
7. Fleets, bases, logistics, and faction operations.
8. Galaxy-scale alliances, conflicts, and persistent consequences.

The order is a design hypothesis. It should be validated against the most compelling first playable slice.

## 7. Characters, heroes, and villains

Characters can be delightfully excessive: visually distinctive, memorable, funny, frightening, brilliant, flawed, and exceptionally capable. Their powers should have identity, readable expression, costs or constraints, and counterplay.

The enemy's overwhelming numbers create pressure, but the enemy should not be a faceless infinite spawn source. Its organization, motives, logistics, command, adaptations, and vulnerabilities should be discoverable.

## 8. Social and persistent universe

The galaxy is a place to interact with other players and non-player characters, establish trust, negotiate, trade knowledge, form alliances, and coordinate operations. Persistence should make relationships and decisions meaningful without requiring every player to be online simultaneously.

The authoritative ownership of world state, faction state, player identity, and cross-session persistence remains an architectural decision to resolve before implementation.

## 9. Learning and progression

The fiction positions early learning as preparation for a demanding future. Progression should reward demonstrated understanding and applied competence: spatial reasoning, systems thinking, engineering intuition, communication, leadership, and collaborative problem-solving.

Avoid turning progression into a narrow grind for numerical power. New knowledge and better decisions should expand what players can meaningfully do.

## 10. Manifestation strategy

The Experience definition remains host-independent. Desktop, browser, and future VR are different manifestations of the same logical Experience. First-person embodiment and spatial interaction may vary by device capability, but identity, mission semantics, progression, and shared world rules must not fork into unrelated games.

## 11. Production discipline

Start with a small, complete, verifiable vertical slice. Reuse existing Workshop domain capabilities and MicroBundles. Do not duplicate Physics, GUI, FSM_COS, AnyApp, networking, or persistence infrastructure. Mark concepts, prototypes, and verified capabilities distinctly.
