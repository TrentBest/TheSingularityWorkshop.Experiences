# Build to Illuminate

**Working principle:** Build enough to shine a light on the way to do it. Leave room for others to do their bit.

The goal is not for one contributor, repository, or implementation to finish the entire imagined world. The goal is to make the path visible: prove a useful pattern, expose the real contracts, demonstrate the experience, and leave a sound foundation that other people can extend, replace, or specialize.

## What “enough” means

A contribution should be small enough to understand and complete, but substantial enough to teach something real.

- **Demonstrate the path.** Prefer a working vertical slice or an honest, concrete design over a sprawling promise.
- **Make the invisible visible.** Use living examples, inspectable state, diagrams, event histories, and reproducible evidence where they materially help someone understand.
- **Keep the seams real.** Show where identity, domain meaning, composition, persistence, simulation, and presentation belong. Do not conceal missing contracts behind mock success.
- **Label the truth.** Distinguish implemented behavior from a design proposal, a placeholder, a planned integration, and an illustrative example.
- **Leave extension points.** Publish clear contracts and bounded examples so other contributors can build alternative rules, content, hosts, renderers, or experiences without rewriting the foundation.
- **Respect ownership.** Improve the repository that owns a capability; do not absorb another layer's responsibilities just to make the demonstration convenient.
- **Stop at the useful boundary.** A reference implementation should establish how to proceed, not monopolize every future implementation choice.
- **Invite independent work.** Explain the purpose, assumptions, limitations, and acceptance criteria well enough that another person can take a coherent piece and own it.

## A reference slice is a lantern, not the whole city

For example, one carefully authored tabletop siege can reveal how an Experience identifies a scenario, applies explicit turn rules, records decisions, and explains outcomes. It need not implement a civilization simulator, a full physics engine, multiplayer infrastructure, and every possible visual manifestation first.

That small slice becomes valuable when its contracts are honest and its boundaries are reusable. Others can then contribute richer rules, additional historical or fictional scenarios, campaign persistence, physical experiments, AI opponents, accessibility, or new hosts independently.

The same principle applies to every Workshop capability: build a sufficiently complete example to illuminate the route, and avoid pretending the example is the destination.

## Practical completion test

Before expanding scope, ask:

1. Can a newcomer see what problem this solves and why it belongs here?
2. Can they inspect a real example or follow a clear, testable design?
3. Are the ownership boundaries and dependencies explicit?
4. Are limitations and unimplemented pieces plainly named?
5. Can someone else extend or replace a part without taking ownership of the whole?
6. Does the result make the next contribution easier to make correctly?

If so, the work may be ready to hand off. More implementation is not automatically more progress.

## Relationship to Experience design

A diegetic institution gives a capability an in-world reason to exist. This principle gives its implementation a collaborative reason to stop at a useful boundary: make the institution and its first meaningful activity understandable, demonstrate the contracts, and leave the wider world open to other authors and builders.

See [Diegetic Institutions — Experience Architecture](../experiences/DIEGETIC_INSTITUTIONS_DESIGN.md) for how this principle informs museums, wargaming halls, strategy chambers, and other proposed Experience destinations.

---

This document records a working engineering and collaboration principle. It does not assert that the examples described here are implemented.
