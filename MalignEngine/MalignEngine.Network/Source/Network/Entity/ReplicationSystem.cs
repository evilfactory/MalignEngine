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
            Logger.LogInfo($"Entity id: {entityId.Value}");
            var replicatorId = readMessage.ReadUInt16();

            _replicators[replicatorId].Deserialize(_entityNetwork.FindEntityByNetId(entityId), readMessage);
        }
    }

    public override void OnUpdate(float deltaTime)
    {
        if (_network.Server == null) { return; }

        List<IWriteMessage> messages = new List<IWriteMessage>();

        EntityManager.Query(new Query().Include<NetEntityId>(), entity =>
        {
            for (int i = 0; i < _replicators.Count; i++)
            {
                if (EntityManager.HasComponent(entity, _replicators[i].ComponentType))
                {
                    var message = new WriteOnlyMessage();
                    message.WriteUInt32(entity.Get<NetEntityId>().Value);
                    message.WriteUInt16((UInt16)i);
                    _replicators[i].Serialize(entity, message);
                    messages.Add(message);
                }
            }
        });

        IWriteMessage data = new WriteOnlyMessage();
        data.WriteInt32(messages.Count);
        messages.ForEach(message => data.WriteBytes(message.Buffer, 0, message.LengthBytes));

        foreach (var connection in _entityNetwork.SyncedClients)
        {
            _network.Server.Send(connection, new ReplicationNetMessage()
            {
                Data = data.Buffer,
                Channel = PacketChannel.Reliable
            });
        }
    }
}