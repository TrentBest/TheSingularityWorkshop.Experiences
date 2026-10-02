# WebApp to AnyApp Security Model

Security is part of the product architecture because the WebApp can initiate contact with a process that has desktop privileges.

Primary rule:

> The browser may request. AnyApp decides.

## Trust zones

    Public Web
        |
        | untrusted input
        v
    Browser
        |
        | origin + token + schema
        v
    Local Bridge
        |
        | validated request
        v
    AnyApp
        |
        | verified artifacts
        v
    FSM_COS / Runtime

## Origin validation

The bridge maintains an explicit origin allowlist.

Development may use a configurable local origin. Production should use the actual deployed WebApp origin.

Origin is one input to the trust decision, not authentication by itself.

## Launch tokens

The WebApp creates a short-lived launch token for a desktop launch request.

The token binds the launch request to the requested Experience identity and intended bridge session.

Tokens should be unpredictable, expire, and be single-use where practical.

They are not permanent credentials.

## Protocol validation

AnyApp rejects unsupported protocol versions, unknown message types, malformed JSON, oversized messages, missing required fields, invalid session IDs, and invalid sequence values where sequencing applies.

The browser should similarly reject malformed responses.

## Artifact trust

A browser-provided Experience ID is a request, not proof.

AnyApp resolves the Experience through the trusted repository path and verifies:

    ExperienceId
    Version
    ContentHash
          |
          v
    repository artifact
          |
          v
    verified bytes

Only then should composition occur.

The browser must never directly supply arbitrary assembly bytes to AnyApp.

## No arbitrary execution

The bridge must not expose generic operations such as shell execution, process launching, arbitrary file access, or arbitrary assembly loading from a URL.

Expose explicit semantic operations owned by the protocol.

Examples include experience.request, experience.state, event, and heartbeat.

If a future operation requires local computation, define its input and output contract explicitly.

## Resource limits

The bridge should have finite limits for concurrent sessions, message size, event frequency, heartbeat rate, request duration, and retained session state.

## User visibility

A connected desktop companion should not be invisible in a way that surprises the user.

AnyApp should eventually expose connection state, connected WebApp origin, Experience identity, session state, available capabilities, and disconnect control.

The browser should expose enough state for the user to understand that a local companion is active.

## Privacy

Only collect data required for operation or explicitly requested telemetry.

Useful technical telemetry can include protocol version, capability flags, viewport, WebXR availability, latency, and lifecycle state.

Avoid collecting unrelated browsing history, arbitrary page contents, filesystem contents, credentials, or unrelated device identifiers.

## Future hardening

Before calling the bridge production-grade, evaluate:

- malicious websites attempting to connect;
- browser extension interference;
- DNS rebinding assumptions;
- localhost/local-network permission behavior;
- token replay;
- stale sessions;
- denial of service;
- executable protocol registration hijacking;
- update and rollback integrity;
- repository compromise;
- artifact signature verification;
- local cache poisoning.

Security work should become tracked implementation issues rather than remaining README promises.
