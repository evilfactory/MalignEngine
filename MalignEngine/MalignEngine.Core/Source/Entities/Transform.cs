using System.Numerics;

namespace MalignEngine;

public static class VectorExtensions
{
    public static Vector2 ToVector2(this Vector3 vec3) => new Vector2(vec3.X, vec3.Y);
    public static Vector3 ToVector3(this Vector2 vec2, float z = 0) => new Vector3(vec2, z);
}

public class TransformSystem : EntitySystem, IPostUpdate
{
    private readonly HierarchySystem _hierarchySystem;

    public TransformSystem(IServiceContainer serviceContainer, HierarchySystem parentSystem) 
        : base(serviceContainer)
    {
        _hierarchySystem = parentSystem;
    }

    public void OnPostUpdate(float deltaTime)
    {
        foreach (Entity root in _hierarchySystem.RootEntities)
        {
            UpdateTransformTree(root, null);
        }
    }

    private void UpdateTransformTree(Entity entity, WorldTransform? parent)
    {
        if (!entity.TryGet(out ComponentRef<Transform> local))
        {
            return;
        }

        WorldTransform world;

        if (parent is null)
        {
            world = CreateRoot(local.Value);
        }
        else
        {
            world = Compose(local.Value, parent.Value);
        }
            
        entity.AddOrSet(world);

        if (!entity.TryGet(out ComponentRef<Children> children)) 
        { 
            return;
        }

        foreach (Entity child in children.Value.Values)
        {
            UpdateTransformTree(child, world);
        }
    }

    private static WorldTransform CreateRoot(Transform local)
    {
        WorldTransform world = new();

        world.Position = local.Position;
        world.Rotation = local.Rotation;
        world.Scale = local.Scale;

        world.Matrix =
            Matrix4x4.CreateScale(world.Scale) *
            Matrix4x4.CreateFromQuaternion(world.Rotation) *
            Matrix4x4.CreateTranslation(world.Position);

        return world;
    }

    public static WorldTransform Compose(Transform local, WorldTransform parent)
    {
        WorldTransform world = new();

        world.Scale = parent.Scale * local.Scale;

        world.Rotation = parent.Rotation * local.Rotation;

        world.Position = parent.Position + Vector3.Transform(local.Position * parent.Scale, parent.Rotation);

        world.Matrix =
            Matrix4x4.CreateScale(world.Scale) *
            Matrix4x4.CreateFromQuaternion(world.Rotation) *
            Matrix4x4.CreateTranslation(world.Position);

        return world;
    }
}

public static class TransformExtensions
{
    public static float GetRotation2D(this Transform t) => MathHelper.ToEulerAngles(t.Rotation).Z;

    public static void SetRotation2D(this ref Transform t, float angle)
    {
        t.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, angle);
    }

    public static float GetRotation2D(this WorldTransform t) => MathHelper.ToEulerAngles(t.Rotation).Z;

    public static void SetRotation2D(this ref WorldTransform t, float angle)
    {
        t.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, angle);
    }
}

[Serializable]
public struct Transform : IComponent
{
    [DataField("Position", save: true)] 
    public Vector3 Position;
    [DataField("Rotation", save: true)]
    public Quaternion Rotation;
    [DataField("Scale", save: true)] 
    public Vector3 Scale;

    public Transform()
    {
        Position = Vector3.Zero;
        Rotation = Quaternion.Identity;
        Scale = Vector3.One;
    }
}

public struct WorldTransform : IComponent
{
    public Matrix4x4 Matrix;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
}