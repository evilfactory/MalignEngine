using MalignEngine;
using MalignEngine.Network;

namespace SteamBolt;

[Serializable]
public struct PlayerMovementComponent : IComponent
{
    [DataField(nameof(MoveSpeed))]
    public float MoveSpeed;
    [DataField(nameof(JumpForce))]
    public float JumpForce;
    public bool IsGrounded;
}