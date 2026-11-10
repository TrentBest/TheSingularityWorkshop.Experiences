# Embodied Combat, Exoskeletons, and Spaceflight

**Status:** candidate direction. No combat or flight implementation is claimed.

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

## Spacecraft motion

Space combat should respect momentum and orientation as separate concerns. A craft may continue translating while rotating to face another direction. Aiming, target lock, and thrust correction should not collapse into a single arcade steering rule.

Candidate concepts include:
- target lock as a maintained relationship to a target, not an automatic guarantee of a hit;
- independent orientation and translation;
- axis-specific thruster corrections;
- momentum-aware intercept and evasion;
- fictional gravity-field technology as a distinct, explicitly defined capability.

The gravity-field concept is not a license to ignore the movement model. Its effects, limits, costs, and failure conditions must be specified before it is treated as simulation truth.

## Scale progression

The visitor's embodied journey may expand through:
**person → exoskeleton → small craft → ship → capital ship → fleet**.

Each step should preserve the underlying physical and organizational relationships. A larger vessel still contains crew, stations, power, sensors, propulsion, maintenance, and command responsibilities.

## Evidence before spectacle

The first prototype should isolate one physical question and produce inspectable evidence. For example, demonstrate independent craft orientation and translation with a small number of thruster axes, then validate the behavior against a clear reference model before layering weapons, target lock, or visual effects.

Do not claim realistic spacecraft physics from a cinematic animation. Keep fictional capabilities labelled and testable.
