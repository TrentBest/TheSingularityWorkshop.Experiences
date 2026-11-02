using TheSingularityWorkshop.Workshop.Gui;

namespace TheSingularityWorkshop.Experiences.GitHubLearningStudio.Issues;

/// <summary>Projects issue-learning state into GUI.Core. Command metadata is not an event loop.</summary>
public static class IssueJourneyGuiAdapter
{
    public static GuiNode Build(IssueJourneySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var root = GuiBuilder.Create("Panel", "github-issues-journey")
            .Property("simulation", "true")
            .Property("lifecycle", snapshot.Lifecycle.ToString())
            .Property("issueNumber", snapshot.IssueNumber?.ToString() ?? string.Empty);
        root.Child("Text", "github-issues-title", child => child.Text($"{snapshot.ProjectName} — Issues as shared project memory"));
        root.Child("Text", "github-issues-notice", child => child.Text(snapshot.SimulationNotice));
        root.Child("Text", "github-issues-explanation", child => child.Text(snapshot.Explanation));
        root.Child("Text", "github-issues-report", child => child.Text($"Observed: {snapshot.ObservedBehavior}"));
        root.Child("Text", "github-issues-expected", child => child.Text($"Expected: {snapshot.ExpectedBehavior}"));
        root.Child("Text", "github-issues-context", child => child.Text($"Context: {snapshot.Context}"));
        root.Child("Text", "github-issues-classification", child => child.Text($"Kind: {snapshot.Kind}; label: {snapshot.Label ?? "none"}; assignee: {snapshot.Assignee ?? "unassigned"}"));
        root.Child("Text", "github-issues-linkage", child => child.Text($"Branch: {snapshot.BranchName ?? "not created"}; pull request: {snapshot.PullRequestNumber?.ToString() ?? "not opened"}; merged: {snapshot.PullRequestMerged}"));
        var criteria = GuiBuilder.Create("Panel", "github-issues-acceptance");
        for (var i = 0; i < snapshot.AcceptanceCriteria.Count; i++)
        {
            var index = i;
            criteria.Child("Text", $"github-issues-criterion-{index + 1}", child =>
                child.Text($"{(snapshot.AcceptanceEvidence[index] ? "[verified]" : "[not verified]")} {snapshot.AcceptanceCriteria[index]}"));
        }
        root.Child(criteria);
        var discussion = GuiBuilder.Create("Panel", "github-issues-discussion");
        foreach (var (entry, index) in snapshot.Discussion.Select((entry, index) => (entry, index)))
            discussion.Child("Text", $"github-issues-comment-{index + 1}", child => child.Text($"{entry.Author}: {entry.Message}"));
        root.Child(discussion);

        var controls = GuiBuilder.Create("Panel", "github-issues-controls");
        foreach (var command in snapshot.AvailableCommands)
        {
            var (id, label, value) = command switch
            {
                IssueJourneyCommands.CreateIssue => ("github-issues-create", "Create useful issue", "Restore missing location attribution on field notes"),
                IssueJourneyCommands.ClassifyIssue => ("github-issues-classify", "Label as bug", "bug"),
                IssueJourneyCommands.AssignIssue => ("github-issues-assign", "Assign maintainer", "maintainer"),
                IssueJourneyCommands.AddClarification => ("github-issues-clarify", "Add clarification", "Which entry is affected, and should both published and text-first views show its location?"),
                IssueJourneyCommands.CreateBranch => ("github-issues-branch", "Create linked branch", (string?)null),
                IssueJourneyCommands.CommitFix => ("github-issues-commit", "Commit focused fix", "Restore location attribution in both views"),
                IssueJourneyCommands.OpenPullRequest => ("github-issues-open-pr", "Open linked pull request", null),
                IssueJourneyCommands.MergePullRequest => ("github-issues-merge-pr", "Merge pull request (simulated)", null),
                IssueJourneyCommands.VerifyAcceptance => ("github-issues-verify", "Verify acceptance criteria", null),
                IssueJourneyCommands.CloseIssue => ("github-issues-close", "Close resolved issue", null),
                IssueJourneyCommands.ReopenIssue => ("github-issues-reopen", "Reopen issue", null),
                _ => throw new InvalidOperationException($"Unknown issue command '{command}'.")
            };
            controls.Child("Button", id, child => child.Text(label).Property("onClickCommand", command).Property("commandValue", value ?? string.Empty));
        }
        root.Child(controls);
        return root.Build();
    }
}

/// <summary>Submits GUI.Core command metadata; host must call this from its own event loop.</summary>
public static class IssueJourneyGuiCommandDispatcher
{
    public static bool TryExecute(IssueJourneySession session, GuiNode control,
        out IssueJourneyCommandResult? result, string? value = null)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(control);
        if (!string.Equals(control.Kind, "Button", StringComparison.Ordinal) ||
            !control.Properties.TryGetValue("onClickCommand", out var command) ||
            string.IsNullOrWhiteSpace(command))
        {
            result = null;
            return false;
        }
        if (value is null && control.Properties.TryGetValue("commandValue", out var declared))
            value = declared;
        result = session.Execute(command, value);
        return true;
    }
}
