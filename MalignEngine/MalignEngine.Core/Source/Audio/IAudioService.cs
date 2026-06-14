using System.Numerics;

namespace MalignEngine;

public interface IAudioService : IService
{
    Vector3 ListenerPosition { get; set; }
    ISoundChannel Play(ISoundResource sound);
    ISoundResource CreateResource(ISoundResourceDescriptor descriptor);
}