using Arch.Core;
using Arch.Core.Extensions;
using System.Diagnostics;
using System.Numerics;
using System.Xml.Linq;

namespace MalignEngine
{
    public class SpriteSerializer : ICustomXmlSerializer
    {
        public object? Deserialize(string dataFieldName, XElement element, Dictionary<int, EntityRef> idRemap)
        {
            XElement spriteElement = element.Element(dataFieldName);
            if (spriteElement == null) { return null; }

            string? texturePath = spriteElement.Attribute("Texture")?.Value;

            if (texturePath == null) { return null; }

            return new Sprite(IoCManager.Resolve<AssetSystem>().Load<Texture2D>(texturePath));
        }

        public void Serialize(object value, string dataFieldName, XElement element)
        {
            Sprite sprite = (Sprite)value;

            XElement spriteElement = new XElement(dataFieldName);
            spriteElement.Add(new XAttribute("Texture", sprite.Texture.AssetPath));

            element.Add(spriteElement);
        }

        public bool SupportsType(Type type)
        {
            return typeof(Sprite) == type;
        }
    }


    public class SpriteRenderingSystem : EntitySystem
    {
        [Dependency]
        protected RenderingSystem RenderingSystem = default!;

        [Dependency(true)]
        protected EditorPerformanceSystem EditorPerformanceSystem = default!;


        public override void OnInitialize()
        {
        }

        public override void OnDraw(float deltaTime)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            RenderingSystem.Begin();
            var query = new QueryDescription().WithAll<SpriteRenderer, WorldTransform>();
            EntityManager.World.Query(query, (EntityRef entity, ref WorldTransform transform, ref SpriteRenderer spriteRenderer) =>
            {
                float depth = 0;
                if (entity.Has<Depth>()) { depth = entity.Get<Depth>().Value; }

                RenderingSystem.DrawTexture2D(spriteRenderer.Sprite.Texture, transform.Position.ToVector2(), transform.Scale.ToVector2(), spriteRenderer.Sprite.UV1, spriteRenderer.Sprite.UV2, spriteRenderer.Color, transform.ZAxis, depth);
            });
            RenderingSystem.End();

            stopwatch.Stop();
            EditorPerformanceSystem?.AddElapsedTicks("SpriteRenderingSystem", new StopWatchPerformanceLogData(stopwatch.ElapsedTicks));

        }
    }

    [Serializable]
    public struct SpriteRenderer : IComponent
    {
        [DataField("Sprite")] public Sprite Sprite;
        [DataField("Color")] public Color Color;

        public SpriteRenderer()
        {
            Color = Color.White;
        }
    }

    public struct Depth : IComponent
    {
        public float Value;

        public Depth(float value)
        {
            Value = value;
        }

        public Depth()
        {
            Value = 0;
        }
    }
}