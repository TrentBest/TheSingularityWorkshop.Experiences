using TheSingularityWorkshop.Games.Blackjack;
using TheSingularityWorkshop.Games.Cards;
using Xunit;

namespace TheSingularityWorkshop.Games.Blackjack.Tests;

public sealed class BlackjackRulesTests
{
    [Fact]
    public void Evaluate_softens_aces_until_hand_no_longer_busts()
    {
        var value = BlackjackRules.Evaluate([
            new PlayingCard(CardSuit.Spades, CardRank.Ace),
            new PlayingCard(CardSuit.Hearts, CardRank.Ace),
            new PlayingCard(CardSuit.Clubs, CardRank.Nine)]);
        Assert.Equal(21, value.Total);
        Assert.True(value.IsSoft);
        Assert.False(value.IsBust);
        Assert.False(value.IsNatural);
    }

    [Fact]
    public void Evaluate_counts_face_cards_and_detects_bust()
    {
        var value = BlackjackRules.Evaluate([
            new PlayingCard(CardSuit.Spades, CardRank.King),
            new PlayingCard(CardSuit.Hearts, CardRank.Queen),
            new PlayingCard(CardSuit.Clubs, CardRank.Five)]);
        Assert.Equal(25, value.Total);
        Assert.True(value.IsBust);
        Assert.False(value.IsSoft);
    }

    [Fact]
    public void Evaluate_detects_two_card_natural()
    {
        var value = BlackjackRules.Evaluate([
            new PlayingCard(CardSuit.Spades, CardRank.Ace),
            new PlayingCard(CardSuit.Hearts, CardRank.King)]);
        Assert.Equal(21, value.Total);
        Assert.True(value.IsNatural);
    }

    [Fact]
    public void Evaluate_does_not_classify_a_split_ace_and_ten_as_natural()
    {
        var value = BlackjackRules.Evaluate([
            new PlayingCard(CardSuit.Spades, CardRank.Ace),
            new PlayingCard(CardSuit.Hearts, CardRank.King)], isSplitHand: true);
        Assert.Equal(21, value.Total);
        Assert.False(value.IsNatural);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void DealerMustHit_respects_soft_seventeen_policy(bool hitsSoft17, bool expected)
    {
        var rules = new BlackjackRules(dealerHitsSoft17: hitsSoft17);
        var dealer = new[] { new PlayingCard(CardSuit.Spades, CardRank.Ace), new PlayingCard(CardSuit.Hearts, CardRank.Six) };
        Assert.Equal(expected, rules.DealerMustHit(dealer));
    }

    [Fact]
    public void GetLegalActions_offers_split_and_double_when_eligible_and_funded()
    {
        var rules = new BlackjackRules(maximumHands: 2);
        var pair = new[] { new PlayingCard(CardSuit.Spades, CardRank.Eight), new PlayingCard(CardSuit.Hearts, CardRank.Eight) };
        var actions = rules.GetLegalActions(pair, 10m, 10m);
        Assert.Contains(BlackjackAction.Hit, actions);
        Assert.Contains(BlackjackAction.Stand, actions);
        Assert.Contains(BlackjackAction.DoubleDown, actions);
        Assert.Contains(BlackjackAction.Split, actions);
    }

    [Fact]
    public void GetLegalActions_does_not_offer_funded_actions_without_balance()
    {
        var rules = new BlackjackRules();
        var pair = new[] { new PlayingCard(CardSuit.Spades, CardRank.Eight), new PlayingCard(CardSuit.Hearts, CardRank.Eight) };
        var actions = rules.GetLegalActions(pair, 10m, 0m);
        Assert.DoesNotContain(BlackjackAction.DoubleDown, actions);
        Assert.DoesNotContain(BlackjackAction.Split, actions);
    }

    [Fact]
    public void GetLegalActions_returns_no_actions_for_bust_or_twenty_one()
    {
        var rules = new BlackjackRules();
        var bust = new[] { new PlayingCard(CardSuit.Spades, CardRank.King), new PlayingCard(CardSuit.Hearts, CardRank.Eight), new PlayingCard(CardSuit.Clubs, CardRank.Five) };
        var twentyOne = new[] { new PlayingCard(CardSuit.Spades, CardRank.King), new PlayingCard(CardSuit.Hearts, CardRank.Ace) };
        Assert.Empty(rules.GetLegalActions(bust, 5m, 100m));
        Assert.Empty(rules.GetLegalActions(twentyOne, 5m, 100m));
    }
}
