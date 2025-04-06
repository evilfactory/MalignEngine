using System.Net;
using Lidgren.Network;
using Microsoft.Extensions.Logging;

namespace MalignEngine.Network;

public partial class LidgrenTransport : ServerTransport, ClientTransport
{
    private ILogger logger;

    private NetClient client;

    public event Action<IReadMessage> OnData;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public event Action<NetworkConnection> OnClientConnected;
    public event Action<NetworkConnection> OnClientDisconnected;
    public event Action<IReadMessage> OnClientData;

    public LidgrenTransport(ILogger networkLogger)
    {
        logger = networkLogger;
    }

    event Action<NetworkConnection, IReadMessage> ServerTransport.OnData
    {
        add
        {
            throw new NotImplementedException();
        }

        remove
        {
            throw new NotImplementedException();
        }
    }

    public void SendData(IWriteMessage message, PacketChannel packetChannel = PacketChannel.Reliable)
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

    public void TryConnect(IPEndPoint endpoint)
    {
        NetPeerConfiguration config = new NetPeerConfiguration("MalignEngine");

        client = new NetClient(config);
        client.Start();
        client.Connect(endpoint);
    }

    public void Disconnect()
    {
        if (client == null) { return; }

        client.Disconnect("disconnect");
    }

    public void Update()
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
                    logger.LogVerbose($"[client] {msg.SenderEndPoint}: {msg.ReadString()}");
                    break;
                case NetIncomingMessageType.ErrorMessage:
                    logger.LogError($"[client] {msg.SenderEndPoint}: {msg.ReadString()}");
                    break;
                case NetIncomingMessageType.StatusChanged:
                    NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();

                    logger.LogVerbose($"[client] {msg.SenderEndPoint}: Status Changed to {status}");

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
                    OnData(readMessage);

                    break;
                default:
                    logger.LogWarning("[client] Unhandled type: " + msg.MessageType);
                    break;
            }
            client.Recycle(msg);
        }
    }

    public void SendData(NetworkConnection connection, IWriteMessage message, PacketChannel channel)
    {
        throw new NotImplementedException();
    }

    public void Disconnect(NetworkConnection connection)
    {
        throw new NotImplementedException();
    }

    public void Listen(IPEndPoint endpoint)
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }
}
