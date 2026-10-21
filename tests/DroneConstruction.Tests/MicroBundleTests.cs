using TheSingularityWorkshop.Experiences.DroneConstruction;
using TheSingularityWorkshop.Experiences.DroneConstruction.MicroBundles;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class MicroBundleTests
{
    [Fact]
    public void ImplementedBundlesExposeUniqueStableIdentitiesAndProviders()
    {
        var bundles = new DroneConstructionMicroBundleBase[]
        {
            new HeightFieldMicroBundle(),
            new BuildGridMicroBundle(),
            new ResourceEmitterMicroBundle(),
            new SkylaneMicroBundle(),
            new HoverLaneMicroBundle(),
            new SlipstreamMicroBundle(),
            new TrafficCellMicroBundle(),
            new ConstructionAgentMicroBundle(),
            new ConstructionTaskSchedulerMicroBundle()
        };

        Assert.Equal(9, bundles.Select(bundle => bundle.Id).Distinct().Count());
        Assert.Equal(9, bundles.SelectMany(bundle => bundle.Descriptor.Providers).Select(provider => provider.Id).Distinct().Count());
        Assert.All(bundles, bundle => Assert.Empty(bundle.Dependencies));
    }

    [Fact]
    public void BuildGridBundleCreatesBoundedTargetAndCountsCompletion()
    {
        var grid = new BuildGridMicroBundle().Create(2, 1, (x, _) => x + 1);

        Assert.Equal(3, grid.RequiredBlockCount);
        Assert.False(grid.IsComplete);
        Assert.True(grid.TryPlaceBlock(0, 0));
        Assert.True(grid.TryPlaceBlock(1, 0));
        Assert.True(grid.TryPlaceBlock(1, 0));
        Assert.False(grid.TryPlaceBlock(1, 0));
        Assert.True(grid.IsComplete);
    }

    [Fact]
    public void ResourceEmitterBundleCreatesTenIndependentEmitters()
    {
        var emitters = new ResourceEmitterMicroBundle().CreateTen(index => 100 + index);

        Assert.Equal(10, emitters.Count);
        Assert.Equal(10, emitters.Select(emitter => emitter.EmitterId).Distinct().Count());
        Assert.Equal("emitter-01", emitters[0].EmitterId);
        Assert.Equal("emitter-10", emitters[9].EmitterId);
        Assert.Equal(100, emitters[0].Inventory);
        Assert.Equal(109, emitters[9].Inventory);
    }

    [Fact]
    public void SkylaneAndHoverLaneAreIndependentCellularGrids()
    {
        var sky = new SkylaneMicroBundle().Create("sky-east", 8, 2, LaneDirection.East, elevation: 3);
        var hover = new HoverLaneMicroBundle().Create("hover-east", 4, 3);

        Assert.Equal(LaneDirection.East, sky.Direction);
        Assert.Equal(3, sky.Elevation);
        Assert.Equal(8, sky.Cells.Width);
        Assert.Equal(4, hover.Cells.Width);
        Assert.NotSame(sky.Cells, hover.Cells);
    }

    [Fact]
    public void SlipstreamBundleKeepsFiveCellLookaheadConfigurable()
    {
        var slipstream = new SlipstreamMicroBundle().Create("transition", transitionCellCount: 2);
        Assert.Equal(5, slipstream.LookaheadCells);
        Assert.Equal(2, slipstream.TransitionCellCount);
    }

    [Fact]
    public void TaskScheduler_AssignsInStableTaskIdOrderAndRejectsDuplicateCompletion()
    {
        var scheduler = new ConstructionTaskSchedulerMicroBundle().Create();
        var agent = new ConstructionAgentMicroBundle().Create(12, new GridCell(0, 0));
        Assert.True(scheduler.Enqueue(new PlacementTask(8, 4, 1, 1)));
        Assert.True(scheduler.Enqueue(new PlacementTask(2, 1, 3, 1)));
        Assert.False(scheduler.Enqueue(new PlacementTask(2, 9, 9, 9)));

        Assert.True(scheduler.TryAssignNext(agent, out var task));
        Assert.Equal(2UL, task!.TaskId);
        Assert.Equal(new GridCell(0, 0), agent.Position);
        Assert.Equal(ConstructionAgentState.QueuedAtEmitter, agent.State);
        Assert.False(scheduler.Complete(2, agent));
        Assert.True(agent.TryTransition(ConstructionAgentState.CarryingBlock));
        Assert.True(agent.TryTransition(ConstructionAgentState.AwaitingTransition));
        Assert.True(agent.TryTransition(ConstructionAgentState.Traveling));
        Assert.True(agent.TryTransition(ConstructionAgentState.PlacingBlock));
        Assert.True(agent.TryTransition(ConstructionAgentState.Completed));
        Assert.True(scheduler.Complete(2, agent));
        Assert.False(scheduler.Complete(2, agent));
        Assert.True(scheduler.IsCompleted(2));
        Assert.Equal(1, scheduler.PendingCount);
        Assert.Equal(0, scheduler.AssignedCount);
    }

    [Fact]
    public void TaskScheduler_RequeuePreservesTaskAndReleasesWaitingAgent()
    {
        var scheduler = new ConstructionTaskSchedulerMicroBundle().Create();
        var firstAgent = new ConstructionAgentMicroBundle().Create(1, new GridCell(0, 0));
        var secondAgent = new ConstructionAgentMicroBundle().Create(2, new GridCell(1, 0));
        var original = new PlacementTask(5, 7, 4, 2);
        Assert.True(scheduler.Enqueue(original));
        Assert.True(scheduler.TryAssignNext(firstAgent, out _));

        Assert.True(scheduler.Requeue(5, firstAgent));
        Assert.Equal(ConstructionAgentState.Idle, firstAgent.State);
        Assert.Null(firstAgent.AssignedTaskId);
        Assert.True(scheduler.TryAssignNext(secondAgent, out var reassigned));
        Assert.Equal(original, reassigned);
    }

    [Fact]
    public void AgentLifecycle_RequiresExplicitLegalTransitions()
    {
        var agent = new ConstructionAgentMicroBundle().Create(44, new GridCell(2, 3));
        var task = new PlacementTask(3, 1, 1, 1);
        Assert.True(agent.TryAssign(task));
        Assert.False(agent.TryTransition(ConstructionAgentState.Traveling));
        Assert.True(agent.TryTransition(ConstructionAgentState.CarryingBlock));
        Assert.True(agent.IsCarryingBlock);
        Assert.True(agent.TryTransition(ConstructionAgentState.AwaitingTransition));
        Assert.True(agent.TryTransition(ConstructionAgentState.Traveling));
        Assert.True(agent.TryTransition(ConstructionAgentState.PlacingBlock));
        Assert.True(agent.TryTransition(ConstructionAgentState.Completed));
        Assert.Equal(ConstructionAgentState.Idle, agent.State);
        Assert.False(agent.IsCarryingBlock);
        Assert.Null(agent.AssignedTaskId);
    }
}
