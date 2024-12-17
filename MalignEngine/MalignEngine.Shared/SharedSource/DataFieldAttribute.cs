namespace MalignEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class DataFieldAttribute : DataFieldBaseAttribute
{
    public readonly string Name;

    public readonly bool Required;

    public DataFieldAttribute(string name, bool readOnly = false, bool save = true, bool required = false, Type? customTypeSerializer = null) : base(readOnly, save, customTypeSerializer)
    {
        Name = name;
        Required = required;
    }

    public override string ToString()
    {
        return Name;
    }
}

public abstract class DataFieldBaseAttribute : Attribute
{
    public readonly int Priority;
    public readonly Type? CustomTypeSerializer;
    public readonly bool ReadOnly;
    public readonly bool Save;

    protected DataFieldBaseAttribute(bool readOnly = false, bool save = true, Type? customTypeSerializer = null)
    {
        Save = save;
        ReadOnly = readOnly;
        CustomTypeSerializer = customTypeSerializer;
    }
}