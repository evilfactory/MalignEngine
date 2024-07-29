using Arch.Core;
using Arch.Core.Extensions;
using System.Numerics;

namespace MalignEngine
{
    public class Transform2DSystem : BaseSystem
    {
        public void SetPosition(in Entity entity, Vector2 position)
        {
            entity.Get<Position2D>().Position = position;
            entity.Add<Dirty<Position2D>>();
        }

        public void SetRotation(in Entity entity, float rotation)
        {
            entity.Get<Rotation2D>().Rotation = rotation;
            entity.Add<Dirty<Rotation2D>>();
        }
    }
}