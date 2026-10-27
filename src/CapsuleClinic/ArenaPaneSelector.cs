namespace TheSingularityWorkshop.Experiences.CapsuleClinic;

/// <summary>Authoritative standings data needed to choose the four locally visible arena panes.</summary>
/// <param name="PlayerId">Stable player identity.</param>
/// <param name="RoundRank">Current round rank; lower positive values rank ahead of larger values.</param>
/// <param name="RoundsWon">Number of rounds officially won in the current match series.</param>
/// <param name="JoinOrder">Stable room join ordinal used to break tied ranks.</param>
/// <param name="IsActive">Whether the player is still active in the current round.</param>
public sealed record ArenaPlayerStanding(
    string PlayerId,
    int RoundRank,
    int RoundsWon,
    long JoinOrder,
    bool IsActive = true);

/// <summary>The four fixed positions in the local 2×2 arena.</summary>
public enum ArenaPane
{
    /// <summary>The local player's own board, always fixed at the upper left.</summary>
    LocalUpperLeft,
    /// <summary>The first comparison board, upper right.</summary>
    ComparisonUpperRight,
    /// <summary>The second comparison board, lower left.</summary>
    ComparisonLowerLeft,
    /// <summary>The third comparison board, lower right.</summary>
    ComparisonLowerRight
}

/// <summary>One selected board identity and its fixed pane position.</summary>
public readonly record struct ArenaPaneAssignment(ArenaPane Pane, string? PlayerId);

/// <summary>
/// Selects a stable four-pane view from a room of any roster size. This is a presentation policy,
/// not a room-capacity policy and not authority over standings.
/// </summary>
public static class ArenaPaneSelector
{
    /// <summary>
    /// Selects all participants for rooms of up to four players. For larger rooms, keeps the local
    /// player upper-left and shows the remaining top-four contenders when the local player is top
    /// four; otherwise it shows rank one and the local player's nearest rank-neighbors.
    /// Ties use join order then ordinal player ID. Inactive players remain in small-room views but
    /// are not selected as comparison boards in large rooms.
    /// </summary>
    public static IReadOnlyList<ArenaPaneAssignment> Select(
        string localPlayerId,
        IEnumerable<ArenaPlayerStanding> standings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localPlayerId);
        ArgumentNullException.ThrowIfNull(standings);

        var players = standings.ToArray();
        if (players.Any(p => p is null))
            throw new ArgumentException("Standings cannot contain null entries.", nameof(standings));
        if (players.Any(p => string.IsNullOrWhiteSpace(p.PlayerId)))
            throw new ArgumentException("Player IDs must not be blank.", nameof(standings));
        if (players.Any(p => p.RoundRank < 1 || p.RoundsWon < 0 || p.JoinOrder < 0))
            throw new ArgumentException("Ranks must be positive and scores/join order non-negative.", nameof(standings));
        if (players.Select(p => p.PlayerId).Distinct(StringComparer.Ordinal).Count() != players.Length)
            throw new ArgumentException("Player IDs must be unique.", nameof(standings));

        var local = players.SingleOrDefault(p => string.Equals(p.PlayerId, localPlayerId, StringComparison.Ordinal));
        if (local is null)
            throw new KeyNotFoundException($"Local player '{localPlayerId}' is not present in standings.");

        var ranked = players
            .OrderBy(p => p.RoundRank)
            .ThenBy(p => p.JoinOrder)
            .ThenBy(p => p.PlayerId, StringComparer.Ordinal)
            .ToArray();

        var selected = new List<ArenaPlayerStanding> { local };
        if (players.Length <= 4)
        {
            selected.AddRange(ranked.Where(p => !string.Equals(p.PlayerId, localPlayerId, StringComparison.Ordinal)));
        }
        else
        {
            var activeRanked = ranked.Where(p => p.IsActive).ToArray();
            var active = activeRanked.Where(p => !string.Equals(p.PlayerId, localPlayerId, StringComparison.Ordinal)).ToArray();
            var localActiveIndex = Array.FindIndex(activeRanked, p => string.Equals(p.PlayerId, localPlayerId, StringComparison.Ordinal));

            IEnumerable<ArenaPlayerStanding> candidates;
            if (localActiveIndex >= 0 && localActiveIndex < 4)
            {
                candidates = activeRanked.Take(4).Where(p => !string.Equals(p.PlayerId, localPlayerId, StringComparison.Ordinal));
            }
            else
            {
                var neighbors = new List<ArenaPlayerStanding>();
                var localRankIndex = Array.FindIndex(ranked, p => string.Equals(p.PlayerId, localPlayerId, StringComparison.Ordinal));
                var previous = ranked.Take(localRankIndex).LastOrDefault(p => p.IsActive);
                var next = ranked.Skip(localRankIndex + 1).FirstOrDefault(p => p.IsActive);
                var leader = activeRanked.FirstOrDefault(p => !string.Equals(p.PlayerId, localPlayerId, StringComparison.Ordinal));
                if (leader is not null) neighbors.Add(leader);
                if (previous is not null) neighbors.Add(previous);
                if (next is not null) neighbors.Add(next);
                candidates = neighbors;
            }

            foreach (var candidate in candidates.Concat(active))
            {
                if (selected.Count == 4) break;
                if (selected.All(p => !string.Equals(p.PlayerId, candidate.PlayerId, StringComparison.Ordinal)))
                    selected.Add(candidate);
            }
        }

        var panes = new[]
        {
            ArenaPane.LocalUpperLeft,
            ArenaPane.ComparisonUpperRight,
            ArenaPane.ComparisonLowerLeft,
            ArenaPane.ComparisonLowerRight
        };

        return panes.Select((pane, index) => new ArenaPaneAssignment(
            pane,
            index < selected.Count ? selected[index].PlayerId : null)).ToArray();
    }
}
