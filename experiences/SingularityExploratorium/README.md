# Singularity Exploratorium

**Wonder first. Touch something. Ask why. Try again.**

The Singularity Workshop has room for more than one kind of exploration. The **Laboratory**, **Museum**, and **Exploratorium** are complementary institutions—not competing names for the same experience.

- **Laboratory — investigate.** Structured experiments, explicit hypotheses, controlled variables, measurements, and reproducible results.
- **Museum — encounter and understand.** Curated collections, historical context, interpretation, preservation, and guided journeys through ideas.
- **Exploratorium — discover by playing.** Hands-on, surprising, low-friction experiences that invite visitors to poke, build, release, rearrange, break, compare, and ask “what if?”

The Exploratorium is the Workshop's **wow-factor discovery floor**. It should make a visitor curious before asking them to study a theory. When curiosity catches, the experience should offer a path deeper into the Museum's context or the Laboratory's rigor.

## The invisible curriculum

**The visitor came to play. Learning came along for the ride.**

Learning should be embedded in the experience rather than announced as a prerequisite. Do not make visitors pass through a lecture, worksheet, or vocabulary gate before they can touch the interesting thing. Let the exhibit make a question irresistible, then let the visitor discover that their next playful choice is also an experiment.

- **Lead with an invitation, not an objective.** “Can you get the marble around the loop?” is a better first encounter than a paragraph about conservation of energy.
- **Make the underlying idea physically legible.** Motion, sound, balance, collisions, patterns, and visible consequences should carry meaning before a visitor reads an explanation.
- **Let visitors form their own hypotheses.** Give them a meaningful choice, a quick reset, and a fair chance to try again.
- **Make failure delightful and informative.** A collapse, stall, missed landing, or unexpected path is a discovery to investigate—not a score that shames the visitor.
- **Reveal the explanation when curiosity asks for it.** Keep deeper hints, measurements, diagrams, and theory available without interrupting play. Never confuse hiding a lesson with hiding useful information.
- **Reward noticing.** A short optional prompt such as “What changed?” or “Can you make it happen again?” can help visitors recognize what they discovered without turning the floor into a test.
- **Respect different kinds of curiosity.** Children, specialists, casual visitors, and people who simply enjoy a beautiful spectacle should all find a satisfying entry point.
- **Be honest about the world being shown.** Label concept art as concept art and live or measured outcomes as such. Wonder is stronger when the exhibit earns trust.

The intended progression is **play → surprise → curiosity → experiment → explanation**. Visitors may stop at any point and still have had a worthwhile experience. Those who want to go deeper can follow the thread into the Museum's context or the Laboratory's reproducible investigation.

## First exhibit families

### Dominoes — discrete causality you can touch

**Domino Studio** lets a visitor arrange and inspect a domino structure. The eventual physical exhibit should let the visitor place the first push and watch later motion emerge from collision, support, and contact—not from a scripted animation pretending to be physics.

It is a particularly legible bridge between authoring and simulation: change one tile, gap, level, or orientation and see whether the chain still propagates.

**Current maturity:** layout/proposal data and authoring validation exist. Full 3D support validation and physics-driven toppling are not yet established. Present the current slice honestly as authoring, not as a completed physical simulation.

### Marble Run — continuous motion at a glance

**Marble Run** is a playground for gravity, ramps, tracks, collisions, and the surprising collective behavior of many moving bodies. Visitors should be able to change the course, release marbles, and compare outcomes.

The long-term spectacle is a large obstacle course with thousands of marbles. That target must be earned with a real solver and reproducible performance measurements—not advertised as a current capability before it exists.

**Current maturity:** scene-configuration contracts exist. A complete rolling-marble solver and high-count performance result are not yet established.

### The growing floor

The Exploratorium is intentionally open-ended. New exhibits can come from any Workshop capability: sound, light, motion, materials, fluids, construction and destruction, celestial mechanics, procedural worlds, AI tools, and ideas we have not discovered yet.

An exhibit belongs here when its central promise is an immediate, playful encounter. It can later link to a structured Laboratory investigation, a curated Museum collection, source, theory, and reproducible evidence.

## Visitor experience pattern

Each exhibit should answer five questions in the visitor's language:

1. **What can I try?** Lead with one obvious, inviting action.
2. **What can I change?** Expose a few meaningful controls, not a cockpit of settings.
3. **What happened?** Show the actual result and any available state/event evidence.
4. **Why did that happen?** Offer a concise explanation, with a deeper route into theory.
5. **Can I go further?** Link to authoring, the Laboratory, the Museum, tests, and implementation.

Use a small number of maturity labels so a visitor can tell whether they are seeing a concept, an authoring tool, a live simulation, or a validated showcase. A static illustration must not masquerade as an interactive exhibit, and an unimplemented physics target must not be presented as measured behavior.

## Architectural boundary

The Exploratorium is an **institution and discovery experience**, not a new physics engine or a second implementation of the WebApp.

- The Experience repository owns exhibit identity, intent, authored configuration, and provenance.
- WebApp/WebPage is the natural browser-facing discovery and interaction surface when an actual route is integrated.
- FSM_COS composes compatible capabilities.
- Physics owns authoritative physical state, contact, and motion outcomes.
- Renderer/Visualize realize the scene without becoming the source of physics truth.
- AnyApp can provide a desktop manifestation where local compute or resources are useful.

One exhibit may have browser, desktop, or future VR manifestations while retaining the same identity and meaning.

## Current directory

- [Domino Studio](../DominoStudio/README.md) — author the discrete arrangement.
- [Marble Run](../MarbleRun/README.md) — author the continuous-motion scene.
- [Shared physics boundary and milestones](../../docs/MARBLE_RUN_AND_DOMINO_MICROBUNDLE_BOUNDARY.md)
- [Experience Directory and discovery contract](../EXPERIENCE_DIRECTORY.md)

## North star

**Make the visitor say “Whoa—how did it do that?” and then give them the tools to find out.**
