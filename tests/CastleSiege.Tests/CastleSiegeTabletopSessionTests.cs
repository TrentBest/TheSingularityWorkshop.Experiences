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
        Assert.Equal(CastleSiegeRole.Defender, session.State.ActiveRole);

        Assert.Equal(2, defend.Sequence);
        Assert.Equal(2, defend.TurnNumber);
        Assert.Equal("wall.abstract-reinforcement", defend.RuleId);
        Assert.Equal(3, defend.StateAfter.WallIntegrity);
        Assert.Equal(CastleSiegeRole.Attacker, session.State.ActiveRole);
        Assert.Equal(2, session.Events.Count);
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
