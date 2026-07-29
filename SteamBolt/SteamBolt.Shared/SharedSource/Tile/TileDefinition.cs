using MalignEngine;
using System.Numerics;

namespace SteamBolt;

public class TileDefinition : IAsset
{
    [DataField("Identifier")]
    public string Identifier { get; private set; }

    [DataField("LayerId")]
    public string LayerId { get; private set; }

    [DataField("Icon")]
    public AssetHandle<Sprite> Icon { get; private set; }

    [DataField("Texture")]
    public AssetHandle<Texture2D> Texture { get; private set; }

    [DataField("Columns")]
    public int Columns { get; private set; }

    [DataField("Rows")]
    public int Rows { get; private set; }

    public TileDefinition() { }

    public (Vector2 UV0, Vector2 UV1) GetTileUVs(int index)
    {
        int x = index % Columns;
        int y = index / Columns;

        Vector2 uv0 = new((float)x / Columns, (float)y / Rows);
        Vector2 uv1 = new((float)(x + 1) / Columns, (float)(y + 1) / Rows);

        return (uv0, uv1);
    }
}