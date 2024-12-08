using FontStashSharp.Interfaces;
using System.Drawing;
using Silk.NET.OpenGL;

namespace MalignEngine
{
    internal class Texture2DManager : ITexture2DManager
    {
        public object CreateTexture(int width, int height)
        {
            Texture2D tex = new Texture2D((uint)width, (uint)height);
            IoCManager.Resolve<AssetSystem>().Add(tex);
            return tex;
        }

        public Point GetTextureSize(object texture)
        {
            var t = (Texture2D)texture;
            return new Point((int)t.Width, (int)t.Height);
        }

        public void SetTextureData(object texture, System.Drawing.Rectangle bounds, byte[] data)
        {
            var t = (Texture2D)texture;
            t.handle.SubmitData(bounds, data);
        }
    }
}