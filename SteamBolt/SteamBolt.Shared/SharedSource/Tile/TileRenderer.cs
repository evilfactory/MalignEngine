using MalignEngine;
using Silk.NET.Maths;
using System.Numerics;

namespace SteamBolt;

public struct TileRendererComponent : IComponent
{
    public Entity TileMapEntity;
}

public class TileRenderer : EntitySystem, ICameraDraw
{
    [Dependency]
    private IRenderingAPI _renderAPI = null!;
    [Dependency]
    private IRenderer2D _renderer2D = null!;
    [Dependency]
    private SpriteRenderingSystem _spriteRenderer = null!;

    public TileRenderer(IServiceContainer serviceContainer) : base(serviceContainer)
    {
    }

    public void OnCameraDraw(float delta, OrthographicCamera camera)
    {
        EntityManager.Query(new Query().Include<TileRendererComponent>(), entity =>
        {
            ref TileRendererComponent tileRendererComponent = ref entity.Get<TileRendererComponent>();

            Vector2 position = Vector2.Zero;

            if (entity.TryGet(out ComponentRef<Transform> transform))
            {
                position = transform.Value.Position.ToVector2();
            }

            TileMapComponent map = tileRendererComponent.TileMapEntity.Get<TileMapComponent>();

            foreach (TileLayer layer in map.Layers)
            {
                // Rendered in another thread, needs to be copied
                Dictionary<Vector2D<int>, Tile> tiles = layer.Tiles.ToDictionary();
                _renderAPI.Submit(ctx =>
                {
                    _renderer2D.Begin(ctx);
                    foreach ((Vector2D<int> point, Tile tile) in tiles)
                    {
                        _spriteRenderer.DrawSprite(tile.Definition.Sprite.Asset, position + new Vector2(point.X, point.Y), new Vector2(1f, 1f), Color.White, depth: (float)layer.Order);
                    }
                    _renderer2D.End();
                });
            }
        });
    }
}