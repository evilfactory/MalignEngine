using System.Numerics;
using MalignEngine;

namespace SteamBolt;

public class PlayerInputSystem : EntitySystem
{
    private readonly IInputService _inputService;
    private readonly ClientSessionSystem _clientSessionSystem;

    public PlayerInputSystem(IServiceContainer serviceContainer, IInputService inputService, ClientSessionSystem clientSessionSystem) : base(serviceContainer)
    {
        _inputService = inputService;
        _clientSessionSystem = clientSessionSystem;
    }

    public override void OnUpdate(float deltaTime)
    {
        if (_clientSessionSystem.MyClient == null) { return; }

        World.Query(new Query()
            .Include<OwnerComponent>()
            .Include<PlayerInputComponent>(),
            entity =>
            {
                ref var owner = ref entity.Get<OwnerComponent>();
                ref var input = ref entity.Get<PlayerInputComponent>();

                if (_clientSessionSystem.MyClient.ClientId == owner.ClientId)
                {
                    input.Up = _inputService.Keyboard.IsKeyPressed(Key.W);
                    input.Down = _inputService.Keyboard.IsKeyPressed(Key.S);
                    input.Left = _inputService.Keyboard.IsKeyPressed(Key.A);
                    input.Right = _inputService.Keyboard.IsKeyPressed(Key.D);
                }
            });
    }
}