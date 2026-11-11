namespace TheSingularityWorkshop.Games.Slots;

/// <summary>
/// Describes a single-line slot machine with independent, cyclic reels and
/// a paytable for matching the same symbol on every reel.
/// </summary>
/// <remarks>
/// This deliberately small first slice does not model paylines, wild symbols,
/// scatter symbols, bonus rounds, animation, or wagering accounts.
/// </remarks>
public sealed class SlotMachineDefinition
{
    private readonly string[][] _reels;
    private readonly IReadOnlyList<IReadOnlyList<string>> _reelViews;
    private readonly IReadOnlyDictionary<string, decimal> _payoutMultipliers;

    /// <summary>
    /// Creates a slot-machine definition from reel strips and a total-return paytable.
    /// </summary>
    /// <param name="reels">At least one non-empty reel strip. Repeated symbols are meaningful weights.</param>
    /// <param name="payoutMultipliers">
    /// Total payout multipliers keyed by symbol. A multiplier of 5 returns five times the stake,
    /// including the stake; it is not a profit-only multiplier. Omitted symbols lose.
    /// </param>
    public SlotMachineDefinition(
        IEnumerable<IEnumerable<string>> reels,
        IReadOnlyDictionary<string, decimal> payoutMultipliers)
    {
        ArgumentNullException.ThrowIfNull(reels);
        ArgumentNullException.ThrowIfNull(payoutMultipliers);

        _reels = reels.Select((reel, index) =>
        {
            ArgumentNullException.ThrowIfNull(reel);
            var symbols = reel.ToArray();
            if (symbols.Length == 0)
                throw new ArgumentException($"Reel {index} must contain at least one stop.", nameof(reels));

            for (var stop = 0; stop < symbols.Length; stop++)
            {
                if (string.IsNullOrWhiteSpace(symbols[stop]))
                    throw new ArgumentException($"Reel {index}, stop {stop} has an empty symbol.", nameof(reels));
            }

            return symbols;
        }).ToArray();

        if (_reels.Length == 0)
            throw new ArgumentException("At least one reel is required.", nameof(reels));

        var copiedPaytable = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var (symbol, multiplier) in payoutMultipliers)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
            if (multiplier < 0m)
                throw new ArgumentOutOfRangeException(nameof(payoutMultipliers), $"Multiplier for '{symbol}' cannot be negative.");
            copiedPaytable.Add(symbol, multiplier);
        }

        _payoutMultipliers = copiedPaytable;
        _reelViews = Array.AsReadOnly(_reels
            .Select(reel => (IReadOnlyList<string>)Array.AsReadOnly(reel))
            .ToArray());
    }

    /// <summary>Gets a read-only snapshot view of the configured reel strips.</summary>
    public IReadOnlyList<IReadOnlyList<string>> Reels => _reelViews;

    /// <summary>Gets a read-only copy of the paytable.</summary>
    public IReadOnlyDictionary<string, decimal> PayoutMultipliers =>
        new System.Collections.ObjectModel.ReadOnlyDictionary<string, decimal>(
            new Dictionary<string, decimal>(_payoutMultipliers, StringComparer.Ordinal));

    /// <summary>Evaluates a supplied stop result without choosing random stops.</summary>
    public SlotSpinResult Evaluate(IReadOnlyList<string> stops, decimal stake)
    {
        ArgumentNullException.ThrowIfNull(stops);
        ValidateStake(stake);

        if (stops.Count != _reels.Length)
            throw new ArgumentException($"Expected {_reels.Length} stops, received {stops.Count}.", nameof(stops));

        var resultStops = new string[stops.Count];
        for (var i = 0; i < stops.Count; i++)
        {
            var symbol = stops[i];
            ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
            if (!_reels[i].Contains(symbol, StringComparer.Ordinal))
                throw new ArgumentException($"Symbol '{symbol}' does not occur on reel {i}.", nameof(stops));
            resultStops[i] = symbol;
        }

        var isMatch = resultStops.All(symbol => string.Equals(symbol, resultStops[0], StringComparison.Ordinal));
        var multiplier = isMatch && _payoutMultipliers.TryGetValue(resultStops[0], out var configuredMultiplier)
            ? configuredMultiplier
            : 0m;

        return new SlotSpinResult(Array.AsReadOnly(resultStops), stake, multiplier, stake * multiplier);
    }

    /// <summary>Calculates the exact expected gross payout as a reduced rational number.</summary>
    public ExactRational CalculateExpectedPayoutPerSpinExact(decimal stake)
    {
        ValidateStake(stake);
        return CalculateReturnToPlayerExact() * ExactRational.FromDecimal(stake);
    }

    /// <summary>
    /// Calculates exact return-to-player as a reduced rational number (for example, 4/9).
    /// Each reel probability and each decimal paytable multiplier is represented without rounding.
    /// </summary>
    public ExactRational CalculateReturnToPlayerExact()
    {
        var expectedMultiplier = ExactRational.Zero;
        foreach (var (symbol, multiplier) in _payoutMultipliers)
        {
            var probability = ExactRational.One;
            foreach (var reel in _reels)
            {
                var occurrences = reel.Count(stop => string.Equals(stop, symbol, StringComparison.Ordinal));
                probability *= new ExactRational(occurrences, reel.Length);
            }

            expectedMultiplier += probability * ExactRational.FromDecimal(multiplier);
        }

        return expectedMultiplier;
    }

    /// <summary>
    /// Returns the expected gross payout as a decimal display value. Use the exact method
    /// when the rational result itself is required; this conversion rounds to decimal precision.
    /// </summary>
    public decimal CalculateExpectedPayoutPerSpin(decimal stake) =>
        CalculateExpectedPayoutPerSpinExact(stake).ToDecimal();

    /// <summary>Returns exact expected gross payout divided by stake as a decimal display value.</summary>
    public decimal CalculateReturnToPlayer(decimal stake)
    {
        ValidateStake(stake);
        return CalculateReturnToPlayerExact().ToDecimal();
    }

    private static decimal ValidateStake(decimal stake)
    {
        if (stake <= 0m)
            throw new ArgumentOutOfRangeException(nameof(stake), "Stake must be greater than zero.");
        return stake;
    }
}
