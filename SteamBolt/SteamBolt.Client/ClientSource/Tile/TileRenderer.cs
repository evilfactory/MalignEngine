using MalignEngine;
using Silk.NET.Maths;
using System.Numerics;

namespace SteamBolt;

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
            Vector2 scale = Vector2.One;

            if (entity.TryGet(out ComponentRef<Transform> transform))
            {
                position = transform.Value.Position.ToVector2();
                scale = transform.Value.Scale.ToVector2();
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

                        int bitwiseIndex = 
                            (n ? 1 : 0) + 
                            (ne ? 2 : 0) + 
                            (e ? 4 : 0) + 
                            (se ? 8 : 0) + 
                            (s ? 16 : 0) + 
                            (sw ? 32 : 0) + 
                            (w ? 64 : 0) + 
                            (nw ? 128 : 0);

                        (int atlasIndex, float rotation) = DetermineFromBitMask(bitwiseIndex);

                        (Vector2 uv0, Vector2 uv1) = tile.Definition.GetTileUVs(atlasIndex);

                        _renderer2D.DrawTexture2D(tile.Definition.Texture.Asset.Resource, position + new Vector2(point.X, point.Y) * scale, scale, uv0, uv1, Color.White, rotation, layer.Order);

                    }
                    _renderer2D.End();
                });
            }
        });
    }

    private static bool HasTile(Dictionary<Vector2D<int>, Tile> tiles, int x, int y, Tile current)
    {
        return tiles.TryGetValue(new Vector2D<int>(x, y), out Tile other) && other.Definition == current.Definition;
    }

    private (int AtlasIndex, float Rotation) DetermineFromBitMask(int bitMask)
    {
        if (bitMask == 0) { return (0, 0f); }

        if (bitMask == 1) { return (1, 0f); }
        if (bitMask == 4) { return (1, MathF.PI * 1.5f); }
        if (bitMask == 16) { return (1, MathF.PI); }
        if (bitMask == 64) { return (1, MathF.PI * 0.5f); }

        if (bitMask == 5) { return (2, 0f); }
        if (bitMask == 20) { return (2, MathF.PI * 1.5f); }
        if (bitMask == 80) { return (2, MathF.PI); }
        if (bitMask == 65) { return (2, MathF.PI * 0.5f); }

        if (bitMask == 7) { return (3, 0f); }
        if (bitMask == 28) { return (3, MathF.PI * 1.5f); }
        if (bitMask == 112) { return (3, MathF.PI); }
        if (bitMask == 193) { return (3, MathF.PI * 0.5f); }

        if (bitMask == 17) { return (4, 0f); }
        if (bitMask == 68) { return (4, MathF.PI * 0.5f); }

        if (bitMask == 21) { return (5, 0f); }
        if (bitMask == 84) { return (5, MathF.PI * 1.5f); }
        if (bitMask == 81) { return (5, MathF.PI); }
        if (bitMask == 69) { return (5, MathF.PI * 0.5f); }

        if (bitMask == 23) { return (6, 0f); }
        if (bitMask == 92) { return (6, MathF.PI * 1.5f); }
        if (bitMask == 113) { return (6, MathF.PI); }
        if (bitMask == 197) { return (6, MathF.PI * 0.5f); }

        if (bitMask == 29) { return (7, 0f); }
        if (bitMask == 116) { return (7, MathF.PI * 1.5f); }
        if (bitMask == 209) { return (7, MathF.PI); }
        if (bitMask == 71) { return (7, MathF.PI * 0.5f); }

        if (bitMask == 31) { return (8, 0f); }
        if (bitMask == 124) { return (8, MathF.PI * 1.5f); }
        if (bitMask == 241) { return (8, MathF.PI); }
        if (bitMask == 199) { return (8, MathF.PI * 0.5f); }

        if (bitMask == 85) { return (9, 0f); }

        if (bitMask == 87) { return (10, 0f); }
        if (bitMask == 93) { return (10, MathF.PI * 1.5f); }
        if (bitMask == 117) { return (10, MathF.PI); }
        if (bitMask == 213) { return (10, MathF.PI * 0.5f); }

        if (bitMask == 95) { return (11, 0f); }
        if (bitMask == 125) { return (11, MathF.PI * 1.5f); }
        if (bitMask == 245) { return (11, MathF.PI); }
        if (bitMask == 215) { return (11, MathF.PI * 0.5f); }

        if (bitMask == 119) { return (12, 0f); }
        if (bitMask == 221) { return (12, MathF.PI * 0.5f); }

        if (bitMask == 127) { return (13, 0f); }
        if (bitMask == 253) { return (13, MathF.PI * 1.5f); }
        if (bitMask == 247) { return (13, MathF.PI); }
        if (bitMask == 223) { return (13, MathF.PI * 0.5f); }

        if (bitMask == 255) { return (14, 0f); }

        return (0, 0f);
    }
}