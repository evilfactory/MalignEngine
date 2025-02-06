using System.Numerics;

namespace MalignEngine
{
    public enum StencilOperation
    {
        Keep,
        Zero,
        Replace,
        Increment,
        IncrementWrap,
        Decrement,
        DecrementWrap,
        Invert
    }

    public enum StencilFunction
    {
        Never,
        Less,
        Equal,
        LessThanOrEqual,
        Greater,
        NotEqual,
        GreaterThanOrEqual,
        Always
    }

    public enum BlendingMode
    {
        AlphaBlend, Additive
    }

    public struct VertexPositionColorTexture
    {
        public Vector3 Position;
        public Color Color;
        public Vector2 TextureCoordinate;

        public VertexPositionColorTexture(Vector3 position, Color color, Vector2 textureCoordinate)
        {
            Position = position;
            Color = color;
            TextureCoordinate = textureCoordinate;
        }
    }

    public interface IRenderingService : IService
    {
        public bool FlipY { get; set; }
        public void Clear(Color color);
        public void Begin(Matrix4x4 matrix, Material material = null, BlendingMode blendingMode = BlendingMode.AlphaBlend);
        public void Begin(Material material = null, BlendingMode blendingMode = BlendingMode.AlphaBlend);
        public void End();
        public void DrawTexture2D(ITexture texture, Vector2 position, Vector2 scale, Vector2 uv1, Vector2 uv2, Color color, float rotation, float layerDepth);
        public void DrawTexture2D(ITexture texture, Vector2 position, Vector2 scale, Color color, float rotation, float layerDepth);
        public void DrawTexture2D(ITexture texture, Vector2 position, Vector2 scale, float layerDepth);
        public void DrawQuad(ITexture texture, VertexPositionColorTexture topRight, VertexPositionColorTexture bottomRight, VertexPositionColorTexture bottomLeft, VertexPositionColorTexture topLeft);
        public TextureHandle CreateTextureHandle();
        public Shader LoadShader(Stream data);
        public void SetMatrix(Matrix4x4 matrix);
        public void SetRenderTarget(RenderTexture renderTexture, int width = 0, int height = 0);
        public void SetStencil(StencilFunction function, int reference, uint mask, StencilOperation fail, StencilOperation zfail, StencilOperation zpass);
    }

    public abstract class BaseRenderingService : IRenderingService, IInit
    {
        public bool FlipY { get; set; }
        public abstract void Clear(Color color);
        public abstract void Begin(Matrix4x4 matrix, Material material = null, BlendingMode blendingMode = BlendingMode.AlphaBlend);
        public abstract void Begin(Material material = null, BlendingMode blendingMode = BlendingMode.AlphaBlend);
        public abstract void End();
        public abstract void DrawTexture2D(ITexture texture, Vector2 position, Vector2 scale, Vector2 uv1, Vector2 uv2, Color color, float rotation, float layerDepth);
        public abstract void DrawTexture2D(ITexture texture, Vector2 position, Vector2 scale, Color color, float rotation, float layerDepth);
        public abstract void DrawTexture2D(ITexture texture, Vector2 position, Vector2 scale, float layerDepth);
        public abstract void DrawQuad(ITexture texture, VertexPositionColorTexture topRight, VertexPositionColorTexture bottomRight, VertexPositionColorTexture bottomLeft, VertexPositionColorTexture topLeft);
        public abstract TextureHandle CreateTextureHandle();
        public abstract Shader LoadShader(Stream data);
        public abstract void SetMatrix(Matrix4x4 matrix);
        public abstract void SetRenderTarget(RenderTexture renderTexture, int width = 0, int height = 0);
        public abstract void SetStencil(StencilFunction function, int reference, uint mask, StencilOperation fail, StencilOperation zfail, StencilOperation zpass);
        public abstract void OnInitialize();
    }
}