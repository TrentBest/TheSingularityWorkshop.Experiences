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
    private static CardDeck OrderedDeck(params PlayingCard[] dealOrder) => new(dealOrder.Reverse());

    [Fact]
    public void Start_deals_in_alternating_order_and_resolves_natural_blackjack()
    {
        var deck = OrderedDeck(
            new PlayingCard(CardSuit.Spades, CardRank.Ace),
            new PlayingCard(CardSuit.Clubs, CardRank.Ten),
            new PlayingCard(CardSuit.Hearts, CardRank.King),
            new PlayingCard(CardSuit.Diamonds, CardRank.Seven));
        var round = BlackjackRound.Start(deck, new BlackjackRules(), 10m, 20m);
        Assert.Equal(BlackjackRoundPhase.DealerTurn, round.Phase);
        Assert.Equal(new PlayingCard(CardSuit.Clubs, CardRank.Ten), round.DealerUpCard);

        var result = round.PlayDealerAndComplete();
        Assert.Equal(BlackjackRoundPhase.Completed, round.Phase);
        Assert.Equal(BlackjackHandOutcome.NaturalBlackjack, result.Hands[0].Outcome);
        Assert.Equal(25m, result.Hands[0].GrossReturn);
    }

    [Fact]
    public void Stand_advances_to_dealer_and_returns_win_result()
    {
        var deck = OrderedDeck(
            new PlayingCard(CardSuit.Spades, CardRank.Ten),
            new PlayingCard(CardSuit.Clubs, CardRank.Ten),
            new PlayingCard(CardSuit.Hearts, CardRank.Seven),
            new PlayingCard(CardSuit.Diamonds, CardRank.Six),
            new PlayingCard(CardSuit.Clubs, CardRank.Eight));
        var round = BlackjackRound.Start(deck, new BlackjackRules(), 5m, 10m);
        Assert.Single(round.DealerCards);
        round.Act(BlackjackAction.Stand);
        Assert.Equal(BlackjackRoundPhase.DealerTurn, round.Phase);

        var result = round.PlayDealerAndComplete();
        Assert.Equal(BlackjackHandOutcome.Win, result.Hands[0].Outcome);
        Assert.Equal(10m, result.Hands[0].GrossReturn);
    }

    [Fact]
    public void Double_down_commits_additional_wager_and_takes_exactly_one_card()
    {
        var deck = OrderedDeck(
            new PlayingCard(CardSuit.Spades, CardRank.Five),
            new PlayingCard(CardSuit.Clubs, CardRank.Ten),
            new PlayingCard(CardSuit.Hearts, CardRank.Six),
            new PlayingCard(CardSuit.Diamonds, CardRank.Seven),
            new PlayingCard(CardSuit.Clubs, CardRank.Ten));
        var round = BlackjackRound.Start(deck, new BlackjackRules(), 10m, 20m);
        round.Act(BlackjackAction.DoubleDown);
        Assert.Equal(0m, round.RemainingBalance);
        Assert.Equal(BlackjackRoundPhase.DealerTurn, round.Phase);
        var result = round.PlayDealerAndComplete();
        Assert.Equal(20m, result.Hands[0].Wager);
        Assert.Equal(BlackjackHandOutcome.Win, result.Hands[0].Outcome);
        Assert.Equal(40m, result.Hands[0].GrossReturn);
    }

    [Fact]
    public void Split_creates_two_independent_hands_and_commits_second_wager()
    {
        var deck = OrderedDeck(
            new PlayingCard(CardSuit.Spades, CardRank.Eight),
            new PlayingCard(CardSuit.Clubs, CardRank.Ten),
            new PlayingCard(CardSuit.Hearts, CardRank.Eight),
            new PlayingCard(CardSuit.Diamonds, CardRank.Seven),
            new PlayingCard(CardSuit.Clubs, CardRank.Three),
            new PlayingCard(CardSuit.Diamonds, CardRank.Two));
        var round = BlackjackRound.Start(deck, new BlackjackRules(maximumHands: 2), 10m, 30m);
        round.Act(BlackjackAction.Split);
        Assert.Equal(2, round.PlayerHands.Count);
        Assert.Equal(10m, round.RemainingBalance);
        Assert.Contains(BlackjackAction.Stand, round.LegalActions);

        round.Act(BlackjackAction.Stand);
        Assert.Equal(1, round.ActiveHandIndex);
        round.Act(BlackjackAction.Stand);
        Assert.Equal(BlackjackRoundPhase.DealerTurn, round.Phase);
        var result = round.PlayDealerAndComplete();
        Assert.Equal(2, result.Hands.Count);
        Assert.All(result.Hands, hand => Assert.Equal(10m, hand.Wager));
    }

    [Fact]
    public void Split_aces_receive_one_card_each_and_cannot_act_again()
    {
        var deck = OrderedDeck(
            new PlayingCard(CardSuit.Spades, CardRank.Ace),
            new PlayingCard(CardSuit.Clubs, CardRank.Ten),
            new PlayingCard(CardSuit.Hearts, CardRank.Ace),
            new PlayingCard(CardSuit.Diamonds, CardRank.Seven),
            new PlayingCard(CardSuit.Clubs, CardRank.Nine),
            new PlayingCard(CardSuit.Diamonds, CardRank.Eight));
        var round = BlackjackRound.Start(deck, new BlackjackRules(maximumHands: 2), 10m, 30m);
        round.Act(BlackjackAction.Split);
        Assert.Equal(BlackjackRoundPhase.DealerTurn, round.Phase);
        Assert.Empty(round.LegalActions);
        Assert.False(BlackjackRules.Evaluate(round.PlayerHands[0], isSplitHand: true).IsNatural);
        Assert.False(BlackjackRules.Evaluate(round.PlayerHands[1], isSplitHand: true).IsNatural);
    }

    [Fact]
    public void Split_ace_and_ten_value_hand_is_21_but_not_natural_and_auto_completes()
    {
        var deck = OrderedDeck(
            new PlayingCard(CardSuit.Spades, CardRank.Ace),
            new PlayingCard(CardSuit.Clubs, CardRank.Ten),
            new PlayingCard(CardSuit.Hearts, CardRank.Ace),
            new PlayingCard(CardSuit.Diamonds, CardRank.Seven),
            new PlayingCard(CardSuit.Clubs, CardRank.King),
            new PlayingCard(CardSuit.Diamonds, CardRank.Eight));
        var round = BlackjackRound.Start(deck, new BlackjackRules(maximumHands: 2), 10m, 30m);
        round.Act(BlackjackAction.Split);
        Assert.Equal(21, BlackjackRules.Evaluate(round.PlayerHands[0], isSplitHand: true).Total);
        Assert.False(BlackjackRules.Evaluate(round.PlayerHands[0], isSplitHand: true).IsNatural);
        Assert.Equal(BlackjackRoundPhase.DealerTurn, round.Phase);
    }

    [Fact]
    public void Dealer_does_not_draw_when_every_player_hand_has_busted()
    {
        var deck = OrderedDeck(
            new PlayingCard(CardSuit.Spades, CardRank.Ten),
            new PlayingCard(CardSuit.Clubs, CardRank.Five),
            new PlayingCard(CardSuit.Hearts, CardRank.Eight),
            new PlayingCard(CardSuit.Diamonds, CardRank.Six),
            new PlayingCard(CardSuit.Clubs, CardRank.Ten));
        var round = BlackjackRound.Start(deck, new BlackjackRules(), 5m, 10m);
        round.Act(BlackjackAction.Hit);
        var result = round.PlayDealerAndComplete();
        Assert.Equal(11, result.DealerValue.Total);
        Assert.Equal(BlackjackHandOutcome.Bust, result.Hands[0].Outcome);
    }

}
