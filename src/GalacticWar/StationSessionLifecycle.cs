using TheSingularityWorkshop.FSM_API;
using FsmRuntime = TheSingularityWorkshop.FSM_API.FSM_API;

namespace TheSingularityWorkshop.Experiences.GalacticWar;

/// <summary>
/// One station visit's lifecycle. The caller owns this context; FSM_API owns only its registered handle.
/// </summary>
public sealed class StationSessionLifecycle : IStateContext, IDisposable
{
    private const string DefinitionName = "GalacticWarStationSession";
    private const string ProcessingGroup = "GalacticWar.StationSession";
    private static readonly object DefinitionGate = new();

    private readonly FSMHandle _handle;
    private bool _disposed;

    public StationSessionLifecycle(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        IsValid = true;
        EnsureDefinition();
        _handle = FsmRuntime.Create.CreateInstance(DefinitionName, this, ProcessingGroup);
    }

    public string Name { get; set; }
    public bool IsValid { get; set; }
    public string CurrentState => _handle.CurrentState;
    public bool IsDisposed => _disposed;

    /// <summary>Moves this visit from Arriving to Active using its own FSM handle.</summary>
    public void MarkArrived()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!string.Equals(_handle.CurrentState, "Arriving", StringComparison.Ordinal))
            throw new InvalidOperationException("Only an arriving station session can enter Active.");
        HasArrived = true;
        _handle.Update(ProcessingGroup);
    }

    /// <summary>Moves this visit from Active to Departed using its own FSM handle.</summary>
    public void Depart()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!string.Equals(_handle.CurrentState, "Active", StringComparison.Ordinal))
            throw new InvalidOperationException("Only an active station session can depart.");
        HasDeparted = true;
        _handle.Update(ProcessingGroup);
    }

    private bool HasArrived { get; set; }
    private bool HasDeparted { get; set; }

    private static void EnsureDefinition()
    {
        lock (DefinitionGate)
        {
            if (FsmRuntime.Interaction.Exists(DefinitionName, ProcessingGroup))
                return;

            FsmRuntime.Create.CreateProcessingGroup(ProcessingGroup);
            FsmRuntime.Create.CreateFiniteStateMachine(DefinitionName, -1, ProcessingGroup)
                .State("Arriving", null, null, null)
                .State("Active", null, null, null)
                .State("Departed", null, null, null)
                .WithInitialState("Arriving")
                .Transition("Arriving", "Active", context => context is StationSessionLifecycle session && session.HasArrived)
                .Transition("Active", "Departed", context => context is StationSessionLifecycle session && session.HasDeparted)
                .BuildDefinition();
        }
    }

    /// <summary>Explicitly unregisters this handle; it does not transfer or dispose the caller-owned context.</summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        FsmRuntime.Interaction.DestroyInstance(_handle);
        IsValid = false;
        _disposed = true;
    }
}
