namespace MalignEngine
{
    public class NetworkConnection
    {
        public byte Id { get; private set; }

        public bool IsInvalid { get; set; } = true;

        public object Data { get; set; }

        public NetworkConnection(byte id)
        {
            Id = id;
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
    }
}