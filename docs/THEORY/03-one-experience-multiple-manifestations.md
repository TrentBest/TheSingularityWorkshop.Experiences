![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# 03 — One Experience, Multiple Manifestations

## 01 What

A manifestation is a host-specific way of encountering an Experience. A browser, desktop application, and immersive client may expose different controls and presentation capabilities while referring to the same authored Experience.

Think of a theatre production touring to different venues. The venue changes the stage, lighting, and available equipment; it should not silently rewrite the play. The analogy stops where software contracts begin: unlike a play, a running Experience may have authoritative state, computation, permissions, and synchronization that must be explicitly defined.

## 02 Why

Without a manifestation boundary, host-specific details leak into domain content. A browser click becomes a domain operation; a WPF control becomes a published requirement; a renderer's coordinate system becomes the Experience's identity.

Separating the layers lets the same Experience be presented in multiple ways. It also forces each host to be honest about what it can actually do. Shared identity does not imply identical capability, synchronized state, or automatic failover.

## 03 How

The current manifestation architecture separates three concerns:

- **Experience layer:** identity, authored content, semantic behavior, MicroBundle requirements, configuration, and creator-declared capabilities.
- **Coordination layer:** session identity, protocol version, capabilities, requirements, requests, events, state publication, sequence, and connection health.
- **Manifestation layer:** browser, desktop, WebXR, future native VR, and their host-specific input and presentation.

The Experience defines the meaning of an operation. A host adapter translates local input into a semantic request. The Experience/domain validates that request against current state and authority. A renderer or visualization backend presents the resulting state; it does not decide the domain result.

### Example: a visitor enters a laboratory

A laboratory Experience may define a room, an experiment, the visitor's available actions, and the evidence required to report an outcome. A browser host might render controls in a web page. A desktop host might use native controls. A future immersive host might map the same semantic action to a controller gesture.

The hosts do not get to invent different chemistry or declare a successful experiment merely because an animation finished. The authoritative domain result determines what can be reported.

### Failure is part of the contract

A manifestation may be unavailable, disconnected, or missing a required capability. The Experience should distinguish:
- required capability: entry or a defined feature cannot proceed without it;
- preferred capability: desirable but not mandatory;
- optional capability: may be omitted under an explicit policy;
- delegable work: may be assigned elsewhere when a compatible provider exists;
- frame-critical work: must remain local to the manifestation responsible for immediate interaction or presentation.

A connected host is not automatically authorized to perform every operation. Capability availability and domain permission are separate checks.

## 04 See it in a minute

```text
             One Experience identity
                       |
             authored semantic contract
                       |
           +-----------+-----------+
           |           |           |
        WebApp       AnyApp        VR
        browser      desktop    immersive
           |           |           |
        host input  native input  tracked input
           +-----------+-----------+
                       |
               semantic request
                       |
            Experience/domain validation
                       |
               authoritative result
                       |
              host-specific display
```

**Conceptual:** this diagram teaches the ownership boundary. It is not a captured runtime trace and does not prove that all three hosts currently share a live session.

The existing [Manifestation Architecture](../MANIFESTATION_ARCHITECTURE.md) describes the intended shared identity/session model and the trust boundary. The [StoryBuilder first-run guide](../STORYBUILDER_ANYAPP_FIRST_RUN.md) is explicit that the complete package-to-AnyApp path is not yet verified.

## 05 Choose your next step

- Read [04 — Requirements, Providers, and Authority](04-requirements-providers-and-authority.md).
- Read [GUI](https://github.com/TrentBest/TheSingularityWorkshop.GUI), [Renderer](https://github.com/TrentBest/TheSingularityWorkshop.Renderer), and [Visualize](https://github.com/TrentBest/TheSingularityWorkshop.Visualize) for the presentation/observation responsibilities they own.
- Read [Security Model](../SECURITY_MODEL.md) before treating a browser-to-desktop connection as trusted.

### Practice

1. A browser reports that a visitor clicked “Run experiment.” What must the domain validate before accepting the request?
2. A VR controller is unavailable. Which operations can continue, and who decides?
3. Why is a shared Experience ID insufficient to prove that two hosts share one session?

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
