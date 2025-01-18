﻿using MalignEngine;
using Silk.NET.OpenAL;
using System.Numerics;

namespace MalignEngine
{
    public class AudioSystem : BaseSystem
    {
        private static List<SoundChannel> soundChannels = new List<SoundChannel>();

        private ALContext alc { get; set; }
        internal AL al { get; private set; }

        public override void OnInitialize()
        {
            unsafe
            {
                alc = ALContext.GetApi(true);
                al = AL.GetApi(true);
                Device* device = alc.OpenDevice("");

                if (device == null)
                {
                    throw new ArgumentException("Could not create device.");
                }

                var context = alc.CreateContext(device, null);
                alc.MakeContextCurrent(context);

                al.GetError();
            }
        }

        public override void OnUpdate(float deltaTime)
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
    }
}