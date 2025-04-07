using NetSerializer;

namespace MalignEngine.Network;

public struct NetId : IComponent
{
    public uint Id;
}

[Serializable]
public class ComponentState : INetSerializable { }

public class ComponentGetState : ComponentEventArgs
{
    public ComponentState State;
}
public class ComponentHandleState : ComponentEventArgs
{
    public ComponentState State;
}

public class NetEntitySpawnNetMessage : NetMessage
{
    public uint EntityId;
    public string SceneId;

    public override void Deserialize(IReadMessage message)
    {
        EntityId = message.ReadUInt32();
        SceneId = message.ReadString();
    }

    public override void Serialize(IWriteMessage message)
    {
        message.WriteUInt32(EntityId);
        message.WriteString(SceneId);
    }
}

public class NetEntityStateNetMessage : NetMessage
{
    public uint EntityId;
    public List<byte[]> States = new List<byte[]>();

    public override void Deserialize(IReadMessage message)
    {
        EntityId = message.ReadUInt32();
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
        message.WriteUInt32(EntityId);
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
    public NetEntitySpawnNetMessage[] Entities;
    public NetEntityStateNetMessage[] States;

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

public class EntityNetworking : EntitySystem
{
    public EntityNetworking()
    {
        RegisterNetMessage<NetEntitySpawnNetMessage>();
        RegisterNetMessage<NetEntityStateNetMessage>();
        RegisterNetMessage<NetEntitySyncNetMessage>();

        RegisterNetMessage<NetEntitySpawnNetMessage>(ClientNetEntitySpawnReceived);
        RegisterNetMessage<NetEntityStateNetMessage>(ClientNetEntityStateReceived);
        RegisterNetMessage<NetEntitySyncNetMessage>(ClientNetEntitySyncReceived);

    }

    private NetEntityStateNetMessage CreateNetStateMessage(EntityRef entity)
    {
        NetEntityStateNetMessage stateMessage = new NetEntityStateNetMessage();
        stateMessage.EntityId = entity.Get<NetId>().Id;

        foreach (IComponent component in entity.GetComponents())
        {
            ComponentGetState getState = new ComponentGetState();
            EntityEvent.RaiseEvent(entity, component, getState);

            if (getState.State != null)
            {
                MemoryStream stream = new MemoryStream();
                serializer.Serialize(stream, getState.State);
                stateMessage.States.Add(stream.ToArray());
            }
        }

        return stateMessage;
    }
}
