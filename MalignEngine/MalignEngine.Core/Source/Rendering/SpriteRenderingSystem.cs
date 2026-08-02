using System.Numerics;

namespace MalignEngine;

public class SpriteRenderingSystem : EntitySystem, ICameraDraw
{
    private IRenderer2D _renderer2D;
    private IRenderingAPI _renderApi;
    private IPerformanceProfiler? _performanceProfiler;

    private struct RenderData
    {
        public SpriteRenderer SpriteRenderer;
        public WorldTransform Transform;
    }

    public SpriteRenderingSystem(IServiceContainer serviceContainer, IRenderingAPI renderApi, IRenderer2D renderer2D, IPerformanceProfiler? performanceProfiler = null)
        : base(serviceContainer)
    {
        _renderApi = renderApi;
        _renderer2D = renderer2D;
        _performanceProfiler = performanceProfiler;
    }

    public void DrawSprite(Sprite sprite, Vector2 position, Vector2 scale, Color color, float rotation = 0f, float depth = 0f)
    {
        _renderer2D.DrawTexture2D(sprite.Texture.Resource, position, scale, new Vector2(sprite.UV1.X, 1f - sprite.UV2.Y), new Vector2(sprite.UV2.X, 1f - sprite.UV1.Y), color, rotation, depth);
    }

    public void OnCameraDraw(CameraDrawContext context)
    {
        List<RenderData> renderData = new List<RenderData>();

        _performanceProfiler?.BeginSample("rendering.entity.sprite.query");

        var query = new Query().WithAll<SpriteRenderer, WorldTransform>();
        World.Query(query, (Entity entity) =>
        {
            ref WorldTransform transform = ref entity.Get<WorldTransform>();
            ref SpriteRenderer spriteRenderer = ref entity.Get<SpriteRenderer>();

            Vector2 halfSize = transform.Scale.ToVector2() / 2f;
            RectangleF worldBounds = new(transform.Position.X - halfSize.X, transform.Position.Y - halfSize.Y, halfSize.X * 2, halfSize.Y * 2);

            if (!context.Camera.VisibleBounds.Intersects(worldBounds))
            {
                return;
            }

            renderData.Add(new RenderData() { Transform = transform, SpriteRenderer = spriteRenderer });
        });

        _performanceProfiler?.EndSample();

        _renderApi.Submit(ctx =>
        {
            _performanceProfiler?.BeginSample("rendering.entity.sprite.draw");

            _renderer2D.Begin(ctx, context.Camera.Matrix);

            for (int i = 0; i < renderData.Count; i++)
            {
                DrawSprite(renderData[i].SpriteRenderer.Sprite, renderData[i].Transform.Position.ToVector2(), renderData[i].Transform.Scale.ToVector2(), renderData[i].SpriteRenderer.Color, renderData[i].Transform.GetRotation2D(), renderData[i].SpriteRenderer.Layer);
            }

            _renderer2D.End();

            _performanceProfiler?.EndSample();
        });
    }
}

[Serializable]
public struct SpriteRenderer : IComponent
{
    [DataField("Sprite", save: true)] public AssetHandle<Sprite> Sprite;
    [DataField("Color", save: true)] public Color Color;
    [DataField("Layer", save: true)] public float Layer;

    public SpriteRenderer()
    {
        Color = Color.White;
    }
}