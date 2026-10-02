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

The shared layer therefore describes identity, intent, state, and contracts, not renderer-specific implementation.

## Three layers

### Experience layer

Describes identity, version, required MicroBundles, configuration, lifecycle, and semantic behavior.

### Coordination layer

Describes session, synchronization, capabilities, requests, events, state publication, and connection health.

### Manifestation layer

Describes WPF, browser, WebXR, future native VR, and other clients.

The resulting shape is:

    Experience semantics
           |
           v
    coordination protocol
           |
           +--------+--------+
           |        |        |
           v        v        v
          Web     Desktop     VR

No manifestation owns the Experience.

## Co-entanglement

The term is intentionally metaphorical.

A connected WebApp and AnyApp share a logical context while remaining separate processes.

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
- A VR client may own local headset pose.

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

Manifestations must tolerate browser closure, AnyApp closure, interruption, bridge restart, stale messages, incompatible protocol versions, unsupported capabilities, and repository unavailability.

A disconnected manifestation is not necessarily a failed Experience.

The Experience should define what continues locally and what requires coordination.

## Capability model

Capabilities are negotiated rather than assumed.

Example:

    WebApp
      WebXR: immersive-vr
      WebSocket: yes
      viewport: 1920x1080

    AnyApp
      LocalCompute: yes
      FileCache: yes
      DesktopUI: yes

    VR
      HeadPose: yes
      Controllers: yes

Capabilities describe what is possible, not what should automatically happen.

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

This is useful when the browser has limited CPU budget, restricted filesystem access, limited device APIs, or no efficient access to cached artifact data.

## VR companion model

VR adds a constrained manifestation.

    VR
     |
     | pose / input / request
     v
    AnyApp
     |
     | suitable local computation
     v
    result
     |
     v
    VR

The architecture does not assume that AnyApp does everything. Frame-critical work may need to remain on the VR device.

The principle is:

> Put computation where the capability exists and where latency permits it.

## Security boundary

The bridge is a trust boundary.

The desktop validates origin, session token, protocol version, message size, message type, payload schema, sequencing, artifact identity, and requested capability.

The browser should also expose enough state for the user to understand that a local companion is active.

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

Each step should be demonstrated by a working Experience before another abstraction is added.
