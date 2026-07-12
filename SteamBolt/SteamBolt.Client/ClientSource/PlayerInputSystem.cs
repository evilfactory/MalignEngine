using System.Numerics;
using MalignEngine;
using MalignEngine.Network;

namespace SteamBolt;

public class PlayerInputSystem : EntitySystem
{
    private readonly IInputService _inputService;
    private readonly ClientSessionSystem _clientSessionSystem;
    private readonly INetworkService _network;

    public PlayerInputSystem(IServiceContainer serviceContainer, IInputService inputService, ClientSessionSystem clientSessionSystem, INetworkService network) : base(serviceContainer)
    {
        _inputService = inputService;
        _network = network;
        _clientSessionSystem = clientSessionSystem;
    }

    public override void OnUpdate(float deltaTime)
    {
        if (_clientSessionSystem.ClientId == null) { return; }

        World.Query(new Query()
            .Include<OwnerComponent>()
            .Include<PlayerInputComponent>(),
            entity =>
            {
                ref var owner = ref entity.Get<OwnerComponent>();
                ref var input = ref entity.Get<PlayerInputComponent>();

                if (owner.ClientId.Equals(_clientSessionSystem.ClientId))
                {
                    input.Up = _inputService.Keyboard.IsKeyPressed(Key.W);
                    input.Down = _inputService.Keyboard.IsKeyPressed(Key.S);
                    input.Left = _inputService.Keyboard.IsKeyPressed(Key.A);
                    input.Right = _inputService.Keyboard.IsKeyPressed(Key.D);
                }
            });
    }
}