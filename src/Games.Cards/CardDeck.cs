namespace TheSingularityWorkshop.Games.Cards;

/// <summary>
/// A mutable deck and discard pile with explicit randomness injection for reproducible simulations.
/// </summary>
/// <remarks>
/// This type models cards only. It does not implement poker or blackjack rules, betting, balances,
/// UI, or casino settlement. Callers own the <see cref="Random"/> instance and its seed.
/// </remarks>
public sealed class CardDeck
{
    private readonly List<PlayingCard> _drawPile;
    private readonly List<PlayingCard> _discardPile = [];

    /// <summary>Creates a deck from the supplied card sequence, preserving its order.</summary>
    public CardDeck(IEnumerable<PlayingCard> cards)
    {
        ArgumentNullException.ThrowIfNull(cards);
        _drawPile = cards.ToList();
        if (_drawPile.Any(card => !Enum.IsDefined(card.Suit) || !Enum.IsDefined(card.Rank)))
            throw new ArgumentException("Every card must use a defined suit and rank.", nameof(cards));
    }

    /// <summary>Creates a standard 52-card deck, ordered by suit and then rank.</summary>
    public static CardDeck CreateStandard()
    {
        var cards = from suit in Enum.GetValues<CardSuit>()
                    from rank in Enum.GetValues<CardRank>()
                    select new PlayingCard(suit, rank);
        return new CardDeck(cards);
    }

    /// <summary>Gets the number of cards remaining in the draw pile.</summary>
    public int Count => _drawPile.Count;

    /// <summary>Gets the number of cards in the discard pile.</summary>
    public int DiscardCount => _discardPile.Count;

    /// <summary>Gets a read-only snapshot of the current draw-pile order.</summary>
    public IReadOnlyList<PlayingCard> Cards => _drawPile.AsReadOnly();

    /// <summary>Gets a read-only snapshot of the current discard-pile order.</summary>
    public IReadOnlyList<PlayingCard> Discards => _discardPile.AsReadOnly();

    /// <summary>Shuffles the draw pile using the supplied random source.</summary>
    public void Shuffle(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);

        for (var i = _drawPile.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (_drawPile[i], _drawPile[j]) = (_drawPile[j], _drawPile[i]);
        }
    }

    /// <summary>Draws the top card, or returns null when the draw pile is empty.</summary>
    public PlayingCard? Draw()
    {
        if (_drawPile.Count == 0)
            return null;

        var lastIndex = _drawPile.Count - 1;
        var card = _drawPile[lastIndex];
        _drawPile.RemoveAt(lastIndex);
        return card;
    }

    /// <summary>Moves a card into the discard pile.</summary>
    public void Discard(PlayingCard card) => _discardPile.Add(card);

    /// <summary>Returns every discarded card to the draw pile and clears the discard pile.</summary>
    public void CollectDiscards()
    {
        _drawPile.AddRange(_discardPile);
        _discardPile.Clear();
    }
}
