using System.Xml.Linq;

namespace MalignEngine;

public abstract class XmlAsset : IAsset
{
    public abstract void Load(XElement element);
    public abstract void Save(XElement element);
}