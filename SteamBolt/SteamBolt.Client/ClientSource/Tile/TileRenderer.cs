using MalignEngine;
using Silk.NET.Maths;
using System.Numerics;

namespace SteamBolt;

public class TileRenderer : EntitySystem, ICameraDraw
{
    public enum QuarterShape
    {
        Full = 2,
        Edge = 3,
        Inner = 0,
        Outer = 1
    }

    public enum QuarterPosition
    {
        TopLeft,
        TopRight,
        BottomRight,
        BottomLeft
    }

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

            TileMapComponent map = tileRendererComponent.TileMap.Get<TileMapComponent>();

            foreach (TileLayer layer in map.Layers)
            {
                // Rendered in another thread, needs to be copied
                Dictionary<Vector2D<int>, Tile> tiles = layer.Tiles.ToDictionary();
                _renderAPI.Submit(ctx =>
                {
                    _renderer2D.Begin(ctx);
                    foreach ((Vector2D<int> point, Tile tile) in tiles)
                    {
                        bool n = HasTile(tiles, point.X, point.Y + 1, tile);
                        bool e = HasTile(tiles, point.X + 1, point.Y, tile);
                        bool s = HasTile(tiles, point.X, point.Y - 1, tile);
                        bool w = HasTile(tiles, point.X - 1, point.Y, tile);

                        bool nw = n && w && HasTile(tiles, point.X - 1, point.Y + 1, tile);
                        bool ne = n && e && HasTile(tiles, point.X + 1, point.Y + 1, tile);
                        bool sw = s && w && HasTile(tiles, point.X - 1, point.Y - 1, tile);
                        bool se = s && e && HasTile(tiles, point.X + 1, point.Y - 1, tile);

                        QuarterShape topLeft = Resolve(n, w, nw);
                        QuarterShape topRight = Resolve(n, e, ne);
                        QuarterShape bottomRight = Resolve(s, e, se);
                        QuarterShape bottomLeft = Resolve(s, w, sw);

                        DrawQuarter(tile, position + new Vector2(point.X, point.Y), topLeft, QuarterPosition.TopLeft, layer.Order, GetEdgeRotation(QuarterPosition.TopLeft, n, w));
                        DrawQuarter(tile, position + new Vector2(point.X, point.Y), topRight, QuarterPosition.TopRight, layer.Order, GetEdgeRotation(QuarterPosition.TopRight, n, e));
                        DrawQuarter(tile, position + new Vector2(point.X, point.Y), bottomRight, QuarterPosition.BottomRight, layer.Order, GetEdgeRotation(QuarterPosition.BottomRight, s, e));
                        DrawQuarter(tile, position + new Vector2(point.X, point.Y), bottomLeft, QuarterPosition.BottomLeft, layer.Order, GetEdgeRotation(QuarterPosition.BottomLeft, s, w));
                    }
                    _renderer2D.End();
                });
            }
        });
    }

    private static (Vector2 Offset, float Rotation) GetQuarterTransform(QuarterPosition position)
    {
        return position switch
        {
            QuarterPosition.TopLeft => (new Vector2(-0.25f, 0.25f), MathF.PI),
            QuarterPosition.TopRight => (new Vector2(0.25f, 0.25f), MathF.PI / 2f),
            QuarterPosition.BottomRight => (new Vector2(0.25f, -0.25f), 0f),
            QuarterPosition.BottomLeft => (new Vector2(-0.25f, -0.25f), MathF.PI * 1.5f),

            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static bool HasTile(Dictionary<Vector2D<int>, Tile> tiles, int x, int y, Tile current)
    {
        return tiles.TryGetValue(new Vector2D<int>(x, y), out Tile other) && other.Definition == current.Definition;
    }

    private void DrawQuarter(Tile tile, Vector2 position, QuarterShape shape, QuarterPosition quarterPosition, int layer, float rotation)
    {
        (Vector2 offset, float quarterRotation) = GetQuarterTransform(quarterPosition);

        (Vector2 uv0, Vector2 uv1) = tile.Definition.GetTileUVs((int)shape);

        _renderer2D.DrawTexture2D(tile.Definition.Texture.Asset.Resource, position + offset, new Vector2(0.5f, 0.5f), uv0, uv1, Color.White, quarterRotation, layer);
    }

    private static float GetEdgeRotation(QuarterPosition position, bool sideA, bool sideB)
    {
        return position switch
        {
            QuarterPosition.TopLeft => (sideA, sideB) switch
            {
                (true, false) => MathF.PI / 2f,
                (false, true) => 0f,
                _ => 0f
            },

            QuarterPosition.TopRight => (sideA, sideB) switch
            {
                (true, false) => MathF.PI,
                (false, true) => MathF.PI / 2f,
                _ => 0f
            },

            QuarterPosition.BottomRight => (sideA, sideB) switch
            {
                (true, false) => MathF.PI * 1.5f,
                (false, true) => MathF.PI,
                _ => 0f
            },

            QuarterPosition.BottomLeft => (sideA, sideB) switch
            {
                (true, false) => 0f,
                (false, true) => MathF.PI * 1.5f,
                _ => 0f
            },

            _ => 0f
        };
    }

    private static QuarterShape Resolve(bool sideA, bool sideB, bool diagonal)
    {
        if (!sideA && !sideB)
        {
            return QuarterShape.Outer;
        }

        if (!sideA || !sideB)
        {
            return QuarterShape.Edge;
        }

        if (!diagonal)
        {
            return QuarterShape.Inner;
        }

        return QuarterShape.Full;
    }
}