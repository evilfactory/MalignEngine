using System.Numerics;

namespace MalignEngine;

public struct PhysicsSpace : IComponent
{
    public Vector2 Origin;
}

public struct PhysicsSpaceMember : IComponent
{
    public Entity Space;
}