# Experience Compute Requirements

An Experience declares what it requires from its available manifestations and compute environment.

The Workshop must not assume a universal ladder such as:

```
VR -> WebApp -> AnyApp
```

That would make the platform decide what an Experience means.

Instead, the **Experience creator defines the requirement envelope**.

## Creator-owned requirements

An Experience may declare that it can run:

- on MyVR alone;
- on WebApp + MyVR;
- with AnyApp as a required companion;
- with AnyApp + MyVR;
- with WebApp + AnyApp;
- with WebApp + AnyApp + MyVR;
- or through another supported manifestation combination.

The declaration describes **requirements and capabilities**, not a forced execution topology.

For example:

```text
Experience
    |
    +-- required: immersive spatial presentation
    +-- required: capability X
    |
    +-- optional: browser computation
    +-- optional: desktop GPU computation
    +-- optional: desktop persistence
    |
    +-- preferred: MyVR + AnyApp
```

A different Experience might require:

```text
Experience
    |
    +-- required: browser presentation
    +-- required: WebGPU
    +-- optional: MyVR
    +-- optional: AnyApp
```

The platform should evaluate these declarations against the capabilities actually available.

## Capability graph, not capability ladder

There is no universal meaning of "more capable."

A VR headset may have capabilities that a desktop does not have.
A browser may have APIs that a desktop process does not expose.
A desktop may have compute, storage, or artifact access that neither browser nor headset can provide.

Therefore the system should model:

```text
                 Experience requirements
                         |
          +--------------+--------------+
          |              |              |
       WebApp          AnyApp          MyVR
          |              |              |
      capabilities    capabilities    capabilities
          \              |              /
           +-------- available --------+
                         |
                  execution plan
```

The execution plan is derived from the creator's declared requirements and the capabilities currently available.

## Minimum, preferred, optional

The eventual Experience contract should distinguish at least:

- **Required** — the Experience cannot enter or cannot provide a defined feature without this capability.
- **Preferred** — the creator identifies a desirable execution location, but the Experience may operate elsewhere when semantics permit.
- **Optional** — the Experience can exploit the capability when available.
- **Delegable** — the creator identifies work that may be performed by another manifestation or companion.
- **Frame-critical** — work that must remain local to the manifestation responsible for the relevant frame or immediate interaction.

These are semantic requirements. They are not hard-coded assumptions about WebApp, AnyApp, or MyVR.

## Capability negotiation

At runtime:

1. the Experience identity is resolved;
2. the Experience requirements are read;
3. available manifestations publish their capabilities;
4. the system determines whether the requirements can be satisfied;
5. compatible capabilities are assigned to the appropriate manifestation or service;
6. unsupported optional capabilities degrade gracefully;
7. unsatisfied required capabilities prevent entry or clearly report the missing requirement.

This means a creator can make an Experience that **requires AnyApp**, while another Experience can run entirely on a VR device.

## Scale of an Experience

As Experiences become more ambitious, it is expected that all manifestations may participate simultaneously:

```text
                    Experience
                         |
          +--------------+--------------+
          |              |              |
       WebApp          AnyApp          MyVR
       browser         desktop         VR
          |              |              |
     browser APIs   local compute    frame-critical
     WebGPU/etc.    storage/GPU       pose/input
          \              |              /
           +------- coordinated --------+
```

There is no architectural problem with using all three at once.

The important constraint is that each operation has an explicit owner and contract.

## AnyApp independence

AnyApp is never intrinsically dependent upon MyVR.

AnyApp can:

- run an Experience without VR;
- work with WebApp without MyVR;
- provide services to MyVR when an Experience requires them;
- participate alongside both WebApp and MyVR when an Experience benefits from all three.

Likewise, MyVR can operate without AnyApp when the Experience requirements and device capabilities permit it.

## Manifestation availability is not authority

The fact that a manifestation is connected does not grant it authority over every operation.

For example:

- MyVR owns device-local pose and immediate input.
- WebApp owns browser-local presentation and browser APIs.
- AnyApp may own explicitly delegated desktop computation or persistence.
- The Experience/runtime owns semantic state according to its contract.
- The repository owns publication and artifact identity.
- FSM_COS owns composition.

The protocol must preserve those ownership boundaries even when every manifestation is connected simultaneously.

## Failure and degradation

If an Experience declares a capability as required and that capability disappears, the Experience must follow its declared failure semantics.

If a capability is optional, the Experience should be able to degrade without pretending that the capability still exists.

Examples:

- AnyApp disconnects: desktop-only computation stops or migrates if the Experience permits it.
- MyVR disconnects: browser/desktop manifestations may continue.
- WebApp closes: AnyApp and MyVR may continue if their Experience semantics permit it.
- all three connect: the Experience may exploit all declared capabilities simultaneously.

## Architectural rule

> **The Experience creator declares what the Experience requires. The platform discovers what is available and composes the compatible execution arrangement.**

This keeps the platform general enough for Experiences far beyond the first WebApp/AnyApp/MyVR demonstration.
