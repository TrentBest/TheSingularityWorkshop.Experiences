using System.Text.Json.Serialization;
using TheSingularityWorkshop.Experiences.CastleSiege;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();
var session = new CastleSiegeTabletopSession();
var sessionGate = new object();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/state", () =>
{
    lock (sessionGate)
    {
        return Results.Ok(CaptureView());
    }
});

app.MapPost("/api/action", (HallActionRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Action) ||
        !Enum.TryParse<CastleSiegeActionKind>(request.Action, ignoreCase: true, out var kind) ||
        !Enum.IsDefined(kind))
    {
        return (IResult)Results.BadRequest(new { error = "Unknown action. Choose an action currently offered by the hall." });
    }

    lock (sessionGate)
    {
        var view = CaptureView();
        if (view.Outcome != CastleSiegeOutcome.InProgress)
        {
            return (IResult)Results.Conflict(new { error = "The tabletop session is complete. Reset the host to start a new session." });
        }

        if (!view.LegalActions.Contains(kind))
        {
            return (IResult)Results.BadRequest(new { error = $"Action {kind} is not legal for the active role." });
        }

        var entry = session.Apply(new CastleSiegeAction(view.ActiveRole, kind));
        return (IResult)Results.Ok(new
        {
            view = CaptureView(),
            acceptedEvent = entry
        });
    }
});

app.MapPost("/api/reset", () =>
{
    lock (sessionGate)
    {
        session = new CastleSiegeTabletopSession();
        return Results.Ok(CaptureView());
    }
});

app.Run();

CastleSiegeHallView CaptureView() =>
    CastleSiegeHallViewModel.FromReport(CastleSiegeSessionReport.Capture(session));

internal sealed record HallActionRequest(string Action);
