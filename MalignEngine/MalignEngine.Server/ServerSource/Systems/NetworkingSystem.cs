using System.Net;

namespace MalignEngine
{
    public interface IEventClientConnected : IEvent
    {
        void OnClientConnected(NetworkConnection connection);
    }

    public interface IEventClientDisconnected : IEvent
    {
        void OnClientDisconnected(NetworkConnection connection);
    }


    partial class NetworkingSystem : BaseSystem
    {
        public List<NetworkConnection> Connections = new List<NetworkConnection>();

        public void StartServer(int port)
        {
            transport.Listen(port);
        }

        public void StopServer()
        {
            transport.Shutdown();
        }

        private void OnClientConnected(NetworkConnection connection)
        {
            SendNetMessage(new ClientDataNetMessage() { ClientId = connection.Id });

            Connections.Add(connection);

            EventSystem.PublishEvent<IEventClientConnected>(x => x.OnClientConnected(connection));
        }

        private void OnClientDisconnected(NetworkConnection connection)
        {
            Connections.Remove(connection);
        }
    }
}