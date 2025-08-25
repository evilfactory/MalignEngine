using System.Numerics;

namespace MalignEngine;

[Serializable]
public struct OrthographicCamera : IComponent
{
    [DataField("ViewSize")]
    public float ViewSize;
    [DataField("IsMain")]
    public bool IsMain;
    [DataField("ClearColor")]
    public Color ClearColor;

    [Access(typeof(CameraSystem))]
    public Matrix4x4 Matrix;

    public IFrameBufferResource Output;
    public PostProcessBaseSystem[] PostProcessingSteps;
}