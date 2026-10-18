# Marble Run

**Status:** first executable scene-configuration contract; no marble dynamics or high-count performance claim yet.

The Experience owns terrain and track authoring, marble placement/release, run controls, camera and presentation. Physics owns physical state and contact outcomes. The scene definition records a coordinate frame, gravity vector, geometry revision, model identity, marble scale/density, and requested count. Mass is derived only under the explicitly stated homogeneous-sphere assumption.

Next: validate authored collision proxies, connect to a tested CPU rigid-body/contact API, and benchmark the same reproducible scene at increasing body counts. Do not claim thousands of marbles until measured. See the [Experience/MicroBundle boundary](../../docs/MARBLE_RUN_AND_DOMINO_MICROBUNDLE_BOUNDARY.md).
