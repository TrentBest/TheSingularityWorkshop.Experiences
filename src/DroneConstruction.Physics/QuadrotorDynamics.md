# QuadrotorDynamics

**Owner:** Trent Best  
**Purpose:** Advance a four-propeller quadrotor from motor commands by calculating propeller forces and torques and integrating the resulting rigid-body motion.  
**Implementation boundary:** This is a separate source-integrated flight model. The discrete DroneConstruction grid remains independent and does not claim to simulate flight.

## What happens during a step

1. Each normalized motor command changes that rotor's target angular speed; a first-order motor response models spool-up/spool-down.
2. Rotor angular speed determines thrust through a quadratic propeller model.
3. Each thrust force applied at its body-local arm creates torque through the cross product of arm position and force. Alternating rotor spin directions create reaction torque.
4. Total thrust is transformed through the achieved orientation into the world frame; gravity and thrust determine linear acceleration.
5. Torque and the diagonal inertia tensor determine angular acceleration using the rigid-body Euler equation, including the gyroscopic term from body angular velocity.
6. The Workshop Physics RigidBodyIntegrator advances position and orientation from the resulting velocities. The model never assigns a requested heading directly to the pose.

## Determinism and units

The model consumes Vector3d, Quaterniond, Pose3d, RigidBodyState, RigidBodyIntegrator, and Squirrel3Keyed from TheSingularityWorkshop.Physics. Stable world seed and numeric drone ID key reproducible per-rotor manufacturing variation; stable purpose IDs separate those samples. The keyed samples are independent of call order.

Coordinate convention is right-handed and Z-up. Position is metres, time seconds, mass kilograms, force newtons, inertia kg·m², thrust torque N·m, and angular speed radians/second. The default parameters are illustrative simulation values, not measured hardware calibration.

## Deliberate limits

This first model does not implement ground/contact collision, blade-resolved aerodynamics, voltage sag, rotor gyroscopic coupling, sensor fusion, or a closed-loop flight controller. It is a physically driven baseline, not validated real-world flight. Keep rendering and construction-cell placement downstream of the achieved physical state; neither may mutate the authoritative pose.

## Build boundary

CI checks out TheSingularityWorkshop.Physics from its development branch and builds/tests this integration project with WorkshopUseRepositorySources=true. The discrete construction core remains dependency-free. The package path must not switch to a Physics NuGet dependency until an actual published version is verified and intentionally adopted.
