# Lenny prescribed-motion host sample

This sample proves the C# host composition boundary for the Physics-owned prescribed-kinematic provider:

1. Instantiate and load `PrescribedKinematicMotionMicroBundle`.
2. Resolve the loaded bundle instance as `IPrescribedKinematicMotionProvider`.
3. Evaluate a versioned motion on a fixed logical-step schedule.
4. Supply the host's achieved `RigidBodyState` separately.
5. Validate the commit and destination arrival.

The sample uses a synthetic 1 m path solely to exercise the provider contract. It is not a Lenny artwork placement, a beach scene, collision-aware locomotion, or a browser runtime bridge. The no-contact sample host explicitly accepts the prescribed kinematic sample as achieved; a real world host must supply its own achieved observation.

Run from a checkout with the Physics repository alongside Experiences:

```bash
dotnet run --project samples/Lenny.MotionHost/Lenny.MotionHost.csproj --configuration Release
```

Set `WorkshopPhysicsRoot` if the Physics checkout is at a different path.
