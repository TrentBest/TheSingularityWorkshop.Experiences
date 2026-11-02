using TheSingularityWorkshop.Experiences.GitHubLearningStudio;
using TheSingularityWorkshop.Workshop.Gui;
using Xunit;

namespace TheSingularityWorkshop.Experiences.GitHubLearningStudio.Tests;

public sealed class GitHubLearningStudioGuiAdapterTests
{
    [Fact]
    public void ProjectsSimulationNoticeRepositoryHistoryAndPagesState()
    {
        var session = new GitHubLearningStudioSession();
        var tree = GitHubLearningStudioGuiAdapter.Build(session.CaptureSnapshot());

        Assert.Equal("github-learning-studio", tree.Id);
        Assert.Equal("true", tree.Properties["simulation"]);
        Assert.Equal("field-notes", tree.Properties["repositoryName"]);
        Assert.Equal("NotConfigured", tree.Properties["pagesLifecycle"]);
        Assert.Contains("simulated GitHub project", tree.Find("github-learning-studio-title").Text);
        Assert.Contains("Teaching simulation only", tree.Find("github-learning-studio-simulation-notice").Text);
        Assert.Equal("Current line of work: main", tree.Find("github-learning-studio-active-branch").Text);
        Assert.Equal("Create the starter project",
            tree.Find("github-learning-studio-commit-1-title").Text);
    }

    [Fact]
    public void ProjectsOnlyCommandsAvailableInTheCurrentLessonState()
    {
        var session = new GitHubLearningStudioSession();
        var initialTree = GitHubLearningStudioGuiAdapter.Build(session.CaptureSnapshot());

        Assert.Equal(GitHubLearningStudioCommands.InspectReadme,
            initialTree.Find("github-learning-studio-inspect-readme").Properties["onClickCommand"]);
        Assert.False(initialTree.TryFind("github-learning-studio-edit-readme", out _));
        Assert.False(initialTree.TryFind("github-learning-studio-publish-pages", out _));

        Assert.True(GitHubLearningStudioGuiCommandDispatcher.TryExecute(
            session, initialTree.Find("github-learning-studio-inspect-readme"), out var inspect));
        Assert.True(inspect!.Accepted);

        var inspectedTree = GitHubLearningStudioGuiAdapter.Build(inspect.Snapshot);
        Assert.True(inspectedTree.TryFind("github-learning-studio-create-branch", out var createBranch));
        Assert.NotNull(createBranch);
        Assert.Equal(GitHubLearningStudioCommands.CreateBranch,
            createBranch.Properties["onClickCommand"]);
        Assert.False(inspectedTree.TryFind("github-learning-studio-edit-readme", out _));

        Assert.True(GitHubLearningStudioGuiCommandDispatcher.TryExecute(
            session, createBranch, out var branchResult));
        var branchedTree = GitHubLearningStudioGuiAdapter.Build(branchResult!.Snapshot);
        Assert.True(branchedTree.TryFind("github-learning-studio-edit-readme", out _));
    }

    [Fact]
    public void DispatcherSubmitsSemanticCommandAndReturnsFreshSnapshot()
    {
        var session = new GitHubLearningStudioSession();
        var tree = GitHubLearningStudioGuiAdapter.Build(session.CaptureSnapshot());
        var control = tree.Find("github-learning-studio-inspect-readme");

        Assert.True(GitHubLearningStudioGuiCommandDispatcher.TryExecute(
            session, control, out var result));

        Assert.NotNull(result);
        Assert.True(result!.Accepted);
        Assert.True(result.Snapshot.ReadmeInspected);
    }

    [Fact]
    public void KnowledgeCheckButtonsSubmitTheirDeclaredAnswerValues()
    {
        var session = new GitHubLearningStudioSession();
        session.Execute(GitHubLearningStudioCommands.InspectReadme);
        session.Execute(GitHubLearningStudioCommands.CreateBranch);
        session.Execute(GitHubLearningStudioCommands.EditReadme, "# Field Notes\n\nA project journal.");
        session.Execute(GitHubLearningStudioCommands.CommitChanges, "Improve project home");
        session.Execute(GitHubLearningStudioCommands.ConfigurePages);
        session.Execute(GitHubLearningStudioCommands.BuildPages);
        session.Execute(GitHubLearningStudioCommands.PublishPages);

        var tree = GitHubLearningStudioGuiAdapter.Build(session.CaptureSnapshot());
        var answer = tree.Find("github-learning-studio-answer-build-publish");

        Assert.Equal(GitHubLearningStudioCommands.AnswerKnowledgeCheck,
            answer.Properties["onClickCommand"]);
        Assert.True(GitHubLearningStudioGuiCommandDispatcher.TryExecute(
            session, answer, out var result));
        Assert.True(result!.Snapshot.LessonCompleted);
    }

    [Fact]
    public void DispatcherIgnoresNodesWithoutButtonCommandMetadata()
    {
        var session = new GitHubLearningStudioSession();
        var before = session.CaptureSnapshot();
        var panel = GuiBuilder.Create("Panel", "not-a-button").Build();
        var buttonWithoutCommand = GuiBuilder.Create("Button", "no-command").Build();

        Assert.False(GitHubLearningStudioGuiCommandDispatcher.TryExecute(
            session, panel, out var panelResult));
        Assert.Null(panelResult);
        Assert.False(GitHubLearningStudioGuiCommandDispatcher.TryExecute(
            session, buttonWithoutCommand, out var buttonResult));
        Assert.Null(buttonResult);

        Assert.Equal(before.ReadmeInspected, session.CaptureSnapshot().ReadmeInspected);
        Assert.Equal(before.Commits, session.CaptureSnapshot().Commits);
    }

    [Fact]
    public void ReprojectionReflectsRecordedCommitAndPublishedLifecycle()
    {
        var session = new GitHubLearningStudioSession();
        session.Execute(GitHubLearningStudioCommands.InspectReadme);
        session.Execute(GitHubLearningStudioCommands.CreateBranch);
        session.Execute(GitHubLearningStudioCommands.EditReadme, "# Field Notes\n\nA public project journal.");
        session.Execute(GitHubLearningStudioCommands.CommitChanges, "Improve project home");
        session.Execute(GitHubLearningStudioCommands.ConfigurePages);
        session.Execute(GitHubLearningStudioCommands.BuildPages);
        session.Execute(GitHubLearningStudioCommands.PublishPages);

        var tree = GitHubLearningStudioGuiAdapter.Build(session.CaptureSnapshot());

        Assert.Equal("Published", tree.Find("github-learning-studio-pages").Properties["lifecycle"]);
        Assert.Equal("2", tree.Find("github-learning-studio-pages").Properties["publishedCommit"]);
        Assert.Contains("example.invalid", tree.Find("github-learning-studio-pages-url").Text);
        Assert.False(tree.TryFind("github-learning-studio-publish-pages", out _));
    }
}
