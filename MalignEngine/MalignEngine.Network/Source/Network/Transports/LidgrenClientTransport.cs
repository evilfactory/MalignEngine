using System.Net;
using Lidgren.Network;
using Microsoft.Extensions.Logging;

namespace MalignEngine.Network;

public sealed class LidgrenClientTransport : IClientTransport
{
    public event Action? Connected;
    public event Action? Disconnected;
    public event Action<IReadMessage>? Received;

    private readonly NetClient _client;

    public LidgrenClientTransport(string applicationIdentifier)
    {
        var config = new NetPeerConfiguration(applicationIdentifier);

        _client = new NetClient(config);
        _client.Start();
    }

    public void Connect(IPEndPoint endpoint)
    {
        _client.Connect(endpoint.Address.ToString(), endpoint.Port);
    }

    public void Disconnect()
    {
        _client.Disconnect("Disconnected");
    }

    public void Send(IWriteMessage payload, PacketChannel channel)
    {
        var msg = _client.CreateMessage();

        msg.Write(payload.Buffer);

        _client.SendMessage(
            msg,
            channel == PacketChannel.Reliable
                ? NetDeliveryMethod.ReliableOrdered
                : NetDeliveryMethod.Unreliable);
    }

    public void Update()
    {
        NetIncomingMessage? msg;

        while ((msg = _client.ReadMessage()) != null)
        {
            switch (msg.MessageType)
            {
                case NetIncomingMessageType.StatusChanged:
                    {
                        var status = (NetConnectionStatus)msg.ReadByte();

                        switch (status)
                        {
                            case NetConnectionStatus.Connected:
                                Connected?.Invoke();
                                break;

                            case NetConnectionStatus.Disconnected:
                                Disconnected?.Invoke();
                                break;
                        }

                        break;
                    }

                case NetIncomingMessageType.Data:
                    {
                        var reader = new ReadOnlyMessage(msg.Data, false, msg.PositionInBytes, msg.LengthBytes);
                        Received?.Invoke(reader);
                        break;
                    }
            }

            _client.Recycle(msg);
        }
    }
}