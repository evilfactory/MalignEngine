using System.Numerics;

namespace MalignEngine
{
    [RegisterComponent]
    public struct Position2D
    {
        public Vector2 Position;

        public float X
        {
            get => Position.X;
            set => Position.X = value;
        }

        public float Y
        {
            get => Position.Y;
            set => Position.Y = value;
        }

        public Position2D(Vector2 position)
        {
            Position = position;
        }

        public Position2D(float x, float y)
        {
            Position = new Vector2(x, y);
        }

        public Position2D()
        {
            Position = Vector2.Zero;
        }
    }

    [RegisterComponent]
    public struct Rotation2D
    {
        public float Rotation;

        public Rotation2D(float rotation)
        {
            Rotation = rotation;
        }

        public Rotation2D()
        {
            Rotation = 0;
        }
    }

    [RegisterComponent]
    public struct Depth
    {
        public float Value;

        public Depth(float value)
        {
            Value = value;
        }

        public Depth()
        {
            Value = 0;
        }
    }

    [RegisterComponent]
    public struct Scale2D
    {
        public Vector2 Scale;

        public Scale2D(Vector2 scale)
        {
            Scale = scale;
        }

        public Scale2D(float x, float y)
        {
            Scale = new Vector2(x, y);
        }

        public Scale2D()
        {
            Scale = Vector2.One;
        }
    }
}