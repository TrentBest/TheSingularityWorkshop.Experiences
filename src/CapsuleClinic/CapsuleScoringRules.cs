namespace TheSingularityWorkshop.Experiences.CapsuleClinic;

/// <summary>Deterministic score policy for a resolved Capsule Clinic board.</summary>
/// <remarks>
/// Each cleared cell awards 10 points, each cleared virus adds 100 bonus points,
/// and each chain wave after the first adds a 50-point bonus. This is a simple
/// baseline policy intended to be replaceable by a future rules configuration.
/// </remarks>
public static class CapsuleScoringRules
{
    /// <summary>Points awarded for each cell removed by a resolution.</summary>
    public const int PointsPerCell = 10;

    /// <summary>Bonus points awarded for each virus removed.</summary>
    public const int BonusPerVirus = 100;

    /// <summary>Bonus points awarded for each chain wave after the first.</summary>
    public const int BonusPerAdditionalChain = 50;

    /// <summary>Calculates the score for a resolution without mutating game state.</summary>
    public static int Calculate(ResolutionResult resolution)
    {
        if (resolution.ChainCount < 0) throw new ArgumentOutOfRangeException(nameof(resolution));
        if (resolution.CellsCleared < 0) throw new ArgumentOutOfRangeException(nameof(resolution));
        if (resolution.VirusesCleared < 0 || resolution.VirusesCleared > resolution.CellsCleared)
            throw new ArgumentOutOfRangeException(nameof(resolution));

        if (resolution.ChainCount == 0 && (resolution.CellsCleared != 0 || resolution.VirusesCleared != 0))
            throw new ArgumentException("A resolution without a chain cannot clear cells.", nameof(resolution));
        if (resolution.ChainCount > 0 && resolution.CellsCleared == 0)
            throw new ArgumentException("A chain must clear at least one cell.", nameof(resolution));

        return checked(
            (resolution.CellsCleared * PointsPerCell) +
            (resolution.VirusesCleared * BonusPerVirus) +
            (Math.Max(0, resolution.ChainCount - 1) * BonusPerAdditionalChain));
    }
}
