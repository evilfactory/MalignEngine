using System.Net;
using System.Net.Sockets;
using Lidgren.Network;

namespace MalignEngine
{
    partial class TcpTransport : Transport
    {
        private class QueuedSendMessage
        {
            public IWriteMessage Message { get; set; }
            public NetworkConnection Connection { get; set; }
        }

        private class QueuedReceiveMessage
        {
            public IReadMessage Message { get; set; }
            public NetworkConnection Connection { get; set; }
        }

        private TcpListener server;

        private Queue<QueuedSendMessage> sendQueue = new Queue<QueuedSendMessage>();
        private Queue<QueuedReceiveMessage> receiveQueue = new Queue<QueuedReceiveMessage>();

        Dictionary<byte, TcpClient> clients = new Dictionary<byte, TcpClient>();

        public override void SendToClient(IWriteMessage message, NetworkConnection connection, PacketChannel packetChannel = PacketChannel.Reliable)
        {
            if (!clients.ContainsKey(connection.Id))
            {
                throw new Exception("Client not found");
            }

            sendQueue.Enqueue(new QueuedSendMessage
            {
                Message = message,
                Connection = connection
            });
        }

        public override void Listen(int port)
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            AcceptClients();
        }

        public override void Shutdown()
        {
            server.Stop();
            server = null;
        }

        public override void Update()
        {
            if (server == null) { return; }

            while (sendQueue.Count > 0)
            {
                QueuedSendMessage message = sendQueue.Dequeue();
                NetworkConnection connection = message.Connection;

                if (connection.IsInvalid)
                {
                    Logger.LogWarning($"Dropped packet for invalid connection {connection}");
                    continue;
                }

                try
                {
                    NetworkStream stream = clients[connection.Id].GetStream();
                    stream.Write(message.Message.Buffer, 0, message.Message.LengthBytes);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"Dropped packet to connection {connection}, error = {ex.Message}");
                    continue;
                }
            }

            while (receiveQueue.Count > 0)
            {
                QueuedReceiveMessage message = receiveQueue.Dequeue();
                if (message.Connection.IsInvalid)
                {
                    Logger.LogWarning($"Ignored packet from invalid connection {message.Connection}");
                    continue;
                }

                OnMessageReceived?.Invoke(message.Message);
            }
        }


        public override void DisconnectClient(NetworkConnection connection, DisconnectReason reason)
        {
            TcpClient client = clients[connection.Id];

            client.Close();
            clients.Remove(connection.Id);
            connection.IsInvalid = true;

            OnClientDisconnected?.Invoke(connection, reason);
        }

        private async void AcceptClients()
        {
            while (server != null)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                var connection = new NetworkConnection(CreateId());
                connection.IsInvalid = false;
                OnClientConnected?.Invoke(connection);
                clients.Add(connection.Id, client);

                HandleClient(client, connection);
            }
        }

        private async void HandleClient(TcpClient client, NetworkConnection connection)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;

            try
            {
                while (true)
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (connection.IsInvalid) { return; }

                    if (bytesRead == 0)
                    {
                        DisconnectClient(connection, DisconnectReason.Unknown);
                        break;
                    }

                    IReadMessage message = new ReadOnlyMessage(buffer, false, 0, bytesRead);
                    message.Sender = connection;

                    receiveQueue.Enqueue(new QueuedReceiveMessage
                    {
                        Message = message,
                        Connection = connection
                    });
                }
            }
            catch (Exception exception)
            {
                Logger.LogVerbose(exception.ToString());
                DisconnectClient(connection, DisconnectReason.Unknown);
            }
        }

        private byte currentId = 0;
        private byte CreateId()
        {
            currentId++;
            return currentId;
        }
    }
}
