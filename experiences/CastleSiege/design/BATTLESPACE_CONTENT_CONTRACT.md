# Castle Siege — Authored Battlespace Contract

**Status:** first authored content contract; schema and example are versioned with the Experience. This is not a Physics solver contract or a published MicroBundle artifact.

## Purpose

Give the Experience a renderer-independent, inspectable source for the named things a creator places in the scene. Stable object identity must survive label changes, presentation changes, and later replacement of the geometry representation.

- The Experience owns these authored object identities and content revisions.
- The current schema supports simple axis-aligned box bounds in metres.
- A target-region ID identifies *what the Experience wants to test*; it does not prove Physics can intersect that shape.
- Physics remains the authority for accepted geometry, trajectories, contact, intersection, and impact. A future adapter must explicitly map this coordinate frame and bounds into a Physics-supported contract.
- WebPage/WebApp, Visualize, and Renderer may present these objects but must not derive authoritative hit, damage, or score from their appearance.
- The file is authored content, not an executable MicroBundle, a MicroBundleDefinition, a repository artifact, or a runtime assembly.

## Coordinate frame

The example declares a right-handed world frame with Y up and metre-valued lengths. This is an Experience-authored convention, not an assertion about the Physics implementation's native frame. Every cross-domain adapter must state and test its conversion rather than silently assuming axes match.

`centerMeters` is the center of the axis-aligned bounds in the declared Experience world frame. `sizeMeters` contains positive full extents, not half-extents.

## Identity rules

- `id` is a stable, author-assigned semantic content ID; do not derive it from the label, array index, numeric bundle ID, or current folder path.
- Keep an ID when only its label or presentation changes.
- If an object is replaced by a meaningfully different authored object, create a new ID and preserve any required history/migration mapping.
- IDs are unique within one battlespace document. A validator should reject duplicates; the schema alone cannot enforce that across array entries.
- `contentRevision` identifies the authored document revision. It is not a MicroBundle version or SHA-256 artifact address.

## Files

- [JSON Schema](../contracts/castle-siege-battlespace.schema.json)
- [Illustrative authored battlespace](../contracts/castle-siege-battlespace.example.json)
- [Experience design](EXPERIENCE.md)
- [MicroBundle growth plan](MICROBUNDLE_GROWTH_PLAN.md)

## Next proof

Before this becomes a runtime capability, add validation for duplicate IDs and geometry sanity, then write a small consumer that can enumerate the authored objects without needing a renderer. Only after Physics exposes a tested target-intersection contract should a separate adapter claim hit/miss/indeterminate from these bounds.
