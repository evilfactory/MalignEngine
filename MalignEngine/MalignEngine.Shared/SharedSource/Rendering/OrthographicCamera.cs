using Silk.NET.OpenGLES;
using System.Numerics;

namespace MalignEngine
{
    // Post processing
    public struct OrthographicCamera : IComponent
    {
        public float ViewSize;
        public bool IsMain;
        public Color ClearColor;

        [Access(typeof(CameraSystem))]
        public Matrix4x4 Matrix;

        public RenderTexture RenderTexture;
        public PostProcessBaseSystem[] PostProcessingSteps;
    }
}