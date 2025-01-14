using MalignEngine;
using System.Net;

namespace SnakeGame;

public enum ServerToClient : byte
{
    LobbyInformation = 0,
    GameConfig = 1,
    AssignPlayerId = 2,
    BoardReset = 3,
    BoardSet = 4,
    BoardReplace = 5,
    PlayerConnected = 6,
    PlayerDisconnected = 7,
    PlayerSpawned = 8,
    PlayerDied = 9,
    PlayerMoved = 10,
    RespawnAllowed = 11,
    ChatMessageSent = 12,
}

public enum ClientToServer : byte
{
    RequestLobbyInfo = 0,
    Connecting = 1,
    Disconnecting = 2,
    FullUpdate = 3,
    PlayerInput = 4,
    RequestRespawn = 5,
    SendChatMessage = 6
}