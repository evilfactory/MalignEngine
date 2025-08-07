using System.Numerics;
using System.Xml.Linq;

namespace MalignEngine;

public static class XmlExtensions
{
    public static string GetAttributeString(this XElement element, string attributeName, string defaultValue = null)
    {
        XAttribute? attribute = element.Attribute(attributeName);
        if (attribute == null) { return defaultValue; }

        return attribute.Value;
    }

    public static int GetAttributeInt(this XElement element, string attributeName, int defaultValue = 0)
    {
        XAttribute? attribute = element.Attribute(attributeName);
        if (attribute == null) { return defaultValue; }

        return int.Parse(attribute.Value);
    }

    public static float GetAttributeFloat(this XElement element, string attributeName, float defaultValue = 0.0f)
    {
        XAttribute? attribute = element.Attribute(attributeName);
        if (attribute == null) { return defaultValue; }

        return float.Parse(attribute.Value);
    }

    public static bool GetAttributeBool(this XElement element, string attributeName, bool defaultValue = false)
    {
        XAttribute? attribute = element.Attribute(attributeName);
        if (attribute == null) { return defaultValue; }

        return bool.Parse(attribute.Value);
    }

    public static Vector2 GetAttributeVector2(this XElement element, string attributeName, Vector2 defaultValue = default)
    {
        XAttribute? attribute = element.Attribute(attributeName);
        if (attribute == null) { return defaultValue; }

        string[] values = attribute.Value.Split(',');
        return new Vector2(float.Parse(values[0]), float.Parse(values[1]));
    }

    public static Vector3 GetAttributeVector3(this XElement element, string attributeName, Vector3 defaultValue = default)
    {
        XAttribute? attribute = element.Attribute(attributeName);
        if (attribute == null) { return defaultValue; }

        string[] values = attribute.Value.Split(',');
        return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
    }

    public static Vector4 GetAttributeVector4(this XElement element, string attributeName, Vector4 defaultValue = default)
    {
        XAttribute? attribute = element.Attribute(attributeName);
        if (attribute == null) { return defaultValue; }

        string[] values = attribute.Value.Split(',');
        return new Vector4(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
    }

    public static Color GetAttributeColor(this XElement element, string attributeName, Color defaultValue = default)
    {
        XAttribute? attribute = element.Attribute(attributeName);
        if (attribute == null) { return defaultValue; }

        string[] values = attribute.Value.Split(',');
        return new Color(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
    }

    public static Rectangle GetAttributeRectangle(this XElement element, string attributeName, Rectangle defaultValue = default)
    {
        XAttribute? attribute = element.Attribute(attributeName);
        if (attribute == null) { return defaultValue; }

        string[] values = attribute.Value.Split(',');
        return new Rectangle(int.Parse(values[0]), int.Parse(values[1]), int.Parse(values[2]), int.Parse(values[3]));
    }

    public static void SetAttributeString(this XElement element, string attributeName, string value)
    {
        element.SetAttributeValue(attributeName, value);
    }

    public static void SetAttributeInt(this XElement element, string attributeName, int value)
    {
        element.SetAttributeValue(attributeName, value);
    }

    public static void SetAttributeFloat(this XElement element, string attributeName, float value)
    {
        element.SetAttributeValue(attributeName, value);
    }

    public static void SetAttributeBool(this XElement element, string attributeName, bool value)
    {
        element.SetAttributeValue(attributeName, value);
    }

    public static void SetAttributeVector2(this XElement element, string attributeName, Vector2 value)
    {
        element.SetAttributeValue(attributeName, $"{value.X},{value.Y}");
    }

    public static void SetAttributeVector3(this XElement element, string attributeName, Vector3 value)
    {
        element.SetAttributeValue(attributeName, $"{value.X},{value.Y},{value.Z}");
    }

    public static void SetAttributeVector4(this XElement element, string attributeName, Vector4 value)
    {
        element.SetAttributeValue(attributeName, $"{value.X},{value.Y},{value.Z},{value.W}");
    }

    public static void SetAttributeColor(this XElement element, string attributeName, Color value)
    {
        element.SetAttributeValue(attributeName, $"{value.R},{value.G},{value.B},{value.A}");
    }

    public static void SetAttributeRectangle(this XElement element, string attributeName, Rectangle value)
    {
        element.SetAttributeValue(attributeName, $"{value.X},{value.Y},{value.Width},{value.Height}");
    }
}