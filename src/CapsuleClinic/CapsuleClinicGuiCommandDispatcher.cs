using TheSingularityWorkshop.Workshop.Gui;

namespace TheSingularityWorkshop.Experiences.CapsuleClinic;

/// <summary>
/// Bridges a semantic GUI button's command metadata to the local session's authoritative intent boundary.
/// </summary>
/// <remarks>
/// A renderer or host must call this method from its own button-event handler and then render the returned
/// snapshot. This class does not install event handlers or provide a GUI runtime.
/// </remarks>
public static class CapsuleClinicGuiCommandDispatcher
{
    /// <summary>
    /// Submits a button's declared command to the session.
    /// Returns false when the node is not a button or has no non-empty command identifier.
    /// A true return means the command was submitted; inspect the result to determine whether it was recognized
    /// and accepted by the authoritative room.
    /// </summary>
    public static bool TrySubmit(
        CapsuleClinicLocalSession session,
        GuiNode control,
        out CapsuleClinicSessionResult? result)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(control);

        if (!string.Equals(control.Kind, "Button", StringComparison.Ordinal)
            || !control.Properties.TryGetValue("onClickCommand", out var intentId)
            || string.IsNullOrWhiteSpace(intentId))
        {
            result = null;
            return false;
        }

        result = session.SubmitIntent(intentId);
        return true;
    }
}
