using MalignEngine;

namespace AxisOne;

public class AxisOneClient : EntitySystem, IEventConnected
{
    [Dependency]
    protected AxisOne AxisOne;

    public void OnConnected()
    {
        AxisOne.LoadGame();
    }
}