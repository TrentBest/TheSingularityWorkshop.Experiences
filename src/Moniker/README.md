# The Singularity Workshop — Moniker

This package is the canonical Workshop Moniker MicroBundle.

The Moniker is deliberately an ordinary MicroBundle:

- **Bundle ID:** `3101`
- **Version:** `1.0.0`
- **Provider:** `workshop-moniker`
- **Dependencies:** none

It owns the semantic GUI surface for:

> THE  
> SINGULARITY  
> WORKSHOP

The package also defines the renderer-independent `IMonikerMotion` contract. It does not implement a specific motion; wave, orbit, dissolve, and future motions can remain independently addressable MicroBundles that implement this contract.

The package does **not** depend on FSM_COS. FSM_COS remains the composition boundary that loads and arbitrates the bundle.

A host such as AnyApp or WebPage can therefore request Bundle `3101` in its startup `RuntimeManifest`, compose it through FSM_COS, and choose its own renderer for the resulting `GuiNode` tree.

That separation is intentional:

```text
Host startup manifest
        |
        | requests 3101
        v
     FSM_COS
        |
        | loads + arbitrates
        v
MonikerMicroBundle (3101)
        |
        | semantic GuiNode
        v
Moniker Experience + optional IMonikerMotion
        |
        v
host renderer
```

The Workshop's startup experience may keep this bundle present regardless of which later Experience is selected. The host owns that bootstrap policy; the composition engine remains generic.
