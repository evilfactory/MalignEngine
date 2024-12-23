using NetSerializer;
using System.Net;

namespace MalignEngine
{
    public interface INetSerializable { }

    public struct NetId : IComponent
    {
        public uint Id;
    }

    public abstract class NetMessage
    {
        public string MsgName { get; }
        protected NetMessage()
        {
            MsgName = GetType().Name;
        }

        public abstract void Deserialize(IReadMessage message);
        public abstract void Serialize(IWriteMessage message);
    }

    public class ClientDataNetMessage : NetMessage
    {
        public byte ClientId;

        public override void Deserialize(IReadMessage message)
        {
            ClientId = message.ReadByte();
        }

        public override void Serialize(IWriteMessage message)
        {
            message.WriteByte(ClientId);
        }
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

    [Serializable]
    public class ComponentState : INetSerializable { }

    public class ComponentGetState : EntityEventArgs
    {
        public ComponentState State;
    }
    public class ComponentHandleState : EntityEventArgs
    {
        public ComponentState State;
    }

    public partial class NetworkingSystem : BaseSystem
    {
        private class MessageData
        {
            public Type Type;
            public Action<NetMessage>? Callback;
        }

        [Dependency]
        protected EventSystem EventSystem = default!;
        [Dependency]
        protected EntityEventSystem EntityEvent = default!;
        [Dependency]
        protected EntityManagerService EntityManager = default!;
        [Dependency]
        protected AssetSystem AssetSystem = default!;
        [Dependency]
        protected SceneSystem SceneSystem = default!;

        protected ILogger Logger;

        private Transport transport;
        private NetSerializer.Serializer serializer;

        private Dictionary<string, MessageData> netReceives;
        private Dictionary<uint, EntityRef> entities;

        public override void OnInitialize()
        {
            Logger = LoggerService.GetSawmill("networking");

            netReceives = new Dictionary<string, MessageData>();
            entities = new Dictionary<uint, EntityRef>();
            transport = new LidgrenTransport(Logger);

            serializer = new NetSerializer.Serializer(new List<Type>() { typeof(INetSerializable) });

#if SERVER
            transport.OnClientConnected += OnClientConnected;
            transport.OnClientDisconnected += OnClientDisconnected;
#elif CLIENT
            transport.OnConnected += OnConnected;
            transport.OnDisconnected += OnDisconnected;
#endif

            transport.OnMessageReceived += OnMessageReceived;

#if SERVER
            RegisterNetMessage<ClientDataNetMessage>();
            RegisterNetMessage<NetEntitySpawnNetMessage>();
            RegisterNetMessage<NetEntityStateNetMessage>();
            RegisterNetMessage<NetEntitySyncNetMessage>();
#elif CLIENT
            RegisterNetMessage<ClientDataNetMessage>(ClientDataReceived);
            RegisterNetMessage<NetEntitySpawnNetMessage>(ClientNetEntitySpawnReceived);
            RegisterNetMessage<NetEntityStateNetMessage>(ClientNetEntityStateReceived);
            RegisterNetMessage<NetEntitySyncNetMessage>(ClientNetEntitySyncReceived);
#endif
        }

        public void RegisterNetMessage<T>(Action<T>? callback = null) where T : NetMessage
        {
            netReceives.Add(typeof(T).Name, new MessageData() { Type = typeof(T), Callback = (NetMessage message) => callback?.Invoke((T)message) });
        }

        public void SendNetMessage<T>(T message, NetworkConnection target = null) where T : NetMessage
        {
            if (!netReceives.TryGetValue(message.MsgName, out MessageData messageData))
            {
                throw new ArgumentException("Tried to send message of a type that is not registered");
            }

            IWriteMessage writeMessage = new WriteOnlyMessage();
            writeMessage.WriteString(message.MsgName); // very bad, optimize later
            message.Serialize(writeMessage);

#if SERVER
            if (target == null)
            {
                foreach (NetworkConnection connection in Connections)
                {
                    transport.SendToClient(writeMessage, connection);
                }
            }
            else
            {
                transport.SendToClient(writeMessage, target);
            }
#elif CLIENT
            transport.SendToServer(writeMessage);
#endif
        }

        private void OnMessageReceived(IReadMessage message)
        {
            string msgName = message.ReadString();
            if (!netReceives.TryGetValue(msgName, out MessageData messageData))
            {
                Logger.LogWarning($"Received unknown message: {msgName}");
            }

            NetMessage msg = (NetMessage)Activator.CreateInstance(messageData.Type);
            msg.Deserialize(message);

            messageData.Callback?.Invoke(msg);
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

        public override void OnUpdate(float deltaTime)
        {
            transport.Update();
        }
    }
}