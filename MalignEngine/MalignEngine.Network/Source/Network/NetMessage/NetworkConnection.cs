namespace MalignEngine.Network;

public class NetworkConnection
{
    public readonly long Id;
    public object? Tag { get; set; }

    private bool _connected = false;

    internal ITransport Transport { get; }
    public bool IsConnected => _connected;

    public NetworkConnection(ITransport transport, long id)
    {
        Id = id;
        Transport = transport;
        _connected = true;
    }

    public override bool Equals(object? obj)
    {
        if (obj is NetworkConnection connection)
        {
            return connection.Id == Id;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string ToString()
    {
        return $"Connection: {Id}";
    }

    internal void Invalidate()
    {
        _connected = false;
    }

    public static bool operator ==(NetworkConnection a, NetworkConnection b)
    {
        return a?.Id == b?.Id;
    }

    public static bool operator !=(NetworkConnection a, NetworkConnection b)
    {
        return a?.Id != b?.Id;
    }
}