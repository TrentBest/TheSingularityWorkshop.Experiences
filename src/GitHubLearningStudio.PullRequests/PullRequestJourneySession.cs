namespace TheSingularityWorkshop.Experiences.GitHubLearningStudio.PullRequests;

/// <summary>Stable semantic actions in the simulated collaboration journey.</summary>
public static class PullRequestJourneyCommands
{
    public const string CreateBranch = "collaboration.create-branch";
    public const string CommitProposal = "collaboration.commit-proposal";
    public const string OpenPullRequest = "collaboration.open-pull-request";
    public const string RequestChanges = "collaboration.request-changes";
    public const string ReviseProposal = "collaboration.revise-proposal";
    public const string RunChecks = "collaboration.run-checks";
    public const string Approve = "collaboration.approve";
    public const string Merge = "collaboration.merge";
}

public enum PullRequestReviewState { NotReviewed, ChangesRequested, Approved }
public enum PullRequestCheckState { NotRun, Failed, Passed }
public enum PullRequestLifecycle { NotOpened, Open, Merged }

public sealed record ProposalCommit(int Sequence, string Message, int Revision);

/// <summary>Immutable state for the PR collaboration lesson.</summary>
public sealed record PullRequestJourneySnapshot(
    string ExperienceId,
    string ProjectName,
    string TaskTitle,
    string TaskDescription,
    string SourceBranch,
    string TargetBranch,
    string BranchName,
    int? PullRequestNumber,
    IReadOnlyList<ProposalCommit> Commits,
    IReadOnlyList<ProposalCommit> SharedHistory,
    int Revision,
    PullRequestLifecycle Lifecycle,
    PullRequestReviewState Review,
    string? ReviewFeedback,
    PullRequestCheckState Checks,
    bool MergeAuthorized,
    string? MergedBy,
    string Explanation,
    string SimulationNotice,
    IReadOnlyList<string> AvailableCommands);

public sealed record PullRequestJourneyCommandResult(
    bool Accepted, string CommandId, string Message, PullRequestJourneySnapshot Snapshot);

/// <summary>
/// Deterministic in-memory collaboration simulation. It never contacts GitHub or changes a real repository.
/// </summary>
public sealed class PullRequestJourneySession
{
    private readonly List<ProposalCommit> _commits = [];
    private readonly List<ProposalCommit> _sharedHistory = [];
    private bool _branchCreated;
    private bool _pullRequestOpened;
    private bool _mergeAuthorized;
    private string? _mergedBy;
    private PullRequestReviewState _review;
    private PullRequestCheckState _checks;
    private PullRequestLifecycle _lifecycle;

    public PullRequestJourneySnapshot CaptureSnapshot()
    {
        var commands = new List<string>();
        if (!_branchCreated) commands.Add(PullRequestJourneyCommands.CreateBranch);
        if (_branchCreated && _commits.Count == 0) commands.Add(PullRequestJourneyCommands.CommitProposal);
        if (_commits.Count > 0 && !_pullRequestOpened) commands.Add(PullRequestJourneyCommands.OpenPullRequest);
        if (_pullRequestOpened && _lifecycle == PullRequestLifecycle.Open)
        {
            if (_review == PullRequestReviewState.NotReviewed)
            {
                commands.Add(PullRequestJourneyCommands.RequestChanges);
                commands.Add(PullRequestJourneyCommands.Approve);
            }
            if (_review == PullRequestReviewState.ChangesRequested)
                commands.Add(PullRequestJourneyCommands.ReviseProposal);
            if (_checks != PullRequestCheckState.Passed)
                commands.Add(PullRequestJourneyCommands.RunChecks);
            if (_review == PullRequestReviewState.Approved && _checks == PullRequestCheckState.Passed)
                commands.Add(PullRequestJourneyCommands.Merge);
        }

        return new PullRequestJourneySnapshot(
            "github-learning-studio.pull-request-collaboration",
            "Field Notes",
            "Clarify the Field Notes project introduction",
            "Make the project purpose easier for a new visitor to understand.",
            "work/improve-field-notes",
            "main",
            _branchCreated ? "work/improve-field-notes" : "(not created)",
            _pullRequestOpened ? 1 : null,
            Array.AsReadOnly(_commits.ToArray()),
            Array.AsReadOnly(_sharedHistory.ToArray()),
            _commits.Count == 0 ? 0 : _commits[^1].Revision,
            _lifecycle,
            _review,
            _review == PullRequestReviewState.ChangesRequested
                ? "Please make the project introduction clearer for a first-time visitor."
                : null,
            _checks,
            _mergeAuthorized,
            _mergedBy,
            BuildExplanation(),
            "Simulation only. No Git commands, GitHub API calls, remote branches, reviews, checks, or merges occur.",
            Array.AsReadOnly(commands.ToArray()));
    }

    public PullRequestJourneyCommandResult Execute(string commandId, string? value = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);
        var accepted = commandId switch
        {
            PullRequestJourneyCommands.CreateBranch => CreateBranch(),
            PullRequestJourneyCommands.CommitProposal => Commit(value),
            PullRequestJourneyCommands.OpenPullRequest => OpenPullRequest(),
            PullRequestJourneyCommands.RequestChanges => RequestChanges(),
            PullRequestJourneyCommands.ReviseProposal => Revise(value),
            PullRequestJourneyCommands.RunChecks => RunChecks(value),
            PullRequestJourneyCommands.Approve => Approve(),
            PullRequestJourneyCommands.Merge => Merge(value),
            _ => false
        };

        var message = accepted
            ? commandId == PullRequestJourneyCommands.Merge
                ? $"The simulated proposal was merged by {_mergedBy}."
                : $"The simulated action '{commandId}' was accepted."
            : $"The action '{commandId}' is not available or its requirements are not satisfied.";
        return new PullRequestJourneyCommandResult(accepted, commandId, message, CaptureSnapshot());
    }

    private bool CreateBranch()
    {
        if (_branchCreated || _pullRequestOpened) return false;
        _branchCreated = true;
        return true;
    }

    private bool Commit(string? message)
    {
        if (!_branchCreated || _pullRequestOpened || string.IsNullOrWhiteSpace(message)) return false;
        _commits.Add(new ProposalCommit(_commits.Count + 1, message.Trim(), 1));
        return true;
    }

    private bool OpenPullRequest()
    {
        if (!_branchCreated || _commits.Count == 0 || _pullRequestOpened) return false;
        _pullRequestOpened = true;
        _lifecycle = PullRequestLifecycle.Open;
        return true;
    }

    private bool RequestChanges()
    {
        if (!_pullRequestOpened || _lifecycle != PullRequestLifecycle.Open ||
            _review != PullRequestReviewState.NotReviewed) return false;
        _review = PullRequestReviewState.ChangesRequested;
        _checks = PullRequestCheckState.NotRun;
        return true;
    }

    private bool Revise(string? message)
    {
        if (!_pullRequestOpened || _lifecycle != PullRequestLifecycle.Open ||
            _review != PullRequestReviewState.ChangesRequested || string.IsNullOrWhiteSpace(message)) return false;
        var revision = _commits[^1].Revision + 1;
        _commits.Add(new ProposalCommit(_commits.Count + 1, message.Trim(), revision));
        _review = PullRequestReviewState.NotReviewed;
        _checks = PullRequestCheckState.NotRun;
        return true;
    }

    private bool RunChecks(string? outcome)
    {
        if (!_pullRequestOpened || _lifecycle != PullRequestLifecycle.Open ||
            outcome is not ("pass" or "fail")) return false;
        _checks = outcome == "pass" ? PullRequestCheckState.Passed : PullRequestCheckState.Failed;
        return true;
    }

    private bool Approve()
    {
        if (!_pullRequestOpened || _lifecycle != PullRequestLifecycle.Open ||
            _review != PullRequestReviewState.NotReviewed) return false;
        _review = PullRequestReviewState.Approved;
        return true;
    }

    private bool Merge(string? actor)
    {
        if (!_pullRequestOpened || _lifecycle != PullRequestLifecycle.Open ||
            _review != PullRequestReviewState.Approved || _checks != PullRequestCheckState.Passed ||
            !_mergeAuthorized || !string.Equals(actor, "maintainer", StringComparison.Ordinal)) return false;
        _mergedBy = actor;
        _sharedHistory.AddRange(_commits);
        _lifecycle = PullRequestLifecycle.Merged;
        return true;
    }

    /// <summary>Host-side authorization input for the simulation; it is not a GitHub credential.</summary>
    public void SetMergeAuthorization(bool authorized)
    {
        _mergeAuthorized = authorized;
    }

    private string BuildExplanation() => _lifecycle == PullRequestLifecycle.Merged
        ? "Merged: the proposal was approved, checks passed, and an explicitly authorized maintainer chose to merge."
        : _review switch
        {
            PullRequestReviewState.ChangesRequested => "Review requested changes. Revise the same proposal; do not open a replacement pull request.",
            PullRequestReviewState.Approved when _checks == PullRequestCheckState.Passed => "Approval and passing checks are both present. Merge remains a separate, explicit decision.",
            PullRequestReviewState.Approved => "The proposal is approved, but approval does not mean automated checks passed.",
            _ when _checks == PullRequestCheckState.Failed => "Automated checks failed. A reviewer decision cannot make failing checks pass.",
            _ when _pullRequestOpened => "The pull request is a proposal for collaboration, not a merge. Review and checks have independent states.",
            _ when _commits.Count > 0 => "A commit records work on the branch. It is not yet a pull request proposal.",
            _ when _branchCreated => "The working branch is a safe line of work. Record a commit before proposing it.",
            _ => "Start with a task, create a working branch, and record a commit."
        };
}
