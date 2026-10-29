using TheSingularityWorkshop.Experiences.CapsuleClinic;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);

var app = builder.Build();
using var session = new CapsuleClinicLocalSession(
    "capsule-clinic-browser",
    "player",
    localSeed: 4101,
    opponentPlayerId: "practice-bot",
    opponentSeed: 9202);
var sessionGate = new object();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/state", () =>
{
    lock (sessionGate)
    {
        var snapshot = session.CaptureSnapshot();
        var local = snapshot.Panes.Single(pane => pane.Pane == ArenaPane.LocalUpperLeft).Player;
        var active = local?.Active;
        return Results.Ok(new
        {
            snapshot.LocalPlayerId,
            snapshot.BoardWidth,
            snapshot.BoardHeight,
            gameOver = local?.GameOver ?? false,
            lastAcceptedSequence = local?.LastAcceptedSequence ?? 0,
            active = active is null ? null : new
            {
                active.PivotX,
                active.PivotY,
                orientation = active.Orientation.ToString()
            }
        });
    }
});

app.MapGet("/api/arena.svg", () =>
{
    lock (sessionGate)
        return Results.Text(CapsuleArenaSvgRenderer.Render(session.CaptureSnapshot()), "image/svg+xml");
});

app.MapPost("/api/key", (KeyRequest request) =>
{
    if (!CapsuleClinicKeyboardBindings.TryMapKey(request.Key, out var intentId))
        return Results.BadRequest(new { error = "Unsupported key. Use the arrow keys or Space." });

    return Submit(intentId);
});

app.MapPost("/api/intent", (IntentRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.IntentId))
        return Results.BadRequest(new { error = "An intent identifier is required." });

    return Submit(request.IntentId);
});

app.Run();

IResult Submit(string intentId)
{
    lock (sessionGate)
    {
        var result = session.SubmitIntent(intentId);
        var local = result.Snapshot.Panes.Single(pane => pane.Pane == ArenaPane.LocalUpperLeft).Player;
        return Results.Ok(new
        {
            intentId,
            result.IntentRecognized,
            accepted = result.Decision.Accepted,
            rejection = result.Decision.Rejection.ToString(),
            sequence = result.Decision.Sequence,
            lastAcceptedSequence = local?.LastAcceptedSequence ?? 0,
            gameOver = local?.GameOver ?? false
        });
    }
}

internal sealed record KeyRequest(string? Key);
internal sealed record IntentRequest(string? IntentId);
