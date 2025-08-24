using System.Numerics;
using System.Xml.Linq;

namespace MalignEngine
{
    /*
    public class SpriteSerializer : ICustomTypeXmlSerializer
    {
        public object? Deserialize(string dataFieldName, XElement element, EntityIdRemap? idRemap = null)
        {
            return Application.Main.ServiceContainer.GetInstance<AssetService>().GetFromId<Sprite>(element.GetAttributeString(dataFieldName));
        }

        public void Serialize(object value, string dataFieldName, XElement element)
        {
            Sprite sprite = (Sprite)value;

            element.SetAttributeString(dataFieldName, sprite.AssetId);
        }

        public bool SupportsType(Type type)
        {
            return typeof(Sprite) == type;
        }
    }
    */

    public class Sprite : XmlAsset
    {
        public Texture2D Texture { get; private set; }
        public Vector2 Origin { get; private set; }
        public Rectangle Rect { get; private set; }

        public Vector2 UV1 { get; private set; }
        public Vector2 UV2 { get; private set; }

        public Sprite() { }

        public Sprite(Texture2D texture)
        {
            Texture = texture;
            Origin = new Vector2(0.5f, 0.5f);
            Rect = new Rectangle(0, 0, (int)texture.Width, (int)texture.Height);

            CalculateUVs();
        }

        public Sprite(Texture2D texture, Vector2 origin, Rectangle rect)
        {
            Texture = texture;
            Origin = origin;
            Rect = rect;

            CalculateUVs();
        }

        public void CalculateUVs()
        {
            UV1 = new Vector2((float)Rect.X / (float)Texture.Width, (float)Rect.Y / (float)Texture.Height);
            UV2 = new Vector2((float)(Rect.X + Rect.Width) / (float)Texture.Width, (Rect.Y + Rect.Height) / (float)Texture.Height);
        }

        public override string ToString()
        {
            return $"Sprite: ({Texture.Width}x{Texture.Height})";
        }

        public override void Load(XElement element)
        {
            string texturePath = element.GetAttributeString("Texture");

            if (texturePath == null) { throw new Exception("No Texture attribute found"); }

            //Texture = Application.Main.ServiceContainer.GetInstance<AssetService>().FromFile<Texture2D>(texturePath);

            Rect = element.GetAttributeRectangle("Rectangle", new Rectangle(0, 0, (int)Texture.Width, (int)Texture.Height));
            Origin = element.GetAttributeVector2("Origin", Origin);

            CalculateUVs();
        }

        public override void Save(XElement element)
        {
            throw new Exception("Not implemented");
        }
    }
}