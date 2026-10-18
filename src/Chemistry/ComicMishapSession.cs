namespace TheSingularityWorkshop.Experiences.Chemistry;

/// <summary>Identifies the evidence behind a comic laboratory incident.</summary>
public enum MishapTriggerKind
{
    /// <summary>An explicitly authored tutorial beat, not a chemistry prediction.</summary>
    ScriptedTutorial,
    /// <summary>A named model reported an incident and supplied its own provenance.</summary>
    ModelReported,
    /// <summary>The selected capabilities cannot classify the outcome.</summary>
    UnknownOutcome
}

/// <summary>Lifecycle stage for a recoverable, non-lethal comic mishap.</summary>
public enum MishapStage
{
    Ready,
    Presented,
    Recovered,
    EvidenceReviewed
}

/// <summary>A stable snapshot reference for the experiment that preceded the gag.</summary>
public sealed record ExperimentSnapshot
{
    public string ExperimentKey { get; }
    public long Revision { get; }
    public string DefinitionJson { get; }

    public ExperimentSnapshot(string experimentKey, long revision, string definitionJson)
    {
        if (string.IsNullOrWhiteSpace(experimentKey))
            throw new ArgumentException("An experiment key is required.", nameof(experimentKey));
        if (revision < 0)
            throw new ArgumentOutOfRangeException(nameof(revision));
        if (string.IsNullOrWhiteSpace(definitionJson))
            throw new ArgumentException("The serialized experiment definition is required.", nameof(definitionJson));

        ExperimentKey = experimentKey;
        Revision = revision;
        DefinitionJson = definitionJson;
    }
}

/// <summary>
/// Authoritative state for a harmless comic mishap. It tracks presentation and recovery only;
/// it never predicts chemistry, mutates the experiment, or models real-world safety.
/// </summary>
public sealed class ComicMishapSession
{
    public const string PresentationKey = "comic-puff";

    public ExperimentSnapshot Experiment { get; }
    public MishapTriggerKind TriggerKind { get; }
    public string? ModelEventKey { get; }
    public MishapStage Stage { get; private set; } = MishapStage.Ready;
    public bool PlayerIsDead => false;
    public bool HasPermanentInjury => false;
    public bool ExperimentPreserved => true;
    public IReadOnlyList<string> Timeline { get; private set; } = ["experiment-snapshot-preserved"];

    public ComicMishapSession(ExperimentSnapshot experiment, MishapTriggerKind triggerKind, string? modelEventKey = null)
    {
        Experiment = experiment ?? throw new ArgumentNullException(nameof(experiment));
        if (triggerKind == MishapTriggerKind.ModelReported && string.IsNullOrWhiteSpace(modelEventKey))
            throw new ArgumentException("A model-reported incident requires a model event key.", nameof(modelEventKey));
        if (triggerKind != MishapTriggerKind.ModelReported && modelEventKey is not null)
            throw new ArgumentException("Only model-reported incidents may include a model event key.", nameof(modelEventKey));

        TriggerKind = triggerKind;
        ModelEventKey = modelEventKey;
    }

    /// <summary>Begins the gag without changing the submitted experiment snapshot.</summary>
    public void Present()
    {
        RequireStage(MishapStage.Ready);
        Stage = MishapStage.Presented;
        AppendTimeline($"mishap-presented:{TriggerKind}");
    }

    /// <summary>Restores player control; no death or permanent injury state is introduced.</summary>
    public void Recover()
    {
        RequireStage(MishapStage.Presented);
        Stage = MishapStage.Recovered;
        AppendTimeline("player-recovered");
    }

    /// <summary>Marks the incident explanation as inspected by the player.</summary>
    public void ReviewEvidence()
    {
        RequireStage(MishapStage.Recovered);
        Stage = MishapStage.EvidenceReviewed;
        AppendTimeline(TriggerKind switch
        {
            MishapTriggerKind.ScriptedTutorial => "evidence:scripted-comic-event",
            MishapTriggerKind.ModelReported => $"evidence:model-event:{ModelEventKey}",
            _ => "evidence:outcome-unknown"
        });
    }

    private void RequireStage(MishapStage expected)
    {
        if (Stage != expected)
            throw new InvalidOperationException($"Cannot perform this transition from stage {Stage}; expected {expected}.");
    }

    private void AppendTimeline(string entry) => Timeline = Timeline.Append(entry).ToArray();
}
