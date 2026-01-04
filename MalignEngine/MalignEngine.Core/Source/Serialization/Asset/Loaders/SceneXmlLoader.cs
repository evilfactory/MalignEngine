using System.Xml.Linq;

namespace MalignEngine;

public class SceneXmlLoader : IXmlLoader
{
    public string RootName => "Scene";

    public Type GetAssetType() => typeof(Scene);

    private IAssetService _assetService;
    private EntitySerializer _entitySerializer;

    public SceneXmlLoader(IAssetService assetService, EntitySerializer entitySerializer)
    {
        _assetService = assetService;
        _entitySerializer = entitySerializer;
    }

    private static XElement MergeXElements(XElement baseElement, XElement overlayElement)
    {
        XElement result = new XElement(baseElement.Name);

        // Merge attributes (overlay overrides base)
        foreach (var attr in baseElement.Attributes().Concat(overlayElement.Attributes()).GroupBy(a => a.Name))
        {
            result.SetAttributeValue(attr.Key, attr.Last().Value);
        }

        // Merge child elements by name and attributes (if applicable)
        var baseChildren = baseElement.Elements().GroupBy(e => e.Name + e.Attributes().ToString());
        var overlayChildren = overlayElement.Elements().GroupBy(e => e.Name + e.Attributes().ToString());

        var allKeys = baseChildren.Select(g => g.Key).Union(overlayChildren.Select(g => g.Key));
        foreach (var key in allKeys)
        {
            var baseGroup = baseChildren.FirstOrDefault(g => g.Key == key);
            var overlayGroup = overlayChildren.FirstOrDefault(g => g.Key == key);

            if (baseGroup != null && overlayGroup != null)
            {
                foreach (var (b, o) in baseGroup.Zip(overlayGroup, Tuple.Create))
                {
                    result.Add(MergeXElements(b, o));
                }
            }
            else if (overlayGroup != null)
            {
                result.Add(overlayGroup);
            }
            else if (baseGroup != null)
            {
                result.Add(baseGroup);
            }
        }

        return result;
    }

    public IAsset Load(XElement element)
    {
        var id = element.Attribute("Id")?.Value ?? null;

        if (id == null)
        {
            throw new InvalidOperationException();
        }

        World world = new World();

        if (element.Attribute("From") != null)
        {
            AssetHandle<Scene>? originalScene = _assetService.GetHandles<Scene>().FirstOrDefault(x => x.Asset.SceneId == element.Attribute("From").Value);

            XElement originalElement = new XElement(originalScene.Asset.OriginalElement);

            MergeXElements(originalElement, element);
            element = originalElement;
        }

        EntityIdRemap remap = new EntityIdRemap();

        List<(Entity, XElement)> entities = new List<(Entity, XElement)>();

        // Create all entities first
        foreach (var entityElement in element.Elements())
        {
            Entity entity = world.CreateEntity();
            remap.AddEntity(int.Parse(entityElement.Attribute("Id")?.Value), entity);
            entities.Add((entity, entityElement));
        }

        // Then deserialize them
        foreach (var (entity, entityElement) in entities)
        {
            _entitySerializer.DeserializeEntity(entity, entityElement, remap);
        }

        // The first entity is the root
        return new Scene(id, world, entities[0].Item1) { OriginalElement = element };
    }

    public void Save(XElement element, IAsset asset)
    {
        Scene scene = (Scene)asset;
        // Go through all entities in the scene and serialize them

        element.SetAttributeValue("Id", scene.SceneId);

        if (scene.Root.Has<SceneComponent>())
        {
            element.SetAttributeString("From", scene.Root.Get<SceneComponent>().SceneId);
        }

        scene.SceneWorld.Query(new Query(), entity =>
        {
            XElement entityElement = new XElement("Entity");
            entityElement.SetAttributeValue("Id", entity.Id.ToString());

            _entitySerializer.SerializeEntity(entity, entityElement);

            element.Add(entityElement);
        });
    }
}
