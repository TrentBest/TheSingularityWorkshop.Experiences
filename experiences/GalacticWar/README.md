# Galactic War — Experience preforge

> **Working title.** A far-future, science-fantasy universe about extraordinary people, overwhelming odds, and the hard-won ability to change the fate of a living galaxy.

This is an independent Experience concept home in the Experiences repository. It captures the vision before gameplay implementation. The eventual playable Experience belongs here; AnyApp, WebApp, and future VR clients are manifestations/hosts, not separate game definitions.

## The promise

A galaxy-spanning conflict is fought by heroes, villains, civilizations, and forces whose capabilities can seem almost magical. The enemy can field overwhelming numbers. Individual power matters, but power alone is not enough.

The player must survive long enough to understand what is happening, discover what the mission actually is, work out how it can be accomplished, prepare the people and resources required, and then carry it through.

The universe should make players feel spectacular without making victory automatic.

## Design documents

- [Core vision and pillars](design/CORE_VISION.md)
- [Game design bible — initial outline](design/GAME_DESIGN_BIBLE.md)
- [Combat, exoskeletons, and spacecraft](design/COMBAT_AND_VEHICLE_FLIGHT.md)
- [Learning, progression, allies, and the living galaxy](design/LEARNING_AND_LIVING_GALAXY.md)
- [Open questions and decisions](design/OPEN_QUESTIONS.md)

## Experience boundary

This directory is the authored source of truth for the Experience concept. It is not a second game engine, physics engine, renderer, networking stack, or host.

- Experience semantics, identity, authored world, missions, and progression belong to the Experience.
- Reusable capabilities belong in their proper MicroBundles and domain repositories.
- Physics owns verified physical outcomes; this design may define fictional rules such as gravity-field technology, but must not silently replace the Physics domain.
- FSM_COS composes capabilities; AnyApp/WebApp/VR provide manifestations.
- Buildable implementation artifacts are downstream of this preforge and its manifest.

## Current stage

**Vision capture / preforge.** No gameplay, combat simulation, flight model, progression system, online galaxy, or educational curriculum is claimed to be implemented. Preserve the concept and resolve the open design questions before choosing a first vertical slice.

## Relationship to raWWar

The desired combat intensity and immersive first-person sensibility are explicitly raWWar-style inspiration. This project is a distinct far-future universe/Experience concept; it does not assume that raWWar's existing identity, scope, or implementation should be overwritten or silently repurposed.
