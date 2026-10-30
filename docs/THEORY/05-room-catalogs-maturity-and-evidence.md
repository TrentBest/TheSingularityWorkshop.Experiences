![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# 05 — Room Catalogs, Maturity, and Evidence

## 01 What

An Experience can be authored as a sequence of places, lessons, exhibits, or activities. A room catalog gives those units a renderer-independent description that a host can inspect without embedding the host's GUI into the domain model.

The current `ExperienceRoomCatalog` is a versioned catalog of `ExperienceRoom` records. Each room carries a stable key, sequence, title, purpose, visitor action, authoritative-state description, evidence rule, maturity, required capabilities, and navigation links.

This is a content contract, not a complete lesson engine or runtime state store.

## 02 Why

A title and a button label are not enough to define a meaningful experience. A visitor needs to know what an activity is for, what action is available, what state is authoritative, and what evidence supports the result.

Separating these fields from presentation gives different hosts a common semantic description. It also makes it harder to accidentally present an authored idea as a completed calculation or validated simulation.

## 03 How

### The room contract

The current catalog validates several structural properties before it can be serialized:

- the catalog has a supported schema version, Experience key, status, and at least one room;
- room keys are non-empty and unique;
- room sequences are within the supported range and unique;
- title, purpose, visitor action, authoritative-state description, and evidence rule are present;
- maturity values are defined;
- required-capability and navigation entries are not duplicated;
- every navigation target names a room in the same catalog.

The catalog's validator does **not** prove that a capability exists, that the visitor action is executable, that the authoritative state is truly authoritative, or that the evidence rule has been satisfied. Those require domain-specific validation and runtime evidence.

### Maturity is a claim that needs evidence

The enum currently defines five levels:

| Maturity | Meaning |
|---|---|
| `Concept` | The room is an authored idea and scope |
| `AuthoredData` | Domain input or content has been authored |
| `Calculated` | A calculation has produced a result |
| `Simulated` | A simulation has produced a result |
| `Validated` | The relevant result has passed its defined validation |

The catalog stores the maturity label but does not enforce a progression, calculate a result, or validate a simulation. A room author must not raise the label merely because a screen or fixture exists.

### Binary persistence

The catalog implements the shared `IBinarySerializable` boundary. Its own format begins with the `ECAT` magic value and a binary-format version, followed by the catalog schema version and room data. Integers are little-endian; strings are UTF-8 with signed 32-bit byte lengths; collection counts are signed 32-bit values.

The reader applies bounds to room counts, per-room collection counts, and string byte lengths. It rejects unknown format versions, malformed UTF-8, truncated payloads, and decoded catalogs that fail validation. The writer also refuses to pack an invalid catalog.

The serializer is a persistence boundary for this catalog. It is not an immutable MicroBundle artifact publisher, repository client, or FSM_COS runtime composition.

### A concrete authored example

`FluidDynamicsLaboratoryCatalog` defines seven linked rooms: Water Gallery, Wave Basin, Bathymetry & Terrain, Rainfall & Runoff, Vessel Test Tank, Shoreline & Structures, and Evidence & Timeline. Each is deliberately marked `Concept` and its evidence rule describes limits that must not be crossed by presentation alone.

For example, the Vessel Test Tank does not claim buoyancy, drag, or simulated vessel motion until a validated coupling model supplies those results. That restraint is part of the room's semantic contract, not merely a disclaimer on a screen.

## 04 See it in a minute

```csharp
var catalog = FluidDynamicsLaboratoryCatalog.Create();

IReadOnlyList<string> errors = catalog.Validate();
if (errors.Count != 0)
    throw new InvalidOperationException(string.Join("; ", errors));

using var stream = new MemoryBinaryStream();
catalog.Pack(stream);
stream.Position = 0;

var restored = new ExperienceRoomCatalog();
restored.Unpack(stream);
```

The focused test suite verifies that the seven-room catalog has unique sequences and resolvable links, that a binary round trip preserves selected semantics, that packing rejects a missing navigation target, and that unpacking rejects a truncated payload.

**Source-checked:** `src/ExperienceCatalog/ExperienceRoomCatalog.cs` and `tests/ExperienceCatalog.Tests/ExperienceRoomCatalogTests.cs`. The tests are evidence for the behaviors named above; they do not demonstrate a host UI or a full author-to-runtime pipeline.

## 05 Choose your next step

- Continue to [03 — One Experience, Multiple Manifestations](03-one-experience-multiple-manifestations.md) for the host boundary.
- Read [Startup Contract](../STARTUP_CONTRACT.md) and [StoryBuilder → Experiences → AnyApp first run](../STORYBUILDER_ANYAPP_FIRST_RUN.md) for the current integration seam.
- Read [Semantic Interaction Requirements](../contracts/SEMANTIC_INTERACTION_REQUIREMENTS.md) for the separate capability requirement contract.

### Practice

1. Why is a room marked `Simulated` not automatically `Validated`?
2. Which catalog checks can run without launching a host? Which claims still need runtime evidence?
3. What additional contract would be needed to prove that a room's required capabilities are available in a particular host?

---

<!-- THE SINGULARITY WORKSHOP FOOTER -->
<p align="center"><img src="https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png" alt="The Singularity Workshop avatar" width="200" /></p>
