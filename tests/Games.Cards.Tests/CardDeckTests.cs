using TheSingularityWorkshop.Games.Cards;
using Xunit;

namespace TheSingularityWorkshop.Games.Cards.Tests;

public sealed class CardDeckTests
{
    [Fact]
    public void CreateStandard_ContainsAll52UniqueCards()
    {
        var deck = CardDeck.CreateStandard();

        Assert.Equal(52, deck.Count);
        Assert.Equal(52, deck.Cards.Distinct().Count());
        Assert.Equal(13, deck.Cards.Count(card => card.Suit == CardSuit.Clubs));
        Assert.Equal(13, deck.Cards.Count(card => card.Suit == CardSuit.Diamonds));
        Assert.Equal(13, deck.Cards.Count(card => card.Suit == CardSuit.Hearts));
        Assert.Equal(13, deck.Cards.Count(card => card.Suit == CardSuit.Spades));
    }

    [Fact]
    public void Shuffle_WithSameSeedProducesSameOrder()
    {
        var first = CardDeck.CreateStandard();
        var second = CardDeck.CreateStandard();

        first.Shuffle(new Random(12345));
        second.Shuffle(new Random(12345));

        Assert.Equal(first.Cards, second.Cards);
    }

    [Fact]
    public void Draw_ExhaustsDeckAndThenReturnsNull()
    {
        var deck = CardDeck.CreateStandard();
        var drawn = Enumerable.Range(0, 52).Select(_ => deck.Draw()).ToArray();

        Assert.All(drawn, card => Assert.True(card.HasValue));
        Assert.Equal(52, drawn.Select(card => card!.Value).Distinct().Count());
        Assert.Null(deck.Draw());
        Assert.Equal(0, deck.Count);
    }

    [Fact]
    public void CollectDiscards_ReturnsCardsToDrawPileAndEmptiesDiscardPile()
    {
        var deck = CardDeck.CreateStandard();
        var card = deck.Draw();

        Assert.NotNull(card);
        deck.Discard(card!.Value);

        Assert.Equal(51, deck.Count);
        Assert.Equal(1, deck.DiscardCount);

        deck.CollectDiscards();

        Assert.Equal(52, deck.Count);
        Assert.Equal(0, deck.DiscardCount);
        Assert.Contains(card.Value, deck.Cards);
    }

    [Fact]
    public void CardColor_IsDerivedFromSuit()
    {
        Assert.True(new PlayingCard(CardSuit.Hearts, CardRank.Ace).IsRed);
        Assert.True(new PlayingCard(CardSuit.Diamonds, CardRank.Two).IsRed);
        Assert.True(new PlayingCard(CardSuit.Clubs, CardRank.Ace).IsBlack);
        Assert.True(new PlayingCard(CardSuit.Spades, CardRank.Two).IsBlack);
    }

    [Fact]
    public void CardsProperty_ReturnsSnapshotRatherThanLivePileView()
    {
        var deck = CardDeck.CreateStandard();
        var snapshot = deck.Cards;

        deck.Draw();

        Assert.Equal(52, snapshot.Count);
        Assert.Equal(51, deck.Count);
    }
}
