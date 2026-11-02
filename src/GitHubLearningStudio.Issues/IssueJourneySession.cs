namespace TheSingularityWorkshop.Experiences.GitHubLearningStudio.Issues;

/// <summary>Stable semantic actions for the simulated issue-tracking lesson.</summary>
public static class IssueJourneyCommands
{
    public const string CreateIssue = "issues.create";
    public const string ClassifyIssue = "issues.classify";
    public const string AssignIssue = "issues.assign";
    public const string AddClarification = "issues.add-clarification";
    public const string CreateBranch = "issues.create-branch";
    public const string CommitFix = "issues.commit-fix";
    public const string OpenPullRequest = "issues.open-pull-request";
    public const string MergePullRequest = "issues.merge-pull-request";
    public const string VerifyAcceptance = "issues.verify-acceptance";
    public const string CloseIssue = "issues.close";
    public const string ReopenIssue = "issues.reopen";
}

public enum IssueKind { Unclassified, Bug, Question, Enhancement }
public enum IssueLifecycle { NotCreated, Open, Closed }

public sealed record IssueDiscussionEntry(string Author, string Message);
public sealed record IssueJourneySnapshot(
    string ExperienceId,
    string ProjectName,
    int? IssueNumber,
    string Title,
    string ObservedBehavior,
    string ExpectedBehavior,
    string Context,
    IReadOnlyList<string> AcceptanceCriteria,
    IReadOnlyList<bool> AcceptanceEvidence,
    IssueKind Kind,
    string? Label,
    string? Assignee,
    IReadOnlyList<IssueDiscussionEntry> Discussion,
    string? BranchName,
    IReadOnlyList<string> Commits,
    int? PullRequestNumber,
    bool PullRequestMerged,
    IssueLifecycle Lifecycle,
    bool AcceptanceVerified,
    string Explanation,
    string SimulationNotice,
    IReadOnlyList<string> AvailableCommands);

public sealed record IssueJourneyCommandResult(bool Accepted, string CommandId, string Message, IssueJourneySnapshot Snapshot);

/// <summary>
/// Deterministic in-memory issue lifecycle. It never contacts GitHub or changes a real repository.
/// </summary>
public sealed class IssueJourneySession
{
    private const string CorrectObserved = "A published field note appears without the location attribution shown on the source record.";
    private const string CorrectExpected = "The published field note displays the associated location attribution.";
    private const string CorrectContext = "Field note FN-014 in the fictional Field Notes project; the omission is visible in the published entry and its text-first representation.";
    private static readonly string[] Criteria =
    [
        "Field note FN-014 displays its associated location attribution.",
        "The text-first representation exposes the same location attribution."
    ];

    private readonly List<IssueDiscussionEntry> _discussion = [];
    private readonly List<string> _commits = [];
    private readonly bool[] _evidence = new bool[2];
    private bool _created;
    private string? _title;
    private bool _branchCreated;
    private bool _pullRequestOpened;
    private bool _pullRequestMerged;
    private bool _acceptanceVerified;
    private IssueKind _kind;
    private string? _label;
    private string? _assignee;
    private IssueLifecycle _lifecycle;
    private int _issueOpenCount;

    public IssueJourneySnapshot CaptureSnapshot()
    {
        var commands = new List<string>();
        if (!_created) commands.Add(IssueJourneyCommands.CreateIssue);
        if (_created && _lifecycle == IssueLifecycle.Open)
        {
            if (_kind == IssueKind.Unclassified) commands.Add(IssueJourneyCommands.ClassifyIssue);
            if (_assignee is null) commands.Add(IssueJourneyCommands.AssignIssue);
            if (_discussion.Count == 0) commands.Add(IssueJourneyCommands.AddClarification);
            if (_kind != IssueKind.Unclassified && _assignee is not null && _discussion.Count > 0 && !_branchCreated)
                commands.Add(IssueJourneyCommands.CreateBranch);
            if (_branchCreated && _commits.Count == 0) commands.Add(IssueJourneyCommands.CommitFix);
            if (_commits.Count > 0 && !_pullRequestOpened) commands.Add(IssueJourneyCommands.OpenPullRequest);
            if (_pullRequestOpened && !_pullRequestMerged) commands.Add(IssueJourneyCommands.MergePullRequest);
            if (_pullRequestMerged && !_acceptanceVerified) commands.Add(IssueJourneyCommands.VerifyAcceptance);
            if (_acceptanceVerified) commands.Add(IssueJourneyCommands.CloseIssue);
        }
        if (_lifecycle == IssueLifecycle.Closed) commands.Add(IssueJourneyCommands.ReopenIssue);

        return new IssueJourneySnapshot(
            "github-learning-studio.issues",
            "Field Notes",
            _created ? 14 : null,
            _title ?? "Untitled issue",
            _created ? CorrectObserved : "A report says: “The place is missing.”",
            _created ? CorrectExpected : "Not yet recorded.",
            _created ? CorrectContext : "A visitor reported the problem but gave little context.",
            Array.AsReadOnly(Criteria),
            Array.AsReadOnly(_evidence.ToArray()),
            _kind, _label, _assignee,
            Array.AsReadOnly(_discussion.ToArray()),
            _branchCreated ? "work/issue-14-location-attribution" : null,
            Array.AsReadOnly(_commits.ToArray()),
            _pullRequestOpened ? 22 : null,
            _pullRequestMerged,
            _lifecycle,
            _acceptanceVerified,
            Explain(),
            "SIMULATION ONLY — issue #14, branch, commit, pull request #22, discussion, merge, and closure are teaching data. No Git or GitHub operation occurs.",
            Array.AsReadOnly(commands.ToArray()));
    }

    public IssueJourneyCommandResult Execute(string commandId, string? value = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);
        var accepted = commandId switch
        {
            IssueJourneyCommands.CreateIssue => CreateIssue(value),
            IssueJourneyCommands.ClassifyIssue => Classify(value),
            IssueJourneyCommands.AssignIssue => Assign(value),
            IssueJourneyCommands.AddClarification => AddClarification(value),
            IssueJourneyCommands.CreateBranch => CreateBranch(),
            IssueJourneyCommands.CommitFix => Commit(value),
            IssueJourneyCommands.OpenPullRequest => OpenPullRequest(),
            IssueJourneyCommands.MergePullRequest => MergePullRequest(),
            IssueJourneyCommands.VerifyAcceptance => VerifyAcceptance(),
            IssueJourneyCommands.CloseIssue => CloseIssue(),
            IssueJourneyCommands.ReopenIssue => ReopenIssue(),
            _ => false
        };
        var message = accepted
            ? $"Simulated action '{commandId}' accepted."
            : $"Action '{commandId}' is unavailable or its requirements are not satisfied.";
        return new IssueJourneyCommandResult(accepted, commandId, message, CaptureSnapshot());
    }

    private bool CreateIssue(string? title)
    {
        if (_created || string.IsNullOrWhiteSpace(title) || title.Trim().Length < 12 ||
            !title.Contains("location", StringComparison.OrdinalIgnoreCase) ||
            !title.Contains("attribution", StringComparison.OrdinalIgnoreCase)) return false;
        _title = title.Trim();
        _created = true;
        _issueOpenCount = 1;
        _lifecycle = IssueLifecycle.Open;
        return true;
    }

    private bool Classify(string? value)
    {
        if (!_created || _lifecycle != IssueLifecycle.Open || _kind != IssueKind.Unclassified) return false;
        _kind = value switch
        {
            "bug" => IssueKind.Bug,
            "question" => IssueKind.Question,
            "enhancement" => IssueKind.Enhancement,
            _ => IssueKind.Unclassified
        };
        if (_kind == IssueKind.Unclassified) return false;
        _label = value;
        return true;
    }

    private bool Assign(string? assignee)
    {
        if (!_created || _lifecycle != IssueLifecycle.Open || _assignee is not null ||
            string.IsNullOrWhiteSpace(assignee)) return false;
        _assignee = assignee.Trim();
        return true;
    }

    private bool AddClarification(string? message)
    {
        if (!_created || _lifecycle != IssueLifecycle.Open || _discussion.Count > 0 ||
            string.IsNullOrWhiteSpace(message) || message.Trim().Length < 12) return false;
        _discussion.Add(new IssueDiscussionEntry("Maintainer", message.Trim()));
        _discussion.Add(new IssueDiscussionEntry("Reporter", "Confirmed: FN-014 is the affected entry; both published and text-first views should show the location."));
        return true;
    }

    private bool CreateBranch()
    {
        if (!_created || _lifecycle != IssueLifecycle.Open || _branchCreated ||
            _kind == IssueKind.Unclassified || _assignee is null || _discussion.Count == 0) return false;
        _branchCreated = true;
        return true;
    }

    private bool Commit(string? message)
    {
        if (!_branchCreated || _lifecycle != IssueLifecycle.Open || _commits.Count > 0 ||
            string.IsNullOrWhiteSpace(message) || message.Trim().Length < 8) return false;
        _commits.Add(message.Trim());
        return true;
    }

    private bool OpenPullRequest()
    {
        if (!_created || _lifecycle != IssueLifecycle.Open || _commits.Count == 0 || _pullRequestOpened) return false;
        _pullRequestOpened = true;
        return true;
    }

    private bool MergePullRequest()
    {
        if (!_pullRequestOpened || _pullRequestMerged || _lifecycle != IssueLifecycle.Open) return false;
        _pullRequestMerged = true;
        return true;
    }

    private bool VerifyAcceptance()
    {
        if (!_pullRequestMerged || _acceptanceVerified || _lifecycle != IssueLifecycle.Open) return false;
        Array.Fill(_evidence, true);
        _acceptanceVerified = true;
        return true;
    }

    private bool CloseIssue()
    {
        if (!_acceptanceVerified || !_pullRequestMerged || _lifecycle != IssueLifecycle.Open) return false;
        _lifecycle = IssueLifecycle.Closed;
        return true;
    }

    private bool ReopenIssue()
    {
        if (_lifecycle != IssueLifecycle.Closed) return false;
        _lifecycle = IssueLifecycle.Open;
        _acceptanceVerified = false;
        Array.Fill(_evidence, false);
        return true;
    }

    private string Explain() => _lifecycle switch
    {
        IssueLifecycle.NotCreated => "An issue begins as a shared record of a problem. Improve the vague report before treating it as actionable.",
        IssueLifecycle.Closed => "The simulated issue is closed because the pull request was merged and both acceptance criteria were checked. Reopen it if the defect returns or evidence is no longer valid.",
        _ when (_pullRequestMerged && !_acceptanceVerified) => "The pull request was merged, but merge alone does not prove the issue is resolved. Verify each acceptance criterion.",
        _ when _pullRequestOpened => "The issue remains the why; the pull request is the proposal for how. Linking them creates traceability, not automatic completion.",
        _ when _branchCreated => "The issue is still open. A branch and commit record implementation work; neither closes the issue.",
        _ when _discussion.Count > 0 => "Discussion preserves clarification. It improves the issue record but does not implement the change.",
        _ when (_kind != IssueKind.Unclassified && _assignee is not null) => "Classification and assignment organize responsibility. They do not mean work is complete.",
        _ when _created => "A useful issue states the observed problem, expected behavior, context, and verifiable acceptance criteria.",
        _ => "Start by turning the vague report into a useful issue."
    };
}
