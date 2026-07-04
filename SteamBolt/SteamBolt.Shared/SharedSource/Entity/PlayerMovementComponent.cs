using MalignEngine;
using MalignEngine.Network;

namespace SteamBolt;

[Serializable]
public struct PlayerMovementComponent : IComponent
{
    public float MoveSpeed;
    public float JumpForce;
    public bool IsGrounded;
}