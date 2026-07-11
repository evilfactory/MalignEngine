using System.Xml.Linq;

namespace MalignEngine;

public class FixtureDataFieldSerializer : IDataFieldSerializer
{
    public void Serialize(DataFieldAttribute dataField, object? obj, XElement element)
    {
        throw new NotImplementedException();
    }

    public object Deserialize(DataFieldAttribute dataField, XElement element)
    {
        List<FixtureData2D> fixturesList = [];
        XElement? fixureElements = element.Element(dataField.Name);

        if (fixureElements != null)
        {
            foreach (var fixture in fixureElements.Elements())
            {
                IPhysicsShape2D? shape = null;

                string type = fixture.GetAttributeString("type", "");
                if (type == "RectangleShape2D")
                {
                    shape = new RectangleShape2D(fixture.GetAttributeFloat("width", 1f), fixture.GetAttributeFloat("height", 1f));
                }
                else if (type == "CircleShape2D")
                {
                    shape = new CircleShape2D(fixture.GetAttributeFloat("radius", 1f));
                }

                if (shape == null)
                {
                    continue;
                }

                fixturesList.Add(new FixtureData2D(shape,
                    fixture.GetAttributeFloat("density", 1f),
                    fixture.GetAttributeFloat("friction", 0.5f),
                    fixture.GetAttributeFloat("restitution", 0.5f)));
            }
        }

        return fixturesList.ToArray();
    }

    public bool SupportsType(Type type)
    {
        return type == typeof(FixtureData2D[]);
    }
}