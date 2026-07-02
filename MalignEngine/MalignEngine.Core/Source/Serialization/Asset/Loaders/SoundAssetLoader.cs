using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalignEngine;

public class SoundAssetLoader : IAssetLoader
{
    private IAudioService _audioService;
    public IReadOnlyCollection<Type> AssetTypes => [typeof(SoundAsset)];

    public SoundAssetLoader(IAudioService audioService)
    {
        _audioService = audioService;
    }

    public IAsset Load(Stream stream)
    {
        using (var memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);

            return new SoundAsset(_audioService.CreateResource(new SoundResourceDescriptor(memoryStream.GetBuffer())));
        }
    }

    public void Save(Stream stream, IAsset asset)
    {
        throw new NotImplementedException();
    }
}
