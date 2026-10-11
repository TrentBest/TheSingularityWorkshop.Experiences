# The Singularity Workshop — Moniker Wave Motion

This project contains one independently addressable MicroBundle:

- **Bundle ID:** `3102`
- **Version:** `1.0.0`
- **Provider:** `workshop-moniker-motion-wave`
- **Dependencies:** none at runtime

It implements the shared `IMonikerMotion` contract from the Moniker package. The canonical Moniker content remains Bundle `3101`; this bundle contributes only the wave presentation parameters.

```text
Moniker MicroBundle (3101)
  static words + semantic GUI
            |
            v
Moniker Experience
            |
            +---- Moniker Wave Motion MicroBundle (3102)
            |
            v
  presentation-ready GUI tree
            |
            v
      host renderer
```

The motion bundle does not depend on WPF, Blazor, a graphics API, or FSM_COS. It returns a new GUI tree and leaves the static source unchanged. The Experience chooses whether to compose this motion or another implementation of `IMonikerMotion`.

The project is built and packed independently from the Moniker content project. It is not published automatically.
