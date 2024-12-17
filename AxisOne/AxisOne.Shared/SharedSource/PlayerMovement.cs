using MalignEngine;

namespace AxisOne;

[Serializable]
public struct PlayerMovementComponent : IComponent
{
    public float Speed;
}

public class PlayerMovementSystem : EntitySystem
{

}