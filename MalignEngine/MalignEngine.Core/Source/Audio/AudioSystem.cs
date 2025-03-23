using MalignEngine;
using Silk.NET.OpenAL;
using System.Numerics;

namespace MalignEngine
{
    public class AudioSystem : IService, IUpdate, IDisposable
    {
        private static List<SoundChannel> soundChannels = new List<SoundChannel>();

        private ALContext alc { get; set; }
        internal AL al { get; private set; }

        private unsafe Device* device;

        private ILogger logger;

        public AudioSystem(ILoggerService loggerService)
        {
            this.logger = loggerService.GetSawmill("audio");

            unsafe
            {
                alc = ALContext.GetApi(true);
                al = AL.GetApi(true);
                device = alc.OpenDevice("");

                if (device == null)
                {
                    throw new ArgumentException("Could not create device.");
                }

                var context = alc.CreateContext(device, null);
                alc.MakeContextCurrent(context);

                CaptureError(al.GetError());

                logger.LogInfo("Audio System initialized.");
            }
        }

        public void OnUpdate(float deltaTime)
        {
            soundChannels.ForEach(channel => channel.Update());
        }

        public Vector3 ListenerPosition
        {
            get
            {
                al.GetListenerProperty(ListenerVector3.Position, out Vector3 value);
                return value;
            }

            set
            {
                al.SetListenerProperty(ListenerVector3.Position, value);
            }
        }

        public SoundChannel PlaySound(Sound sound)
        {
            SoundChannel channel = new SoundChannel(this, sound.buffer);
            soundChannels.Add(channel);
            return channel;
        }

        public void RemoveChannel(SoundChannel channel)
        {
            soundChannels.Remove(channel);
        }

        private void CaptureError(AudioError error)
        {
            if (error != AudioError.NoError)
            {
                logger.LogError($"OpenAL Error: {error}");
            }
        }

        public void Dispose()
        {
            unsafe
            {
                if (device != null)
                {
                    alc.CloseDevice(device);
                }

                soundChannels.ForEach(channel => channel.Dispose());
                al.Dispose();
                alc.Dispose();

                logger.LogInfo("Audio System disposed.");
            }
        }
    }
}
