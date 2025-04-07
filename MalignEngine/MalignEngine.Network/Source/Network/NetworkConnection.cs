namespace MalignEngine
{
    public class NetworkConnection
    {
        public long Id { get; private set; }

        public object Data { get; set; }

        public bool IsValid
        {
            get {  return Id != 0; }
        }

        public NetworkConnection(long id)
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

        public override string ToString()
        {
            return $"Connection: {Id}";
        }

        internal void Invalidate()
        {
            Id = 0;
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
}