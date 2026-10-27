namespace TheSingularityWorkshop.Experiences.CapsuleClinic;

/// <summary>One pane in the locally rendered arena, including the host snapshot for its selected player.</summary>
/// <param name="Pane">The fixed 2×2 pane position.</param>
/// <param name="Standing">The selected player's current rank and cumulative round wins, or null for an empty pane.</param>
/// <param name="Player">The selected player's authoritative board snapshot, or null for an empty pane.</param>
public sealed record CapsuleArenaPaneView(
    ArenaPane Pane,
    ArenaPlayerStanding? Standing,
    PuzzlePlayerView? Player);

/// <summary>A consistent presentation input for one local player's four-pane arena.</summary>
/// <remarks>
/// Capture this after the host has applied the current tick's commands and evaluated any round boundary.
/// This is presentation data only; it does not grant authority to the local client or expose unselected boards.
/// </remarks>
public sealed record CapsuleArenaSnapshot(
    string LocalPlayerId,
    IReadOnlyList<CapsuleArenaPaneView> Panes,
    int BoardWidth = 8,
    int BoardHeight = 16)
{
    /// <summary>
    /// Captures the selected arena panes from authoritative room standings and board snapshots.
    /// </summary>
    public static CapsuleArenaSnapshot Capture(AuthoritativePuzzleRoom room, string localPlayerId)
    {
        ArgumentNullException.ThrowIfNull(room);
        ArgumentException.ThrowIfNullOrWhiteSpace(localPlayerId);

        var standings = room.GetStandings();
        var standingById = standings.ToDictionary(p => p.PlayerId, StringComparer.Ordinal);
        var assignments = ArenaPaneSelector.Select(localPlayerId, standings);
        var panes = assignments.Select(assignment =>
        {
            if (assignment.PlayerId is null)
                return new CapsuleArenaPaneView(assignment.Pane, null, null);

            if (!standingById.TryGetValue(assignment.PlayerId, out var standing))
                throw new InvalidOperationException("The arena selector returned a player absent from authoritative standings.");

            return new CapsuleArenaPaneView(
                assignment.Pane,
                standing,
                room.GetPlayerView(assignment.PlayerId));
        }).ToArray();

        return new CapsuleArenaSnapshot(localPlayerId, Array.AsReadOnly(panes), room.Width, room.Height);
    }
}
