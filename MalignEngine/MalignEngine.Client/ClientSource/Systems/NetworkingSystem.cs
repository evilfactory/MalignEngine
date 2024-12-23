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

        private void OnConnected()
        {

        }

        private void OnDisconnected()
        {
            Connection = null;

            EventSystem.PublishEvent<IEventDisconnected>(x => x.OnDisconnected());
        }

        private void ClientDataReceived(ClientDataNetMessage clientData)
        {
            Logger.LogInfo($"Connected to server: {clientData.ClientId}");

            Connection = new NetworkConnection(clientData.ClientId);

            EventSystem.PublishEvent<IEventConnected>(x => x.OnConnected());
        }

        private void ClientNetEntitySpawnReceived(NetEntitySpawnNetMessage entitySpawn)
        {
            EntityRef entity = SceneSystem.LoadScene(AssetSystem.GetOfType<Scene>().Where(x => x.Asset.SceneId == entitySpawn.SceneId).First());
            entity.Add(new NetId() { Id = entitySpawn.EntityId });
            entities.Add(entitySpawn.EntityId, entity);
        }

        private void ClientNetEntityStateReceived(NetEntityStateNetMessage states)
        {
            EntityRef entity = entities[states.EntityId];

            foreach (byte[] state in states.States)
            {
                ComponentState compState = (ComponentState)serializer.Deserialize(new MemoryStream(state));
                EntityEvent.RaiseEvent(entity, new ComponentHandleState() { State = compState });
            }
        }

        private void ClientNetEntitySyncReceived(NetEntitySyncNetMessage sync)
        {
            foreach (NetEntitySpawnNetMessage entity in sync.Entities)
            {
                ClientNetEntitySpawnReceived(entity);
            }

            for (int i = 0; i < sync.States.Length; i++)
            {
                ClientNetEntityStateReceived(sync.States[i]);
            }
        }
    }
}