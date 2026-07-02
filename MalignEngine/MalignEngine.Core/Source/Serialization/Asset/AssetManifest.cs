using System.Collections.Immutable;

namespace MalignEngine;

public class AssetManifest
{
    public ImmutableArray<(Type Type, AssetPath AssetPath)> Assets { get; private set; }

    public AssetManifest(IEnumerable<(Type Type, AssetPath AssetPath)> assets)
    {
        Assets = assets.ToImmutableArray();
    }
}