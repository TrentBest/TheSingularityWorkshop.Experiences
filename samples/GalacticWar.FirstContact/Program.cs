using TheSingularityWorkshop.Experiences.GalacticWar;

var runtime = new StationMediaRuntime("vesper-reach");
var signalId = Guid.Parse("10000000-0000-0000-0000-000000000001");
var initialIssueId = Guid.Parse("20000000-0000-0000-0000-000000000001");
var correctionIssueId = Guid.Parse("20000000-0000-0000-0000-000000000002");
var initialDeliveryId = Guid.Parse("30000000-0000-0000-0000-000000000001");
var correctionDeliveryId = Guid.Parse("30000000-0000-0000-0000-000000000002");

runtime.RecordOccurrence(new SignalOccurrence(signalId, Guid.Parse("40000000-0000-0000-0000-000000000001"), "frontier-observatory", new CampaignInstant(10)));
runtime.IssueBroadcast(new BroadcastIssue(initialIssueId, signalId, "story-artifact:official-bulletin-v1", "source:frontier-observatory", new CampaignInstant(20)));
runtime.ScheduleDelivery(new MediaDelivery(initialDeliveryId, initialIssueId, "lounge-display", "vesper-reach", new CampaignInstant(30)));
runtime.RecordReceipt(new ObserverReceipt(Guid.Parse("50000000-0000-0000-0000-000000000001"), initialDeliveryId, "mara-quill", new CampaignInstant(32), AcquisitionMode.Heard));
runtime.RecordReceipt(new ObserverReceipt(Guid.Parse("50000000-0000-0000-0000-000000000002"), initialDeliveryId, "player", new CampaignInstant(40), AcquisitionMode.Viewed));
runtime.RecordPlayerChoice(new StationStoryDecision(Guid.NewGuid(), "player", StationStoryChoice.Watch, new CampaignInstant(40)));

Console.WriteLine("Vesper Reach — station shift");
Console.WriteLine("T+10  Signal occurrence recorded at the frontier observatory.");
Console.WriteLine("T+20  Official bulletin issued.");
Console.WriteLine("T+30  Lounge display makes the bulletin available.");
Console.WriteLine("T+32  Mara acquires the bulletin by hearing it.");
Console.WriteLine("T+40  Player watches the lounge display.");
Console.WriteLine($"T+40  Mara knows {runtime.GetReceivedIssues("mara-quill", new CampaignInstant(40)).Count} issue; Teren knows {runtime.GetReceivedIssues("teren", new CampaignInstant(40)).Count}; player has {runtime.GetReceivedIssues("player", new CampaignInstant(40)).Count}.");

runtime.IssueBroadcast(new BroadcastIssue(correctionIssueId, signalId, "story-artifact:official-bulletin-v2", "source:frontier-observatory", new CampaignInstant(60), initialIssueId));
runtime.ScheduleDelivery(new MediaDelivery(correctionDeliveryId, correctionIssueId, "lounge-display", "vesper-reach", new CampaignInstant(65)));
runtime.RecordReceipt(new ObserverReceipt(Guid.Parse("50000000-0000-0000-0000-000000000003"), correctionDeliveryId, "mara-quill", new CampaignInstant(66), AcquisitionMode.Heard));

Console.WriteLine("T+60  A correction is issued as a new bulletin edition.");
Console.WriteLine("T+65  The lounge display carries the correction.");
Console.WriteLine("T+66  Mara acquires the correction; the player and Teren do not.");
Console.WriteLine($"T+66  Mara's issue history: {string.Join(", ", runtime.GetReceivedIssues("mara-quill", new CampaignInstant(66)).Select(issue => issue.AuthoredContentArtifactId))}");
Console.WriteLine($"T+66  Player's issue history: {runtime.GetReceivedIssues("player", new CampaignInstant(66)).Count} issue(s).");
