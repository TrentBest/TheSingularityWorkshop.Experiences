namespace TheSingularityWorkshop.Games.Slots;

/// <summary>The evaluated result of a single supplied slot spin.</summary>
/// <param name="Stops">The validated symbol at each reel's selected stop.</param>
/// <param name="Stake">The virtual stake supplied to the evaluator.</param>
/// <param name="PayoutMultiplier">The configured total-return multiplier, or zero for a loss.</param>
/// <param name="GrossPayout">Stake multiplied by the total-return multiplier.</param>
public sealed record SlotSpinResult(
    IReadOnlyList<string> Stops,
    decimal Stake,
    decimal PayoutMultiplier,
    decimal GrossPayout);
