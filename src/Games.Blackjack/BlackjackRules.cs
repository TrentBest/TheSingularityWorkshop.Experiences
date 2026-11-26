using TheSingularityWorkshop.Games.Cards;

namespace TheSingularityWorkshop.Games.Blackjack;

/// <summary>Computed value of a Blackjack hand.</summary>
/// <param name="Total">The best non-busting total, or the lowest total when bust.</param>
/// <param name="IsSoft">Whether an ace is currently counted as eleven.</param>
/// <param name="IsBust">Whether the hand exceeds 21.</param>
/// <param name="IsNatural">Whether the original two-card hand is an ace plus a ten-value card.</param>
public readonly record struct BlackjackHandValue(int Total, bool IsSoft, bool IsBust, bool IsNatural);

/// <summary>Actions that can be offered by the first Blackjack rules slice.</summary>
public enum BlackjackAction
{
    /// <summary>Take one additional card.</summary>
    Hit,
    /// <summary>Keep the current hand total.</summary>
    Stand,
    /// <summary>Double the wager and take exactly one additional card.</summary>
    DoubleDown,
    /// <summary>Split an eligible initial pair into two hands.</summary>
    Split
}

/// <summary>Deterministic Blackjack scoring and table-policy helpers.</summary>
/// <remarks>
/// This library consumes supplied cards and state. It does not shuffle, draw, animate, hold player funds,
/// settle wagers, or access a host/runtime. A round coordinator can compose these pure rules with Cards
/// and an explicitly authorized virtual ledger operation.
/// </remarks>
public sealed class BlackjackRules
{
    /// <summary>Creates a rule policy.</summary>
    public BlackjackRules(bool dealerHitsSoft17 = true, decimal blackjackPayout = 1.5m,
        bool allowDoubleAfterSplit = true, int maximumHands = 4)
    {
        if (blackjackPayout < 0m) throw new ArgumentOutOfRangeException(nameof(blackjackPayout));
        if (maximumHands < 1) throw new ArgumentOutOfRangeException(nameof(maximumHands));
        DealerHitsSoft17 = dealerHitsSoft17;
        BlackjackPayout = blackjackPayout;
        AllowDoubleAfterSplit = allowDoubleAfterSplit;
        MaximumHands = maximumHands;
    }

    /// <summary>Gets whether the dealer hits soft 17.</summary>
    public bool DealerHitsSoft17 { get; }
    /// <summary>Gets the natural Blackjack net-profit multiplier.</summary>
    public decimal BlackjackPayout { get; }
    /// <summary>Gets whether doubling is allowed after a split.</summary>
    public bool AllowDoubleAfterSplit { get; }
    /// <summary>Gets the maximum number of player hands in one round.</summary>
    public int MaximumHands { get; }

    /// <summary>Evaluates a supplied hand, counting aces as eleven when that does not bust.</summary>
    /// <param name="cards">Cards currently held in the hand.</param>
    /// <param name="isSplitHand">Whether this hand originated from a split; split hands cannot receive natural Blackjack status.</param>
    public static BlackjackHandValue Evaluate(IEnumerable<PlayingCard> cards, bool isSplitHand = false)
    {
        ArgumentNullException.ThrowIfNull(cards);
        var hand = cards.ToArray();
        if (hand.Any(card => !Enum.IsDefined(card.Suit) || !Enum.IsDefined(card.Rank)))
            throw new ArgumentException("Every card must have a defined suit and rank.", nameof(cards));

        var total = 0;
        var aces = 0;
        foreach (var card in hand)
        {
            if (card.Rank == CardRank.Ace) { total += 11; aces++; }
            else total += card.Rank switch
            {
                CardRank.Jack or CardRank.Queen or CardRank.King => 10,
                _ => (int)card.Rank
            };
        }

        while (total > 21 && aces > 0) { total -= 10; aces--; }
        var isBust = total > 21;
        var isNatural = !isSplitHand && hand.Length == 2 && total == 21 && hand.Any(card => card.Rank == CardRank.Ace);
        return new BlackjackHandValue(total, aces > 0, isBust, isNatural);
    }

    /// <summary>Determines whether the dealer must take another card under this policy.</summary>
    public bool DealerMustHit(IEnumerable<PlayingCard> dealerCards)
    {
        var value = Evaluate(dealerCards);
        return !value.IsBust && (value.Total < 17 || (value.Total == 17 && value.IsSoft && DealerHitsSoft17));
    }

    /// <summary>Returns legal actions from the supplied hand and available virtual balance.</summary>
    public IReadOnlyList<BlackjackAction> GetLegalActions(IReadOnlyList<PlayingCard> cards, decimal wager,
        decimal availableBalance, bool isFirstDecision = true, bool isSplitHand = false, int currentHandCount = 1)
    {
        ArgumentNullException.ThrowIfNull(cards);
        if (wager <= 0m) throw new ArgumentOutOfRangeException(nameof(wager));
        if (availableBalance < 0m) throw new ArgumentOutOfRangeException(nameof(availableBalance));
        if (currentHandCount < 1) throw new ArgumentOutOfRangeException(nameof(currentHandCount));

        var value = Evaluate(cards);
        if (value.IsBust || value.Total == 21 || cards.Count == 0) return Array.Empty<BlackjackAction>();

        var actions = new List<BlackjackAction> { BlackjackAction.Hit, BlackjackAction.Stand };
        if (isFirstDecision && availableBalance >= wager && (!isSplitHand || AllowDoubleAfterSplit))
            actions.Add(BlackjackAction.DoubleDown);
        if (isFirstDecision && cards.Count == 2 && cards[0].Rank == cards[1].Rank &&
            currentHandCount < MaximumHands && availableBalance >= wager)
            actions.Add(BlackjackAction.Split);
        return actions.AsReadOnly();
    }
}
