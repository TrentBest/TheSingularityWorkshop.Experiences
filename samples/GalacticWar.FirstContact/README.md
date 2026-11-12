# Galactic War — First Contact Runtime Proof

A small, deterministic console trace for the Vesper Reach first-contact opening.

Run:

    dotnet run --project samples/GalacticWar.FirstContact/GalacticWar.FirstContact.csproj

The sample demonstrates the runtime boundary only:
- a campaign signal occurrence is distinct from its authored StoryBuilder event identity;
- an issued bulletin is distinct from channel availability;
- channel availability does not imply observer receipt;
- Mara and the player acquire the initial bulletin at different fictional times;
- Teren receives nothing and therefore has no runtime receipt;
- a correction is a second issue with an explicit predecessor, not a destructive rewrite;
- the player does not acquire the correction merely because it exists.

The story-artifact and source identifiers stand in for stable references supplied by a validated authored composition. This sample does not implement StoryBuilder authoring, NPC dialogue, a GUI, or a complete playable station.
