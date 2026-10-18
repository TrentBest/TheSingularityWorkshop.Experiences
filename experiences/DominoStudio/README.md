# Domino Studio

Status: first executable layout-data slice. Domino Studio is an Experience, not a Physics MicroBundle.

The Experience converts an image-derived or hand-authored proposal into an editable grid, checks basic constraints, and later expands valid slots into explicit 3D body placements for the shared Physics runtime. Image interpretation, image processing, editing, and presentation stay here. Physics owns authoritative motion and contact outcomes.

## Grid encoding

- The occupancy texture stores a bit mask per cell: bit 0 marks a base-level domino, bit 1 marks the first overpass, and so on. A cell can represent any combination of levels up to the configured 64-level mask limit.
- The orientation texture is flattened as cellIndex × maximumLevels + level. Each set occupancy bit requires a corresponding nonzero orientation code; empty levels require zero.
- Code 1 maps to the first angle in the saved orientation table. A zero-radian orientation is distinct from an empty cell.
- A per-level orientation field is necessary because stacked dominoes in one cell may have different rotations.
- The current validator checks height and coarse spacing constraints. It does not prove 3D collision-free placement, support stability, or successful toppling.

## Next

1. Save image digest, conversion version/parameters, confidence/ambiguity, units and coordinate frame.
2. Expand slots to stable body IDs, 3D poses, dimensions, mass/inertia and contact material.
3. Validate overlap/support with the shared Physics model.
4. Apply only the first physical push; never script subsequent fall order.
5. Record model versions, timestep, impulse and run diagnostics for replay.
6. Keep Gestures untouched unless its owning LLM agrees to a defined integration contract.
