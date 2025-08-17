using Silk.NET.OpenAL;
using System.Numerics;

namespace MalignEngine;

public interface IAudioService : IService
{
    Vector3 ListenerPosition { get; set; }
    SoundChannel Play(ISoundResource sound);
    ISoundResource CreateResource(ISoundResourceDescriptor descriptor);
}

public class AudioService : IAudioService, IUpdate, IDisposable
{
    private List<SoundChannel> _soundChannels = new List<SoundChannel>();

    private ALContext _alc { get; set; }
    internal AL _al { get; private set; }

    private unsafe Device* _device;

    private ILogger _logger;

    public AudioService(ILoggerService loggerService)
    {
        _logger = loggerService.GetSawmill("audio");

        unsafe
        {
            _alc = ALContext.GetApi(true);
            _al = AL.GetApi(true);
            _device = _alc.OpenDevice("");

            if (_device == null)
            {
                throw new ArgumentException("Could not create device.");
            }

            var context = _alc.CreateContext(_device, null);
            _alc.MakeContextCurrent(context);

            CaptureError(_al.GetError());

            _logger.LogInfo("Audio System initialized.");
        }
    }

    public void OnUpdate(float deltaTime)
    {
        _soundChannels.ForEach(channel => channel.Update());
    }

    public Vector3 ListenerPosition
    {
        get
        {
            _al.GetListenerProperty(ListenerVector3.Position, out Vector3 value);
            return value;
        }

        set
        {
            _al.SetListenerProperty(ListenerVector3.Position, value);
        }
    }

    public SoundChannel Play(ISoundResource sound)
    {
        SoundChannel channel = new SoundChannel(this, ((IALBuffer)sound).GetALBuffer());
        _soundChannels.Add(channel);
        return channel;
    }

    public ISoundResource CreateResource(ISoundResourceDescriptor descriptor)
    {
        return new SoundResource(_al, descriptor);
    }

    public void RemoveChannel(SoundChannel channel)
    {
        _soundChannels.Remove(channel);
    }

    private void CaptureError(AudioError error)
    {
        if (error != AudioError.NoError)
        {
            _logger.LogError($"OpenAL Error: {error}");
        }
    }

    public void Dispose()
    {
        unsafe
        {
            if (_device != null)
            {
                _alc.CloseDevice(_device);
            }

            _soundChannels.ForEach(channel => channel.Dispose());
            _al.Dispose();
            _alc.Dispose();

            _logger.LogInfo("Audio System disposed.");
        }
    }
}
