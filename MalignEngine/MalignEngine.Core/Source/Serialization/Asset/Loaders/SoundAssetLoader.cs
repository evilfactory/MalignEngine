using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalignEngine;

public class SoundAssetLoader : IAssetLoader
{
    private IAudioService _audioService;

    public SoundAssetLoader(IAudioService audioService)
    {
        _audioService = audioService;
    }

    public Type GetAssetType(AssetPath assetPath) => typeof(SoundAsset);

    public IEnumerable<string> GetSubIds(AssetPath assetPath)
    {
        return Enumerable.Empty<string>();
    }

    public bool IsCompatible(AssetPath assetPath)
    {
        return assetPath.Extension == "wav";
    }

    public IAsset Load(AssetPath assetPath)
    {
        AssetSource source = AssetSource.Get(assetPath);

        using (var memoryStream = new MemoryStream())
        {
            source.GetStream().CopyTo(memoryStream);

            return new SoundAsset(_audioService.CreateResource(new SoundResourceDescriptor(memoryStream.GetBuffer())));
        }
    }
}
