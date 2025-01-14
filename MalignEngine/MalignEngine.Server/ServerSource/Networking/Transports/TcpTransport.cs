using System.Net;
using System.Net.Sockets;
using Lidgren.Network;

namespace MalignEngine
{
    partial class TcpTransport : Transport
    {
        private TcpListener server;

        Dictionary<byte, TcpClient> clients = new Dictionary<byte, TcpClient>();

        public override void SendToClient(IWriteMessage message, NetworkConnection connection, PacketChannel packetChannel = PacketChannel.Reliable)
        {
            if (!clients.ContainsKey(connection.Id))
            {
                throw new Exception("Client not found");
            }

            NetworkStream stream = clients[connection.Id].GetStream();
            stream.Write(message.Buffer, 0, message.LengthBytes);
        }

        public override void Listen(int port)
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            AcceptClients();
        }

        public async void AcceptClients()
        {
            while (server != null)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                var connection = new NetworkConnection(CreateId());
                OnClientConnected?.Invoke(connection);
                clients.Add(connection.Id, client);

                HandleClient(client, connection);
            }
        }

        public async void HandleClient(TcpClient client, NetworkConnection connection)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;

            try
            {
                while (true)
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        clients.Remove(connection.Id);
                        OnClientDisconnected?.Invoke(connection, DisconnectReason.Unknown);
                        break;
                    }

                    IReadMessage message = new ReadOnlyMessage(buffer, false, 0, bytesRead);
                    message.Sender = connection;

                    OnMessageReceived?.Invoke(message);
                }
            }
            catch (Exception exception)
            {
                clients.Remove(connection.Id);
                OnClientDisconnected?.Invoke(connection, DisconnectReason.Unknown);
            }
        }

        public override void Shutdown()
        {
            server.Stop();
            server = null;
        }

        public override void Update()
        {

        }


        private byte currentId = 0;
        private byte CreateId()
        {
            currentId++;
            return currentId;
        }

        public override void DisconnectClient(NetworkConnection connection, DisconnectReason reason)
        {
            TcpClient client = clients[connection.Id];

            client.Close();
            clients.Remove(connection.Id);

            OnClientDisconnected?.Invoke(connection, reason);
        }
    }
}
