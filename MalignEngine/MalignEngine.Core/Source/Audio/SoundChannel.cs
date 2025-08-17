using Silk.NET.OpenAL;
using System.Numerics;

namespace MalignEngine;

// TODO: properly interface this

public class SoundChannel : IDisposable
{
    private AudioService audioSystem;

    private float volume = 1f;
    public float Volume
    {
        get
        {
            return volume;
        }

        set
        {
            volume = value;
            al.SetSourceProperty(source, SourceFloat.Gain, volume);
        }
    }

    private float pitch = 1f;
    public float Pitch
    {
        get
        {
            return pitch;
        }

        set
        {
            pitch = value;
            al.SetSourceProperty(source, SourceFloat.Pitch, pitch);
        }
    }

    private bool looping = false;
    public bool Looping
    {
        get
        {
            return looping;
        }

        set
        {
            looping = value;
            al.SetSourceProperty(source, SourceBoolean.Looping, looping);
        }
    }

    public float Near
    {
        get
        {
            al.GetSourceProperty(source, SourceFloat.ReferenceDistance, out float value);
            return value;
        }
        set
        {
            al.SetSourceProperty(source, SourceFloat.ReferenceDistance, value);
        }
    }

    public float Far
    {
        get
        {
            al.GetSourceProperty(source, SourceFloat.MaxDistance, out float value);
            return value;
        }
        set
        {
            al.SetSourceProperty(source, SourceFloat.MaxDistance, value);
        }
    }

    public Vector3 Position { get; set; }
    public bool Directional { get; set; } = true;


    private AL al;
    private uint buffer;
    private uint source;

    public SoundChannel(AudioService audioSystem, uint buffer)
    {
        this.audioSystem = audioSystem;
        this.buffer = buffer;

        source = al.GenSource();
        al.SetSourceProperty(source, SourceInteger.Buffer, buffer);
        al.SetSourceProperty(source, SourceBoolean.Looping, Looping);
        al.SetSourceProperty(source, SourceBoolean.SourceRelative, false);
    }

    public void Play()
    {
        al.SourcePlay(source);
    }

    public void Play(Vector3 position)
    {
        Position = position;
        Play();
    }

    public void Pause()
    {
        al.SourcePause(source);
    }

    public void Stop()
    {
        al.SourceStop(source);
    }

    public void Update()
    {
        if (Directional)
        {
            al.SetSourceProperty(source, SourceBoolean.SourceRelative, false);
            al.SetSourceProperty(source, SourceVector3.Position, Position);
        }
        else
        {
            al.SetSourceProperty(source, SourceBoolean.SourceRelative, true);
            al.SetSourceProperty(source, SourceVector3.Position, new Vector3(0f, 0f, Vector3.Distance(Position, audioSystem.ListenerPosition)));
        }
    }

    public void Dispose()
    {
        al.DeleteSource(source);
        audioSystem.RemoveChannel(this);
    }
}
