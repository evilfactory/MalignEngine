using NetSerializer;

namespace MalignEngine.Network;


[Serializable]
public class ComponentState : INetSerializable { }

/*
public class ComponentGetState : ComponentEventArgs
{
    public ComponentState State;
}
public class ComponentHandleState : ComponentEventArgs
{
    public ComponentState State;
}
*/

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

public class EntityNetworkSystem : EntitySystem
{
    private readonly INetworkService _networkService;
    private readonly IClientSessionRetrieval _clientSessionRetrieval;

    public EntityNetworkSystem(IServiceContainer serviceContainer, INetworkService networkService, IClientSessionRetrieval clientSessionRetrieval) : base(serviceContainer)
    {
        _networkService = networkService;
        _clientSessionRetrieval = clientSessionRetrieval;
    }

    public void SpawnEntity(Entity entity)
    {

    }
}