using MalignEngine;
using MalignEngine.Network;

namespace SteamBolt;

[Serializable]
public struct PlayerMovementComponent : IComponent
{
    [DataField(nameof(MoveSpeed))]
    public float MoveSpeed;
    [DataField(nameof(JumpVelocity))]
    public float JumpVelocity;
    [DataField(nameof(JumpCutMultiplier))]
    public float JumpCutMultiplier;
    [DataField(nameof(GroundAcceleration))]
    public float GroundAcceleration;
    [DataField(nameof(AirAcceleration))]
    public float AirAcceleration;
    [DataField(nameof(GroundDeceleration))]
    public float GroundDeceleration;
    [DataField(nameof(AirDeceleration))]
    public float AirDeceleration;

    public bool IsGrounded;
}