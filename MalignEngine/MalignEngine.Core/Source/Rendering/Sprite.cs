using System.Numerics;
using System.Xml.Linq;

namespace MalignEngine;

public class Sprite : IAsset
{
    public AssetHandle<Texture2D> TextureAsset { get; private set; }
    public Texture2D Texture => TextureAsset.Asset;
    public Vector2 Origin { get; private set; }
    public Rectangle Rect { get; private set; }

    public Vector2 UV1 { get; private set; }
    public Vector2 UV2 { get; private set; }

    public Sprite() { }

    public Sprite(AssetHandle<Texture2D> texture)
    {
        TextureAsset = texture;
        Origin = new Vector2(0.5f, 0.5f);
        Rect = new Rectangle(0, 0, (int)Texture.Width, (int)Texture.Height);

        CalculateUVs();
    }

    public Sprite(AssetHandle<Texture2D> texture, Vector2 origin, Rectangle rect)
    {
        TextureAsset = texture;
        Origin = origin;
        Rect = rect;

        CalculateUVs();
    }

    public void CalculateUVs()
    {
        UV1 = new Vector2((float)Rect.X / (float)Texture.Width, (float)Rect.Y / (float)Texture.Height);
        UV2 = new Vector2((float)(Rect.X + Rect.Width) / (float)Texture.Width, (float)(Rect.Y + Rect.Height) / (float)Texture.Height);
    }

    public override string ToString()
    {
        return $"Sprite: ({Texture.Width}x{Texture.Height})";
    }
}