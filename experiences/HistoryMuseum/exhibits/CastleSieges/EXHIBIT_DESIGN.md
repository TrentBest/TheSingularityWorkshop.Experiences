# Castle Sieges — History Museum Booth

**Status:** proposed exhibit and MicroBundle taxonomy. This document defines authored content boundaries; it does not claim that the bundles, booth runtime, or historical simulation are implemented.

## Place in the wider Siege Wing

This booth is the **historical-history track's first planned case**, not the whole collection. The proposed [Siege Wing](../../SIEGE_WING_DESIGN.md) will eventually connect historical cases with literary/epic narratives and science-fiction sieges through shared learning questions. This booth remains disciplined about historical evidence: a literary siege or fictional planetary assault belongs in its own evidence domain and should be compared through explicit dimensions, not folded into historical claims.

## Purpose

Let visitors understand castle sieges as historically situated human and engineering systems. The booth begins with a grounded question—**how did attackers and defenders try to change each other's options over time?**—and lets the visitor inspect fortifications, tools, logistics, chronology, and consequences.

The booth links to the existing [Castle Siege Experience preforge](../../CastleSiege/README.md), which owns the authored battlespace and its stable object identities. It does not copy that scene into a separate museum model.

## Visitor journey

1. **Orient:** a map and timeline establish the region, period, and specific case being discussed.
2. **Understand the place:** inspect the castle's location, approaches, water access, walls, gates, towers, and other features supported by the selected evidence.
3. **Compare options:** examine defender preparations and attacker methods, including the limits and costs of each.
4. **Follow a siege:** move through a sourced chronology of preparation, investment, negotiation, assault or bombardment where applicable, relief, surrender, withdrawal, or aftermath. Do not force every historical siege into one sequence.
5. **Inspect technology:** examine a catapult/trebuchet or other siege technology in its actual regional and chronological context. Do not imply all engines existed together or were interchangeable.
6. **Experiment carefully:** optionally change a declared launch input in a Physics-backed experiment. Label the model's assumptions and omissions; distinguish a simplified projectile experiment from a faithful reconstruction of a historical engine.
7. **Reflect:** explore civilian impact, disease, supply, political aims, repair, rebuilding, and what evidence remains uncertain.

## Proposed content MicroBundles

These are stable *semantic keys for planning*, not assigned numeric IDs, provider IDs, versions, or published artifacts. Allocate formal identities only when an artifact is authored against the real MicroBundleDomain contract.

| Semantic key | Focus | Required content / contract |
|---|---|---|
| `history.case-study` | One named siege or bounded historical case | Stable case ID, place, date range with uncertainty, participants, context, outcome, source set |
| `history.chronology` | Events and temporal relationships | Stable event IDs, date precision, ordering confidence, event-to-source links |
| `history.place-and-map` | Geographic and strategic setting | Place IDs, map/source provenance, historical versus modern geography, coordinate/reference convention |
| `history.fortification` | Defensive architecture | Feature IDs, type, period/region applicability, observed/inferred/reconstructed status |
| `history.siege-technology` | Engines, tools, materials and methods | Object IDs, attested date/region, operating principle, constraints, evidence and reconstruction notes |
| `history.logistics-and-supply` | Food, water, fuel, labor, transport, disease and time | Claims with source links, context, assumptions, quantitative units when supported |
| `history.people-and-perspectives` | Participants and civilian experience | Role/perspective records, source limitations, respectful treatment of contested accounts |
| `history.claim-and-source` | Provenance shared across the booth | Claim ID, source identity, source location, author/date where known, confidence/qualification, quotation limits and rights metadata |
| `history.glossary` | Period and technical vocabulary | Stable term IDs, definitions, language/region/period, related objects and claims |
| `history.comparison` | Compare cases or approaches | Explicit comparison dimensions, scope, non-equivalence warnings, linked source claims |
| `history.learning-activity` | Guided questions and classroom use | Audience/level, learning objectives, activity steps, evidence prompts, answer guidance and accessibility notes |
| `history.exhibit-composition` | Assemble the booth journey | References to versioned content bundles, visitor sequence, optional stations and presentation hints; no copied authoritative facts |
| `castle-siege.authored-battlespace` | Existing scene content | Reuse the Castle Siege contract and its stable object IDs; keep authored geometry distinct from physics state |
| `physics.projectile-trajectory` | Optional experiment | Physics-owned run record, model/version, units, inputs, assumptions, samples; never reimplement the solver in historical content |

Do not create a separate bundle for every paragraph or object. A capability merits an independently versioned bundle when it has a coherent responsibility, meaningful reuse, or its own lifecycle. Several small content records may be data within one case-study artifact; the table is a vocabulary and boundary proposal, not a mandate to ship fourteen packages.

## Evidence and historical honesty

Every exhibit claim should be classifiable as one of:

- **Attested:** directly supported by a named primary or reliable secondary source, with the actual scope stated.
- **Interpretation:** an argument or synthesis supported by evidence; identify it as interpretation.
- **Uncertain / disputed:** evidence is incomplete, conflicting, or debated; show what is uncertain.
- **Reconstruction:** a visual or interactive model made to help explain a concept, not itself evidence that the scene looked or behaved exactly that way.
- **Experiment result:** generated by a named, versioned model from recorded inputs. This category is not a historical claim unless independently supported by historical evidence.

A confidence label alone is not a citation. Store source references at claim level so a visitor can inspect why a statement appears. Do not flatten conflicting sources into a false consensus. Record the date and region boundaries that make a claim true.

For visuals, preserve the distinction between a surviving artifact, an archaeological reconstruction, an illustrative diagram, and a real-time simulation. If licensing or reuse rights are unknown, do not silently present an asset as cleared.

## Learning objectives

A visitor should be able to explain, with evidence:

- why a siege was attempted and what political or military goals shaped it;
- how fortification design, terrain, water, supply, labor and time constrained both sides;
- why a siege engine's presence in one period or region cannot be generalized to every castle;
- how a projectile demonstration differs from a complete machine and from historical evidence;
- why outcomes included negotiation, relief, disease, starvation, withdrawal and surrender as well as assault;
- how uncertainty, source bias and reconstruction choices affect what the exhibit can responsibly claim.

## Dependencies and ownership

- The museum/booth composition references content artifacts; it does not own their truth by virtue of presenting them.
- The historical bundles own the meaning and provenance of historical claims.
- Castle Siege content owns the authored scene/object identities; the booth references them.
- Physics owns physical equations and actual observations. The exhibit can teach without a live solver, but must not label an illustrative arc as a computed result.
- WebPage/WebApp owns navigation, visitor interaction, accessible text, controls and presentation.
- Visualize/Renderer owns visual manifestation; no renderer is the source of historical or physical truth.
- MicroBundleRepository stores/retrieves immutable versioned artifacts; FSM_COS composes them; neither invents domain meaning.

## Acceptance gates for the first booth

- [ ] One bounded case study is selected and its time, place, and scope are explicit.
- [ ] Each factual claim has traceable source metadata or is marked as uncertain/reconstruction.
- [ ] The timeline references stable event IDs and preserves date uncertainty.
- [ ] Fortification and siege-engine content is period/region-qualified.
- [ ] The booth references Castle Siege authored IDs rather than duplicating scene objects.
- [ ] Historical explanation remains usable when Physics is unavailable.
- [ ] Any experiment panel displays the actual model/version, inputs, units and limitations, or is unmistakably marked illustrative.
- [ ] Text and interactions have an accessible non-3D path.
- [ ] The composed artifact pins versions and content hashes before trusted/published execution.
- [ ] Review verifies the source rights and avoids presenting graphic violence as spectacle.

## First recommended case study

Choose a single, well-documented siege only after selecting authoritative sources and deciding the intended learning level. Do not choose a case merely because it supplies the most dramatic catapult image. A good first case has enough evidence to connect place, fortification, logistics, chronology, attacker/defender choices, and consequences while making source gaps visible.

The first vertical slice can ship as a text-and-diagram booth with sourced artifacts. A real-time 3D castle and a Physics-backed catapult are later manifestations, not prerequisites for sound historical teaching.
