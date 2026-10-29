![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# Capsule Clinic presentation integration

This project is the Experience-owned bridge between Capsule Clinic's authoritative snapshot and the generic Renderer → Visualize → GUI.SVG presentation path.

## Responsibilities

- Convert a captured `CapsuleArenaSnapshot` into deterministic Renderer draw commands and a Visualize resource catalog.
- Preserve the four-pane layout, settled cell colors, and active capsule segments.
- Adapt the Renderer frame to Visualize and manifest it through GUI.SVG.
- Return the SVG as a string for the browser host.

It does **not** own the session, advance rules, accept player intents, or make gameplay decisions. Those remain in the Experiences domain and browser host.

## Source dependencies

The integration currently links the in-development source repositories instead of depending on an unpublished package. By default it expects the Visualize checkout at `VisualizeSource/` beside the Experiences repository root. Visualize in turn expects Renderer at `VisualizeSource/integrations/RendererAdapter/RendererSource/` and GUI at `VisualizeSource/integrations/GuiSvg/GuiSource/`. The browser workflow provisions these paths explicitly.

This source-linked arrangement is an integration proof, not a claim that the cross-repository package distribution story is finished.

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/Avatar.jpg" alt="The Singularity Workshop avatar" width="180" /></p>
