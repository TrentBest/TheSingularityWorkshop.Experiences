# Drone Construction — Web and 3D Projection Roadmap

## First milestone: isometric browser manifestation

The showcase now renders each committed simulation snapshot through `ConstructionSnapshotIsometricSvgRenderer`. The browser page remains self-contained HTML and SVG, with no Unity dependency or GPU requirement.

The renderer projects the 10×10 logical grid into an isometric view, draws placed blocks as stacked cube faces (capped at five visible layers), and projects each drone marker into a relative lane-elevation layer. Lane labels E10/E9/E8/E7 are assigned presentation lanes. They are **not measured world-space altitude**: the current construction loop still advances discrete grid cells and does not integrate the quadrotor rigid-body model.

## Why this is 2.5D first

A useful bridge toward real 3D is to separate three contracts:

1. **Authoritative model:** target heights, placed blocks, drone state, lane identity/elevation, emitter inventory, and simulation tick.
2. **Projection:** map model coordinates into a camera-relative view; support isometric and orthographic views without changing simulation behavior.
3. **Browser host:** responsive canvas/SVG/WebGL surface, camera controls, selection, telemetry, and level-of-detail policy.

This first projection intentionally uses SVG to make the spatial mapping inspectable in any modern browser. It is not yet a perspective camera, mesh renderer, WebGL/WebGPU backend, or physics integration.

## Image-derived construction targets

The next model step is a source-image import contract: decode an image, choose a bounded target resolution, and convert pixels to height values. A 4K source image is an input-resolution goal, not a requirement to create 16 million independent physical blocks or draw calls. Keep source dimensions, target grid dimensions, height quantization, and maximum build height separate. Resampling and a configurable channel-to-height mapping should be explicit and deterministic.

For a first usable importer:
- preserve aspect ratio unless the user explicitly requests stretching;
- cap target grid dimensions and total cells before allocation;
- map source luminance or a selected channel into integer heights;
- clamp height to the configured build limit (currently five for the showcase);
- record source dimensions, target dimensions, mapping mode, and clamp count in import metadata;
- test empty, one-pixel, non-square, transparent, very large, and malformed images.

## Real 3D progression

1. **This commit:** isometric SVG projection from committed snapshots.
2. **Next:** add camera-independent world coordinates and explicit elevation to the renderer-facing snapshot model; do not infer altitude from agent ID.
3. **Then:** introduce a browser renderer adapter behind the existing renderer-neutral model, with WebGL/WebGPU capability detection and a clear SVG fallback.
4. **Then:** add perspective camera orbit/zoom, instanced block geometry, and GPU instancing for drone representations.
5. **Only after contracts align:** feed actual position/orientation from the rotor-driven physics kernel into construction-agent snapshots and validate control, collision, and ground-contact behavior.
6. **Scale gate:** measure simulation throughput separately from visible draw submissions, GPU memory, browser frame time, and level-of-detail behavior at 1k/10k/100k agents.

The renderer must never become the source of truth. GPU resources are disposable projections of authoritative simulation state.

## Current limitation

The current snapshot has placed-block counts and discrete grid positions; it does not yet expose a complete continuous 3D transform, per-cell required-height field, camera state, or physical drone pose. This milestone creates the browser-visible projection seam without pretending those data already exist.
