# QuadrotorDynamics — UML

```mermaid
classDiagram
    class QuadrotorDynamics {
        -double massKg
        -Vector3d diagonalInertia
        -double armLengthMetres
        -double thrustCoefficient
        -double reactionTorqueCoefficient
        -double[] motorEfficiency
        +CreateInitialState(droneId, worldPosition) QuadrotorState
        +Step(state, motorCommands, deltaTimeSeconds) QuadrotorState
    }
    class QuadrotorState {
        +RigidBodyState Body
        +double Rotor0RadiansPerSecond
        +double Rotor1RadiansPerSecond
        +double Rotor2RadiansPerSecond
        +double Rotor3RadiansPerSecond
        +RotorSpeed(index) double
    }
    class RigidBodyState {
        +Pose3d Pose
        +Vector3d LinearVelocity
        +Vector3d AngularVelocity
        +long StepIndex
        +double ElapsedTime
    }
    class RigidBodyIntegrator {
        +Advance(state, linearAcceleration, deltaTimeSeconds) RigidBodyState
    }
    class Squirrel3Keyed {
        +UnitDouble(seed, entityId, logicalTick, purposeId) double
    }
    QuadrotorDynamics --> QuadrotorState : advances
    QuadrotorDynamics --> RigidBodyIntegrator : integrates achieved motion
    QuadrotorDynamics --> Squirrel3Keyed : keyed motor variation
    QuadrotorState --> RigidBodyState : contains
```

## Use cases

- Apply four motor commands and calculate physically induced roll, pitch, yaw, and translation.
- Replay the same setup using a stable world seed and numeric drone identity.
- Keep authoritative achieved pose separate from requested commands and renderer interpolation.
- Test deterministic motion, symmetric thrust, invalid commands, and motor-induced torque.
