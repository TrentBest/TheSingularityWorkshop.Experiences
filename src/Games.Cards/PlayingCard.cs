namespace TheSingularityWorkshop.Games.Cards;

/// <summary>The four suits in a standard 52-card French-suited deck.</summary>
public enum CardSuit
{
    Clubs,
    Diamonds,
    Hearts,
    Spades
}

/// <summary>Ranks in a standard playing-card deck, ordered from low to high.</summary>
public enum CardRank
{
    Two = 2,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,
    Queen,
    King,
    Ace
}

/// <summary>An immutable playing card identified by suit and rank.</summary>
public readonly record struct PlayingCard(CardSuit Suit, CardRank Rank)
{
    /// <summary>Gets whether this card belongs to a red suit.</summary>
    public bool IsRed => Suit is CardSuit.Diamonds or CardSuit.Hearts;

    /// <summary>Gets whether this card belongs to a black suit.</summary>
    public bool IsBlack => !IsRed;

    /// <inheritdoc />
    public override string ToString() => $"{Rank} of {Suit}";
}
