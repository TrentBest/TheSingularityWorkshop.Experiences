namespace TheSingularityWorkshop.Experiences.CapsuleClinic;

/// <summary>Named local decision styles for the first Capsule Clinic agent baseline.</summary>
public enum CapsuleAgentStyle
{
    /// <summary>Prefer a central, predictable placement.</summary>
    Steady,
    /// <summary>Prefer the currently lowest column to preserve headroom.</summary>
    LowColumn,
    /// <summary>Prefer a column whose visible top matches one of the active capsule colors.</summary>
    ColorOpportunist
}

/// <summary>
/// A deterministic, offline policy that proposes one action from an immutable observation.
/// This baseline intentionally uses inexpensive heuristics; it is not a search-based solver.
/// The authoritative game remains responsible for validating and applying every action.
/// </summary>
public sealed class CapsuleAgentPolicy
{
    public CapsuleAgentPolicy(CapsuleAgentStyle style)
    {
        if (!Enum.IsDefined(style)) throw new ArgumentOutOfRangeException(nameof(style));
        Style = style;
    }

    /// <summary>The configured decision style.</summary>
    public CapsuleAgentStyle Style { get; }

    /// <summary>Chooses one deterministic action. Repeated calls on the same observation agree.</summary>
    public CapsuleAction ChooseAction(CapsuleAgentObservation observation)
    {
        ArgumentNullException.ThrowIfNull(observation);
        if (observation.GameOver || observation.Active is null)
            return CapsuleAction.Tick;

        var active = observation.Active.Value;
        var targetX = ChooseTargetColumn(observation, active);
        if (active.PivotX < targetX)
            return CapsuleAction.MoveRight;
        if (active.PivotX > targetX)
            return CapsuleAction.MoveLeft;

        // Keep the first policy slice simple and observable: orient vertically for a narrow
        // footprint, then let the authoritative engine perform the deterministic drop.
        if (active.Orientation != CapsuleOrientation.Vertical)
            return CapsuleAction.Rotate;

        return CapsuleAction.HardDrop;
    }

    private int ChooseTargetColumn(CapsuleAgentObservation observation, ActiveCapsule active)
    {
        var maxPivot = observation.Width - (active.Orientation == CapsuleOrientation.Horizontal ? 2 : 1);
        if (maxPivot < 0) return 0;

        var bestX = Math.Clamp(observation.Width / 2, 0, maxPivot);
        var bestValue = double.PositiveInfinity;

        for (var x = 0; x <= maxPivot; x++)
        {
            var value = Style switch
            {
                CapsuleAgentStyle.Steady => Math.Abs(x - (observation.Width - 1) / 2.0),
                CapsuleAgentStyle.LowColumn => ColumnHeight(observation, x, active.Orientation) + Math.Abs(x - active.PivotX) * 0.01,
                CapsuleAgentStyle.ColorOpportunist => ColorMismatchPenalty(observation, x, active) + ColumnHeight(observation, x, active.Orientation) * 0.001,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (value < bestValue)
            {
                bestValue = value;
                bestX = x;
            }
        }

        return bestX;
    }

    private static int ColumnHeight(CapsuleAgentObservation observation, int x, CapsuleOrientation orientation)
    {
        var height = observation.ColumnHeights[x];
        if (orientation == CapsuleOrientation.Horizontal && x + 1 < observation.Width)
            height = Math.Max(height, observation.ColumnHeights[x + 1]);
        return height;
    }

    private static int ColorMismatchPenalty(CapsuleAgentObservation observation, int x, ActiveCapsule active)
    {
        var pivotColor = TopColor(observation, x);
        var partnerX = active.Orientation == CapsuleOrientation.Horizontal ? x + 1 : x;
        var partnerColor = TopColor(observation, partnerX);

        var penalty = 0;
        if (pivotColor is { } first && first != active.Pair.PivotColor) penalty++;
        if (partnerColor is { } second && second != active.Pair.PartnerColor) penalty++;
        return penalty;
    }

    private static CapsuleColor? TopColor(CapsuleAgentObservation observation, int x)
    {
        if ((uint)x >= (uint)observation.Width) return null;
        var y = observation.ColumnHeights[x] - 1;
        return y >= 0 ? observation.GetCell(x, y)?.Color : null;
    }
}
