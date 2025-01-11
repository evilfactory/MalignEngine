using Arch.Core;
using Arch.Core.Extensions;
using System.Diagnostics;
using System.Numerics;
using System.Xml.Linq;

namespace MalignEngine
{
    public class SpriteRenderingSystem : EntitySystem
    {
        [Dependency]
        protected RenderingSystem RenderingSystem = default!;

        [Dependency(true)]
        protected EditorPerformanceSystem EditorPerformanceSystem = default!;


        public void DrawSprite(Sprite sprite, Vector2 position, Vector2 scale, Color color, float rotation = 0f, float depth = 0f)
        {
            RenderingSystem.DrawTexture2D(sprite.Texture, position, scale, new Vector2(sprite.UV1.X, 1f - sprite.UV2.Y), new Vector2(sprite.UV2.X, 1f - sprite.UV1.Y), color, rotation, depth);
        }

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
                DrawSprite(spriteRenderer.Sprite, transform.Position.ToVector2(), transform.Scale.ToVector2(), spriteRenderer.Color, transform.ZAxis, spriteRenderer.Layer);
            });
            RenderingSystem.End();

            stopwatch.Stop();
            EditorPerformanceSystem?.AddElapsedTicks("SpriteRenderingSystem", new StopWatchPerformanceLogData(stopwatch.ElapsedTicks));

        }
    }

    [Serializable]
    public struct SpriteRenderer : IComponent
    {
        [DataField("Sprite")] public Sprite Sprite;
        [DataField("Color")] public Color Color;
        [DataField("Layer")] public float Layer;

        public SpriteRenderer()
        {
            Color = Color.White;
        }
    }
}