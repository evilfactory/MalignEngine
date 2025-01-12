using Lidgren.Network;
using System.Net;

namespace MalignEngine
{
    partial class LidgrenTransport : Transport
    {
        private NetClient client;

        public override void SendToServer(IWriteMessage message, PacketChannel packetChannel = PacketChannel.Reliable)
        {
            if (client == null) { return; }

            NetOutgoingMessage msg = client.CreateMessage();
            msg.Write(message.Buffer);

            switch (packetChannel)
            {
                case PacketChannel.Reliable:
                    client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
                    break;
                case PacketChannel.Unreliable:
                    client.SendMessage(msg, NetDeliveryMethod.Unreliable);
                    break;
                default:
                    throw new Exception("Unknown PacketChannel");
            }
        }

        public override void Connect(IPEndPoint endpoint)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("MalignEngine");

            client = new NetClient(config);
            client.Start();
            client.Connect(endpoint);
        }

        public override void Disconnect()
        {
            if (client == null) { return; }

            client.Disconnect("disconnect");
        }

        public override void Update()
        {
            if (client == null) { return; }

            NetIncomingMessage msg;
            while ((msg = client.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                        Logger.LogVerbose($"[client] {msg.SenderEndPoint}: {msg.ReadString()}");
                        break;
                    case NetIncomingMessageType.ErrorMessage:
                        Logger.LogError($"[client] {msg.SenderEndPoint}: {msg.ReadString()}");
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();

                        Logger.LogVerbose($"[client] {msg.SenderEndPoint}: Status Changed to {status}");

                        if (status == NetConnectionStatus.Connected)
                        {
                            OnConnected();
                        }

                        if (status == NetConnectionStatus.Disconnected)
                        {
                            OnDisconnected();
                        }

                        break;
                    case NetIncomingMessageType.Data:
                        byte[] data = msg.ReadBytes(msg.LengthBytes);
                        IReadMessage readMessage = new ReadOnlyMessage(data, false, 0, data.Length);
                        OnMessageReceived(readMessage);

                        break;
                    default:
                        Logger.LogWarning("[client] Unhandled type: " + msg.MessageType);
                        break;
                }
                client.Recycle(msg);
            }
        }
    }
}
