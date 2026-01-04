
using nkast.Aether.Physics2D.Dynamics;
using System.Numerics;

namespace MalignEngine;

public enum PhysicsBodyType
{
    Static,
    Kinematic,
    Dynamic
}

[Serializable]
public struct PhysicsBody2D : IComponent
{
    [DataField("BodyType", save: true)]
    public PhysicsBodyType BodyType;
    [DataField("Mass", save: true)]
    public float Mass;

    public Vector2 LinearVelocity;
    public float AngularVelocity;

    public FixtureData2D[] Fixtures;
}