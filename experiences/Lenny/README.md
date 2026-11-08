# Little Lenny and the Big Ocean Problem

A small reading Experience built around the supplied children's story manuscript.

## Read the story

**[Read Little Lenny and the Big Ocean Problem](https://trentbest.github.io/TheSingularityWorkshop.Experiences/experiences/Lenny/)**

The reader is published from the `development` branch by the **Publish Lenny reading preview** workflow whenever that branch changes. It publishes only the static reader and its `story.json` content.

## Intent

Present the author's story as a clear, page-by-page reading experience. Keep the story itself at the center: a child, a parent, a beach, and a conversation about caring for ocean life.

This is a story presentation, not a game, ocean simulator, or environmental data visualization.

## Source material

- [Source manuscript transcription](SourceManuscript.md)
- [Structured reading sequence](story.json)
- [StoryBuilder composition manifest](CompositionManifest.json) — source-linked, stable page IDs and exact transcription text for authoring import.
- [Character and visual story plan](VisualStoryPlan.md)
- [StoryBuilder composition pilot](https://github.com/TrentBest/StoryBuilder/blob/development/docs/LENNY_COMPOSITION_PILOT.md)
- Original supplied source: `LENNY MANUSCRIPT.pdf` (35 pages)

The structured sequence is derived directly from the transcription: title, dedication, 33 numbered story pages, and closing page. `CompositionManifest.json` provides the same source material in an authoring-oriented shape with stable page IDs and source-blob provenance; it does not replace the reader's `story.json`. The transcription preserves the extracted wording and printed story-page order. The supplied 35-page PDF is already available as the original source reference and contains original illustrations on 29 of the 33 story pages. Those illustrations establish the visual baseline for character design, props, environments, palette, and rendering style. The PDF is retained as source material rather than copied into the public reader repository by default.

## Runnable reading preview

Open [index.html](index.html) through a static HTTP server. From the repository root, for example:

```bash
python -m http.server 8000
```

Then visit `http://localhost:8000/experiences/Lenny/`.

The reader currently provides:

- Title and dedication pages.
- One manuscript page at a time, followed by the original closing statement.
- Previous/next buttons with bounded navigation.
- Keyboard navigation with the left/right arrow keys.
- Swipe navigation on touch screens.
- Reading progress and adjustable text size.
- Responsive layout and reduced-motion handling.

The page reads its content from `story.json`; it does not embed a second copy of the manuscript in the HTML. This makes the authored sequence independently inspectable and keeps presentation separate from story content.

## Character and visual development

The supplied manuscript already includes a substantial body of original artwork. [VisualStoryPlan.md](VisualStoryPlan.md) now treats that artwork as the primary continuity reference and maps the visual direction across all 33 story pages.

The plan separates:
- **Manuscript canon:** facts explicitly established by the author's story.
- **Visual interpretation:** staging, expression, pose, and composition used to communicate a scene.
- **Open design decisions:** physical traits and costume details the manuscript does not specify.

Preserve the source-art designs: Lenny's blond hair, light-blue shirt, orange shorts, and childlike proportions; Dad's brown shoulder-length hair, mustache, tan shirt, and dark shorts; and the established gray-blue dolphin design. The original illustrations also establish a soft watercolor-edged presentation, warm beach palette, and deeper blue-green underwater palette. New artwork should extend this visual language, not redesign the cast.

## Ownership boundary

This Experience owns the story's presentation identity, authored sequence, and reader journey.

The host owns browser/application lifecycle and navigation transport. Shared rendering and GUI packages own reusable presentation primitives. Story-authoring tools belong in StoryBuilder, not in this Experience. StoryBuilder's provenance and canon discipline informs the visual brief, but the reading Experience remains independently runnable. The StoryBuilder repository now tracks the composition-to-publication pilot, including visual asset provenance, restrained animation, and the GitHub Pages publication boundary.

## Current maturity

**Runnable static text-reading preview with source-art reference available.** The manuscript transcription and structured page sequence are committed, and a browser reader can be served locally. The original PDF contains illustrations for 29 story pages; those images have not yet been integrated into the reader. Four story pages (4, 13, 19, and 23) are text-only in the supplied source. This is not yet integrated into the Workshop WebPage host or a runtime-loaded Experience manifest.

## Acceptance criteria for this slice

- Every story page appears in the correct order.
- Displayed wording matches the source transcription.
- Previous/next navigation cannot move outside the sequence.
- Progress reflects the selected page.
- The dedication and ending are included.
- The experience remains readable without illustrations.
- No placeholder is represented as approved artwork or a live simulation.
