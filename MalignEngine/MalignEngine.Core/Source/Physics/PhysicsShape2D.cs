using System.Numerics;

namespace MalignEngine;

public enum ShapeType
{
    Polygon,
    Circle
}

public interface IPhysicsShape2D
{
    public ShapeType Type { get; }
    public Vector2[] Vertices { get; }
    public float Radius { get; }
}

public class RectangleShape2D : IPhysicsShape2D
{
    public ShapeType Type => ShapeType.Polygon;
    public Vector2[] Vertices { get; private set; }
    public float Radius => throw new NotImplementedException();
    public Vector2 Offset = Vector2.Zero;

    public RectangleShape2D(float width, float height) : this(width, height, Vector2.Zero) { }
    public RectangleShape2D(float width, float height, Vector2 offset)
    {
        Offset = offset;

        Vertices = new Vector2[]
        {
            new Vector2(-width / 2, -height / 2) + offset,
            new Vector2(width / 2, -height / 2) + offset,
            new Vector2(width / 2, height / 2) + offset,
            new Vector2(-width / 2, height / 2) + offset
        };
    }
}

public class CircleShape2D : IPhysicsShape2D
{
    public ShapeType Type => ShapeType.Circle;
    public Vector2[] Vertices { get; private set; }
    public float Radius { get; private set; }

    public CircleShape2D(float radius)
    {
        Radius = radius;
    }
}