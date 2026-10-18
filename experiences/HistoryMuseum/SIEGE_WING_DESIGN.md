# Siege Wing — cross-world exhibit architecture

**Status:** design contract only. This does not claim that the wing, its bundles, or its runtime are implemented.

The History Museum's Castle Sieges booth is the first doorway into a broader **Siege Wing**: a persistent, extensible collection for studying siege and defense across documented history, literature and myth, and speculative science fiction.

The wing is organized around reusable questions and concepts, not one game, era, author, or renderer. It lets visitors compare how a besieging force and a defended place interact with terrain, time, logistics, engineering, information, morale, political objectives, and civilian or noncombatant consequences.

## Three evidence domains, one learning experience

| Domain | What it can assert | What it must not imply |
|---|---|---|
| Documented history | Claims grounded in traceable primary and secondary sources, with scope and uncertainty | That a reconstruction is eyewitness evidence, or that one period represents all periods |
| Literature, epic and myth | What a named work or tradition narrates; textual variants, themes, and later reception where sourced | That narrative events are automatically verified historical events |
| Speculative fiction | What a named fictional work establishes, plus clearly labeled interpretation or extrapolation | That fictional technology or events are historical facts or canonical when they are not |

These domains can share a teaching framework, vocabulary, diagrams, and comparison dimensions while retaining distinct provenance rules. A shared interface is not a claim that the evidence is equivalent.

## Candidate collection

These are candidate exhibit families, not completed exhibits or bundles.

- **Historical sieges:** one bounded case per exhibit, grounded in sources and explicitly scoped by date and place.
- **Epic and literary sieges:** for example, the Trojan War tradition as represented in Homer's *Iliad*. The *Odyssey* offers a different lens centered on Odysseus's return and aftermath; do not treat the poems as interchangeable siege chronicles.
- **Science-fiction planetary and orbital sieges:** works such as *Starship Troopers* can be explored as named fictional sources, with a clear distinction between what the work depicts and what an exhibit author infers about doctrine, logistics, technology, or strategy.
- **Comparative stations:** ask the same question across cases—such as how defenders obtain supplies, what makes a position defensible, or how attackers try to change the defender's options—while displaying non-equivalence warnings.

Specific literary and fictional works should be handled with bibliographic attribution, respect for copyright, and only the quotation or media use permitted by the applicable rights. A comparison can discuss a work without reproducing its expressive content.

## Shared learning questions

Each exhibit should select the questions appropriate to its evidence and audience:

1. What is the objective of the attacker, defender, or other participants?
2. What makes the place defensible or vulnerable?
3. Which constraints come from terrain, movement, supply, time, labor, communication, and information?
4. Which technologies are available in this particular world, period, or fictional canon—and what are their limits?
5. What alternatives exist to direct assault: negotiation, deception, relief, withdrawal, blockade, or changing the objective?
6. Who experiences the consequences, including civilians and people outside the main combatant roles?
7. What is directly supported by evidence, what is interpretation, and what is a reconstruction or speculative model?

Do not force every exhibit to answer every question. Missing evidence is a meaningful result, not a gap to fill with invented certainty.

## Proposed MicroBundle vocabulary

These semantic keys are planning names only. They are not BundleIds, versions, published artifacts, or a substitute for the real MicroBundleDomain contract.

### Reusable cross-domain capabilities

- `siege.case-study` — identity, scope, learning goals, and links to domain-specific evidence.
- `siege.place-and-battlespace` — a place or authored fictional battlespace, keeping source geometry distinct from runtime state.
- `siege.objective-and-participants` — goals, participant roles, relationships, and declared perspective.
- `siege.chronology` — ordered events with explicit date/time precision and ordering confidence.
- `siege.fortification-and-obstacles` — defensive features, barriers, access, and constraints.
- `siege.logistics-and-sustainment` — supply, movement, repair, labor, and time.
- `siege.methods-and-technology` — methods and devices with domain-specific provenance.
- `siege.claim-and-source` — claim-level evidence, citation location, attribution, rights, and qualification.
- `siege.comparison` — selected comparison dimensions and explicit limits to equivalence.
- `siege.learning-activity` — prompts, expected reasoning, accessible alternatives, and educator guidance.
- `siege.exhibit-composition` — presentation order and references to exact artifact versions; no duplicated authoritative claims.

### Domain-specific extensions

- `history.case-study`, `history.claim-and-source` and related history keys — historical evidence and historical uncertainty.
- `literature.work-and-passage` — a specific work, edition/translation, passage locator, textual context, and rights metadata.
- `fiction.canon-and-technology` — the named fictional source, the source's own assertions, continuity/version, and separately labeled extrapolations.
- `physics.experiment-definition` — optional executable experiment definition owned by the relevant simulation domain.
- `physics.experiment-run` — immutable model/version, inputs, units, assumptions, output and run identity produced by the physics owner.

Do not split every paragraph into its own bundle. Use independent bundles when a capability has coherent responsibility, reuse value, or a distinct version lifecycle. Group smaller records within a case-study artifact when that is simpler and still addressable.

## Evidence and simulation labels

Every exhibit element that could be mistaken for a factual assertion must make its status inspectable:

- **Source statement:** what a named source explicitly says.
- **Historical claim:** a scoped claim supported by traceable historical evidence.
- **Interpretation:** an argument derived from evidence or from a literary/fictional work.
- **Disputed / uncertain:** a real limitation, conflict, or unresolved question.
- **Reconstruction:** an authored visualization of incomplete evidence.
- **Fictional canon:** what the identified work/version establishes.
- **Speculative extrapolation:** a proposed implication not explicitly established by the source.
- **Experiment result:** output from an identified model and recorded inputs.

A physics result answers what the selected model computes, not what happened historically or what a fictional author intended. A visual model must not silently become evidence.

## Composition and ownership

- **Experiences** owns the wing's visitor journey, exhibit catalog, learning objectives, and composition requirements.
- **Historical, literary, and fiction content artifacts** own their claims, source references, and domain-specific meaning.
- **Castle Siege** owns its authored scene and stable scene-object identities.
- **Physics** owns equations, simulation behavior, units, and actual run records.
- **WebPage/WebApp** owns host navigation, interaction, accessibility, and presentation adapters.
- **Visualize/Renderer** owns visual manifestation, not the truth of what is shown.
- **MicroBundleRepository** stores and retrieves immutable artifacts.
- **FSM_COS** resolves and composes the declared dependencies; it does not arbitrate historical truth or fictional canon.

The wing must remain useful as a sourced text, diagram, timeline, or classroom experience even if a renderer or physics capability is unavailable.

## Suggested visitor flow

1. Enter the Siege Wing from the museum.
2. Choose a lens: **History**, **Epic & Literature**, **Science Fiction**, or **Compare**.
3. Select a specific case and inspect its scope, source type, and learning questions.
4. Explore place, participants, chronology, logistics, methods, and consequences.
5. Open claim/source details or the named literary/fictional passage.
6. Optionally enter a clearly labeled reconstruction or experiment.
7. Compare cases only through declared dimensions, with differences and missing evidence visible.
8. Leave with a short synthesis or learning activity linked to the exact content versions used.

## Implementation sequence

1. Keep the Castle Sieges booth as the first narrow vertical slice; do not wait for every domain.
2. Define and validate the common case-study, claim/source, and exhibit-composition contracts against the actual MicroBundleDomain schema.
3. Author one source-backed historical case and preserve the current Castle Siege authored IDs.
4. Add one literature case with a named edition/passage and explicit textual-versus-historical distinctions.
5. Add one science-fiction case with work/version attribution and a clear canon-versus-extrapolation boundary.
6. Build a comparison station only after the cases expose genuinely shared dimensions.
7. Connect physics experiments only when real run records and interface contracts are implemented and tested.

## Acceptance gates

- [ ] Each case identifies its domain, scope, version, and learning objective.
- [ ] Claims link to source records; fictional canon links to a named work/version and, where possible, passage.
- [ ] Historical evidence, literary narrative, fictional canon, interpretation, reconstruction, and experiment outputs are not conflated.
- [ ] Cross-case comparisons declare dimensions and warn when concepts are not equivalent.
- [ ] Artifacts use the actual MicroBundleDomain identity and validation rules before being called MicroBundles.
- [ ] The exhibit works without a live physics solver or 3D renderer.
- [ ] Optional simulation shows model/version, inputs, units, assumptions, and actual outputs.
- [ ] Presentation provides an accessible text-based route and respects source/media rights.
- [ ] Published compositions pin immutable artifact identities and hashes.

## Relationship to existing work

- [History Museum preforge](README.md)
- [Castle Sieges booth design](exhibits/CastleSieges/EXHIBIT_DESIGN.md)
- [Castle Siege Experience](../CastleSiege/README.md)
- [Castle Siege MicroBundle growth plan](../CastleSiege/design/MICROBUNDLE_GROWTH_PLAN.md)
- [MicroBundleDomain](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleDomain)
- [MicroBundleRepository](https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleRepository)
