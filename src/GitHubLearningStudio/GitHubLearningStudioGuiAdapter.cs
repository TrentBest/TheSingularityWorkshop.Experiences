using System.Globalization;
using TheSingularityWorkshop.Workshop.Gui;

namespace TheSingularityWorkshop.Experiences.GitHubLearningStudio;

/// <summary>Projects a lesson snapshot into GUI.Core's host-neutral semantic tree.</summary>
/// <remarks>
/// This adapter declares command identifiers as metadata. It does not install callbacks or execute commands.
/// The host must dispatch a selected button through <see cref="GitHubLearningStudioGuiCommandDispatcher"/>
/// and project the returned snapshot again.
/// </remarks>
public static class GitHubLearningStudioGuiAdapter
{
    public static GuiNode Build(GitHubLearningStudioSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var root = GuiBuilder.Create("Panel", "github-learning-studio")
            .Property("experience", snapshot.ExperienceId)
            .Property("simulation", "true")
            .Property("projectName", snapshot.ProjectName)
            .Property("repositoryName", snapshot.RepositoryName)
            .Property("activeBranch", snapshot.ActiveBranch)
            .Property("pagesLifecycle", snapshot.PagesLifecycle.ToString());

        root.Child("Text", "github-learning-studio-title", child =>
            child.Text($"{snapshot.ProjectName} — a simulated GitHub project"));
        root.Child("Text", "github-learning-studio-explanation", child =>
            child.Text(snapshot.Explanation));
        root.Child("Text", "github-learning-studio-simulation-notice", child =>
            child.Text("Teaching simulation only. No GitHub repository or website is changed."));

        var repository = GuiBuilder.Create("Panel", "github-learning-studio-repository")
            .Child("Text", "github-learning-studio-repository-name", child =>
                child.Text($"Repository: {snapshot.RepositoryName}"))
            .Child("Text", "github-learning-studio-active-branch", child =>
                child.Text($"Current line of work: {snapshot.ActiveBranch}"))
            .Child("Text", "github-learning-studio-readme", child =>
                child.Text(snapshot.ReadmeInspected
                    ? snapshot.ReadmeContent
                    : "Inspect the README to learn what this project is for."))
            .Property("hasUncommittedChanges", snapshot.HasUncommittedChanges ? "true" : "false");

        root.Child(repository);

        var history = GuiBuilder.Create("Panel", "github-learning-studio-history");
        history.Child("Text", "github-learning-studio-history-title", child =>
            child.Text("Project history"));
        foreach (var commit in snapshot.Commits)
        {
            history.Child("Panel", $"github-learning-studio-commit-{commit.Sequence}", item =>
                item.Child("Text", $"github-learning-studio-commit-{commit.Sequence}-title", child =>
                        child.Text($"Commit {commit.Sequence}: {commit.Message}"))
                    .Child("Text", $"github-learning-studio-commit-{commit.Sequence}-branch", child =>
                        child.Text($"Branch: {commit.Branch}")));
        }
        root.Child(history);

        var pages = GuiBuilder.Create("Panel", "github-learning-studio-pages")
            .Property("lifecycle", snapshot.PagesLifecycle.ToString())
            .Property("configured", snapshot.PagesConfigured ? "true" : "false")
            .Property("builtCommit", snapshot.BuiltCommitSequence?.ToString(CultureInfo.InvariantCulture) ?? "")
            .Property("publishedCommit", snapshot.PublishedCommitSequence?.ToString(CultureInfo.InvariantCulture) ?? "")
            .Child("Text", "github-learning-studio-pages-status", child =>
                child.Text($"Pages: {snapshot.PagesLifecycle}"))
            .Child("Text", "github-learning-studio-pages-url", child =>
                child.Text(snapshot.SimulatedPublishedUrl is null
                    ? "No simulated site has been published yet."
                    : $"Simulated site: {snapshot.SimulatedPublishedUrl}"));
        root.Child(pages);

        var controls = GuiBuilder.Create("Panel", "github-learning-studio-controls");
        foreach (var commandId in snapshot.AvailableCommands)
        {
            var (controlId, label) = DescribeCommand(commandId);
            controls.Child("Button", controlId, child =>
                child.Text(label).Property("onClickCommand", commandId));
        }
        root.Child(controls);

        return root.Build();
    }

    private static (string ControlId, string Label) DescribeCommand(string commandId) =>
        commandId switch
        {
            GitHubLearningStudioCommands.InspectReadme =>
                ("github-learning-studio-inspect-readme", "Inspect project README"),
            GitHubLearningStudioCommands.CreateBranch =>
                ("github-learning-studio-create-branch", "Create a working branch"),
            GitHubLearningStudioCommands.EditReadme =>
                ("github-learning-studio-edit-readme", "Edit README"),
            GitHubLearningStudioCommands.CommitChanges =>
                ("github-learning-studio-commit", "Record a commit"),
            GitHubLearningStudioCommands.ConfigurePages =>
                ("github-learning-studio-configure-pages", "Configure simulated Pages"),
            GitHubLearningStudioCommands.BuildPages =>
                ("github-learning-studio-build-pages", "Build simulated site"),
            GitHubLearningStudioCommands.PublishPages =>
                ("github-learning-studio-publish-pages", "Publish simulated site"),
            _ => throw new InvalidOperationException($"No GUI label is defined for command '{commandId}'.")
        };
}

/// <summary>Dispatches a GUI.Core button's declared command to the lesson session.</summary>
/// <remarks>The host owns the actual button event and must re-project the returned snapshot.</remarks>
public static class GitHubLearningStudioGuiCommandDispatcher
{
    public static bool TryExecute(
        GitHubLearningStudioSession session,
        GuiNode control,
        out GitHubLearningStudioCommandResult? result,
        string? value = null)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(control);

        if (!string.Equals(control.Kind, "Button", StringComparison.Ordinal)
            || !control.Properties.TryGetValue("onClickCommand", out var commandId)
            || string.IsNullOrWhiteSpace(commandId))
        {
            result = null;
            return false;
        }

        result = session.Execute(commandId, value);
        return true;
    }
}
