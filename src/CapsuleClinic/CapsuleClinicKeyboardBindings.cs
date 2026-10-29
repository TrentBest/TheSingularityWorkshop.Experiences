namespace TheSingularityWorkshop.Experiences.CapsuleClinic;

/// <summary>Maps conventional keyboard key names to stable Capsule Clinic semantic intents.</summary>
/// <remarks>
/// This is a host-neutral mapping only: it does not subscribe to keyboard events, prevent browser defaults,
/// schedule simulation ticks, or mutate game state. A host submits the returned intent through
/// <see cref="CapsuleClinicLocalSession"/> and presents the resulting authoritative snapshot.
/// Key names follow the common UI convention (for example, <c>ArrowLeft</c> and <c> </c> for Space).
/// </remarks>
public static class CapsuleClinicKeyboardBindings
{
    /// <summary>Maps a key name to a semantic intent. Unknown keys return false and an empty intent.</summary>
    public static bool TryMapKey(string? key, out string intentId)
    {
        intentId = key switch
        {
            "ArrowLeft" => CapsuleClinicIntents.MoveLeft,
            "ArrowRight" => CapsuleClinicIntents.MoveRight,
            "ArrowUp" => CapsuleClinicIntents.Rotate,
            "ArrowDown" => CapsuleClinicIntents.SoftDrop,
            " " => CapsuleClinicIntents.HardDrop,
            _ => string.Empty
        };

        return intentId.Length != 0;
    }
}
