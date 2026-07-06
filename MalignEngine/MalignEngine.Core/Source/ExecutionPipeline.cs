namespace MalignEngine;

public interface IExecutionStep
{
    void Execute(ExecutionContext context);
}

public sealed record class ExecutionContext(IScheduleManager ScheduleManager, double DeltaTime);

public sealed class StageStep<TSchedule> : IExecutionStep where TSchedule : ISchedule
{
    Action<TSchedule, ExecutionContext> _action;

    public StageStep(Action<TSchedule, ExecutionContext> action)
    {
        _action = action;
    }

    public void Execute(ExecutionContext context)
    {
        context.ScheduleManager.Run<TSchedule>(x => _action(x, context));
    }
}

public sealed class ExecutionPipeline
{
    private readonly List<IExecutionStep> _steps = new();

    public ExecutionPipeline Add(IExecutionStep step)
    {
        _steps.Add(step);
        return this;
    }

    public ExecutionPipeline Stage<TSchedule>(Action<TSchedule, ExecutionContext> action) where TSchedule : ISchedule
    {
        return Add(new StageStep<TSchedule>(action));
    }

    public void Execute(ExecutionContext context)
    {
        foreach (var step in _steps)
        {
            step.Execute(context);
        }
    }
}