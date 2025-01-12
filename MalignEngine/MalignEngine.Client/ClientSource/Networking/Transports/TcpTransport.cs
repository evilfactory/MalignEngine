using System.Net;
using System.Net.Sockets;

namespace MalignEngine
{
    partial class TcpTransport : Transport
    {
        private TcpClient client;

        public override void SendToServer(IWriteMessage message, PacketChannel packetChannel = PacketChannel.Reliable)
        {
            NetworkStream stream = client.GetStream();
            stream.Write(message.Buffer, 0, message.LengthBytes);
        }

        public override void Connect(IPEndPoint endpoint)
        {
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

            while (true)
            {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    OnDisconnected?.Invoke();
                    break;
                }

                IReadMessage message = new ReadOnlyMessage(buffer, false, 0, buffer.Length);

                OnMessageReceived?.Invoke(message);
            }
        }

        public override void Disconnect()
        {
            client.Close();
            client = null;
        }

        public override void Update()
        {

        }
    }
}
