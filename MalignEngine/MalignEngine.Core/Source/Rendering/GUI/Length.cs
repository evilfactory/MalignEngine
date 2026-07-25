namespace MalignEngine;

public enum LengthUnit
{
    Auto,
    Pixels,
    Percent,
    Fill
}

public readonly struct Length
{
    public LengthUnit Unit { get; }
    public float Value { get; }

    public Length(LengthUnit unit, float value)
    {
        Unit = unit;
        Value = value;
    }

    public static Length Auto => new(LengthUnit.Auto, 0);
    public static Length Fill => new(LengthUnit.Fill, 0);
    public static Length Pixels(float pixels) => new(LengthUnit.Pixels, pixels);
    public static Length Percent(float percent) => new(LengthUnit.Percent, percent);
}