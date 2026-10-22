namespace TheSingularityWorkshop.Experiences.DroneConstruction;

/// <summary>
/// Stateless helpers for velocity-aware gravity-sort braking. This is a decision policy, not a
/// physics integrator; the caller owns the authoritative position and velocity updates.
/// Upward altitude and vertical velocity are positive.
/// </summary>
public static class GravitySortBrakingPolicy
{
    /// <summary>
    /// Computes ideal stopping distance while descending under constant net upward acceleration.
    /// </summary>
    /// <param name="verticalVelocity">Current vertical velocity; negative means descending.</param>
    /// <param name="gravity">Positive gravitational acceleration magnitude.</param>
    /// <param name="maximumThrustAcceleration">Maximum upward acceleration supplied by thrust before gravity is subtracted.</param>
    /// <returns>Distance lost in altitude before vertical velocity reaches zero.</returns>
    public static double GetStoppingDistance(
        double verticalVelocity,
        double gravity,
        double maximumThrustAcceleration)
    {
        ValidateFinite(verticalVelocity, nameof(verticalVelocity));
        ValidatePositive(gravity, nameof(gravity));
        ValidatePositive(maximumThrustAcceleration, nameof(maximumThrustAcceleration));

        var netBrakingAcceleration = maximumThrustAcceleration - gravity;
        if (netBrakingAcceleration <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(maximumThrustAcceleration),
                "Maximum thrust must exceed gravity to arrest a descending drone.");

        if (verticalVelocity >= 0)
            return 0;

        return verticalVelocity * verticalVelocity / (2 * netBrakingAcceleration);
    }

    /// <summary>
    /// Returns true when a descending drone has entered its velocity-derived braking window.
    /// The safety margin accounts for discrete tick timing and model uncertainty.
    /// </summary>
    public static bool ShouldBeginBraking(
        double currentAltitude,
        double laneAltitude,
        double verticalVelocity,
        double gravity,
        double maximumThrustAcceleration,
        double safetyMargin)
    {
        ValidateFinite(currentAltitude, nameof(currentAltitude));
        ValidateFinite(laneAltitude, nameof(laneAltitude));
        ValidateNonNegative(safetyMargin, nameof(safetyMargin));

        if (verticalVelocity >= 0)
            return false;

        var altitudeGap = currentAltitude - laneAltitude;
        if (altitudeGap < 0)
            return false;

        var stoppingDistance = GetStoppingDistance(
            verticalVelocity, gravity, maximumThrustAcceleration);
        return altitudeGap <= stoppingDistance + safetyMargin;
    }

    /// <summary>
    /// Confirms lane capture only when both altitude and vertical speed are within tolerance.
    /// </summary>
    public static bool IsLaneCaptured(
        double currentAltitude,
        double laneAltitude,
        double verticalVelocity,
        double altitudeTolerance,
        double verticalSpeedTolerance)
    {
        ValidateFinite(currentAltitude, nameof(currentAltitude));
        ValidateFinite(laneAltitude, nameof(laneAltitude));
        ValidateFinite(verticalVelocity, nameof(verticalVelocity));
        ValidateNonNegative(altitudeTolerance, nameof(altitudeTolerance));
        ValidateNonNegative(verticalSpeedTolerance, nameof(verticalSpeedTolerance));

        return Math.Abs(currentAltitude - laneAltitude) <= altitudeTolerance
            && Math.Abs(verticalVelocity) <= verticalSpeedTolerance;
    }

    private static void ValidateFinite(double value, string parameterName)
    {
        if (!double.IsFinite(value))
            throw new ArgumentOutOfRangeException(parameterName, "Value must be finite.");
    }

    private static void ValidatePositive(double value, string parameterName)
    {
        ValidateFinite(value, parameterName);
        if (value <= 0)
            throw new ArgumentOutOfRangeException(parameterName, "Value must be greater than zero.");
    }

    private static void ValidateNonNegative(double value, string parameterName)
    {
        ValidateFinite(value, parameterName);
        if (value < 0)
            throw new ArgumentOutOfRangeException(parameterName, "Value cannot be negative.");
    }
}
