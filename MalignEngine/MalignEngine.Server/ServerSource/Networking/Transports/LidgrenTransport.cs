using System.Net;
using Lidgren.Network;

namespace MalignEngine
{
    partial class LidgrenTransport : Transport
    {
        private NetServer server;

        private Dictionary<long, NetworkConnection> connections = new Dictionary<long, NetworkConnection>();

        public override void SendToClient(IWriteMessage message, NetworkConnection connection, PacketChannel packetChannel = PacketChannel.Reliable)
        {
            if (server == null) { return; }

            NetOutgoingMessage msg = server.CreateMessage();
            msg.Write(message.Buffer);

            NetConnection netConnection = null;
            foreach ((long remoteId, NetworkConnection c) in connections)
            {
                if (c.Id == connection.Id)
                {
                    netConnection = server.Connections.First(x => x.RemoteUniqueIdentifier == remoteId);
                    break;
                }
            }

            if (netConnection == null)
            {
                throw new Exception("Connection not found");
            }

            switch (packetChannel)
            {
                case PacketChannel.Reliable:
                    server.SendMessage(msg, netConnection, NetDeliveryMethod.ReliableOrdered);
                    break;
                case PacketChannel.Unreliable:
                    server.SendMessage(msg, netConnection, NetDeliveryMethod.Unreliable);
                    break;
                default:
                    throw new Exception("Unknown PacketChannel");
            }
        }

        public override void Listen(int port)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("MalignEngine");
            config.Port = port;

            server = new NetServer(config);
            server.Start();
        }

        public override void Shutdown()
        {
            if (server == null) { return; }

            server.Shutdown("shutdown");

            server = null;
            connections = new Dictionary<long, NetworkConnection>();
        }

        public override void Update()
        {
            if (server == null) { return; }

            NetIncomingMessage msg;
            while ((msg = server.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                        Logger.LogVerbose($"[server] {msg.SenderEndPoint}: {msg.ReadString()}");
                        break;
                    case NetIncomingMessageType.ErrorMessage:
                        Logger.LogError($"[server] {msg.SenderEndPoint}: {msg.ReadString()}");
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();

                        Logger.LogVerbose($"[server] {msg.SenderEndPoint}: Status Changed to {status}");

                        if (status == NetConnectionStatus.Connected)
                        {
                            if (!connections.ContainsKey(msg.SenderConnection.RemoteUniqueIdentifier))
                            {
                                connections.Add(msg.SenderConnection.RemoteUniqueIdentifier, new NetworkConnection(CreateId()));
                            }

                            OnClientConnected(connections[msg.SenderConnection.RemoteUniqueIdentifier]);
                        }

                        if (status == NetConnectionStatus.Disconnected)
                        {
                            OnClientDisconnected(connections[msg.SenderConnection.RemoteUniqueIdentifier], DisconnectReason.Unknown);

                            if (connections.ContainsKey(msg.SenderConnection.RemoteUniqueIdentifier))
                            {
                                connections[msg.SenderConnection.RemoteUniqueIdentifier].IsInvalid = true;
                                connections.Remove(msg.SenderConnection.RemoteUniqueIdentifier);
                            }
                        }

                        break;
                    case NetIncomingMessageType.Data:
                        if (!connections.ContainsKey(msg.SenderConnection.RemoteUniqueIdentifier))
                        {
                            return;
                        }

                        byte[] data = msg.ReadBytes(msg.LengthBytes);
                        IReadMessage readMessage = new ReadOnlyMessage(data, false, 0, data.Length);
                        readMessage.Sender = connections[msg.SenderConnection.RemoteUniqueIdentifier];
                        OnMessageReceived(readMessage);

                        break;
                    default:
                        Logger.LogWarning("[server] Unhandled type: " + msg.MessageType);
                        break;
                }
                server.Recycle(msg);
            }
        }

        public override void DisconnectClient(NetworkConnection connection, DisconnectReason reason)
        {
            if (server == null) { return; }

            NetConnection netConnection = null;
            foreach ((long remoteId, NetworkConnection c) in connections)
            {
                if (c.Id == connection.Id)
                {
                    netConnection = server.Connections.First(x => x.RemoteUniqueIdentifier == remoteId);
                    break;
                }
            }

            if (netConnection == null)
            {
                throw new Exception("Connection not found");
            }

            netConnection.Disconnect("disconnect");
        }


        private byte currentId = 0;
        private byte CreateId()
        {
            currentId++;
            return currentId;
        }
    }
}
