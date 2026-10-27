using TheSingularityWorkshop.FSM_API;
using FsmRuntime = TheSingularityWorkshop.FSM_API.FSM_API;

namespace TheSingularityWorkshop.Experiences.CapsuleClinic;

/// <summary>
/// Serializes Capsule Clinic's registry mutations against the shared FSM_API registry.
/// Game stepping remains instance-local and does not tick every registered player.
/// </summary>
internal static class CapsuleClinicFsmRuntime
{
    internal const string ProcessingGroup = "CapsuleClinic";
    private static readonly object RegistryGate = new();

    internal static void EnsureDefinition(string definitionName, Action define)
    {
        ArgumentNullException.ThrowIfNull(define);
        lock (RegistryGate)
        {
            if (FsmRuntime.Interaction.Exists(definitionName, ProcessingGroup))
                return;

            FsmRuntime.Create.CreateProcessingGroup(ProcessingGroup);
            define();
        }
    }

    internal static FSMHandle CreateInstance(string definitionName, IStateContext context)
    {
        lock (RegistryGate)
            return FsmRuntime.Create.CreateInstance(definitionName, context, ProcessingGroup);
    }

    internal static void DestroyInstance(FSMHandle handle)
    {
        lock (RegistryGate)
            FsmRuntime.Interaction.DestroyInstance(handle);
    }
}
