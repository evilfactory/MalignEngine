using MalignEngine;
using MalignEngine.Network;

namespace SteamBolt;

public struct PlayerInputComponent : IComponent
{
    public bool Up, Down;
    public bool Left, Right;
}