using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalignEngine;

public enum TextureFormat
{
    // Uncompressed formats
    R8,             // 8-bit red channel
    RG8,            // 8-bit red + green
    RGB8,           // 8-bit RGB
    RGBA8,          // 8-bit RGBA (default for most)
    R16F,           // 16-bit float red
    RG16F,          // 16-bit float RG
    RGB16F,         // 16-bit float RGB
    RGBA16F,        // 16-bit float RGBA
    R32F,           // 32-bit float red
    RG32F,          // 32-bit float RG
    RGB32F,         // 32-bit float RGB
    RGBA32F,        // 32-bit float RGBA

    // Depth/stencil formats
    Depth16,
    Depth24,
    Depth32F,
    Depth24Stencil8,

    // Compressed formats
    DXT1,
    DXT3,
    DXT5,
    BC7,
}

public enum TextureWrapMode
{
    Repeat,
    MirroredRepeat,
    ClampToEdge,
    ClampToBorder
}

public enum TextureFilter
{
    Nearest,
    Linear,
    NearestMipmapNearest,
    LinearMipmapNearest,
    NearestMipmapLinear,
    LinearMipmapLinear
}

public interface ITextureDescriptor
{
    int Width { get; }
    int Height { get; }

    TextureFormat Format { get; }

    bool GenerateMipmaps { get; }

    TextureWrapMode WrapS { get; }
    TextureWrapMode WrapT { get; }

    TextureFilter FilterMin { get; }
    TextureFilter FilterMag { get; }

    Color[]? InitialData { get; }
}

public class TextureDescriptor : ITextureDescriptor
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int Depth { get; set; } = 1; 

    public TextureFormat Format { get; set; } = TextureFormat.RGBA8;

    public bool GenerateMipmaps { get; set; } = false;

    public TextureWrapMode WrapS { get; set; } = TextureWrapMode.Repeat;
    public TextureWrapMode WrapT { get; set; } = TextureWrapMode.Repeat;

    public TextureFilter FilterMin { get; set; } = TextureFilter.Linear;
    public TextureFilter FilterMag { get; set; } = TextureFilter.Linear;

    public Color[]? InitialData { get; set; } = null;

    public TextureDescriptor() { }

    public TextureDescriptor(int width, int height, TextureFormat format)
    {
        Width = width;
        Height = height;
        Format = format;
    }
}
