using TheSingularityWorkshop.Experiences.CastleSiege;
using Xunit;

namespace TheSingularityWorkshop.Experiences.CastleSiege.Tests;

public sealed class CastleSiegeTabletopSessionTests
{
    [Fact]
    public void NewSession_ExposesVersionedScenarioAndExplicitInitialState()
    {
        var session = new CastleSiegeTabletopSession();

        Assert.Equal("castle.west-wall", CastleSiegeTabletopSession.ScenarioKey);
        Assert.Equal(1, CastleSiegeTabletopSession.ScenarioVersion);
        Assert.Equal("castle-siege-tabletop/1", CastleSiegeTabletopSession.RulesVersion);
        Assert.Equal(CastleSiegeTabletopSession.InitialState(), session.State);
        Assert.Equal(CastleSiegeActionKind.FireAtWestWall, Assert.Single(session.GetLegalActions()));
    }

    [Fact]
    public void Apply_ProducesOrderedExplainableAbstractEventsAndAlternatesRoles()
    {
        var session = new CastleSiegeTabletopSession();

        var attack = session.Apply(new CastleSiegeAction(CastleSiegeRole.Attacker, CastleSiegeActionKind.FireAtWestWall));
        var defend = session.Apply(new CastleSiegeAction(CastleSiegeRole.Defender, CastleSiegeActionKind.ReinforceWestWall));

        Assert.Equal(1, attack.Sequence);
        Assert.Equal(1, attack.TurnNumber);
        Assert.Equal("wall.abstract-breach-point", attack.RuleId);
        Assert.Equal("abstract-tabletop", attack.Classification);
        Assert.Equal(2, attack.StateAfter.WallIntegrity);
        Assert.Equal(CastleSiegeRole.Defender, attack.StateAfter.ActiveRole);

        Assert.Equal(2, defend.Sequence);
        Assert.Equal(2, defend.TurnNumber);
        Assert.Equal("wall.abstract-reinforcement", defend.RuleId);
        Assert.Equal(3, defend.StateAfter.WallIntegrity);
        Assert.Equal(CastleSiegeRole.Attacker, session.State.ActiveRole);
        Assert.Equal(2, session.Events.Count);
    }

    [Fact]
    public void Report_CapturesIdentityStateLegalActionsAndOrderedEventsAsStableSnapshot()
    {
        var session = new CastleSiegeTabletopSession();
        session.Apply(new CastleSiegeAction(CastleSiegeRole.Attacker, CastleSiegeActionKind.FireAtWestWall));

        var report = CastleSiegeSessionReport.Capture(session);

        Assert.Equal(CastleSiegeTabletopSession.ScenarioKey, report.ScenarioKey);
        Assert.Equal(CastleSiegeTabletopSession.ScenarioVersion, report.ScenarioVersion);
        Assert.Equal(CastleSiegeTabletopSession.RulesVersion, report.RulesVersion);
        Assert.Equal(session.State, report.State);
        Assert.Equal(session.State.Outcome, report.Outcome);
        Assert.Equal(CastleSiegeActionKind.ReinforceWestWall, Assert.Single(report.LegalActions.Where(
            action => action == CastleSiegeActionKind.ReinforceWestWall)));
        Assert.Equal(1, Assert.Single(report.Events).Sequence);

        session.Apply(new CastleSiegeAction(CastleSiegeRole.Defender, CastleSiegeActionKind.HoldPosition));

        Assert.Single(report.Events);
        Assert.Equal(CastleSiegeRole.Defender, report.State.ActiveRole);
        Assert.Equal(1, report.Events[0].Sequence);
        Assert.Equal(2, session.Events.Count);
    }

    [Fact]
    public void Report_CollectionsAreReadOnly()
    {
        var report = CastleSiegeSessionReport.Capture(new CastleSiegeTabletopSession());

        Assert.Throws<NotSupportedException>(() =>
            ((IList<CastleSiegeActionKind>)report.LegalActions).Clear());
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CastleSiegeTabletopEvent>)report.Events).Clear());
    }

    [Fact]
    public void Report_CaptureRejectsNullSession()
    {
        Assert.Throws<ArgumentNullException>(() => CastleSiegeSessionReport.Capture(null!));
    }

    [Fact]
    public void Apply_RejectsOutOfTurnActionWithoutChangingStateOrEvidence()
    {
        var session = new CastleSiegeTabletopSession();
        var before = session.State;

        var exception = Assert.Throws<InvalidOperationException>(() =>
            session.Apply(new CastleSiegeAction(CastleSiegeRole.Defender, CastleSiegeActionKind.HoldPosition)));

        Assert.Contains("Out-of-turn", exception.Message);
        Assert.Equal(before, session.State);
        Assert.Empty(session.Events);
    }

    [Fact]
    public void Apply_RejectsIllegalActionWithoutChangingStateOrEvidence()
    {
        var session = new CastleSiegeTabletopSession();
        var before = session.State;

        Assert.Throws<InvalidOperationException>(() =>
            session.Apply(new CastleSiegeAction(CastleSiegeRole.Attacker, CastleSiegeActionKind.HoldPosition)));

        Assert.Equal(before, session.State);
        Assert.Empty(session.Events);
    }

    [Fact]
    public void RepeatedAcceptedActionSequence_ProducesIdenticalStateAndEventSequence()
    {
        var actions = new[]
        {
            new CastleSiegeAction(CastleSiegeRole.Attacker, CastleSiegeActionKind.FireAtWestWall),
            new CastleSiegeAction(CastleSiegeRole.Defender, CastleSiegeActionKind.HoldPosition),
            new CastleSiegeAction(CastleSiegeRole.Attacker, CastleSiegeActionKind.FireAtWestWall),
            new CastleSiegeAction(CastleSiegeRole.Defender, CastleSiegeActionKind.HoldPosition),
            new CastleSiegeAction(CastleSiegeRole.Attacker, CastleSiegeActionKind.FireAtWestWall)
        };

        var first = Run(actions);
        var second = Run(actions);

        Assert.Equal(first.State, second.State);
        Assert.Equal(first.Events, second.Events);
        Assert.Equal(CastleSiegeOutcome.AttackerVictory, first.State.Outcome);
        Assert.Equal(0, first.State.WallIntegrity);
        Assert.Equal(5, first.Events.Count);
    }

    [Fact]
    public void EmptyWall_EndsSessionAndFurtherActionsAreRejected()
    {
        var session = new CastleSiegeTabletopSession();
        session.Apply(new CastleSiegeAction(CastleSiegeRole.Attacker, CastleSiegeActionKind.FireAtWestWall));
        session.Apply(new CastleSiegeAction(CastleSiegeRole.Defender, CastleSiegeActionKind.HoldPosition));
        session.Apply(new CastleSiegeAction(CastleSiegeRole.Attacker, CastleSiegeActionKind.FireAtWestWall));
        session.Apply(new CastleSiegeAction(CastleSiegeRole.Defender, CastleSiegeActionKind.HoldPosition));
        session.Apply(new CastleSiegeAction(CastleSiegeRole.Attacker, CastleSiegeActionKind.FireAtWestWall));

        Assert.Equal(CastleSiegeOutcome.AttackerVictory, session.State.Outcome);
        Assert.Empty(session.GetLegalActions());
        var before = session.State;

        Assert.Throws<InvalidOperationException>(() =>
            session.Apply(new CastleSiegeAction(CastleSiegeRole.Defender, CastleSiegeActionKind.HoldPosition)));

        Assert.Equal(before, session.State);
        Assert.Equal(5, session.Events.Count);
    }

    [Fact]
    public void ExhaustedAttackerShots_EndsWithDefenderVictory()
    {
        var session = new CastleSiegeTabletopSession();
        session.Apply(new CastleSiegeAction(CastleSiegeRole.Attacker, CastleSiegeActionKind.FireAtWestWall));
        session.Apply(new CastleSiegeAction(CastleSiegeRole.Defender, CastleSiegeActionKind.ReinforceWestWall));
        session.Apply(new CastleSiegeAction(CastleSiegeRole.Attacker, CastleSiegeActionKind.FireAtWestWall));
        session.Apply(new CastleSiegeAction(CastleSiegeRole.Defender, CastleSiegeActionKind.ReinforceWestWall));
        session.Apply(new CastleSiegeAction(CastleSiegeRole.Attacker, CastleSiegeActionKind.FireAtWestWall));

        Assert.Equal(CastleSiegeOutcome.DefenderVictory, session.State.Outcome);
        Assert.Equal(2, session.State.WallIntegrity);
        Assert.Equal(0, session.State.AttackerShotsRemaining);
        Assert.Empty(session.GetLegalActions());
    }

    [Fact]
    public void ReportText_FormatsInitialStateAndExplicitlyMarksEvidenceBoundary()
    {
        var report = CastleSiegeSessionReport.Capture(new CastleSiegeTabletopSession());

        var text = CastleSiegeSessionReportText.Format(report);

        Assert.Contains("Scenario: castle.west-wall (v1)", text);
        Assert.Contains("Rules: castle-siege-tabletop/1", text);
        Assert.Contains("Outcome: InProgress", text);
        Assert.Contains("Active role: Attacker", text);
        Assert.Contains("  - FireAtWestWall", text);
        Assert.Contains("Event history (abstract-tabletop):", text);
        Assert.Contains("(no accepted actions)", text);
        Assert.Contains("no physical trajectory, intersection, impact, or damage was calculated", text);
    }

    [Fact]
    public void ReportText_FormatsOrderedEventsWithoutAdvancingTheSession()
    {
        var session = new CastleSiegeTabletopSession();
        session.Apply(new CastleSiegeAction(CastleSiegeRole.Attacker, CastleSiegeActionKind.FireAtWestWall));
        var report = CastleSiegeSessionReport.Capture(session);
        var stateBeforeFormat = session.State;
        var eventCountBeforeFormat = session.Events.Count;

        var text = CastleSiegeSessionReportText.Format(report);

        Assert.Contains("#1 | turn 1 | Attacker | FireAtWestWall", text);
        Assert.Contains("Rule: wall.abstract-breach-point", text);
        Assert.Contains("State: wall 3 -> 2; shots 3 -> 2; repairs 2 -> 2", text);
        Assert.Contains("The tabletop attack spends one shot", text);
        Assert.Equal(stateBeforeFormat, session.State);
        Assert.Equal(eventCountBeforeFormat, session.Events.Count);
    }

    [Fact]
    public void ReportText_RejectsNullReport()
    {
        Assert.Throws<ArgumentNullException>(() => CastleSiegeSessionReportText.Format(null!));
    }


    [Fact]
    public void HallViewModel_ProjectsReportStateAndIntegrityPipsWithoutMutatingSession()
    {
        var session = new CastleSiegeTabletopSession();
        session.Apply(new CastleSiegeAction(CastleSiegeRole.Attacker, CastleSiegeActionKind.FireAtWestWall));
        var report = CastleSiegeSessionReport.Capture(session);
        var stateBeforeProjection = session.State;
        var eventCountBeforeProjection = session.Events.Count;

        var view = CastleSiegeHallViewModel.FromReport(report);

        Assert.Equal(report.ScenarioKey, view.ScenarioKey);
        Assert.Equal(report.ScenarioVersion, view.ScenarioVersion);
        Assert.Equal(report.RulesVersion, view.RulesVersion);
        Assert.Equal(report.State.TurnNumber, view.TurnNumber);
        Assert.Equal(CastleSiegeRole.Defender, view.ActiveRole);
        Assert.Equal(2, view.WallIntegrity);
        Assert.Equal(3, view.MaximumWallIntegrity);
        Assert.Equal(new[] { true, true, false }, view.WallIntegrityPips);
        Assert.Equal(report.LegalActions, view.LegalActions);
        Assert.Equal(report.Events, view.Events);
        Assert.Equal(CastleSiegeHallViewModel.TabletopEvidenceBoundary, view.EvidenceBoundary);
        Assert.Equal(stateBeforeProjection, session.State);
        Assert.Equal(eventCountBeforeProjection, session.Events.Count);
    }

    [Fact]
    public void HallViewModel_CollectionsAreReadOnlyAndRejectsNullReport()
    {
        var view = CastleSiegeHallViewModel.FromReport(
            CastleSiegeSessionReport.Capture(new CastleSiegeTabletopSession()));

        Assert.Throws<NotSupportedException>(() => ((IList<bool>)view.WallIntegrityPips).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<CastleSiegeActionKind>)view.LegalActions).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<CastleSiegeTabletopEvent>)view.Events).Clear());
        Assert.Throws<ArgumentNullException>(() => CastleSiegeHallViewModel.FromReport(null!));
    }

    private static CastleSiegeTabletopSession Run(IEnumerable<CastleSiegeAction> actions)
    {
        var session = new CastleSiegeTabletopSession();
        foreach (var action in actions)
        {
            session.Apply(action);
        }

        return session;
    }
}
