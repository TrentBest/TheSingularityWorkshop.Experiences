using TheSingularityWorkshop.Experiences.GitHubLearningStudio;
using Xunit;

namespace TheSingularityWorkshop.Experiences.GitHubLearningStudio.Tests;

public sealed class GitHubLearningStudioSessionTests
{
    [Fact]
    public void StartsWithInspectableFictionalRepositoryAndNoExternalPublication()
    {
        var session = new GitHubLearningStudioSession();
        var snapshot = session.CaptureSnapshot();

        Assert.Equal("github-learning-studio.pages-first", snapshot.ExperienceId);
        Assert.Equal("Field Notes", snapshot.ProjectName);
        Assert.Equal("field-notes", snapshot.RepositoryName);
        Assert.Equal("main", snapshot.ActiveBranch);
        Assert.False(snapshot.ReadmeInspected);
        Assert.Single(snapshot.Commits);
        Assert.Equal(PagesLifecycle.NotConfigured, snapshot.PagesLifecycle);
        Assert.Null(snapshot.SimulatedPublishedUrl);
        Assert.Contains("simulation", typeof(GitHubLearningStudioSession).GetCustomAttributes(false).ToString() ?? "");
    }

    [Fact]
    public void RejectsCommandsThatAreNotAvailableInTheCurrentLessonState()
    {
        var session = new GitHubLearningStudioSession();

        var result = session.Execute(GitHubLearningStudioCommands.PublishPages);

        Assert.False(result.Accepted);
        Assert.Equal(PagesLifecycle.NotConfigured, result.Snapshot.PagesLifecycle);
        Assert.Null(result.Snapshot.PublishedCommitSequence);
    }

    [Fact]
    public void ModelsTheFirstRepositoryToPagesJourney()
    {
        var session = new GitHubLearningStudioSession();

        AssertAccepted(session, GitHubLearningStudioCommands.InspectReadme);
        AssertAccepted(session, GitHubLearningStudioCommands.CreateBranch);
        var edit = AssertAccepted(session, GitHubLearningStudioCommands.EditReadme,
            "# Field Notes\n\nA project journal shared with the world.");
        Assert.True(edit.Snapshot.HasUncommittedChanges);
        AssertAccepted(session, GitHubLearningStudioCommands.CommitChanges, "Improve the project home");

        var configured = AssertAccepted(session, GitHubLearningStudioCommands.ConfigurePages);
        Assert.Equal(PagesLifecycle.Configured, configured.Snapshot.PagesLifecycle);
        var built = AssertAccepted(session, GitHubLearningStudioCommands.BuildPages);
        Assert.Equal(2, built.Snapshot.BuiltCommitSequence);
        var published = AssertAccepted(session, GitHubLearningStudioCommands.PublishPages);
        Assert.Equal(PagesLifecycle.Published, published.Snapshot.PagesLifecycle);
        Assert.Equal(2, published.Snapshot.PublishedCommitSequence);
        Assert.Equal("https://example.invalid/field-notes/", published.Snapshot.SimulatedPublishedUrl);
    }

    [Fact]
    public void ACommittedEditMakesThePublishedSiteOutOfDateUntilRebuiltAndRepublished()
    {
        var session = new GitHubLearningStudioSession();
        PublishFirstVersion(session);

        AssertAccepted(session, GitHubLearningStudioCommands.EditReadme,
            "# Field Notes\n\nVersion two adds a project update.");
        AssertAccepted(session, GitHubLearningStudioCommands.CommitChanges, "Add project update");
        var changed = session.CaptureSnapshot();
        Assert.Equal(PagesLifecycle.UpdateAvailable, changed.PagesLifecycle);
        Assert.Equal(2, changed.PublishedCommitSequence);
        Assert.Equal(3, changed.Commits[^1].Sequence);

        var rebuilt = AssertAccepted(session, GitHubLearningStudioCommands.BuildPages);
        Assert.Equal(3, rebuilt.Snapshot.BuiltCommitSequence);
        Assert.Equal(2, rebuilt.Snapshot.PublishedCommitSequence);
        var republished = AssertAccepted(session, GitHubLearningStudioCommands.PublishPages);
        Assert.Equal(3, republished.Snapshot.PublishedCommitSequence);
        Assert.Equal(PagesLifecycle.Published, republished.Snapshot.PagesLifecycle);
    }

    [Fact]
    public void CannotBuildUncommittedContentOrPublishAnUnbuiltCommit()
    {
        var session = new GitHubLearningStudioSession();
        AssertAccepted(session, GitHubLearningStudioCommands.InspectReadme);
        AssertAccepted(session, GitHubLearningStudioCommands.CreateBranch);
        AssertAccepted(session, GitHubLearningStudioCommands.EditReadme, "# Edited");
        Assert.False(session.Execute(GitHubLearningStudioCommands.CommitChanges, " ").Accepted);
        Assert.False(session.Execute(GitHubLearningStudioCommands.ConfigurePages).Accepted);
    }

    [Fact]
    public void UnknownCommandsDoNotChangeTheSnapshot()
    {
        var session = new GitHubLearningStudioSession();
        var before = session.CaptureSnapshot();

        var result = session.Execute("github.real-account.delete-repository");

        Assert.False(result.Accepted);
        Assert.Equal(before, result.Snapshot);
        Assert.Equal(before.Commits, result.Snapshot.Commits);
    }

    [Fact]
    public void SessionsWithTheSameCommandsProduceTheSameCommitHistory()
    {
        var first = new GitHubLearningStudioSession();
        var second = new GitHubLearningStudioSession();

        PublishFirstVersion(first);
        PublishFirstVersion(second);

        Assert.Equal(first.CaptureSnapshot(), second.CaptureSnapshot());
        Assert.Equal(first.CaptureSnapshot().Commits, second.CaptureSnapshot().Commits);
    }

    private static GitHubLearningStudioSnapshot AssertAccepted(
        GitHubLearningStudioSession session,
        string command,
        string? value = null)
    {
        var result = session.Execute(command, value);
        Assert.True(result.Accepted, result.Message);
        return result.Snapshot;
    }

    private static void PublishFirstVersion(GitHubLearningStudioSession session)
    {
        AssertAccepted(session, GitHubLearningStudioCommands.InspectReadme);
        AssertAccepted(session, GitHubLearningStudioCommands.CreateBranch);
        AssertAccepted(session, GitHubLearningStudioCommands.EditReadme,
            "# Field Notes\n\nA project journal.");
        AssertAccepted(session, GitHubLearningStudioCommands.CommitChanges, "Improve the project home");
        AssertAccepted(session, GitHubLearningStudioCommands.ConfigurePages);
        AssertAccepted(session, GitHubLearningStudioCommands.BuildPages);
        AssertAccepted(session, GitHubLearningStudioCommands.PublishPages);
    }
}
