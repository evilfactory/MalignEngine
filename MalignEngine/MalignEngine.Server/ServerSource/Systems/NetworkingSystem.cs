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
            Logger.LogInfo($"Client connected: {connection.Id}");

            Connections.Add(connection);

            SendNetMessage(new ClientDataNetMessage() { ClientId = connection.Id }, connection);

            NetEntitySyncNetMessage syncMessage = new NetEntitySyncNetMessage();
            syncMessage.Entities = entities.Values.Select(x => new NetEntitySpawnNetMessage() { EntityId = x.Get<NetId>().Id, SceneId = x.Get<SceneComponent>().SceneId } ).ToArray();
            syncMessage.States = entities.Values.Select(x => CreateNetStateMessage(x)).ToArray();
            SendNetMessage(syncMessage, connection);

            EventSystem.PublishEvent<IEventClientConnected>(x => x.OnClientConnected(connection));
        }

        private void OnClientDisconnected(NetworkConnection connection)
        {
            Logger.LogInfo($"Client disconnected: {connection.Id}");

            Connections.Remove(connection);
        }

        public void SpawnEntity(EntityRef entity)
        {
            uint id = RetrieveNextEntityId();
            entity.Add(new NetId() { Id = id });
            entities.Add(id, entity);

            SendNetMessage(new NetEntitySpawnNetMessage() { EntityId = id, SceneId = entity.Get<SceneComponent>().SceneId });
            SendNetMessage(CreateNetStateMessage(entity));
        }

        private uint RetrieveNextEntityId()
        {
            uint id = 1;
            while (entities.ContainsKey(id))
            {
                id++;
            }
            return id;
        }
    }
}