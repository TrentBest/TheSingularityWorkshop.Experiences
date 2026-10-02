# Experience publication

Forge and Moniker are host-independent MicroBundles. Their build outputs are ordinary .NET assemblies containing the public parameterless `IMicroBundle` implementation.

## Artifact boundary

```text
Experience project
      |
      | dotnet build
      v
   *.dll
      |
      | SHA-256
      v
MicroBundleArtifact
  BundleId
  Version
  ContentHash
  Content = DLL bytes
      |
      v
MicroBundleRepository
      |
      v
AnyApp / another composition host
      |
      | materialize
      v
IMicroBundle
      |
      v
FSM_COS
```

The repository does not understand .NET assemblies. It stores and verifies opaque bytes. The composition host decides how those bytes become an executable MicroBundle.

There is deliberately no second binary envelope around the DLL. Bundle identity and immutable content identity already belong to the repository artifact address.

## Experience manifest responsibility

An Experience manifest identifies every artifact required by its published composition closure:

- Bundle ID
- immutable artifact version
- SHA-256 content hash
- optional MicroBundle configuration

This keeps dependency discovery and arbitration on the composition side. The repository remains delivery/storage infrastructure.

## Current build outputs

The CI build stages each Experience assembly alongside its NuGet package. The staged DLL is the candidate repository artifact; publication to a repository is a separate delivery action.

This distinction is intentional:

- **build** creates the executable MicroBundle artifact;
- **identity** records its immutable hash;
- **publication** makes that identity discoverable;
- **composition** materializes and executes it.

No host is the source of truth for Forge or Moniker.