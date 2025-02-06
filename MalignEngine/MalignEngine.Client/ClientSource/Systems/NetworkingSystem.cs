using System.Net;

namespace MalignEngine
{
    public interface IEventConnected : ISchedule
    {
        void OnConnected();
    }

    public interface IEventDisconnected : ISchedule
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

        public void Disconnect(DisconnectReason reason)
        {
            transport.Disconnect(reason);
        }

        private void OnConnected()
        {

        }

        private void OnDisconnected(DisconnectReason reason)
        {
            Connection = null;

            EventSystem.Run<IEventDisconnected>(x => x.OnDisconnected());
        }

        private void ClientDataReceived(ClientDataNetMessage clientData)
        {
            Logger.LogInfo($"Connected to server: {clientData.ClientId}");

            Connection = new NetworkConnection(clientData.ClientId);

            EventSystem.Run<IEventConnected>(x => x.OnConnected());
        }

        private void ClientNetEntitySpawnReceived(NetEntitySpawnNetMessage entitySpawn)
        {
            EntityRef entity = SceneSystem.Instantiate(AssetSystem.GetFromId<Scene>(entitySpawn.SceneId));
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