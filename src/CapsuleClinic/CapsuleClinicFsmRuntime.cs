#if WORKSHOP_USE_FSM_API_REPOSITORY_SOURCE
using WorkshopFsmApi = FSM_API.FsmApi;
#else
using FsmRuntime = TheSingularityWorkshop.FSM_API.FSM_API;
#endif
using TheSingularityWorkshop.FSM_API;

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
#if WORKSHOP_USE_FSM_API_REPOSITORY_SOURCE
            if (WorkshopFsmApi.Interact.Exists(definitionName, ProcessingGroup))
                return;

            WorkshopFsmApi.Create.CreateProcessingGroup(ProcessingGroup);
#else
            if (FsmRuntime.Interaction.Exists(definitionName, ProcessingGroup))
                return;

            FsmRuntime.Create.CreateProcessingGroup(ProcessingGroup);
#endif
            define();
        }
    }

    internal static FSMHandle CreateInstance(string definitionName, IStateContext context)
    {
        lock (RegistryGate)
        {
#if WORKSHOP_USE_FSM_API_REPOSITORY_SOURCE
            return WorkshopFsmApi.Create.CreateInstance(definitionName, context, ProcessingGroup);
#else
            return FsmRuntime.Create.CreateInstance(definitionName, context, ProcessingGroup);
#endif
        }
    }

    internal static void DestroyInstance(FSMHandle handle)
    {
        lock (RegistryGate)
        {
#if WORKSHOP_USE_FSM_API_REPOSITORY_SOURCE
            WorkshopFsmApi.Interact.DestroyInstance(handle);
#else
            FsmRuntime.Interaction.DestroyInstance(handle);
#endif
        }
    }
}
