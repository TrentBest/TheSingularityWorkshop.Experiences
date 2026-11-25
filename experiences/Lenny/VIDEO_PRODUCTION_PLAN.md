# Lenny — Cinematic Video and Artwork Production Plan

**Status:** storyboard authored; original page artwork and final video renderer are not yet produced.

## Creative direction

The next edition is not a slideshow of the old manuscript pages. It is a cinematic retelling built from the same story, with flexible shot boundaries and deliberate pacing. The manuscript remains the authority for events and narration, but the film may combine, split, hold, or reorder camera coverage where doing so does not change the story's meaning.

The visual target is original children's-book / animated-film artwork: painterly environments, coherent character models, expressive poses, natural light, atmospheric depth, and composed frames. Avoid the current impression of assembled SVG components, flat icons, sticker-like characters, or diagrams. SVG remains useful for interface overlays and simple authored graphics; it should not be the primary illustration medium.

The excluded original concept-PDF artwork remains excluded. Every new character and environment design is a fresh provisional proposal until author review.

## What the audience should see

The opening is a real sequence, not one repeated beach picture:

1. Establish the beach as a place: wide shoreline, surf, sky, people, and depth.
2. Find Lenny playing in the sand with his bucket and shovel.
3. See Dad farther along the beach picking something up.
4. Follow Lenny running toward him, water spilling from the bucket.
5. Let Dad show the bottle and stage their conversation in a readable two-shot.
6. Reveal the wider beach and its ordinary activities before the story explains the ocean connection.

When the family returns the next day, the geography should match the opening so the change is immediately understood. The beach is now busier and littered. Beachgoers are not frozen extras: people recline on towels or cots, sit up, turn their heads, adjust an arm, talk, surf, picnic, and build sandcastles. Their movement should be subtle and varied, never synchronized or exaggerated. Waves should continuously travel, crest, break, wash up the shore, and recede.

The later emotional arc should be staged with equal care: Lenny's discouragement, Dad kneeling and putting an arm around him, the mysterious cans and bottles, the dolphin reveal, the near-wave rescue, the underwater bubble, the damaged habitats, and the hopeful return.

## Artwork delivery standard

- One distinct, original illustration for each of the 33 numbered story pages (33 page images minimum).
- Prefer high-quality raster artwork (WebP or PNG masters) over hand-assembled SVG shapes.
- Master composition: 16:9 cinematic frame, with safe crop areas for the reading layout.
- Reuse approved character model sheets and environment references to maintain Lenny, Dad, beach geography, clothing, props, and dolphin continuity.
- Distinguish each page's moment; do not reuse the same generic beach image for unrelated actions.
- Keep background characters alive through pose variation and small independent gestures.
- Do not invent exact character ages, physical traits, species, permanent markings, litter quantities, or plot events that the manuscript does not establish.
- Environmental harm is emotionally clear but non-graphic.
- Every image remains provisional until author approval; record provenance and revision identity.

The storyboard currently defines 37 cinematic shots mapped to the 33 source pages. Some pages receive more than one shot so an action can breathe, while the page illustration remains a stable authored asset.

## Shared scene model, two presentation targets

`story.json` remains the canonical wording. `storyboard.json` is the presentation timeline: shots, source-page anchors, visual direction, performance notes, emotional beats, target durations, and artwork paths. It does not duplicate or rewrite the manuscript.

The reader and video renderer should both consume that shared model:

- **Reader:** one image per story page, page navigation, readable text, optional ambient motion.
- **Video:** 16:9 shot timeline, camera framing/movement, animated performances, narration, sound design, transitions, and credits.

Do not fork the story into separate reader and video manuscripts. A video may combine or split shots, but every narration segment must resolve back to canonical story text.

## Video production pipeline

A YouTube-ready deliverable is more than an HTML page with autoplay. The production pipeline should have explicit stages:

1. Validate storyboard references and require approved artwork for every numbered page.
2. Resolve narration from `story.json`; allow reviewed narration takes and timing metadata without editing the canonical prose.
3. Render each shot at a fixed output profile (initial target: 1920×1080, 30 fps).
4. Animate camera and authored performances from timeline time, not browser frame count.
5. Mix narration, ambience, surf, and music on separate tracks with clear licensing/provenance.
6. Render a final MP4 (H.264 video + AAC audio), with captions/subtitles and a thumbnail.
7. Review the full film for continuity, pacing, accessibility, audio levels, and factual/story fidelity.
8. Treat uploading or publishing to YouTube as a separate user-authorized action.

The static GitHub Pages reader cannot itself provide a dependable MP4 production pipeline. A browser preview is useful for review, but final rendering should run in a controlled renderer/encoder environment (for example, a deterministic frame renderer followed by FFmpeg). Speech synthesis can be a temporary timing aid, not the final narration track.

## Motion and ownership

- Experiences owns the authored shot sequence, story anchors, actor identities, performance intent, and timing metadata.
- Physics owns authoritative world motion and fixed-step observations when a scene requires physical movement.
- Gestures owns reusable gesture intent and procedures where applicable.
- Renderer / Visualization owns camera, composition, interpolation, raster/vector presentation, and visual output.
- The video renderer owns frame scheduling and encoding; it must not invent a second movement solver.
- Ambient background motion may be presentation-only, but must be labelled as such until driven by authoritative observations.

The existing Lenny Physics contract remains in force. Do not call CSS animation or a browser transform Physics-backed. A first cinematic pass can use authored presentation performances for waves, breathing, glances, and background activity while the runtime bridge is developed, provided the distinction is explicit.

## Current gaps

- [ ] 33 original page illustrations created and author-reviewed.
- [ ] Character model sheets for Lenny, Dad, beachgoers, and dolphins.
- [ ] Beach environment continuity sheet and surf/wave reference.
- [ ] One distinct reader illustration wired to each story page.
- [ ] Cinematic timeline preview.
- [ ] Narration recording and timing data.
- [ ] Ambient sound and licensed/original music.
- [ ] Deterministic 1080p frame renderer and MP4 encoder.
- [ ] Captions, thumbnail, and final review.
- [ ] Optional production host bridge for Physics-backed actor root motion.

## Review gates

No visual proposal becomes approved canon without explicit author review. No YouTube upload or public release is implied by creating the production pipeline. Keep the reader usable without animation, respect reduced-motion preferences, and preserve all manuscript text unless the author explicitly approves an editorial rewrite.
