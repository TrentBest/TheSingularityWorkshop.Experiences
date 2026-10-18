# Safe Mishaps — Chemistry Laboratory Experience Contract

**Status:** design contract plus a host-neutral executable lifecycle state model. This is not a chemistry hazard predictor or implemented avatar animation.

## Design goal

The player is allowed to make a mess. When an experiment produces an unexpected or intentionally scripted *gameplay* outcome, the lab can deliver a memorable cartoon “mixture in the face” moment: a sudden pop, puff of smoke, soot on the face, frazzled hair, ringing ears, a brief dazed idle, then recovery.

The player is not killed, permanently injured, or forced to restart. The gag creates a story and a teachable moment, not a punishment. The laboratory remains a place for curiosity and repeatable experiments.

## Separate the truth from the spectacle

The incident pipeline must keep three things distinct:

1. **Experiment result:** the authoritative output of the selected chemistry capability, including whether the result is calculated, simulated, validated, hypothetical, or unknown.
2. **Incident classification:** a gameplay policy that maps a result or a deliberately authored tutorial event to a safe incident category.
3. **Presentation:** VFX, sound, avatar reaction, temporary controls, cleanup, and explanatory UI.

The current atom/charge conservation analyzer cannot predict an explosion. A balanced reaction is not evidence that a reaction occurs; an unbalanced reaction is not a predictor of a blast. Until a verified model reports a relevant event, any face-full-of-smoke sequence must be labelled and treated as a **scripted comic mishap**, not scientific output.

## Executable contract

The host-neutral [ComicMishapSession](../../../src/Chemistry/ComicMishapSession.cs) now records the experiment snapshot, distinguishes scripted tutorial / model-reported / unknown triggers, enforces the presentation → recovery → evidence-review sequence, and prevents the session from representing death or permanent injury. Its tests are in [ComicMishapSessionTests.cs](../../../tests/Chemistry.Tests/ComicMishapSessionTests.cs). This is lifecycle logic only: it does not animate the avatar, run chemistry, or claim to infer hazardous outcomes.

## Event lifecycle

`ExperimentSubmitted → ResultClassified → IncidentPresented → PlayerRecovers → EvidenceReviewed`

- Preserve the experiment inputs and model/version before the incident.
- Record the actual trigger: model-reported event, failed precondition, or explicitly scripted tutorial event.
- If the chemistry result is unknown, show a generic “unexpected result” gag rather than inventing a chemical cause.
- Present a short visual cue, then the puff/pop and avatar reaction. Keep camera motion brief and avoid flashing patterns.
- Temporarily pause the experiment interaction while the gag plays; do not silently mutate the submitted experiment.
- Restore player control and the prior experiment state automatically.
- Offer a concise explanation that distinguishes observed output from the gameplay embellishment.
- Add the event to the experiment timeline so the player can replay and compare.

## Initial incident vocabulary

| Key | Trigger | Presentation | Recovery |
|---|---|---|---|
| `comic-puff` | Scripted tutorial beat or safe generic unexpected outcome | Small pop, harmless smoke puff, soot face, frazzled hair | Shake head, blink, wipe face |
| `foamy-overflow` | Authored visual scenario; never inferred from atom counts | Bubbles overflow the bench into a contained catch tray | Mop/auto-reset; experiment inputs remain inspectable |
| `thermal-surprise` | Only when a selected thermal model actually reports its threshold event; otherwise scripted | Warm shimmer, surprised recoil, non-damaging glow | Cooldown animation and model/evidence panel |
| `unknown-outcome` | No supported model can classify the submitted combination | Generic cartoon sputter and “We don't know yet” prompt | Retry, inspect assumptions, or save as a question |

These keys are presentation categories, not claims that particular real substances produce those effects.

## Player safety and tone

- No death, lasting injury, realistic burns, or persistent loss of progress from a comic lab mishap.
- No real-world dangerous mixture recipes, quantities, or procedural instructions.
- Do not use the gag to teach that a real hazardous reaction is safe.
- Use clear, accessible visual and audio alternatives; keep effects short and allow reduced-motion settings.
- A player can skip/replay the gag and inspect the experiment record afterward.
- A real-world laboratory mode, if ever introduced, must be a separate reviewed safety experience—not an extension of the cartoon immunity rule.

## Acceptance criteria

- A scripted tutorial can trigger the comic incident without pretending that the chemistry core predicted it.
- The same experiment record is preserved across the incident.
- Player control returns automatically and no permanent damage/death state is written.
- The timeline identifies the trigger type and labels scripted presentation as such.
- Unknown chemistry produces uncertainty, not a fabricated scientific explanation.
- Tests cover recovery and state preservation before any visual polish is called complete.
