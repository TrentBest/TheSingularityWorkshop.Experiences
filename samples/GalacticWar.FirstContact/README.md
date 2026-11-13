# Galactic War — First Contact Runtime Proof

A small, deterministic console trace for the Vesper Reach first-contact opening.

Run:

    dotnet run --project samples/GalacticWar.FirstContact/GalacticWar.FirstContact.csproj

The sample demonstrates the runtime boundary only:
- a campaign signal occurrence is distinct from its authored StoryBuilder event identity;
- an official bulletin and an independent survey-ship account reference the same campaign occurrence while retaining distinct source identities;
- each account is separately issued, made available on its own station channel, and acquired only by the observer with an explicit receipt;
- Mara acquires the official bulletin, Ilyan acquires the independent account, and neither acquisition grants the other observer's knowledge;
- Teren receives nothing and therefore has no runtime receipt;
- the player acquires and is presented with the official bulletin at a separate fictional time;
- a correction is a second official issue with an explicit predecessor, not a destructive rewrite;
- Mara acquires the correction, while the player and Teren do not acquire it merely because it exists.

The deterministic test suite verifies that the independent account references the same signal occurrence, preserves a distinct source, and does not leak into Mara's or Teren's receipt history.

The story-artifact and source identifiers stand in for stable references supplied by a validated authored composition. This sample does not implement StoryBuilder authoring, NPC dialogue, a GUI, or a complete playable station.
