using System.Net;

namespace MalignEngine
{
    public interface INetSerializable { }

    public abstract class NetMessage : INetSerializable
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

    public partial class NetworkingSystem : BaseSystem
    {
        private class MessageData
        {
            public Type Type;
            public Action<NetMessage>? Callback;
        }

        [Dependency]
        protected EventSystem EventSystem = default!;

        protected ILogger Logger;

        private Transport transport;
        private NetSerializer.Serializer serializer;

        private Dictionary<string, MessageData> netReceives;

        public override void OnInitialize()
        {
            Logger = LoggerService.GetSawmill("networking");

            netReceives = new Dictionary<string, MessageData>();

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
#elif CLIENT
            RegisterNetMessage<ClientDataNetMessage>(ClientDataReceived);
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

        public override void OnUpdate(float deltaTime)
        {
            transport.Update();
        }
    }
}