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

    public Matrix4x4 Matrix;
    public int Width, Height;

    public PostProcessBaseSystem[] PostProcessingSteps;
}