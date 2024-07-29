using Arch.Core;
using Arch.Core.Extensions;
using Microsoft.Xna.Framework;

namespace MalignEngine
{
    public class SpriteRenderingSystem : EntitySystem
    {
        [Dependency]
        protected RenderingSystem RenderingSystem = default!;

        public override void Draw(float deltaTime)
        {
            RenderingSystem.Begin();
            var query = new QueryDescription().WithAll<SpriteRenderer, Position2D>();
            World.Query(query, (Entity entity, ref Position2D pos, ref SpriteRenderer spriteRenderer) =>
            {
                Vector2 scale = new Vector2(1, 1);
                float rotation = 0;
                float depth = 0;
                if (entity.Has<Scale2D>()) { scale = entity.Get<Scale2D>().Scale; }
                if (entity.Has<Rotation2D>()) { rotation = entity.Get<Rotation2D>().Rotation; }
                if (entity.Has<Depth>()) { depth = entity.Get<Depth>().Value; }

                RenderingSystem.Draw(spriteRenderer.Sprite.Texture, pos.Position, spriteRenderer.Sprite.Rect, spriteRenderer.Color, rotation, spriteRenderer.Sprite.Origin, scale, depth);
            });
            RenderingSystem.End();
        }
    }

    [RegisterComponent]
    public struct SpriteRenderer
    {
        public Sprite Sprite;
        public Color Color;
    }
}