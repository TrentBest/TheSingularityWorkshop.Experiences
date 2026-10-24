using System.Net;
using System.Text;
using TheSingularityWorkshop.Experiences.DroneConstruction;

// A real, deterministic simulation run rendered as a browser-openable animated artifact.
// This is an approachable show-and-tell path while the Renderer -> Visualize adapter is
// being connected to committed construction snapshots.
const int width = 12;
const int height = 8;
const int maxTicks = 900;
var agents = Enumerable.Range(1, 6)
    .Select(id => new ConstructionAgent((ulong)id, new GridCell((id - 1) % 4, (id - 1) / 4)));
var simulation = new ConstructionSimulation(
    width,
    height,
    requiredHeight: (x, _) => x == 10 ? 1 : 0,
    agents: agents,
    emitters: [new PositionedBlockEmitter(new GridCell(0, 0), new BlockEmitter("showcase-supply", 64))],
    spaceId: "web-showcase");
var frames = new List<(long Tick, int Placed, string Svg)>();
var snapshot = simulation.Snapshot();
frames.Add((snapshot.Tick, snapshot.PlacedBlocks, ConstructionSnapshotSvgRenderer.Render(snapshot, width, height, "web-showcase", 40)));
var captureEvery = 3;
while (!snapshot.IsComplete && snapshot.Tick < maxTicks)
{
    snapshot = simulation.Step();
    if (snapshot.Tick % captureEvery == 0 || snapshot.IsComplete)
        frames.Add((snapshot.Tick, snapshot.PlacedBlocks, ConstructionSnapshotSvgRenderer.Render(snapshot, width, height, "web-showcase", 40)));
}

var outputPath = Path.GetFullPath(args.Length > 0 ? args[0] : "artifacts/drone-construction-showcase.html");
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
await File.WriteAllTextAsync(outputPath, BuildPage(frames, snapshot, width, height), new UTF8Encoding(false));
Console.WriteLine($"Drone Construction showcase: {snapshot.PlacedBlocks}/{snapshot.RequiredBlocks} blocks, {snapshot.Agents.Count} agents, tick {snapshot.Tick}, {frames.Count} captured frames.");
Console.WriteLine($"Completion: {snapshot.IsComplete}; output: {outputPath}");
if (!snapshot.IsComplete) Environment.ExitCode = 1;

static string BuildPage(
    IReadOnlyList<(long Tick, int Placed, string Svg)> frames,
    ConstructionSimulationSnapshot finalSnapshot,
    int width,
    int height)
{
    var html = new StringBuilder();
    var duration = Math.Max(1.0, frames.Count * 0.12);
    html.AppendLine("<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\">");
    html.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
    html.AppendLine("<title>Drone Construction · Living Proof</title>");
    html.AppendLine("""
<style>
:root{color-scheme:dark;background:#07111b;color:#edf5ff;font:16px/1.5 system-ui,sans-serif}
*{box-sizing:border-box}body{margin:0;padding:clamp(16px,4vw,48px);max-width:1400px;margin-inline:auto}
header{display:flex;gap:24px;align-items:end;justify-content:space-between;flex-wrap:wrap}
h1{font-size:clamp(2rem,5vw,4rem);line-height:1.02;letter-spacing:-.04em;margin:0}
.kicker{color:#55d9ff;text-transform:uppercase;letter-spacing:.16em;font-size:.78rem;font-weight:800}
p{color:#a9bacb;max-width:70ch}.stage{position:relative;margin-top:28px;aspect-ratio:1.65;border:1px solid #263a4c;border-radius:18px;overflow:hidden;background:#101923;box-shadow:0 24px 90px #0008}
.frame{position:absolute;inset:0;display:grid;place-items:center;opacity:0;animation:replay __DURATION__s steps(1,end) infinite}
.frame svg{width:100%;height:100%;max-height:76vh}
@keyframes replay{0%{opacity:1}__KEYFRAMES__100%{opacity:0}}
.metrics{display:grid;grid-template-columns:repeat(auto-fit,minmax(150px,1fr));gap:12px;margin-top:16px}
.metric{border:1px solid #263a4c;border-radius:12px;padding:14px;background:#0c1824}
.metric strong{display:block;font-size:1.6rem;color:#49d6a2}.metric span{color:#a9bacb;font-size:.86rem}
footer{margin-top:24px;color:#7890a5;font-size:.85rem}
@media(prefers-reduced-motion:reduce){.frame{animation:none}.frame:first-child{opacity:1}}
</style></head><body>
<header><div><div class="kicker">The Singularity Workshop · Living Proof</div><h1>Drone Construction</h1><p>Watch a deterministic multi-agent construction run advance from committed simulation snapshots. This is real simulation output, not a storyboard.</p></div></header>
<div class="stage" aria-label="Animated sequence of committed construction simulation snapshots">
""");
    for (var i = 0; i < frames.Count; i++)
    {
        var svg = frames[i].Svg.Replace("<svg ", "<svg preserveAspectRatio=\"xMidYMid meet\" ", StringComparison.Ordinal);
        html.Append("<div class=\"frame\" style=\"animation-delay:-")
            .Append((duration - i * duration / frames.Count).ToString("0.000", System.Globalization.CultureInfo.InvariantCulture))
            .Append("s\">").Append(svg).AppendLine("</div>");
    }
    html.AppendLine("</div><section class=\"metrics\" aria-label=\"Run summary\">");
    Metric(html, finalSnapshot.Agents.Count.ToString(), "cooperating agents");
    Metric(html, $"{finalSnapshot.PlacedBlocks}/{finalSnapshot.RequiredBlocks}", "blocks placed");
    Metric(html, finalSnapshot.Tick.ToString(), "simulation ticks");
    Metric(html, frames.Count.ToString(), "captured snapshots");
    html.AppendLine("</section>");
    html.AppendLine($"<footer>Run: web-showcase · grid: {width} × {height} · deterministic CPU simulation · completion: {(finalSnapshot.IsComplete ? "verified" : "incomplete")}. The presentation intentionally samples committed ticks rather than drawing every simulation step.</footer>");
    html.AppendLine("</body></html>");
    var page = html.ToString();
    var keyframes = new StringBuilder();
    for (var i = 1; i < frames.Count; i++)
        keyframes.Append((i * 100.0 / frames.Count).ToString("0.###", System.Globalization.CultureInfo.InvariantCulture))
            .Append("%{opacity:1}").Append(((i * 100.0 / frames.Count) + 0.01).ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)).Append("%{opacity:0}");
    return page.Replace("__DURATION__", duration.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal)
        .Replace("__KEYFRAMES__", keyframes.ToString(), StringComparison.Ordinal);
}

static void Metric(StringBuilder html, string value, string label) =>
    html.Append("<div class=\"metric\"><strong>").Append(WebUtility.HtmlEncode(value))
        .Append("</strong><span>").Append(WebUtility.HtmlEncode(label)).AppendLine("</span></div>");
