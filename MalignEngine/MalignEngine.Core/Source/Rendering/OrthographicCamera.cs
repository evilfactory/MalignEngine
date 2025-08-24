using System.Numerics;

namespace MalignEngine;

public struct OrthographicCamera : IComponent
{
    public float ViewSize;
    public bool IsMain;
    public Color ClearColor;

    [Access(typeof(CameraSystem))]
    public Matrix4x4 Matrix;

    public IFrameBufferResource Output;
    public PostProcessBaseSystem[] PostProcessingSteps;
}