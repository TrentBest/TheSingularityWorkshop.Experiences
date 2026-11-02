using TheSingularityWorkshop.Workshop.Gui;

namespace TheSingularityWorkshop.Experiences.GitHubLearningStudio.PullRequests;

/// <summary>Projects collaboration state and available semantic commands into GUI.Core.</summary>
/// <remarks>Commands are metadata only. The host owns event handling and re-projects after dispatch.</remarks>
public static class PullRequestJourneyGuiAdapter
{
    public static GuiNode Build(PullRequestJourneySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var root = GuiBuilder.Create("Panel", "github-pr-journey")
            .Property("simulation", "true")
            .Property("lifecycle", snapshot.Lifecycle.ToString())
            .Property("review", snapshot.Review.ToString())
            .Property("checks", snapshot.Checks.ToString());

        root.Child("Text", "github-pr-title", child => child.Text($"{snapshot.ProjectName} — propose, review, revise, and merge"));
        root.Child("Text", "github-pr-explanation", child => child.Text(snapshot.Explanation));
        root.Child("Text", "github-pr-simulation-notice", child => child.Text(snapshot.SimulationNotice));
        root.Child("Text", "github-pr-branch", child => child.Text($"Working branch: {snapshot.BranchName}"));
        root.Child("Text", "github-pr-lifecycle", child => child.Text($"Proposal: {snapshot.Lifecycle}"));
        root.Child("Text", "github-pr-review", child => child.Text($"Review: {snapshot.Review}"));
        root.Child("Text", "github-pr-checks", child => child.Text($"Automated checks: {snapshot.Checks}"));
        var history = GuiBuilder.Create("Panel", "github-pr-commit-history");
        foreach (var commit in snapshot.Commits)
            history.Child("Text", $"github-pr-commit-{commit.Sequence}", child =>
                child.Text($"Commit {commit.Sequence} (revision {commit.Revision}): {commit.Message}"));
        root.Child(history);

        var controls = GuiBuilder.Create("Panel", "github-pr-controls");
        foreach (var command in snapshot.AvailableCommands)
        {
            var (id, label, value) = command switch
            {
                PullRequestJourneyCommands.CreateBranch => ("github-pr-create-branch", "Create working branch", (string?)null),
                PullRequestJourneyCommands.CommitProposal => ("github-pr-commit", "Commit proposal", "Add project notes"),
                PullRequestJourneyCommands.OpenPullRequest => ("github-pr-open", "Open pull request", null),
                PullRequestJourneyCommands.RequestChanges => ("github-pr-request-changes", "Request changes", null),
                PullRequestJourneyCommands.ReviseProposal => ("github-pr-revise", "Revise same proposal", "Address review feedback"),
                PullRequestJourneyCommands.RunChecks => ("github-pr-run-checks", "Run passing checks", "pass"),
                PullRequestJourneyCommands.Approve => ("github-pr-approve", "Approve proposal", null),
                PullRequestJourneyCommands.Merge => ("github-pr-merge", "Merge (simulated)", "maintainer"),
                _ => throw new InvalidOperationException($"Unknown command '{command}'.")
            };
            controls.Child("Button", id, child =>
            {
                child.Text(label).Property("onClickCommand", command);
                if (value is not null) child.Property("commandValue", value);
            });
        }
        root.Child(controls);
        return root.Build();
    }
}

/// <summary>Dispatches declared GUI.Core commands; the host must invoke this from its event loop.</summary>
public static class PullRequestJourneyGuiCommandDispatcher
{
    public static bool TryExecute(PullRequestJourneySession session, GuiNode control,
        out PullRequestJourneyCommandResult? result, string? value = null)
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
        var submitted = value;
        if (submitted is null && control.Properties.TryGetValue("commandValue", out var declared))
            submitted = declared;
        result = session.Execute(command, submitted);
        return true;
    }
}
