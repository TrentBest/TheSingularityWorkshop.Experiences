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
    /// <param name="cards">The initial draw-pile contents. Repeated valid cards are permitted for multi-deck shoes.</param>
    /// <exception cref="ArgumentNullException"><paramref name="cards"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">A card has an undefined suit or rank.</exception>
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
    public IReadOnlyList<PlayingCard> Cards => Array.AsReadOnly(_drawPile.ToArray());

    /// <summary>Gets a read-only snapshot of the current discard-pile order.</summary>
    public IReadOnlyList<PlayingCard> Discards => Array.AsReadOnly(_discardPile.ToArray());

    /// <summary>Shuffles the draw pile using the supplied random source.</summary>
    /// <param name="random">The random source used to select each swap index.</param>
    /// <exception cref="ArgumentNullException"><paramref name="random"/> is <see langword="null"/>.</exception>
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

    /// <summary>
    /// Adds a valid card to the discard pile.
    /// </summary>
    /// <remarks>
    /// This method does not track card ownership or prevent duplicates. The caller owns those rules,
    /// which allows a game to discard cards held outside this draw pile or to model multi-deck shoes.
    /// </remarks>
    /// <param name="card">The card to discard.</param>
    /// <exception cref="ArgumentException">The card has an undefined suit or rank.</exception>
    public void Discard(PlayingCard card)
    {
        if (!Enum.IsDefined(card.Suit) || !Enum.IsDefined(card.Rank))
            throw new ArgumentException("A discarded card must use a defined suit and rank.", nameof(card));

        _discardPile.Add(card);
    }

    /// <summary>Returns every discarded card to the draw pile and clears the discard pile.</summary>
    public void CollectDiscards()
    {
        _drawPile.AddRange(_discardPile);
        _discardPile.Clear();
    }
}
