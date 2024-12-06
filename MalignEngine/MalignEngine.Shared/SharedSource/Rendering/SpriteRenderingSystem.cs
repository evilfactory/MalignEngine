using Arch.Core;
using Arch.Core.Extensions;
using System.Numerics;

namespace MalignEngine
{
    public class SpriteRenderingSystem : EntitySystem
    {
        [Dependency]
        protected RenderingSystem RenderingSystem = default!;

        public override void OnInitialize()
        {
        }

        public override void OnDraw(float deltaTime)
        {
            RenderingSystem.Begin();
            var query = new QueryDescription().WithAll<SpriteRenderer, WorldTransform>();
            World.Query(query, (Entity entity, ref WorldTransform transform, ref SpriteRenderer spriteRenderer) =>
            {
                float depth = 0;
                if (entity.Has<Depth>()) { depth = entity.Get<Depth>().Value; }

                RenderingSystem.DrawTexture2D(spriteRenderer.Sprite.Texture, transform.Position.ToVector2(), transform.Scale.ToVector2(), spriteRenderer.Sprite.Origin, spriteRenderer.Sprite.Rect, spriteRenderer.Color, transform.ZAxis, depth);
            });
            RenderingSystem.End();
        }
    }

    public struct SpriteRenderer : IComponent
    {
        public Sprite Sprite;
        public Color Color;
    }

    public struct Depth : IComponent
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
}