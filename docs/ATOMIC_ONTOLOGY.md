# Atomic Ontology Test Corpus

The Workshop should not have to invent a complete universal ontology before it can test composition.

Instead, start with things that are small enough to reason about and large enough to expose composition boundaries.

## Why atoms?

A MicroBundle should be able to stand on its own when its semantic boundary is meaningful.

Larger concepts should not force their entire knowledge graph into one bundle merely because that is convenient for the first host.

The useful test is:

```text
thing
 |
 +-- atomic enough?
 |      |
 |      +-- yes -> independent MicroBundle
 |      |
 |      +-- no
 |           |
 |           +-- identify constituent MicroBundles
 |           +-- identify relationships
 |           +-- identify required/preferred capabilities
 |           +-- compose
 |
 v
Experience
```

This is a composition test, not a claim that the Workshop has discovered the one true ontology.

## Corpus A — Elements

The periodic table is an unusually clean first corpus.

An element can have a stable semantic identity and atomic data while supporting many manifestations:

```text
Element: Iron
    |
    +-- atomic number
    +-- symbol
    +-- name
    +-- atomic weight
    |
    +-- textbook manifestation
    +-- scientific manifestation
    +-- sci-fi manifestation
    +-- compact/debug manifestation
    +-- interactive detail manifestation
```

The semantic element should not be duplicated for each visual style.

The manifestation changes. The identity does not.

The existing Workshop element data is a useful seed for extraction. Preserve semantic data independently from legacy renderer-specific presentation code.

## Corpus B — Digital Logic

Digital Logic gives us a second useful scale because it naturally contains nested structure.

A practical starting decomposition is:

```text
Digital Logic
 |
 +-- Physical
 +-- Combinational
 +-- Sequential
 +-- Modular Functional Units
 +-- Control and Timing
 +-- Programmable
 +-- Visualization
```

Each area can become a separately addressable composition boundary where that separation carries useful semantics.

For example, a larger logic Experience might compose gates, adders, registers, clocks, multiplexers, counters, and visualization independently.

The important test is that the larger Experience does not have to become one giant MicroBundle.

## Corpus C — Software Design Patterns

Software patterns provide a different kind of atom.

A pattern can expose semantic knowledge such as:

- identity;
- intent;
- structure;
- participants;
- relationships;
- constraints;
- examples;
- alternative representations.

A collection of patterns can then compose individual patterns without changing their identities.

The same pattern could have documentation, diagrammatic, code-oriented, teaching, or sci-fi manifestations without becoming multiple semantic patterns.

## The arbitrary-reality test

Once the three seed corpora work, choose something arbitrary from reality.

Ask:

1. Is it atomic enough to stand alone?
2. If not, what smaller meaningful MicroBundles compose it?
3. What relationships connect them?
4. Which data is semantic and which is manifestation?
5. Which bundles are required for the Experience?
6. Which capabilities are required, preferred, optional, delegable, or frame-critical?
7. Can the resulting closure be represented by immutable artifact identities?
8. Can FSM_COS compose it without host-specific knowledge?

If the answer to #8 is no, that is valuable information. The corpus is exposing an architectural gap.

## What this corpus tests

| Domain | Primary pressure |
|---|---|
| Elements | identity vs manifestation |
| Digital Logic | nested composition and dependency closure |
| Design Patterns | semantic knowledge vs presentation |
| Arbitrary reality | whether the architecture generalizes |

The corpus should eventually exercise:

- MicroBundleDomain contracts;
- FSM_COS arbitration;
- immutable repository artifacts;
- Experience manifests;
- GUI manifestations;
- creator-owned compute requirements;
- WebApp/AnyApp/MyVR capability negotiation.

## Anti-monolith rule

When a concept becomes large, do not immediately make a larger MicroBundle.

First ask what smaller semantic units it contains.

A larger bundle is justified when it represents a meaningful reusable boundary, not merely because the host needs somewhere to put code.

## The intended direction

```text
atomic semantic unit
        |
        v
independent MicroBundle
        |
        v
composed closure
        |
        v
Experience
        |
        +--------+--------+
        |        |        |
      WebApp   AnyApp    MyVR
```

The ontology grows by composition.

The platform remains responsible for composition and manifestation mechanics.

The content remains responsible for what the things mean.
