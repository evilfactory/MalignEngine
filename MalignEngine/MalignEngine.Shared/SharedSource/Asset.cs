namespace MalignEngine
{
    public abstract class Asset
    {
        public string Identifier { get; private set; }

        public Asset(string identifier)
        {
            Identifier = identifier;
        }
    }
}