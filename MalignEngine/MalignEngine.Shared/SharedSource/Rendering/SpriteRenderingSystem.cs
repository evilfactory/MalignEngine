using Arch.Core;
using Arch.Core.Extensions;
using System.Numerics;

namespace MalignEngine
{
    public class SpriteRenderingSystem : EntitySystem
    {
        [Dependency]
        protected RenderingSystem RenderingSystem = default!;

        private Shader shader;

        public override void Initialize()
        {
            using (var stream = File.OpenRead("Content/SpriteShader.glsl"))
            {
                shader = RenderingSystem.LoadShader(stream);
            }
        }

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

                RenderingSystem.DrawTexture2D(spriteRenderer.Sprite.Texture, pos.Position, scale, spriteRenderer.Sprite.Origin, spriteRenderer.Sprite.Rect, spriteRenderer.Color, rotation, depth);
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