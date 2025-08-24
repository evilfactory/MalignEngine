using Silk.NET.OpenGL;

namespace MalignEngine;

public class GLTextureResource : ITextureResource, IGLGpuHandle
{
    public int Width { get; private set; }

    public int Height { get; private set; }

    private GL _gl;
    private ITextureDescriptor _descriptor;
    private GLRenderingAPI _renderAPI;

    private uint _handle;

    public GLTextureResource(GL gl, GLRenderingAPI renderAPI, ITextureDescriptor descriptor)
    {
        _gl = gl;
        _descriptor = descriptor;
        _renderAPI = renderAPI;

        Width = _descriptor.Width;
        Height = _descriptor.Height;

        _renderAPI.Submit(ctx =>
        {
            _handle = gl.GenTexture();
        });

        Resize(descriptor.Width, descriptor.Height);

        if (_descriptor.InitialData != null)
        {
            SubmitData(_descriptor.InitialData);
        }
    }

    public void Resize(int width, int height)
    {
        _renderAPI.Submit(ctx =>
        {

            Width = width;
            Height = height;

            unsafe
            {
                Bind();

                GLFormatInfo formatInfo = _descriptor.Format.ToGLFormat();

                _gl.TexImage2D(GLEnum.Texture2D, 0, (int)formatInfo.InternalFormat, (uint)width, (uint)height, 0, formatInfo.PixelFormat, formatInfo.PixelType, null);

                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)WrapModeToGLType(_descriptor.WrapS));
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)WrapModeToGLType(_descriptor.WrapT));

                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureFilterToGLType(_descriptor.FilterMin));
                _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureFilterToGLType(_descriptor.FilterMag));

                if (_descriptor.GenerateMipmaps)
                {
                    _gl.GenerateMipmap(TextureTarget.Texture2D);
                }
            }

        });
    }

    public void SubmitData(Color[] data)
    {
        _renderAPI.Submit(ctx =>
        {

            unsafe
            {
                Span<Color> dataSpan = data.AsSpan();
                fixed (void* d = &dataSpan[0])
                {
                    GLFormatInfo formatInfo = _descriptor.Format.ToGLFormat();

                    _gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, (uint)Width, (uint)Height, formatInfo.PixelFormat, formatInfo.PixelType, d);
                }
            }
        });
    }

    public void SubmitData(System.Drawing.Rectangle bounds, byte[] data)
    {
        _renderAPI.Submit(ctx =>
        {

            unsafe
            {
                GLFormatInfo formatInfo = _descriptor.Format.ToGLFormat();

                Bind();
                Span<byte> dataSpan = data.AsSpan();
                fixed (void* ptr = dataSpan)
                {
                    _gl.TexSubImage2D(
                        target: TextureTarget.Texture2D,
                        level: 0,
                        xoffset: bounds.Left,
                        yoffset: bounds.Top,
                        width: (uint)bounds.Width,
                        height: (uint)bounds.Height,
                        format: formatInfo.PixelFormat,
                        type: formatInfo.PixelType,
                        pixels: ptr
                    );
                }
            }

        });
    }

    public void SubmitData(System.Drawing.Rectangle bounds, Color[] data)
    {
        _renderAPI.Submit(ctx =>
        {

            unsafe
            {
                GLFormatInfo formatInfo = _descriptor.Format.ToGLFormat();

                Bind();
                Span<Color> dataSpan = data.AsSpan();
                fixed (void* ptr = dataSpan)
                {
                    _gl.TexSubImage2D(
                        target: TextureTarget.Texture2D,
                        level: 0,
                        xoffset: bounds.Left,
                        yoffset: bounds.Top,
                        width: (uint)bounds.Width,
                        height: (uint)bounds.Height,
                        format: formatInfo.PixelFormat,
                        type: formatInfo.PixelType,
                        pixels: ptr
                    );
                }
            }

        });
    }

    public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
    {
        _gl.ActiveTexture(textureSlot);
        _gl.BindTexture(TextureTarget.Texture2D, _handle);
    }

    public void Dispose()
    {
        _renderAPI.Submit(ctx =>
        {
            _gl.DeleteTexture(_handle);
        });
    }

    private static GLEnum WrapModeToGLType(TextureWrapMode wrapMode)
    {
        switch (wrapMode)
        {
            case TextureWrapMode.Repeat:
                return GLEnum.Repeat;
            case TextureWrapMode.MirroredRepeat:
                return GLEnum.MirroredRepeat;
            case TextureWrapMode.ClampToEdge:
                return GLEnum.ClampToEdge;
            case TextureWrapMode.ClampToBorder:
                return GLEnum.ClampToBorder;
            default:
                throw new Exception("Invalid wrap mode");
        }
    }

    private static GLEnum TextureFilterToGLType(TextureFilter textureFilter)
    {
        switch (textureFilter)
        {
            case TextureFilter.Linear:
                return GLEnum.Linear;
            case TextureFilter.LinearMipmapLinear:
                return GLEnum.LinearMipmapLinear;
            case TextureFilter.NearestMipmapLinear:
                return GLEnum.NearestMipmapLinear;
            case TextureFilter.Nearest:
                return GLEnum.Nearest;
            default:
                throw new Exception("Invalid wrap mode");
        }
    }

    public uint GetHandle()
    {
        return _handle;
    }
}

public readonly struct GLFormatInfo
{
    public readonly GLEnum InternalFormat;
    public readonly GLEnum PixelFormat;
    public readonly GLEnum PixelType;

    public GLFormatInfo(GLEnum internalFormat, GLEnum pixelFormat, GLEnum pixelType)
    {
        InternalFormat = internalFormat;
        PixelFormat = pixelFormat;
        PixelType = pixelType;
    }
}

public static class TextureFormatExtensions
{
    public static GLFormatInfo ToGLFormat(this TextureFormat format)
    {
        return format switch
        {
            TextureFormat.R8 => new GLFormatInfo(GLEnum.R8, GLEnum.Red, GLEnum.UnsignedByte),
            TextureFormat.RG8 => new GLFormatInfo(GLEnum.RG8, GLEnum.RG, GLEnum.UnsignedByte),
            TextureFormat.RGB8 => new GLFormatInfo(GLEnum.Rgb8, GLEnum.Rgb, GLEnum.UnsignedByte),
            TextureFormat.RGBA8 => new GLFormatInfo(GLEnum.Rgba8, GLEnum.Rgba, GLEnum.UnsignedByte),

            TextureFormat.R16F => new GLFormatInfo(GLEnum.R16f, GLEnum.Red, GLEnum.HalfFloat),
            TextureFormat.RG16F => new GLFormatInfo(GLEnum.RG16f, GLEnum.RG, GLEnum.HalfFloat),
            TextureFormat.RGB16F => new GLFormatInfo(GLEnum.Rgb16f, GLEnum.Rgb, GLEnum.HalfFloat),
            TextureFormat.RGBA16F => new GLFormatInfo(GLEnum.Rgba16f, GLEnum.Rgba, GLEnum.HalfFloat),

            TextureFormat.R32F => new GLFormatInfo(GLEnum.R32f, GLEnum.Red, GLEnum.Float),
            TextureFormat.RG32F => new GLFormatInfo(GLEnum.RG32f, GLEnum.RG, GLEnum.Float),
            TextureFormat.RGB32F => new GLFormatInfo(GLEnum.Rgb32f, GLEnum.Rgb, GLEnum.Float),
            TextureFormat.RGBA32F => new GLFormatInfo(GLEnum.Rgba32f, GLEnum.Rgba, GLEnum.Float),

            TextureFormat.Depth16 => new GLFormatInfo(GLEnum.DepthComponent16, GLEnum.DepthComponent, GLEnum.UnsignedShort),
            TextureFormat.Depth24 => new GLFormatInfo(GLEnum.DepthComponent24, GLEnum.DepthComponent, GLEnum.UnsignedInt),
            TextureFormat.Depth32F => new GLFormatInfo(GLEnum.DepthComponent32f, GLEnum.DepthComponent, GLEnum.Float),
            TextureFormat.Depth24Stencil8 => new GLFormatInfo(GLEnum.Depth24Stencil8, GLEnum.DepthStencil, GLEnum.UnsignedInt248),

            //TextureFormat.DXT1 => new GLFormatInfo(GLEnum.CompressedRgbS3tcDxt1Ext, 0, 0),
            //TextureFormat.DXT3 => new GLFormatInfo(GLEnum.CompressedRgbaS3tcDxt3Ext, 0, 0),
            //TextureFormat.DXT5 => new GLFormatInfo(GLEnum.CompressedRgbaS3tcDxt5Ext, 0, 0),
            //TextureFormat.BC7 => new GLFormatInfo(GLEnum.CompressedRgbaBptcUnormArb, 0, 0),

            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported texture format")
        };
    }
}