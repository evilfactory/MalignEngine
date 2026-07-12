using MalignEngine;

namespace SteamBolt;

public class TileDefinition
{
    public string Identifier { get; private set; }
    public string LayerId { get; private set; }
    public AssetHandle<Sprite> Sprite { get; private set; }

    public TileDefinition(string identifier, string layerId, AssetHandle<Sprite> sprite)
    {
        Identifier = identifier;
        LayerId = layerId;
        Sprite = sprite;
    }
}