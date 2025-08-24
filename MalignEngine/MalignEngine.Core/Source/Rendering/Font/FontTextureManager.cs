using FontStashSharp.Interfaces;
using System.Drawing;
using Silk.NET.OpenGL;

namespace MalignEngine;

internal class Texture2DManager : ITexture2DManager
{
    private IRenderingAPI _renderAPI;

    public Texture2DManager(IRenderingAPI renderApi)
    {
        _renderAPI = renderApi;
    }

    public object CreateTexture(int width, int height)
    {
        return _renderAPI.CreateTexture(new TextureDescriptor() { Width = width, Height = height });
    }

    public Point GetTextureSize(object texture)
    {
        var t = (ITextureResource)texture;
        return new Point((int)t.Width, (int)t.Height);
    }

    public void SetTextureData(object texture, System.Drawing.Rectangle bounds, byte[] data)
    {
        var t = (ITextureResource)texture;
        t.SubmitData(bounds, data);
    }
}