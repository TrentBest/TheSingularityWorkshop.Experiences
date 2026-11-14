using TheSingularityWorkshop.Experiences.GalacticWar;

var runtime = new StationMediaRuntime("vesper-reach");
using var stationSession = new StationSessionLifecycle("vesper-reach-visitor-session");
stationSession.MarkArrived();
var signalId = Guid.Parse("10000000-0000-0000-0000-000000000001");
var initialIssueId = Guid.Parse("20000000-0000-0000-0000-000000000001");
var independentIssueId = Guid.Parse("20000000-0000-0000-0000-000000000003");
var correctionIssueId = Guid.Parse("20000000-0000-0000-0000-000000000002");
var initialDeliveryId = Guid.Parse("30000000-0000-0000-0000-000000000001");
var independentDeliveryId = Guid.Parse("30000000-0000-0000-0000-000000000003");
var correctionDeliveryId = Guid.Parse("30000000-0000-0000-0000-000000000002");

runtime.RecordOccurrence(new SignalOccurrence(signalId, Guid.Parse("40000000-0000-0000-0000-000000000001"), "frontier-observatory", new CampaignInstant(10)));
runtime.IssueBroadcast(new BroadcastIssue(initialIssueId, signalId, "story-artifact:official-bulletin-v1", "source:frontier-observatory", new CampaignInstant(20)));
runtime.ScheduleDelivery(new MediaDelivery(initialDeliveryId, initialIssueId, "lounge-display", "vesper-reach", new CampaignInstant(30)));
runtime.RecordReceipt(new ObserverReceipt(Guid.Parse("50000000-0000-0000-0000-000000000001"), initialDeliveryId, "mara-quill", new CampaignInstant(32), AcquisitionMode.Heard));
runtime.IssueBroadcast(new BroadcastIssue(independentIssueId, signalId, "story-artifact:independent-account-v1", "source:independent-survey-ship", new CampaignInstant(34)));
runtime.ScheduleDelivery(new MediaDelivery(independentDeliveryId, independentIssueId, "analyst-terminal", "vesper-reach", new CampaignInstant(35)));
runtime.RecordReceipt(new ObserverReceipt(Guid.Parse("50000000-0000-0000-0000-000000000004"), independentDeliveryId, "ilyan-voss", new CampaignInstant(36), AcquisitionMode.Read));
runtime.RecordReceipt(new ObserverReceipt(Guid.Parse("50000000-0000-0000-0000-000000000002"), initialDeliveryId, "player", new CampaignInstant(40), AcquisitionMode.Viewed));
var initialPresentationId = Guid.NewGuid();
runtime.RecordPresentation(new PlayerPresentation(initialPresentationId, "player", initialIssueId, new CampaignInstant(42)));
runtime.RecordPlayerChoice(new StationStoryDecision(
    Guid.NewGuid(), "player", StationStoryChoice.Watch, new CampaignInstant(43), PresentationId: initialPresentationId));

Console.WriteLine("Vesper Reach — station shift");
Console.WriteLine($"Station session FSM state: {stationSession.CurrentState}.");
Console.WriteLine("T+10  Signal occurrence recorded at the frontier observatory.");
Console.WriteLine("T+20  Official bulletin issued by the frontier observatory.");
Console.WriteLine("T+30  Lounge display makes the official bulletin available.");
Console.WriteLine("T+32  Mara acquires the official bulletin by hearing it.");
Console.WriteLine("T+34  An independent survey-ship account is issued about the same signal.");
Console.WriteLine("T+35  The analyst terminal makes the independent account available.");
Console.WriteLine("T+36  Ilyan acquires the independent account by reading it.");
Console.WriteLine("T+40  Player acquires the lounge display delivery.");
Console.WriteLine("T+42  The official bulletin is presented to the player.");
Console.WriteLine("T+43  Player chooses to watch.");
Console.WriteLine($"T+40  Mara has {runtime.GetReceivedIssues("mara-quill", new CampaignInstant(40)).Count} issue(s); Ilyan has {runtime.GetReceivedIssues("ilyan-voss", new CampaignInstant(40)).Count}; Teren has {runtime.GetReceivedIssues("teren", new CampaignInstant(40)).Count}; player has {runtime.GetReceivedIssues("player", new CampaignInstant(40)).Count}.");

runtime.IssueBroadcast(new BroadcastIssue(correctionIssueId, signalId, "story-artifact:official-bulletin-v2", "source:frontier-observatory", new CampaignInstant(60), initialIssueId));
runtime.ScheduleDelivery(new MediaDelivery(correctionDeliveryId, correctionIssueId, "lounge-display", "vesper-reach", new CampaignInstant(65)));
runtime.RecordReceipt(new ObserverReceipt(Guid.Parse("50000000-0000-0000-0000-000000000003"), correctionDeliveryId, "mara-quill", new CampaignInstant(66), AcquisitionMode.Heard));

Console.WriteLine("T+60  A correction is issued as a new official bulletin edition.");
Console.WriteLine("T+65  The lounge display carries the correction.");
Console.WriteLine("T+66  Mara acquires the correction; the player and Teren do not.");
Console.WriteLine($"T+66  Mara's issue history: {string.Join(", ", runtime.GetReceivedIssues("mara-quill", new CampaignInstant(66)).Select(issue => issue.AuthoredContentArtifactId))}");
Console.WriteLine($"T+66  Player's issue history: {runtime.GetReceivedIssues("player", new CampaignInstant(66)).Count} issue(s).");
