using System.Numerics;

namespace MalignEngine;

public interface ISoundChannel : IDisposable
{
    float Volume { get; set; }
    float Pitch { get; set; }
    bool Looping { get; set; }

    float Near { get; set; }
    float Far { get; set; }

    Vector3 Position { get; set; }
    bool Directional { get; set; }

    void Play();
    void Play(Vector3 position);
    void Pause();
    void Stop();
    void Update();
}