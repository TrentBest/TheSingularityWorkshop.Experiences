# Little Lenny and the Big Ocean Problem

A small reading Experience built around the supplied children's story manuscript.

## Intent

Present the author's story as a clear, page-by-page reading experience. Keep the story itself at the center: a child, a parent, a beach, and a conversation about caring for ocean life.

This is a story presentation, not a game, ocean simulator, or environmental data visualization.

## Source material

- [Source manuscript transcription](SourceManuscript.md)
- [Structured reading sequence](story.json)
- Original supplied source: `LENNY MANUSCRIPT.pdf` (35 pages)

The structured sequence is derived directly from the transcription: title, dedication, 33 numbered story pages, and closing page. The transcription preserves the extracted wording and printed story-page order. The original PDF remains the authority for its page layout and illustrations. The PDF itself has not yet been added to this repository.

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

## Artwork and presentation direction

The author has expressed dissatisfaction with the original artwork. No replacement art direction has been approved.

Therefore:

- Do not reuse, imitate, or replace the original illustrations by assumption.
- Do not generate finished illustrations or establish a visual style without the author's direction.
- Keep the reader structure independent of its eventual artwork.
- The current interface is a restrained, text-first reading preview, not approved book artwork or a final visual design.

## Ownership boundary

This Experience owns the story's presentation identity, authored sequence, and reader journey.

The host owns browser/application lifecycle and navigation transport. Shared rendering and GUI packages own reusable presentation primitives. Story-authoring tools belong in StoryBuilder, not in this Experience.

## Current maturity

**Runnable static reading preview.** The manuscript transcription and structured page sequence are committed, and a browser reader can be served locally. This is not yet integrated into the Workshop WebPage host or a runtime-loaded Experience manifest. Approved artwork and the original PDF are not yet included.

## Acceptance criteria for this slice

- Every story page appears in the correct order.
- Displayed wording matches the source transcription.
- Previous/next navigation cannot move outside the sequence.
- Progress reflects the selected page.
- The dedication and ending are included.
- The experience remains readable without illustrations.
- No placeholder is represented as approved artwork or a live simulation.
