namespace MalignEngine;

public class BeforeEndFrame : Stage
{
    public override float Priority => 0.999f;
}

public class AfterBeginFrame : Stage
{
    public override float Priority => 0.00000001f;
}

[Stage<IDraw, LowestPriorityStage>]
public class DrawLoopBefore : IService, IDraw
{
    private IRenderingAPI _renderAPI;

    public DrawLoopBefore(IRenderingAPI renderAPI)
    {
        _renderAPI = renderAPI;
    }

    public void OnDraw(float deltaTime)
    {
        _renderAPI.BeginFrame();
    }
}

[Stage<IDraw, HighestPriorityStage>]
public class DrawLoopAfter : IService, IDraw
{
    private IRenderingAPI _renderAPI;

    public DrawLoopAfter(IRenderingAPI renderAPI)
    {
        _renderAPI = renderAPI;
    }

    public void OnDraw(float deltaTime)
    {
        _renderAPI.EndFrame();
    }
}
