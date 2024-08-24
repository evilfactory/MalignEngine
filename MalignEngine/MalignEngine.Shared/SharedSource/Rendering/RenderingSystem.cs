using System.Numerics;

namespace MalignEngine
{
    public abstract class RenderingSystem : BaseSystem
    {
        public abstract void Clear(Color color);
        public abstract void Begin(Shader shader, Matrix4x4 matrix);
        public abstract void Begin(Matrix4x4 matrix);
        public abstract void Begin();
        public abstract void End();
        public abstract void DrawRenderTexture(RenderTexture texture, Vector2 position, Vector2 size, Vector2 origin, Rectangle sourceRectangle, Color color, float rotation, float layerDepth);
        public abstract void DrawTexture2D(Texture2D texture, Vector2 position, Vector2 scale, Vector2 origin, Rectangle sourceRectangle, Color color, float rotation, float layerDepth);
        public abstract TextureHandle CreateTextureHandle(Texture2D texture);
        public abstract RenderTextureHandle CreateRenderTextureHandle(RenderTexture renderTexture);

        public abstract Shader LoadShader(Stream data);
        public abstract void SetMatrix(Matrix4x4 matrix);
        public abstract void SetRenderTarget(RenderTexture renderTexture, uint width = 0, uint height = 0);
    }
}