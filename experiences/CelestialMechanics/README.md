# Celestial Mechanics Laboratory

**Status:** preforge design. This is not yet an executable simulation or published MicroBundle.

Compose a solar system from stars, planets, moons, asteroids, and comets. Inspect the same saved scenario through three linked views:

- **System view:** bodies, trajectories, close approaches, and timeline controls.
- **Gravity field:** acceleration vectors, potential contours, and influence heatmaps.
- **Impact observatory:** zoom from an asteroid encounter into the modeled momentum transfer and impact response.

The Experience owns scenario semantics and the creator workflow. Physics owns simulation primitives; Visualize/Renderer own derived presentation; GUI/manifestations own interaction; FSM_COS composes capabilities. Editable scenario persistence remains separate from immutable MicroBundle publication.

See the [full Experience design](design/EXPERIENCE.md) and [example preforge manifest](manifest/celestial-mechanics-experience-manifest.example.json).

The design deliberately distinguishes a simplified orbital/collision model from a high-energy impact solver. A shockwave, temperature field, ejecta, or material mixing must be labeled as a proxy unless the selected solver actually computes it. The manifest has no assigned numeric Experience ID or capability roots yet; those remain pending verification.
