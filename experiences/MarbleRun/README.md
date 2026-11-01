![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# Marble Run

**Status:** first executable scene-configuration contract; no marble dynamics or high-count performance claim yet.

The Experience owns terrain and track authoring, marble placement/release, run controls, camera and presentation. Physics owns physical state and contact outcomes. The scene definition records a coordinate frame, gravity vector, geometry revision, model identity, marble scale/density, and requested count. Mass is derived only under the explicitly stated homogeneous-sphere assumption.

Next: validate authored collision proxies, connect to a tested CPU rigid-body/contact API, and benchmark the same reproducible scene at increasing body counts. Do not claim thousands of marbles until measured. See the [Experience/MicroBundle boundary](../../docs/MARBLE_RUN_AND_DOMINO_MICROBUNDLE_BOUNDARY.md).

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop" width="200" /></p>
