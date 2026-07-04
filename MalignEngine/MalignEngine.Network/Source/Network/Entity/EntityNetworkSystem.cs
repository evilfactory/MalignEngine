using NetSerializer;
using System.Xml.Linq;

namespace MalignEngine.Network;


[Serializable]
public class ComponentState : INetSerializable { }

public class NetEntitySpawnNetMessage : NetMessage
{
    public NetEntityId NetEntityId = default!;
    public string SceneId = default!;

    public override void Deserialize(IReadMessage message)
    {
        NetEntityId = new NetEntityId() { Value = message.ReadUInt32() };
        SceneId = message.ReadString();
    }

    public override void Serialize(IWriteMessage message)
    {
        message.WriteUInt32(NetEntityId.Value);
        message.WriteString(SceneId);
    }
}

public class NetEntityStateNetMessage : NetMessage
{
    public NetEntityId NetEntityId;
    public List<byte[]> States = new List<byte[]>();

    public override void Deserialize(IReadMessage message)
    {
        NetEntityId = new NetEntityId() { Value = message.ReadUInt32() };
        int count = message.ReadInt32();
        States = new List<byte[]>(count);
        for (int i = 0; i < count; i++)
        {
            int length = message.ReadInt32();
            byte[] state = message.ReadBytes(length);
            States.Add(state);
        }
    }

    public override void Serialize(IWriteMessage message)
    {
        message.WriteUInt32(NetEntityId.Value);
        message.WriteInt32(States.Count);
        foreach (byte[] state in States)
        {
            message.WriteInt32(state.Length);
            message.WriteBytes(state, 0, state.Length);
        }
    }
}

public class NetEntitySyncNetMessage : NetMessage
{
    public required NetEntitySpawnNetMessage[] Entities;
    public required NetEntityStateNetMessage[] States;

    public override void Deserialize(IReadMessage message)
    {
        int count = message.ReadInt32();
        Entities = new NetEntitySpawnNetMessage[count];
        for (int i = 0; i < count; i++)
        {
            NetEntitySpawnNetMessage entity = new NetEntitySpawnNetMessage();
            entity.Deserialize(message);
            Entities[i] = entity;
        }

        count = message.ReadInt32();
        States = new NetEntityStateNetMessage[count];
        for (int i = 0; i < count; i++)
        {
            NetEntityStateNetMessage state = new NetEntityStateNetMessage();
            state.Deserialize(message);
            States[i] = state;
        }
    }

    public override void Serialize(IWriteMessage message)
    {
        message.WriteInt32(Entities.Length);
        foreach (NetEntitySpawnNetMessage entity in Entities)
        {
            entity.Serialize(message);
        }

        message.WriteInt32(States.Length);
        foreach (NetEntityStateNetMessage state in States)
        {
            state.Serialize(message);
        }
    }
}

public class ServerEntityNetworkSystem : EntitySystem
{
    private INetworkServer _server;

    private uint nextEntityId;

    public ServerEntityNetworkSystem(IServiceContainer serviceContainer, INetworkServer server) : base(serviceContainer)
    {
        _server = server;
    }

    public void SpawnEntity(Entity entity)
    {
        nextEntityId++;
        NetEntityId id = new NetEntityId() { Value = nextEntityId };
        entity.AddOrSet(new NetEntityId() { Value = nextEntityId });

        _server.Broadcast(new NetEntitySpawnNetMessage()
        {
            NetEntityId = id,
            SceneId = entity.Get<SceneComponent>().SceneId
        });
    }
}

public class ClientEntityNetworkSystem : EntitySystem
{
    private INetworkClient _client;
    private IAssetService _assetService;
    private SceneSystem _sceneSystem;

    public ClientEntityNetworkSystem(IServiceContainer serviceContainer, IAssetService assetService, SceneSystem sceneSystem, INetworkClient client) : base(serviceContainer)
    {
        _client = client;
        _assetService = assetService;
        _sceneSystem = sceneSystem;

        _client.Register<NetEntitySpawnNetMessage>(ReceiveNetEntitySpawn);
    }

    private void ReceiveNetEntitySpawn(NetEntitySpawnNetMessage netMessage)
    {
        AssetHandle<Scene>? scene = _assetService.GetHandles<Scene>().FirstOrDefault(x => x.Asset.SceneId == netMessage.SceneId);

        Entity entity = _sceneSystem.Instantiate(scene);
        entity.AddOrSet(netMessage.NetEntityId);
    }
}