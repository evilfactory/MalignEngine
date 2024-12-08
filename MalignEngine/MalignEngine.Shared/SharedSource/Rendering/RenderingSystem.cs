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

    public abstract class RenderingSystem : BaseSystem
    {
        public bool FlipY { get; set; }
        public abstract void Clear(Color color);
        public abstract void Begin(Matrix4x4 matrix, Material material = null, BlendingMode blendingMode = BlendingMode.AlphaBlend);
        public abstract void Begin(Material material = null, BlendingMode blendingMode = BlendingMode.AlphaBlend);
        public abstract void End();
        public abstract void DrawRenderTexture(RenderTexture texture, Vector2 position, Vector2 size, Vector2 origin, Rectangle sourceRectangle, Color color, float rotation, float layerDepth);
        public abstract void DrawTexture2D(Texture2D texture, Vector2 position, Vector2 scale, Vector2 origin, Rectangle sourceRectangle, Color color, float rotation, float layerDepth);
        public abstract void DrawQuad(Texture2D texture, VertexPositionColorTexture topRight, VertexPositionColorTexture bottomRight, VertexPositionColorTexture bottomLeft, VertexPositionColorTexture topLeft);
        public abstract void DrawQuad(RenderTexture texture, VertexPositionColorTexture topRight, VertexPositionColorTexture bottomRight, VertexPositionColorTexture bottomLeft, VertexPositionColorTexture topLeft);
        public abstract TextureHandle CreateTextureHandle(Texture2D texture);
        public abstract RenderTextureHandle CreateRenderTextureHandle(RenderTexture renderTexture);

        public abstract Shader LoadShader(Stream data);
        public abstract void SetMatrix(Matrix4x4 matrix);
        public abstract void SetRenderTarget(RenderTexture renderTexture, uint width = 0, uint height = 0);
        public abstract void SetStencil(StencilFunction function, int reference, uint mask, StencilOperation fail, StencilOperation zfail, StencilOperation zpass);
    }
}