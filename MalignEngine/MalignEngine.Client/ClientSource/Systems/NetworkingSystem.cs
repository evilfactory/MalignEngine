using System.Net;

namespace MalignEngine
{
    public interface IEventConnected : IEvent
    {
        void OnConnected();
    }

    public interface IEventDisconnected : IEvent
    {
        void OnDisconnected();
    }


    partial class NetworkingSystem : BaseSystem
    {
        public NetworkConnection Connection { get; private set; }


        public void Connect(IPEndPoint endpoint)
        {
            transport.Connect(endpoint);
        }

        public void Disconnect()
        {
            transport.Disconnect();
        }

        private void ClientDataReceived(ClientDataNetMessage clientData)
        {
            Connection = new NetworkConnection(clientData.ClientId);

            EventSystem.PublishEvent<IEventConnected>(x => x.OnConnected());
        }

        private void OnConnected()
        {

        }

        private void OnDisconnected()
        {
            Connection = null;

            EventSystem.PublishEvent<IEventDisconnected>(x => x.OnDisconnected());
        }
    }
}