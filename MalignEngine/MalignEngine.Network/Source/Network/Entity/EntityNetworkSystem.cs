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

public interface IEntityNetwork
{
    IEnumerable<NetworkConnection> SyncedClients { get; }
    Entity FindEntityByNetId(NetEntityId id);
}


public class EntityNetworkSystem : EntitySystem, IEntityNetwork
{
    public IEnumerable<NetworkConnection> SyncedClients => _syncedClients;

    private HashSet<NetworkConnection> _syncedClients = [];
    private Dictionary<NetEntityId, Entity> _netEntities = [];

    private INetworkService _network;
    private IAssetService _assetService;
    private SceneSystem _sceneSystem;

    private uint nextEntityId;

    public EntityNetworkSystem(IServiceContainer serviceContainer, INetworkService network, IAssetService assetService, SceneSystem sceneSystem) : base(serviceContainer)
    {
        _network = network;
        _assetService = assetService;
        _sceneSystem = sceneSystem;

        if (network.Client != null)
        {
            network.Client.Register<NetEntitySpawnNetMessage>(ReceiveNetEntitySpawn);
            network.Client.Register<NetEntitySyncNetMessage>(ReceiveNetSyncEntity);
        }
    }

    public void SpawnEntity(Entity entity)
    {
        if (_network.Server == null)
        {
            throw new InvalidOperationException();
        }

        nextEntityId++;
        NetEntityId id = new NetEntityId() { Value = nextEntityId };
        entity.AddOrSet(id);

        _netEntities.Add(id, entity);

        foreach (var connection in _syncedClients)
        {
            _network.Server.Send(connection, new NetEntitySpawnNetMessage()
            {
                NetEntityId = id,
                SceneId = entity.Get<SceneComponent>().SceneId
            });
        }
    }

    private void ReceiveNetSyncEntity(NetEntitySyncNetMessage message)
    {
        foreach (var spawnMessage in message.Entities)
        {
            ReceiveNetEntitySpawn(spawnMessage);
        }
    }

    private void ReceiveNetEntitySpawn(NetEntitySpawnNetMessage netMessage)
    {
        AssetHandle<Scene>? scene = _assetService.GetHandles<Scene>().FirstOrDefault(x => x.Asset.SceneId == netMessage.SceneId);

        Entity entity = _sceneSystem.Instantiate(scene);
        entity.AddOrSet(netMessage.NetEntityId);
        _netEntities.Add(netMessage.NetEntityId, entity);
    }

    public void SyncEntities(NetworkConnection connection)
    {
        if (_network.Server == null)
        {
            throw new InvalidOperationException();
        }

        List<NetEntitySpawnNetMessage> spawnMessages = [];
        foreach (var (netId, entity) in _netEntities)
        {
            spawnMessages.Add(new NetEntitySpawnNetMessage() { NetEntityId = netId, SceneId = entity.Get<SceneComponent>().SceneId });
        }

        _network.Server.Send(connection, new NetEntitySyncNetMessage() { Entities = spawnMessages.ToArray(), States = [] });

        _syncedClients.Add(connection);
    }

    public Entity FindEntityByNetId(NetEntityId id)
    {
        return _netEntities[id];
    }
}