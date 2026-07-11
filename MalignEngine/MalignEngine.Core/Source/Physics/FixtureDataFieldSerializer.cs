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

                string type = fixture.GetAttributeString("Type", "");
                if (type == "RectangleShape2D")
                {
                    shape = new RectangleShape2D(fixture.GetAttributeFloat("Width", 1f), fixture.GetAttributeFloat("Height", 1f));
                }
                else if (type == "CircleShape2D")
                {
                    shape = new CircleShape2D(fixture.GetAttributeFloat("Radius", 1f));
                }

                if (shape == null)
                {
                    continue;
                }

                fixturesList.Add(new FixtureData2D(shape,
                    fixture.GetAttributeFloat("Density", 1f),
                    fixture.GetAttributeFloat("Friction", 0.5f),
                    fixture.GetAttributeFloat("Restitution", 0.5f)));
            }
        }

        return fixturesList.ToArray();
    }

    public bool SupportsType(Type type)
    {
        return type == typeof(FixtureData2D[]);
    }
}