using MalignEngine;
using System.Numerics;

namespace AxisOne;

public class AxisOneServer : EntitySystem, IEventClientConnected
{
    public void OnClientConnected(NetworkConnection connection)
    {
        EntityRef player = EntityManager.World.CreateEntity();
        player.Add(new PlayerMovementComponent { Speed = 5.0f });
        player.Add(new Transform { Position = new Vector3(0, 0, 0) });
        player.Add(new SpriteRenderer { Sprite = new Sprite(Texture2D.White), Color = Color.Red });
    }
}