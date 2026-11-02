using TheSingularityWorkshop.Experiences.GitHubLearningStudio.Issues;
using Xunit;

namespace TheSingularityWorkshop.Experiences.GitHubLearningStudio.Issues.Tests;

public sealed class IssueJourneySessionTests
{
    [Fact]
    public void VagueReportMustBeImprovedBeforeIssueCanBeCreated()
    {
        var session = new IssueJourneySession();
        Assert.False(session.Execute(IssueJourneyCommands.CreateIssue, "Fix").Accepted);
        Assert.False(session.Execute(IssueJourneyCommands.CreateIssue, "Improve the home page").Accepted);
        Assert.Equal(IssueLifecycle.NotCreated, session.CaptureSnapshot().Lifecycle);
        Assert.True(session.Execute(IssueJourneyCommands.CreateIssue, "Restore missing location attribution on field notes").Accepted);
        var snapshot = session.CaptureSnapshot();
        Assert.Equal(14, snapshot.IssueNumber);
        Assert.Equal(IssueLifecycle.Open, snapshot.Lifecycle);
        Assert.Contains("expected", snapshot.ExpectedBehavior, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, snapshot.AcceptanceCriteria.Count);
        Assert.All(snapshot.AcceptanceEvidence, Assert.False);
    }

    [Fact]
    public void ClassificationAssignmentAndDiscussionAreSeparateFromImplementation()
    {
        var session = PreparedIssue();
        Assert.True(session.Execute(IssueJourneyCommands.ClassifyIssue, "bug").Accepted);
        Assert.True(session.Execute(IssueJourneyCommands.AssignIssue, "maintainer").Accepted);
        Assert.True(session.Execute(IssueJourneyCommands.AddClarification, "Which entry is affected and which views need the attribution?").Accepted);
        var snapshot = session.CaptureSnapshot();
        Assert.Equal(IssueKind.Bug, snapshot.Kind);
        Assert.Equal("bug", snapshot.Label);
        Assert.Equal("maintainer", snapshot.Assignee);
        Assert.Equal(2, snapshot.Discussion.Count);
        Assert.Null(snapshot.BranchName);
        Assert.Empty(snapshot.Commits);
    }

    [Fact]
    public void IssueToBranchPullRequestAndClosureRequireDistinctEvidence()
    {
        var session = PreparedIssue();
        session.Execute(IssueJourneyCommands.ClassifyIssue, "bug");
        session.Execute(IssueJourneyCommands.AssignIssue, "maintainer");
        session.Execute(IssueJourneyCommands.AddClarification, "Which entry is affected and which views need the attribution?");
        Assert.True(session.Execute(IssueJourneyCommands.CreateBranch).Accepted);
        Assert.True(session.Execute(IssueJourneyCommands.CommitFix, "Restore location attribution in both views").Accepted);
        Assert.True(session.Execute(IssueJourneyCommands.OpenPullRequest).Accepted);
        Assert.False(session.Execute(IssueJourneyCommands.CloseIssue).Accepted);
        Assert.True(session.Execute(IssueJourneyCommands.MergePullRequest).Accepted);
        Assert.False(session.Execute(IssueJourneyCommands.CloseIssue).Accepted);
        Assert.False(session.CaptureSnapshot().AcceptanceVerified);
        Assert.True(session.Execute(IssueJourneyCommands.VerifyAcceptance).Accepted);
        var verified = session.CaptureSnapshot();
        Assert.All(verified.AcceptanceEvidence, Assert.True);
        Assert.True(session.Execute(IssueJourneyCommands.CloseIssue).Accepted);
        Assert.Equal(IssueLifecycle.Closed, session.CaptureSnapshot().Lifecycle);
    }

    [Fact]
    public void IssueCannotBeImplementedBeforeItIsActionable()
    {
        var session = PreparedIssue();
        Assert.False(session.Execute(IssueJourneyCommands.CreateBranch).Accepted);
        Assert.False(session.Execute(IssueJourneyCommands.CommitFix, "Implement fix").Accepted);
        Assert.False(session.Execute(IssueJourneyCommands.OpenPullRequest).Accepted);
    }

    [Fact]
    public void ClosedIssueCanBeReopenedAndAcceptanceMustBeReverified()
    {
        var session = PreparedIssue();
        session.Execute(IssueJourneyCommands.ClassifyIssue, "bug");
        session.Execute(IssueJourneyCommands.AssignIssue, "maintainer");
        session.Execute(IssueJourneyCommands.AddClarification, "Which entry is affected and which views need the attribution?");
        session.Execute(IssueJourneyCommands.CreateBranch);
        session.Execute(IssueJourneyCommands.CommitFix, "Restore location attribution in both views");
        session.Execute(IssueJourneyCommands.OpenPullRequest);
        session.Execute(IssueJourneyCommands.MergePullRequest);
        session.Execute(IssueJourneyCommands.VerifyAcceptance);
        session.Execute(IssueJourneyCommands.CloseIssue);
        Assert.True(session.Execute(IssueJourneyCommands.ReopenIssue).Accepted);
        var reopened = session.CaptureSnapshot();
        Assert.Equal(IssueLifecycle.Open, reopened.Lifecycle);
        Assert.False(reopened.AcceptanceVerified);
        Assert.All(reopened.AcceptanceEvidence, Assert.False);
        Assert.Contains(IssueJourneyCommands.VerifyAcceptance, reopened.AvailableCommands);
    }

    [Fact]
    public void GuiProjectionContainsIssueStateAndSemanticCommands()
    {
        var session = PreparedIssue();
        session.Execute(IssueJourneyCommands.ClassifyIssue, "bug");
        var tree = IssueJourneyGuiAdapter.Build(session.CaptureSnapshot());
        Assert.Equal("github-issues-journey", tree.Id);
        Assert.Contains(tree.Children, node => node.Id == "github-issues-controls");
        Assert.Contains(tree.Children, node => node.Id == "github-issues-notice");
    }

    [Fact]
    public void GuiDispatcherUsesCommandMetadataAndReturnsFreshSnapshot()
    {
        var session = new IssueJourneySession();
        var tree = IssueJourneyGuiAdapter.Build(session.CaptureSnapshot());
        var controls = Assert.Single(tree.Children.Where(node => node.Id == "github-issues-controls"));
        var button = Assert.Single(controls.Children);
        Assert.True(IssueJourneyGuiCommandDispatcher.TryExecute(session, button, out var result));
        Assert.True(result!.Accepted);
        Assert.Equal(IssueLifecycle.Open, result.Snapshot.Lifecycle);
    }

    private static IssueJourneySession PreparedIssue()
    {
        var session = new IssueJourneySession();
        Assert.True(session.Execute(IssueJourneyCommands.CreateIssue, "Restore missing location attribution on field notes").Accepted);
        return session;
    }
}
