using System.Numerics;

namespace MalignEngine
{
    public abstract class Shader : IDisposable
    {
        public abstract void Set(string name, int value);
        public abstract void Set(string name, uint value);
        public abstract void Set(string name, Matrix4x4 value);
        public abstract void Set(string name, float value);
        public abstract void Set(string name, int[] value);
        public abstract void Set(string name, Color color);
        public abstract void Set(string name, Vector2[] value);
        public abstract void Dispose();
    }
}