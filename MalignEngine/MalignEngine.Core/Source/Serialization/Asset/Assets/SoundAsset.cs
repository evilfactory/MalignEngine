using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalignEngine;

public class SoundAsset : IAsset
{
    public ISoundResource SoundResource { get; private set; }

    public SoundAsset(ISoundResource soundResource)
    {
        SoundResource = soundResource;
    }
}
