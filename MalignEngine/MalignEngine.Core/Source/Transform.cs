using Arch.Core;
using Arch.Core.Extensions;
using System.Numerics;

namespace MalignEngine
{
    public static class VectorExtensions
    {
        public static Vector2 ToVector2(this Vector3 vec3) => new Vector2(vec3.X, vec3.Y);
        public static Vector3 ToVector3(this Vector2 vec2, float z = 0) => new Vector3(vec2, z);
    }

    public class TransformMoveEvent : ComponentEventArgs
    {
        EntityRef Entity;
        public Transform Transform;

        public TransformMoveEvent(EntityRef entity, Transform transform)
        {
            Entity = entity;
            Transform = transform;
        }
    }

    public class TransformSystem : EntitySystem, IPostUpdate
    {
        [Dependency]
        protected ParentSystem ParentSystem = default!;

        public override void OnInitialize()
        {
            EventService.Register(new ComponentEventChannel<TransformMoveEvent>());

            // remove, stupid
            EventService.Get<ComponentEventChannel<ComponentAddedEvent>>().Subscribe<Transform>((entity, args) =>
            {
                if (!entity.Has<WorldTransform>())
                {
                    entity.Add(new WorldTransform());
                }
            });
        }

        public void OnPostUpdate(float deltaTime)
        {
            foreach (EntityRef entity in ParentSystem.RootEntities)
            {
                UpdateTransformTree(entity);
            }

            EntityManager.World.Query(EntityManager.World.CreateQuery().WithAll<Transform, WorldTransform>(), (EntityRef entity, ref Transform transform, ref WorldTransform worldTransform) =>
            {
                if (transform.LastPosition != transform.Position)
                {
                    EventService.Get<ComponentEventChannel<TransformMoveEvent>>().Raise(entity, new TransformMoveEvent(entity, transform));
                }

                transform.LastPosition = transform.Position;
                worldTransform.LastPosition = worldTransform.Position;
            });
        }

        public void UpdateTransformTree(EntityRef root)
        {
            if (root.Has<WorldTransform>() && root.TryGet(out Transform parentTransform))
            {
                if (!root.Has<ParentOf>())
                {
                    root.Set(new WorldTransform(parentTransform));
                }
            }

            if (root.TryGet(out Children children))
            {
                foreach (EntityRef child in children.Childs)
                {
                    // Update WorldPosition
                    if (child.TryGet(out Transform transform))
                    {
                        if (root.TryGet(out WorldTransform parentWorldTransform))
                        {
                            // put WorldTransform which is the world position/rotation relative to the root entity
                            child.Set(new WorldTransform(transform.Position + parentWorldTransform.Position, transform.Rotation, transform.Scale));
                        }
                        else
                        {
                            child.Set(new WorldTransform(transform));
                        }
                    }

                    UpdateTransformTree(child);
                }
            }
        }
    }

    [Serializable]
    public struct Transform : IComponent
    {
        [DataField("Position", save: true)] public Vector3 Position;
        public Quaternion Rotation;
        [DataField("Scale", save: true)] public Vector3 Scale;

        [DataField("EulerAngles", save: true)] 
        public Vector3 EulerAngles
        {
            get => MathHelper.ToEulerAngles(Rotation);
            set => Rotation = Quaternion.CreateFromYawPitchRoll(value.Y, value.X, value.Z);
        }

        [DataField("ZAxis", save: false)]
        public float ZAxis
        {
            get => EulerAngles.Z;
            set
            {
                Vector3 euler = MathHelper.ToEulerAngles(Rotation);
                euler.Z = value;
                Rotation = Quaternion.CreateFromYawPitchRoll(euler.Y, euler.X, euler.Z);
            }
        }

        public Vector3 LastPosition;

        public Transform()
        {
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
        }

        public Transform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public Transform(Vector3 position, Vector3 eulerAngles, Vector3 scale)
        {
            Position = position;
            Rotation = Quaternion.CreateFromYawPitchRoll(eulerAngles.Y, eulerAngles.X, eulerAngles.Z);
            Scale = scale;
        }

        public Transform(Vector3 position, Vector3 eulerAngles)
        {
            Position = position;
            Rotation = Quaternion.CreateFromYawPitchRoll(eulerAngles.Y, eulerAngles.X, eulerAngles.Z);
            Scale = Vector3.One;
        }

        public Transform(Vector3 position)
        {
            Position = position;
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
        }

        public Transform(Vector2 position)
        {
            Position = position.ToVector3();
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
        }

        public Transform(Vector2 position, float rotation, Vector2 scale)
        {
            Position = position.ToVector3();
            Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, rotation);
            Scale = new Vector3(scale, 1);
        }

        public Transform(Vector2 position, float rotation)
        {
            Position = new Vector3(position, 0);
            Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, rotation);
            Scale = Vector3.One;
        }
    }

    public struct WorldTransform : IComponent
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public Vector3 LastPosition;

        public Vector3 EulerAngles
        {
            get => MathHelper.ToEulerAngles(Rotation);
            set => Rotation = Quaternion.CreateFromYawPitchRoll(value.Y, value.X, value.Z);
        }

        public float ZAxis
        {
            get => EulerAngles.Z;
            set
            {
                Vector3 euler = MathHelper.ToEulerAngles(Rotation);
                euler.Z = value;
                Rotation = Quaternion.CreateFromYawPitchRoll(euler.Y, euler.X, euler.Z);
            }
        }

        public WorldTransform()
        {
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
        }

        public WorldTransform(Transform transform)
        {
            Position = transform.Position;
            Rotation = transform.Rotation;
            Scale = transform.Scale;
        }

        public WorldTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public WorldTransform(Vector3 position, Vector3 eulerAngles, Vector3 scale)
        {
            Position = position;
            Rotation = Quaternion.CreateFromYawPitchRoll(eulerAngles.Y, eulerAngles.X, eulerAngles.Z);
            Scale = scale;
        }

        public WorldTransform(Vector3 position, Vector3 eulerAngles)
        {
            Position = position;
            Rotation = Quaternion.CreateFromYawPitchRoll(eulerAngles.Y, eulerAngles.X, eulerAngles.Z);
            Scale = Vector3.One;
        }

        public WorldTransform(Vector3 position)
        {
            Position = position;
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
        }

        public WorldTransform(Vector2 position, float rotation, Vector2 scale)
        {
            Position = new Vector3(position, 0);
            Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, rotation);
            Scale = new Vector3(scale, 1);
        }

        public WorldTransform(Vector2 position, float rotation)
        {
            Position = new Vector3(position, 0);
            Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, rotation);
            Scale = Vector3.One;
        }
    }
}