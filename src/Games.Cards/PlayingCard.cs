namespace TheSingularityWorkshop.Games.Cards;

/// <summary>The four suits in a standard 52-card French-suited deck.</summary>
public enum CardSuit
{
    /// <summary>Clubs, a black suit.</summary>
    Clubs,

    /// <summary>Diamonds, a red suit.</summary>
    Diamonds,

    /// <summary>Hearts, a red suit.</summary>
    Hearts,

    /// <summary>Spades, a black suit.</summary>
    Spades
}

/// <summary>Ranks in a standard playing-card deck, ordered from low to high.</summary>
public enum CardRank
{
    /// <summary>Rank two.</summary>
    Two = 2,

    /// <summary>Rank three.</summary>
    Three,

    /// <summary>Rank four.</summary>
    Four,

    /// <summary>Rank five.</summary>
    Five,

    /// <summary>Rank six.</summary>
    Six,

    /// <summary>Rank seven.</summary>
    Seven,

    /// <summary>Rank eight.</summary>
    Eight,

    /// <summary>Rank nine.</summary>
    Nine,

    /// <summary>Rank ten.</summary>
    Ten,

    /// <summary>Jack.</summary>
    Jack,

    /// <summary>Queen.</summary>
    Queen,

    /// <summary>King.</summary>
    King,

    /// <summary>Ace; game rules determine its scoring value.</summary>
    Ace
}

/// <summary>An immutable playing card identified by suit and rank.</summary>
/// <param name="Suit">The card's suit.</param>
/// <param name="Rank">The card's rank.</param>
/// <remarks>This type represents identity only; game-specific scoring belongs to the game rules.</remarks>
public readonly record struct PlayingCard(CardSuit Suit, CardRank Rank)
{
    /// <summary>Gets whether this card belongs to a red suit.</summary>
    public bool IsRed => Suit is CardSuit.Diamonds or CardSuit.Hearts;

    /// <summary>Gets whether this card belongs to a black suit.</summary>
    /// <remarks>Returns <see langword="false"/> for an undefined suit value.</remarks>
    public bool IsBlack => Suit is CardSuit.Clubs or CardSuit.Spades;

    /// <inheritdoc />
    public override string ToString() => $"{Rank} of {Suit}";
}
