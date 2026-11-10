# Embodied Combat, Exoskeletons, Spaceflight, and Ship Presence

**Status:** candidate direction. No combat, flight, or interior-navigation implementation is claimed.

## First-person embodiment

The visitor should inhabit a person in the world, not merely move a camera over a game board. Spatial awareness, physical constraints, equipment, communication, and teamwork should make the experience feel embodied.

The exoskeleton is a functional system and an extension of the wearer's capabilities. It should have inspectable assemblies, interfaces, sensors, control behavior, power and thermal constraints, maintenance requirements, and failure modes. Its capabilities must emerge from explicit system rules rather than a list of unexplained bonuses.

## Ground engagement

Combat is one possible expression of the world, not the only reason it exists. When it occurs, position, visibility, movement, equipment configuration, coordination, and decisions should matter. The presentation can be spectacular without relying on gratuitous brutality.

The design should support clear distinctions between:
- what the person can perceive;
- what their equipment senses;
- what their team communicates;
- what the authoritative simulation knows;
- what the visitor believes.

## Spacecraft motion and dogfighting

Space combat should respect momentum and orientation as separate concerns. A craft may continue translating while rotating to face another direction. A pilot can maintain a target relationship, rotate or flip the craft, and continue firing while the vessel coasts along its existing trajectory. Axis-specific thrusters provide deliberate corrections to translation and orientation.

Candidate concepts include:
- target lock as a maintained relationship to a target, not an automatic guarantee of a hit;
- independent orientation and translation;
- axis-specific thruster corrections;
- momentum-aware intercept and evasion;
- firing solutions that account for relative motion, weapon behavior, and target movement;
- fictional gravity-field technology as a distinct, explicitly defined capability.

The gravity-field concept is not a license to ignore the movement model. Its effects, limits, costs, and failure conditions must be specified before it is treated as simulation truth.

## Ships as places

The visitor's view should evolve with the vessel and the scale of the journey.

1. **Viewport presence:** in a small craft, the primary view is through the actual viewport or canopy. The frame, glass, reflections, instruments, hull geometry, and physical cockpit establish that the visitor is inside a machine.
2. **Readable systems:** the visitor can inspect meaningful ship systems and understand where they are relative to the vessel. The ship is not an abstract HUD surrounding a camera.
3. **Walkable interiors:** on vessels large enough to support them, the visitor can leave the control station and move through corridors, rooms, hangars, engineering spaces, crew areas, and other authored spaces.
4. **Traveling home and base:** large ships become places to live and work. During long-distance travel, the vessel continues its journey while the visitor moves through it, interacts with systems, speaks with people, prepares equipment, rests, and participates in shipboard life.

The transition should be diegetic and spatially legible. The visitor should understand whether they are piloting, riding, walking aboard, remotely operating, or observing. The ship's authoritative position and movement continue independently of which interior space the visitor currently occupies.

Not every vessel needs a fully walkable interior. Small craft can remain cockpit-centered; interior scale, crew complement, and interaction depth should follow the vessel's role and size. A larger ship is not merely a larger cockpit.

## Scale progression

The visitor's embodied journey may expand through:
**person → exoskeleton → small craft → crewed vessel → capital ship → fleet**.

Each step should preserve the underlying physical and organizational relationships. A larger vessel still contains crew, stations, power, sensors, propulsion, maintenance, and command responsibilities.

## Evidence before spectacle

The first prototype should isolate one physical question and produce inspectable evidence. For example, demonstrate independent craft orientation and translation with a small number of thruster axes, then validate the behavior against a clear reference model before layering weapons, target lock, or visual effects.

Do not claim realistic spacecraft physics from a cinematic animation. Keep fictional capabilities labelled and testable.
