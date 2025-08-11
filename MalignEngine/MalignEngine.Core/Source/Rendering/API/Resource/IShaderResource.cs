using System.Numerics;

namespace MalignEngine;

public interface IShaderResource : IGpuResource
{
    void Set(string name, int value);
    void Set(string name, uint value);
    void Set(string name, Matrix4x4 value);
    void Set(string name, float value);
    void Set(string name, int[] value);
    void Set(string name, Color color);
    void Set(string name, Vector2[] value);
}
