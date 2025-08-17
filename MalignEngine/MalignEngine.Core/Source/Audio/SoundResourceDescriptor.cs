using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalignEngine;

public interface ISoundResourceDescriptor
{
    public byte[] SoundData { get; }
}

public class SoundResourceDescriptor : ISoundResourceDescriptor
{
    public byte[] SoundData { get; set; }

    public SoundResourceDescriptor(byte[] soundData)
    {
        SoundData = soundData;
    }
}
