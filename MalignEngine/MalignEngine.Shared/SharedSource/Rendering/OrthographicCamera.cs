using Silk.NET.OpenGLES;
using System.Numerics;

namespace MalignEngine
{
    // Post processing
    public struct OrthographicCamera
    {
        public float ViewSize;
        public bool IsMain;
        public Matrix4x4 Matrix;

        public RenderTexture RenderTexture;
        public PostProcessBaseSystem[] PostProcessingSteps;
    }
}