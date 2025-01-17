namespace MalignEngine
{
    public struct NameComponent : IComponent
    {
        public string Name;

        public NameComponent(string name)
        {
            Name = name;
        }
    }
}