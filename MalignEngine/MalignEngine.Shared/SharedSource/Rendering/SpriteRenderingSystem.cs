using Arch.Core;
using Arch.Core.Extensions;
using System.Diagnostics;
using System.Numerics;

namespace MalignEngine
{
    public class SpriteRenderingSystem : EntitySystem
    {
        [Dependency]
        protected RenderingSystem RenderingSystem = default!;

        [Dependency(true)]
        protected EditorPerformanceSystem EditorPerformanceSystem = default!;


        public override void OnInitialize()
        {
        }

        public override void OnDraw(float deltaTime)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            RenderingSystem.Begin();
            var query = new QueryDescription().WithAll<SpriteRenderer, WorldTransform>();
            EntityManager.World.Query(query, (EntityRef entity, ref WorldTransform transform, ref SpriteRenderer spriteRenderer) =>
            {
                float depth = 0;
                if (entity.Has<Depth>()) { depth = entity.Get<Depth>().Value; }

                RenderingSystem.DrawTexture2D(spriteRenderer.Sprite.Texture, transform.Position.ToVector2(), transform.Scale.ToVector2(), spriteRenderer.Sprite.UV1, spriteRenderer.Sprite.UV2, spriteRenderer.Color, transform.ZAxis, depth);
            });
            RenderingSystem.End();

            stopwatch.Stop();
            EditorPerformanceSystem?.AddElapsedTicks("SpriteRenderingSystem", new StopWatchPerformanceLogData(stopwatch.ElapsedTicks));

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