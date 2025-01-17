using System.Net;
using System.Net.Sockets;

namespace MalignEngine
{
    partial class TcpTransport : Transport
    {
        private TcpClient client;

        private Queue<IWriteMessage> sendQueue = new Queue<IWriteMessage>();
        private Queue<IReadMessage> receiveQueue = new Queue<IReadMessage>();

        public override void SendToServer(IWriteMessage message, PacketChannel packetChannel = PacketChannel.Reliable)
        {
            if (client == null)
            {
                throw new Exception("Client not connected");
            }

            sendQueue.Enqueue(message);
        }

        public override void Connect(IPEndPoint endpoint)
        {
            sendQueue.Clear();
            receiveQueue.Clear();

            client = new TcpClient();
            client.Connect(endpoint);

            OnConnected?.Invoke();

            ReceiveDataAsync();
        }

        private async void ReceiveDataAsync()
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
                        Disconnect(DisconnectReason.Unknown);
                        break;
                    }

                    receiveQueue.Enqueue(new ReadOnlyMessage(buffer, false, 0, bytesRead));
                }
            }
            catch
            {
                Disconnect(DisconnectReason.Unknown);
            }
        }

        public override void Disconnect(DisconnectReason reason)
        {
            client.Close();
            client = null;

            OnDisconnected?.Invoke(reason);
        }

        public override void Update()
        {
            if (client == null || !client.Connected) { return; }

            while (sendQueue.Count > 0)
            {
                IWriteMessage message = sendQueue.Dequeue();

                NetworkStream stream = client.GetStream();
                stream.Write(message.Buffer, 0, message.LengthBytes);
            }

            while (receiveQueue.Count > 0)
            {
                OnMessageReceived?.Invoke(receiveQueue.Dequeue());
            }
        }
    }
}
