using Xunit;
using TheSingularityWorkshop.Experiences.GitHubLearningStudio.PullRequests;

namespace TheSingularityWorkshop.Experiences.GitHubLearningStudio.PullRequests.Tests;

public sealed class PullRequestJourneySessionTests
{
    [Fact]
    public void HappyPath_RequiresProposalReviewChecksAndExplicitMerge()
    {
        var session = StartProposal();
        Assert.True(session.Execute(PullRequestJourneyCommands.OpenPullRequest).Accepted);
        Assert.True(session.Execute(PullRequestJourneyCommands.Approve).Accepted);
        Assert.False(session.Execute(PullRequestJourneyCommands.Merge, "maintainer").Accepted);
        Assert.True(session.Execute(PullRequestJourneyCommands.RunChecks, "pass").Accepted);
        Assert.False(session.Execute(PullRequestJourneyCommands.Merge, "maintainer").Accepted);
        session.SetMergeAuthorization(true);
        var merged = session.Execute(PullRequestJourneyCommands.Merge, "maintainer");
        Assert.True(merged.Accepted);
        Assert.Equal(PullRequestLifecycle.Merged, merged.Snapshot.Lifecycle);
        Assert.Equal("maintainer", merged.Snapshot.MergedBy);
    }

    [Fact]
    public void RequestedChanges_RevisionUpdatesTheSameOpenProposal()
    {
        var session = StartProposal();
        session.Execute(PullRequestJourneyCommands.OpenPullRequest);
        Assert.True(session.Execute(PullRequestJourneyCommands.RequestChanges).Accepted);
        Assert.True(session.Execute(PullRequestJourneyCommands.ReviseProposal, "Clarify the project purpose").Accepted);
        var snapshot = session.CaptureSnapshot();
        Assert.Equal(PullRequestLifecycle.Open, snapshot.Lifecycle);
        Assert.Equal(PullRequestReviewState.NotReviewed, snapshot.Review);
        Assert.Equal(2, snapshot.Revision);
        Assert.Equal(2, snapshot.Commits.Count);
    }

    [Fact]
    public void FailedChecksPreventMergeEvenAfterApproval()
    {
        var session = StartProposal();
        session.Execute(PullRequestJourneyCommands.OpenPullRequest);
        session.Execute(PullRequestJourneyCommands.Approve);
        session.Execute(PullRequestJourneyCommands.RunChecks, "fail");
        session.SetMergeAuthorization(true);
        Assert.False(session.Execute(PullRequestJourneyCommands.Merge, "maintainer").Accepted);
        Assert.Equal(PullRequestCheckState.Failed, session.CaptureSnapshot().Checks);
    }

    [Fact]
    public void PassingChecksDoNotImplyApproval()
    {
        var session = StartProposal();
        session.Execute(PullRequestJourneyCommands.OpenPullRequest);
        session.Execute(PullRequestJourneyCommands.RunChecks, "pass");
        session.SetMergeAuthorization(true);
        Assert.Equal(PullRequestReviewState.NotReviewed, session.CaptureSnapshot().Review);
        Assert.False(session.Execute(PullRequestJourneyCommands.Merge, "maintainer").Accepted);
    }

    [Fact]
    public void ApprovalDoesNotMergeAndMergeRequiresAuthorizedMaintainer()
    {
        var session = StartProposal();
        session.Execute(PullRequestJourneyCommands.OpenPullRequest);
        session.Execute(PullRequestJourneyCommands.Approve);
        session.Execute(PullRequestJourneyCommands.RunChecks, "pass");
        Assert.Equal(PullRequestLifecycle.Open, session.CaptureSnapshot().Lifecycle);
        Assert.False(session.Execute(PullRequestJourneyCommands.Merge, "maintainer").Accepted);
        session.SetMergeAuthorization(true);
        Assert.False(session.Execute(PullRequestJourneyCommands.Merge, "learner").Accepted);
        Assert.True(session.Execute(PullRequestJourneyCommands.Merge, "maintainer").Accepted);
    }

    [Fact]
    public void InvalidOrderingIsRejected()
    {
        var session = new PullRequestJourneySession();
        Assert.False(session.Execute(PullRequestJourneyCommands.OpenPullRequest).Accepted);
        Assert.False(session.Execute(PullRequestJourneyCommands.CommitProposal, "premature").Accepted);
        Assert.True(session.Execute(PullRequestJourneyCommands.CreateBranch).Accepted);
        Assert.False(session.Execute(PullRequestJourneyCommands.OpenPullRequest).Accepted);
    }

    [Fact]
    public void GuiProjectionDeclaresCommandsWithoutExecutingThem()
    {
        var session = new PullRequestJourneySession();
        var tree = PullRequestJourneyGuiAdapter.Build(session.CaptureSnapshot());
        Assert.Equal(PullRequestLifecycle.NotOpened.ToString(), tree.Properties["lifecycle"]);
        Assert.Equal(0, session.CaptureSnapshot().Commits.Count);
        Assert.Contains(tree.Children, node => node.Id == "github-pr-controls");
    }

    private static PullRequestJourneySession StartProposal()
    {
        var session = new PullRequestJourneySession();
        Assert.True(session.Execute(PullRequestJourneyCommands.CreateBranch).Accepted);
        Assert.True(session.Execute(PullRequestJourneyCommands.CommitProposal, "Add project overview").Accepted);
        return session;
    }
}
