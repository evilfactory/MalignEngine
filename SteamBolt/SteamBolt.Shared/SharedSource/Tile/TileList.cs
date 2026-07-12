using MalignEngine;

namespace SteamBolt;

public class TileList : IAsset
{
    public IReadOnlyList<TileDefinition> Definitions { get; private set; }

    public TileList(IEnumerable<TileDefinition> data)
    {
        Definitions = data.ToList();
    }
}