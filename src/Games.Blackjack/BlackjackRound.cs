using TheSingularityWorkshop.Games.Cards;

namespace TheSingularityWorkshop.Games.Blackjack;

/// <summary>The lifecycle phase of a Blackjack round.</summary>
public enum BlackjackRoundPhase
{
    /// <summary>One or more player hands can still act.</summary>
    PlayerTurn,
    /// <summary>Player hands are complete and the dealer must resolve.</summary>
    DealerTurn,
    /// <summary>All hands have final outcomes.</summary>
    Completed
}

/// <summary>Why additional virtual wager funds were committed during a round.</summary>
public enum BlackjackWagerCommitmentKind
{
    /// <summary>The initial wager placed to start the round.</summary>
    Initial,
    /// <summary>The additional wager required to create a split hand.</summary>
    Split,
    /// <summary>The additional wager required to double down.</summary>
    DoubleDown
}

/// <summary>An incremental virtual wager commitment that the host must escrow separately.</summary>
/// <param name="Sequence">Monotonically increasing commitment sequence within the round.</param>
/// <param name="HandId">Stable player hand identity receiving this wager amount.</param>
/// <param name="Amount">Incremental amount to escrow, not the hand's cumulative wager.</param>
/// <param name="Kind">Action that caused the commitment.</param>
public sealed record BlackjackWagerCommitment(
    int Sequence,
    int HandId,
    decimal Amount,
    BlackjackWagerCommitmentKind Kind);

/// <summary>The outcome of one player hand compared with the dealer.</summary>
public enum BlackjackHandOutcome
{
    /// <summary>The hand lost its wager.</summary>
    Loss,
    /// <summary>The hand exceeded 21.</summary>
    Bust,
    /// <summary>The hand tied the dealer.</summary>
    Push,
    /// <summary>The hand beat the dealer without a natural Blackjack.</summary>
    Win,
    /// <summary>The original two-card hand won with natural Blackjack.</summary>
    NaturalBlackjack
}

/// <summary>An immutable settlement-ready result for one hand; it does not post ledger entries.</summary>
/// <param name="Cards">Final cards held by the hand.</param>
/// <param name="HandId">Stable identity of the hand within this round.</param>
/// <param name="Value">Final hand value.</param>
/// <param name="Wager">Total virtual wager committed to this hand.</param>
/// <param name="Outcome">Comparison outcome.</param>
/// <param name="GrossReturn">Total virtual chips returned, including returned stake.</param>
public sealed record BlackjackHandResult(
    IReadOnlyList<PlayingCard> Cards,
    int HandId,
    BlackjackHandValue Value,
    decimal Wager,
    BlackjackHandOutcome Outcome,
    decimal GrossReturn);

/// <summary>A completed round result, including the dealer's final hand.</summary>
/// <param name="DealerCards">Dealer's final cards.</param>
/// <param name="DealerValue">Dealer's final value.</param>
/// <param name="Hands">Final results in player-hand order.</param>
public sealed record BlackjackRoundResult(
    IReadOnlyList<PlayingCard> DealerCards,
    BlackjackHandValue DealerValue,
    IReadOnlyList<BlackjackHandResult> Hands);

/// <summary>
/// Coordinates a single deterministic Blackjack round using a caller-owned card shoe.
/// </summary>
/// <remarks>
/// The caller controls shuffle/randomness and supplies a deck. This session mutates that deck by drawing.
/// It is not thread-safe and does not own player balances, post wagers, settle the ledger, or provide UI.
/// A round is deterministic for the same initial deck order, rules, and player actions.
/// </remarks>
public sealed class BlackjackRound
{
    private sealed class HandState(int id, List<PlayingCard> cards, decimal wager, bool isSplit)
    {
        public int Id { get; } = id;
        public List<PlayingCard> Cards { get; } = cards;
        public decimal Wager { get; set; } = wager;
        public bool IsSplit { get; set; } = isSplit;
        public bool HasActed { get; set; }
        public bool IsComplete { get; set; }
    }

    private readonly CardDeck _deck;
    private readonly BlackjackRules _rules;
    private readonly List<PlayingCard> _dealer = [];
    private readonly List<HandState> _hands = [];
    private readonly List<BlackjackWagerCommitment> _wagerCommitments = [];
    private decimal _remainingBalance;
    private int _activeHandIndex;
    private int _nextHandId = 1;

    private BlackjackRound(CardDeck deck, BlackjackRules rules, decimal wager, decimal availableBalance)
    {
        _deck = deck;
        _rules = rules;
        _remainingBalance = availableBalance - wager;
        InitialWager = wager;

        var playerCards = new List<PlayingCard> { DrawRequired() };
        _dealer.Add(DrawRequired());
        playerCards.Add(DrawRequired());
        _dealer.Add(DrawRequired());
        _hands.Add(new HandState(0, playerCards, wager, false));
        _wagerCommitments.Add(new BlackjackWagerCommitment(0, 0, wager, BlackjackWagerCommitmentKind.Initial));
        Phase = BlackjackRoundPhase.PlayerTurn;

        ResolveOpeningNaturals();
        AdvanceCompletedHands();
    }

    /// <summary>Starts a round and deals two cards to the player and dealer.</summary>
    public static BlackjackRound Start(CardDeck deck, BlackjackRules rules, decimal wager, decimal availableBalance)
    {
        ArgumentNullException.ThrowIfNull(deck);
        ArgumentNullException.ThrowIfNull(rules);
        if (wager <= 0m) throw new ArgumentOutOfRangeException(nameof(wager));
        if (availableBalance < wager) throw new ArgumentOutOfRangeException(nameof(availableBalance));
        if (deck.Count < 4) throw new ArgumentException("At least four cards are required to start a round.", nameof(deck));
        return new BlackjackRound(deck, rules, wager, availableBalance);
    }

    /// <summary>Gets the original wager.</summary>
    public decimal InitialWager { get; }
    /// <summary>Gets the current phase.</summary>
    public BlackjackRoundPhase Phase { get; private set; }
    /// <summary>Gets the balance remaining after committed wagers.</summary>
    public decimal RemainingBalance => _remainingBalance;
    /// <summary>Gets the dealer's visible up-card.</summary>
    public PlayingCard DealerUpCard => _dealer[0];
    /// <summary>Gets a snapshot of dealer cards currently dealt.</summary>
    public IReadOnlyList<PlayingCard> DealerCards => Array.AsReadOnly((Phase == BlackjackRoundPhase.PlayerTurn ? _dealer.Take(1) : _dealer).ToArray());
    /// <summary>Gets the current player hand index.</summary>
    public int ActiveHandIndex => _activeHandIndex;
    /// <summary>Gets every incremental wager commitment in the order it was created.</summary>
    public IReadOnlyList<BlackjackWagerCommitment> WagerCommitments => Array.AsReadOnly(_wagerCommitments.ToArray());
    /// <summary>Gets snapshots of all player hands.</summary>
    public IReadOnlyList<IReadOnlyList<PlayingCard>> PlayerHands =>
        Array.AsReadOnly(_hands.Select(hand => (IReadOnlyList<PlayingCard>)Array.AsReadOnly(hand.Cards.ToArray())).ToArray());

    /// <summary>Gets the legal actions for the active hand.</summary>
    public IReadOnlyList<BlackjackAction> LegalActions
    {
        get
        {
            if (Phase != BlackjackRoundPhase.PlayerTurn || _activeHandIndex >= _hands.Count)
                return Array.Empty<BlackjackAction>();
            var hand = _hands[_activeHandIndex];
            if (hand.IsComplete) return Array.Empty<BlackjackAction>();
            return _rules.GetLegalActions(hand.Cards, hand.Wager, _remainingBalance,
                isFirstDecision: !hand.HasActed, isSplitHand: hand.IsSplit, currentHandCount: _hands.Count);
        }
    }

    /// <summary>Applies a legal player action and advances the round when appropriate.</summary>
    public void Act(BlackjackAction action)
    {
        if (Phase != BlackjackRoundPhase.PlayerTurn)
            throw new InvalidOperationException("The round is not accepting player actions.");
        if (!LegalActions.Contains(action))
            throw new InvalidOperationException($"Action {action} is not legal for the active hand.");

        if ((action is BlackjackAction.Hit or BlackjackAction.DoubleDown) && _deck.Count < 1)
            throw new InvalidOperationException("The card shoe is empty.");
        if (action == BlackjackAction.Split && _deck.Count < 2)
            throw new InvalidOperationException("At least two cards must remain to split.");

        var hand = _hands[_activeHandIndex];
        switch (action)
        {
            case BlackjackAction.Hit:
                hand.Cards.Add(DrawRequired());
                hand.HasActed = true;
                if (BlackjackRules.Evaluate(hand.Cards, hand.IsSplit).IsBust ||
                    BlackjackRules.Evaluate(hand.Cards, hand.IsSplit).Total == 21)
                    hand.IsComplete = true;
                break;
            case BlackjackAction.Stand:
                hand.HasActed = true;
                hand.IsComplete = true;
                break;
            case BlackjackAction.DoubleDown:
                _remainingBalance -= hand.Wager;
                _wagerCommitments.Add(new BlackjackWagerCommitment(_wagerCommitments.Count, hand.Id, hand.Wager, BlackjackWagerCommitmentKind.DoubleDown));
                hand.Wager *= 2m;
                hand.Cards.Add(DrawRequired());
                hand.HasActed = true;
                hand.IsComplete = true;
                break;
            case BlackjackAction.Split:
                SplitActiveHand(hand);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }

        AdvanceCompletedHands();
    }

    /// <summary>Runs the dealer's turn and returns final immutable hand outcomes.</summary>
    public BlackjackRoundResult PlayDealerAndComplete()
    {
        if (Phase != BlackjackRoundPhase.DealerTurn)
            throw new InvalidOperationException("The dealer can act only after all player hands finish.");
        var dealerNeedsToPlay = _hands.Any(hand =>
        {
            var value = BlackjackRules.Evaluate(hand.Cards, hand.IsSplit);
            return !value.IsBust && !value.IsNatural && value.Total < 21;
        });
        while (dealerNeedsToPlay && _rules.DealerMustHit(_dealer))
            _dealer.Add(DrawRequired());

        var dealerValue = BlackjackRules.Evaluate(_dealer);
        var results = _hands.Select(hand => CreateResult(hand, dealerValue)).ToArray();
        Phase = BlackjackRoundPhase.Completed;
        return new BlackjackRoundResult(Array.AsReadOnly(_dealer.ToArray()), dealerValue, Array.AsReadOnly(results));
    }

    private void SplitActiveHand(HandState original)
    {
        var first = original.Cards[0];
        var second = original.Cards[1];
        var splitAces = first.Rank == CardRank.Ace;
        _remainingBalance -= original.Wager;
        original.Cards.Clear();
        original.Cards.Add(first);
        original.Cards.Add(DrawRequired());
        original.HasActed = false;
        original.IsSplit = true;
        original.IsComplete = splitAces || BlackjackRules.Evaluate(original.Cards, isSplitHand: true).Total >= 21;

        var secondCards = new List<PlayingCard> { second, DrawRequired() };
        var secondValue = BlackjackRules.Evaluate(secondCards, isSplitHand: true);
        var secondHand = new HandState(_nextHandId++, secondCards, original.Wager, true)
        {
            IsComplete = splitAces || secondValue.Total >= 21
        };
        _hands.Insert(_activeHandIndex + 1, secondHand);
        _wagerCommitments.Add(new BlackjackWagerCommitment(_wagerCommitments.Count, secondHand.Id, original.Wager, BlackjackWagerCommitmentKind.Split));
    }

    private void ResolveOpeningNaturals()
    {
        var dealerNatural = BlackjackRules.Evaluate(_dealer).IsNatural;
        var playerNatural = BlackjackRules.Evaluate(_hands[0].Cards).IsNatural;
        if (dealerNatural || playerNatural)
        {
            _hands[0].IsComplete = true;
            Phase = BlackjackRoundPhase.DealerTurn;
        }
    }

    private void AdvanceCompletedHands()
    {
        while (_activeHandIndex < _hands.Count && _hands[_activeHandIndex].IsComplete)
            _activeHandIndex++;
        if (_activeHandIndex >= _hands.Count)
            Phase = BlackjackRoundPhase.DealerTurn;
    }

    private BlackjackHandResult CreateResult(HandState hand, BlackjackHandValue dealerValue)
    {
        var value = BlackjackRules.Evaluate(hand.Cards, hand.IsSplit);
        BlackjackHandOutcome outcome;
        decimal gross;
        if (value.IsBust)
        {
            outcome = BlackjackHandOutcome.Bust;
            gross = 0m;
        }
        else if (dealerValue.IsNatural)
        {
            outcome = value.IsNatural ? BlackjackHandOutcome.Push : BlackjackHandOutcome.Loss;
            gross = value.IsNatural ? hand.Wager : 0m;
        }
        else if (value.IsNatural)
        {
            outcome = BlackjackHandOutcome.NaturalBlackjack;
            gross = hand.Wager * (1m + _rules.BlackjackPayout);
        }
        else if (dealerValue.IsBust || value.Total > dealerValue.Total)
        {
            outcome = BlackjackHandOutcome.Win;
            gross = hand.Wager * 2m;
        }
        else if (value.Total == dealerValue.Total)
        {
            outcome = BlackjackHandOutcome.Push;
            gross = hand.Wager;
        }
        else
        {
            outcome = BlackjackHandOutcome.Loss;
            gross = 0m;
        }

        return new BlackjackHandResult(Array.AsReadOnly(hand.Cards.ToArray()), hand.Id, value, hand.Wager, outcome, gross);
    }

    private PlayingCard DrawRequired() =>
        _deck.Draw() ?? throw new InvalidOperationException("The card shoe is empty before the round is complete.");
}
