using TheSingularityWorkshop.Workshop.Gui;
using TheSingularityWorkshop.Workshop.Gui.Svg;
using TheSingularityWorkshop.Experiences.StoryForge;

var outputDirectory = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath("artifacts/living-docs");
Directory.CreateDirectory(outputDirectory);

var renderer = new SvgGuiRenderer();
Write("casino-floor.svg", renderer.Render(CasinoFloor(), width: 1200, height: 760));
Write("story-forge-coverage.svg", renderer.Render(StoryForgeCoverage(), width: 1200, height: 800));
Console.WriteLine($"Generated two Experience scenes through GUI.Core → GUI.SVG: {outputDirectory}");

void Write(string name, string svg)
{
    var path = Path.Combine(outputDirectory, name);
    File.WriteAllText(path, svg);
    Console.WriteLine($"{name}: {new FileInfo(path).Length:N0} bytes");
}

static GuiNode CasinoFloor()
{
    var root = Scene("casino-floor-scene",
        "The casino is an economy you can see.",
        "Simulation casino floor: cage, player value, table escrow, slots, journal-derived metrics, and a reconciliation invariant.");
    Rect(root, "background", 0, 0, 1200, 760, "#10152a", 0);
    Text(root, "eyebrow", "THE SINGULARITY WORKSHOP · LIVING EXPERIENCE", 48, 52, "#58d9ff", 13, 800);
    Text(root, "title", "The casino is an economy you can see.", 48, 96, "#f4f7ff", 32, 800);
    Text(root, "subtitle", "Simulation only · every chip movement leaves a trace · reconciliation is the truth test", 48, 124, "#aebbd5", 15);
    Rect(root, "floor-panel", 48, 153, 1104, 352, "#111a2c", 20, "#34425f");
    Text(root, "floor-label", "THE FLOOR · DOMAIN STATE WITH A PRESENTATION", 72, 183, "#aebbd5", 12, 700);

    Station(root, "cage", 72, 204, 220, 118, "CAGE", "S$ ↔ CHIP", "BUY-IN / CASH-OUT", "#ffd76a");
    Station(root, "table", 350, 204, 292, 118, "TABLE", "Player → escrow → result", "WAGERS STAY AUDITABLE", "#58d9ff");
    Station(root, "slots", 700, 204, 428, 118, "SLOT MACHINES", "Deterministic game outcomes", "RULES ARE NOT ANIMATION", "#ff79c6");

    Flow(root, "flow-buyin", "M182 322V364H182V405", "#ffd76a");
    Flow(root, "flow-wager", "M495 322V365H445V405", "#58d9ff");
    Flow(root, "flow-payout", "M840 322V384H723V405", "#ff79c6");
    Flow(root, "flow-cashout", "M182 322V382H1005V405", "#64e5bf");
    FlowCard(root, "buyin", 72, 405, 220, "BUY-IN", "virtual currency → chips", "#ffd76a");
    FlowCard(root, "wager", 350, 405, 220, "WAGER", "player → table escrow", "#58d9ff");
    FlowCard(root, "payout", 628, 405, 220, "PAYOUT", "escrow / reserve → player", "#ff79c6");
    FlowCard(root, "cashout", 906, 405, 198, "CASH-OUT", "chips → virtual currency", "#64e5bf");

    Rect(root, "operator-panel", 48, 529, 1104, 185, "#18223a", 20, "#425270");
    Text(root, "operator-label", "OPERATOR VIEW · DERIVED FROM THE JOURNAL, NEVER A SECOND SOURCE OF TRUTH", 72, 561, "#aebbd5", 12, 700);
    Metric(root, "gross-wagers", 72, "GROSS WAGERS", "Σ stakes", "activity · not revenue", "#58d9ff");
    Metric(root, "gross-payouts", 334, "GROSS PAYOUTS", "Σ returns", "returned value", "#ff79c6");
    Metric(root, "chip-obligation", 596, "CHIP OBLIGATION", "Σ unredeemed", "not profit", "#ffd76a");
    Rect(root, "reconciliation-card", 858, 580, 266, 88, "#102d2a", 12, "#64e5bf", 2);
    Text(root, "reconciliation-label", "RECONCILIATION", 876, 605, "#aebbd5", 12, 700);
    Text(root, "reconciliation-zero", "Δ = 0", 876, 637, "#64e5bf", 27, 800);
    Text(root, "reconciliation-caption", "invariant · all currencies", 876, 657, "#64e5bf", 11);
    Text(root, "boundary", "Illustrative visual proof · no real-money handling or redemption", 48, 741, "#aebbd5", 11);
    return root.Build();
}

static GuiNode StoryForgeCoverage()
{
    var root = Scene("story-forge-scene",
        "The world has time. The book has order.",
        "Parallel world, character, and archive timelines. Colored story-time ranges connect by string lines to matching colored sequence blocks in books and series.");
    Rect(root, "background", 0, 0, 1200, 800, "#111629", 0);
    Text(root, "eyebrow", "THE SINGULARITY WORKSHOP · STORY FORGE", 42, 48, "#62d8ff", 12, 800);
    Text(root, "title", "The world has time. The book has order.", 42, 88, "#f5f6ff", 32, 800);
    Text(root, "subtitle", "Follow a moment through parallel timelines into the exact place a reader encounters it.", 42, 116, "#aab7d3", 15);
    Rect(root, "timeline-panel", 40, 143, 1120, 344, "#121b30", 18, "#39496b");
    Text(root, "timeline-label", "STORY TIME · EACH TIMELINE HAS ITS OWN DOMAIN AND SCALE", 64, 173, "#aab7d3", 12, 700);

    // These ranges are real StoryForge domain values. Their distinct units and domain extents
    // are mapped to the same drawing width only for display; the source coordinates stay independent.
    var world = new StoryTimeline("world", "World events", "years", 5m);
    var worldCoverage = new PublicationCoverage("book-one", "signal", "chapter", 0,
        new StoryTimeRange(world.At(2m), world.At(7m)));
    var mira = new StoryTimeline("mira", "Mira", "days", 25m);
    var miraCoverage = new PublicationCoverage("book-two", "disappearance", "chapter", 0,
        new StoryTimeRange(mira.At(42m), mira.At(73m)));
    var archive = new StoryTimeline("archive", "The Archive", "reigns", 1m);
    var archiveCoverage = new PublicationCoverage("book-one", "fragment", "chapter", 1,
        new StoryTimeRange(archive.At(1.3m), archive.At(3.2m)));

    Timeline(root, world, worldCoverage, 20m, "world timeline", 217, "#62d8ff", "First signal");
    Timeline(root, mira, miraCoverage, 100m, "character arc", 297, "#ff8cc8", "Disappearance");
    Timeline(root, archive, archiveCoverage, 5m, "institutional record", 377, "#ffd36a", "Hidden record");
    AddTicks(root, "world-years", new[] { "Year 0", "Year 5", "Year 10", "Year 15", "Year 20" }, 198);
    AddTicks(root, "mira-days", new[] { "Day 0", "Day 25", "Day 50", "Day 75", "Day 100" }, 278);
    AddTicks(root, "archive-reigns", new[] { "Reign I", "Reign II", "Reign III", "Reign IV", "Reign V" }, 358);

    Rect(root, "publication-panel", 40, 505, 1120, 226, "#121b30", 18, "#39496b");
    Text(root, "publication-label", "PUBLICATION ORDER · SERIES CONTAIN BOOKS; COLLECTIONS CONTAIN SERIES", 64, 535, "#aab7d3", 12, 700);

    // Reserve the cards first, then draw string lines, then place blocks on top of their termini.
    BookPanel(root, "book-one", 72);
    BookPanel(root, "book-two", 382);
    // The archive interval is represented by the gold "Fragment" block in Book 1.
    var worldCenter = RangeCenter(worldCoverage, 20m);
    var miraCenter = RangeCenter(miraCoverage, 100m);
    var archiveCenter = RangeCenter(archiveCoverage, 5m);
    Flow(root, "world-to-book", $"M{F(worldCenter)} 217 C{F(worldCenter)} 455 124 478 124 548 L124 613", "#62d8ff");
    Flow(root, "mira-to-book", $"M{F(miraCenter)} 297 C{F(miraCenter)} 455 434 478 434 548 L434 613", "#ff8cc8");
    Flow(root, "archive-to-book", $"M{F(archiveCenter)} 377 C{F(archiveCenter)} 455 204 478 204 548 L204 613", "#ffd36a");

    BookContent(root, "book-one", 72, "Series A", "Book 1 · Ashes",
        new[] { ("Signal", "#62d8ff"), ("Fragment", "#ffd36a"), ("Memory", "#ff8cc8") });
    BookContent(root, "book-two", 382, "Series A", "Book 2 · Echoes",
        new[] { ("Disappearance", "#ff8cc8"), ("Hidden record", "#ffd36a"), ("Aftermath", "#62d8ff") });
    Rect(root, "collection-card", 692, 555, 424, 146, "#1b2b45", 12, "#62d8ff");
    Text(root, "collection-title", "World collection", 708, 580, "#f5f6ff", 17, 800);
    Text(root, "collection-subtitle", "Optional container · multiple series", 708, 600, "#aab7d3", 12);
    Rect(root, "series-a-card", 708, 614, 180, 58, "#121b30", 8, "#506184");
    Text(root, "series-a-title", "Series A", 720, 638, "#f5f6ff", 12, 700);
    Text(root, "series-a-books", "2 books · shared canon", 720, 657, "#aab7d3", 11);
    Rect(root, "series-b-card", 900, 614, 200, 58, "#121b30", 8, "#506184");
    Text(root, "series-b-title", "Series B", 912, 638, "#f5f6ff", 12, 700);
    Text(root, "series-b-books", "parallel story · own order", 912, 657, "#aab7d3", 11);
    Text(root, "boundary", "Concept model · generated through GUI.Core → GUI.SVG; StoryForge remains the canonical data owner", 42, 765, "#aab7d3", 11);
    return root.Build();
}

static GuiBuilder Scene(string id, string title, string description) =>
    GuiBuilder.Create("svg-scene", id)
        .Text(title)
        .Property("viewBox", id == "casino-floor-scene" ? "0 0 1200 760" : "0 0 1200 800")
        .Property("aria-label", title)
        .Property("description", description);

static void Rect(GuiBuilder root, string id, double x, double y, double width, double height,
    string fill, double radius = 0, string? stroke = null, double strokeWidth = 1)
{
    root.Child("svg-rect", id, node =>
    {
        node.Property("x", F(x)).Property("y", F(y)).Property("width", F(width)).Property("height", F(height))
            .Property("rx", F(radius)).Property("fill", fill);
        if (stroke is not null) node.Property("stroke", stroke).Property("stroke-width", F(strokeWidth));
    });
}

static void Text(GuiBuilder root, string id, string text, double x, double y, string fill,
    double size, int weight = 400)
{
    root.Child("svg-text", id, node => node.Text(text)
        .Property("x", F(x)).Property("y", F(y)).Property("fill", fill)
        .Property("font-family", "Inter, Segoe UI, Arial, sans-serif")
        .Property("font-size", F(size)).Property("font-weight", weight.ToString()));
}

static void Station(GuiBuilder root, string id, double x, double y, double width, double height,
    string title, string subtitle, string caption, string accent)
{
    Rect(root, id + "-panel", x, y, width, height, "#18223a", 14, "#425270");
    Rect(root, id + "-accent", x + 16, y + 18, 6, 70, accent, 3);
    Text(root, id + "-title", title, x + 34, y + 36, "#f4f7ff", 16, 800);
    Text(root, id + "-subtitle", subtitle, x + 34, y + 59, "#aebbd5", 13);
    Text(root, id + "-caption", caption, x + 34, y + 82, accent, 10, 800);
}

static void Flow(GuiBuilder root, string id, string pathData, string color)
{
    root.Child("svg-path", id, path => path.Property("d", pathData).Property("fill", "none")
        .Property("stroke", color).Property("stroke-width", "3").Property("stroke-dasharray", "8 10")
        .Property("opacity", "0.9")
        .Child("svg-animate", id + "-motion", animation => animation
            .Property("attributeName", "stroke-dashoffset").Property("values", "0;-72")
            .Property("dur", "2.6s").Property("repeatCount", "indefinite")));
}

static void FlowCard(GuiBuilder root, string id, double x, double y, double width,
    string title, string subtitle, string accent)
{
    Rect(root, id + "-panel", x, y, width, 63, "#10182b", 12, accent);
    Text(root, id + "-title", title, x + 18, y + 26, "#f4f7ff", 16, 800);
    Text(root, id + "-subtitle", subtitle, x + 18, y + 47, "#aebbd5", 12);
}

static void Metric(GuiBuilder root, string id, double x, string label, string value, string caption, string accent)
{
    Rect(root, id + "-panel", x, 580, 240, 88, "#10182b", 12, "#425270");
    Text(root, id + "-label", label, x + 18, 605, "#aebbd5", 11, 700);
    Text(root, id + "-value", value, x + 18, 637, "#f4f7ff", 25, 800);
    Text(root, id + "-caption", caption, x + 18, 657, accent, 11);
}

static void AddTicks(GuiBuilder root, string id, string[] labels, double y)
{
    var positions = new[] { 270, 465, 660, 855, 1050 };
    for (var i = 0; i < positions.Length; i++)
        Text(root, id + "-" + i, labels[i], positions[i], y, "#aab7d3", 11);
}

static void Timeline(GuiBuilder root, StoryTimeline timeline, PublicationCoverage coverage,
    decimal domainEnd, string subtitle, double y, string accent, string rangeLabel)
{
    var id = timeline.Id;
    Text(root, id + "-title", timeline.Name, 65, y + 3, "#f5f6ff", 17, 800);
    Text(root, id + "-subtitle", subtitle, 65, y + 23, "#aab7d3", 12);
    root.Child("svg-line", id + "-axis", line => line.Property("x1", "250").Property("y1", F(y))
        .Property("x2", "1100").Property("y2", F(y)).Property("stroke", "#6b7897").Property("stroke-width", "2"));
    var x = 250 + RangeStart(coverage, domainEnd);
    var width = RangeWidth(coverage, domainEnd);
    Rect(root, id + "-range", x, y - 15, width, 30, accent, 6, accent, 2);
    Text(root, id + "-range-label", rangeLabel, x + 8, y + 5, "#111629", 12, 800);
    foreach (var tick in new[] { 270, 465, 660, 855, 1050 })
        root.Child("svg-line", id + "-tick-" + tick, line => line.Property("x1", tick.ToString())
            .Property("y1", F(y - 7)).Property("x2", tick.ToString()).Property("y2", F(y + 7))
            .Property("stroke", "#6b7897").Property("stroke-width", "1"));
}

static double RangeStart(PublicationCoverage coverage, decimal domainEnd) =>
    850d * (double)(coverage.StoryRange.Start.Value / domainEnd);

static double RangeWidth(PublicationCoverage coverage, decimal domainEnd) =>
    850d * (double)(coverage.StoryRange.Duration / domainEnd);

static double RangeCenter(PublicationCoverage coverage, decimal domainEnd) =>
    250 + RangeStart(coverage, domainEnd) + RangeWidth(coverage, domainEnd) / 2;

static void BookPanel(GuiBuilder root, string id, double x) =>
    Rect(root, id + "-panel", x, 555, 285, 146, "#1a2340", 12, "#506184");

static void BookContent(GuiBuilder root, string id, double x, string series, string title,
    (string Label, string Color)[] blocks)
{
    // Labels occupy the right-hand gutter so the colored threads can reach their exact sequence blocks.
    Text(root, id + "-series", series, x + 178, 580, "#f5f6ff", 14, 800);
    Text(root, id + "-title", title, x + 178, 600, "#aab7d3", 12);
    for (var i = 0; i < blocks.Length; i++)
    {
        var block = blocks[i];
        var blockX = x + 16 + i * 80;
        Rect(root, id + "-sequence-" + i, blockX, 613, 72, 28, block.Color, 5, block.Color);
        Text(root, id + "-sequence-label-" + i, block.Label, blockX + 5, 631, "#111629", 9, 800);
    }
    Text(root, id + "-edition", "sequence ≠ chronology", x + 16, 674, "#aab7d3", 11);
}

static string F(double value) => value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
