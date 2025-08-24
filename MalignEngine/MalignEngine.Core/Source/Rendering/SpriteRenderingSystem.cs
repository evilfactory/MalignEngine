using System.Numerics;

namespace MalignEngine;

public class SpriteRenderingSystem : IService, ICameraDraw
{
    private IEntityManager _entityManager;
    private IRenderer2D _renderer2D;
    private IRenderingAPI _renderApi;
    private IPerformanceProfiler? _performanceProfiler;

    private struct RenderData
    {
        public SpriteRenderer SpriteRenderer;
        public WorldTransform Transform;
    }

    public SpriteRenderingSystem(IEntityManager entityManager, IRenderingAPI renderApi, IRenderer2D renderer2D, IPerformanceProfiler? performanceProfiler = null)
    {
        _entityManager = entityManager;
        _renderApi = renderApi;
        _renderer2D = renderer2D;
        _performanceProfiler = performanceProfiler;
    }

    public void DrawSprite(Sprite sprite, Vector2 position, Vector2 scale, Color color, float rotation = 0f, float depth = 0f)
    {
        _renderer2D.DrawTexture2D(sprite.Texture.Resource, position, scale, new Vector2(sprite.UV1.X, 1f - sprite.UV2.Y), new Vector2(sprite.UV2.X, 1f - sprite.UV1.Y), color, rotation, depth);
    }

    public void OnCameraDraw(float deltaTime, OrthographicCamera camera)
    {
        List<RenderData> renderData = new List<RenderData>();

        var query = _entityManager.World.CreateQuery().WithAll<SpriteRenderer, WorldTransform>();
        _entityManager.World.Query(query, (EntityRef entity, ref WorldTransform transform, ref SpriteRenderer spriteRenderer) =>
        {
            renderData.Add(new RenderData() { Transform = transform, SpriteRenderer = spriteRenderer });
        });

        _renderApi.Submit(ctx =>
        {
            _performanceProfiler?.BeginSample("DrawSprites");

            _renderer2D.Begin(ctx, camera.Matrix);

            for (int i = 0; i < renderData.Count; i++)
            {
                DrawSprite(renderData[i].SpriteRenderer.Sprite, renderData[i].Transform.Position.ToVector2(), renderData[i].Transform.Scale.ToVector2(), renderData[i].SpriteRenderer.Color, renderData[i].Transform.ZAxis, renderData[i].SpriteRenderer.Layer);
            }

            _renderer2D.End();

            _performanceProfiler?.EndSample();
        });
    }
}

[Serializable]
public struct SpriteRenderer : IComponent
{
    [DataField("Sprite")] public Sprite Sprite;
    [DataField("Color", save: true)] public Color Color;
    [DataField("Layer")] public float Layer;

    public SpriteRenderer()
    {
        Color = Color.White;
    }
}