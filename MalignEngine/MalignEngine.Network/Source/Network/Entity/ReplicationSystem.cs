namespace MalignEngine.Network;

public class ReplicationNetMessage : NetMessage
{
    public required byte[] Data;

    public override void Deserialize(IReadMessage message)
    {
        int num = message.ReadInt32();
        Data = message.ReadBytes(num);
    }

    public override void Serialize(IWriteMessage message)
    {
        message.WriteInt32(Data.Length);
        message.WriteBytes(Data, 0, Data.Length);
    }
}

public class ReplicationSystem : EntitySystem
{
    private INetworkService _network;
    public IEntityNetwork _entityNetwork;
    private List<IReplicator> _replicators;

    public ReplicationSystem(IServiceContainer serviceContainer, INetworkService network, IEntityNetwork entityNetwork, IEnumerable<IReplicator> replicators) : base(serviceContainer)
    {
        _entityNetwork = entityNetwork;
        _replicators = replicators.ToList();
        _network = network;

        _network.Client?.Register<ReplicationNetMessage>(ReceiveReplicationMessage);
    }

    private void ReceiveReplicationMessage(ReplicationNetMessage message)
    {
        IReadMessage readMessage = new ReadOnlyMessage(message.Data, false, 0, message.Data.Length);

        int count = readMessage.ReadInt32();

        for (int i = 0; i < count; i++)
        {
            var entityId = new NetEntityId() { Value = readMessage.ReadUInt32() };
            var replicatorId = readMessage.ReadUInt16();

            Entity entity = _entityNetwork.FindEntityByNetId(entityId);

            _replicators[replicatorId].Deserialize(new ReplicationContext(null!), entity, readMessage);
        }
    }

    public override void OnUpdate(float deltaTime)
    {
        if (_network.Server == null) { return; }

        foreach (var connection in _entityNetwork.SyncedClients)
        {
            ReplicationContext context = new ReplicationContext(connection);

            List<IWriteMessage> messages = new List<IWriteMessage>();

            EntityManager.Query(new Query().Include<NetEntityId>(), entity =>
            {
                for (int i = 0; i < _replicators.Count; i++)
                {
                    IReplicator replicator = _replicators[i];

                    if (EntityManager.HasComponent(entity, replicator.ComponentType) && replicator.HasChanged(context, entity))
                    {
                        var message = new WriteOnlyMessage();
                        message.WriteUInt32(entity.Get<NetEntityId>().Value);
                        message.WriteUInt16((UInt16)i);
                        _replicators[i].Replicate(context, entity, message);
                        messages.Add(message);
                    }
                }
            });

            IWriteMessage data = new WriteOnlyMessage();
            data.WriteInt32(messages.Count);
            messages.ForEach(message => data.WriteBytes(message.Buffer, 0, message.LengthBytes));

            _network.Server.Send(connection, new ReplicationNetMessage()
            {
                Data = data.Buffer,
                Channel = PacketChannel.Reliable
            });
        }
    }
}