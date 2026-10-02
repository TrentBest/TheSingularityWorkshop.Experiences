# Manifestation Architecture

## One Experience, multiple doors

The Workshop should not treat WebApp, AnyApp, and VR as three independent products.

They are manifestations of a common Experience.

    EXPERIENCE
        |
        +--------+--------+
        |        |        |
        v        v        v
      WEBAPP   ANYAPP    VR

Each manifestation has different capabilities and constraints.

The shared layer therefore describes identity, intent, state, contracts, and **creator-declared requirements**, not renderer-specific implementation.

## Three layers

### Experience layer

Describes identity, version, required MicroBundles, configuration, lifecycle, semantic behavior, and the capabilities/compute envelope declared by the Experience creator.

### Coordination layer

Describes session, synchronization, capabilities, requirements, requests, events, state publication, and connection health.

### Manifestation layer

Describes WPF, browser, WebXR, future native VR, and other clients.

The resulting shape is:

    Experience semantics + creator requirements
                    |
                    v
             capability negotiation
                    |
                    v
             execution arrangement
                    |
           +--------+--------+
           |        |        |
           v        v        v
          Web     Desktop     VR

No manifestation owns the Experience.

## Creator-owned compute requirements

There is no universal capability ladder.

An Experience may require MyVR, WebApp + MyVR, AnyApp, AnyApp + MyVR, WebApp + AnyApp, all three, or another supported arrangement.

Requirements should distinguish at least:

- **Required** — needed for entry or a defined feature.
- **Preferred** — a desirable execution location when available.
- **Optional** — useful but not necessary.
- **Delegable** — work that may be performed by another manifestation/service.
- **Frame-critical** — work that must remain local to the manifestation responsible for immediate presentation or interaction.

The platform evaluates these requirements against the capabilities actually present.

The result is a capability graph, not a fixed upgrade path.

## Co-entanglement

The term is intentionally metaphorical.

A connected WebApp, AnyApp, and MyVR may share a logical context while remaining separate processes/devices.

The minimum shared state is:

    ExperienceIdentity
    SessionIdentity
    ProtocolVersion
    Sequence
    LifecycleState
    Capabilities

More state should be shared only when an Experience actually needs it.

This prevents the bridge from becoming a generic synchronization bus.

## Authority

Authority belongs to the specific domain of an operation.

For example:

- WebApp may own public navigation state.
- AnyApp may own local computation state.
- An Experience may own semantic state.
- Repository publication owns artifact identity.
- FSM_COS owns composition.
- MyVR may own local headset pose and immediate device input.

The protocol should distinguish EVENT, STATE, REQUEST, RESPONSE, OBSERVATION, and CAPABILITY rather than treating every message as the same kind of thing.

## Synchronization

The first protocol does not need distributed-consensus machinery.

Start with:

- monotonic sequence numbers;
- timestamps;
- session identity;
- explicit event names;
- lifecycle state;
- heartbeat;
- request/response correlation where required.

Only introduce stronger synchronization when an actual Experience demonstrates the need.

## Failure is normal

Manifestations must tolerate browser closure, AnyApp closure, MyVR disconnection, interruption, bridge restart, stale messages, incompatible protocol versions, unsupported capabilities, and repository unavailability.

A disconnected manifestation is not necessarily a failed Experience.

The Experience should define what continues locally and what requires coordination.

## Capability model

Capabilities are negotiated rather than assumed.

Example:

    WebApp
      WebXR: immersive-vr
      WebSocket: yes
      browser-compute: available

    AnyApp
      LocalCompute: yes
      FileCache: yes
      DesktopUI: yes
      DesktopGPU: available

    MyVR
      HeadPose: yes
      Controllers: yes
      ImmersivePresentation: yes

Capabilities describe what is possible, not what should automatically happen.

## Simultaneous execution

All manifestations may participate at once when the Experience requires or benefits from it:

    WebApp
       | \
       |  \
       |   v
       |  AnyApp
       |   |
       v   |
      MyVR <+

The actual topology is Experience-defined and capability-driven.

There is no requirement that one manifestation be the "master" of all computation.

## Desktop companion model

AnyApp can provide bounded local computation.

    WebApp
       |
       | request
       v
    AnyApp
       |
       | compute
       v
    result
       |
       v
    WebApp

The same pattern can be used by MyVR when an Experience delegates suitable non-frame-critical work to AnyApp.

## VR companion model

VR adds a constrained manifestation.

    MyVR
       |
       | pose / input / bounded request
       v
    AnyApp
       |
       | suitable local computation
       v
    result
       |
       v
    MyVR

The architecture does not assume that AnyApp does everything. Frame-critical work remains local to the VR device.

## Security boundary

The bridge is a trust boundary.

Each side validates protocol version, session identity, message size, message type, payload schema, sequencing, artifact identity, and requested capability. A connection does not grant arbitrary authority.

The browser and VR client should expose enough state for the user to understand when a local companion is active.

## Evolution

Grow vertically:

    launch
       |
    launch + handshake
       |
    launch + handshake + shared state
       |
    shared state + local computation
       |
    same contracts + VR manifestation
       |
    capability-driven multi-manifestation execution

Each step should be demonstrated by a working Experience before another abstraction is added.
