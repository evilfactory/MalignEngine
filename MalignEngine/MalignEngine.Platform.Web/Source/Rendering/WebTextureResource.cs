using nkast.Wasm.Canvas.WebGL;

namespace MalignEngine;

public class WebTextureResource : ITextureResource
{
    public int Width { get; private set; }

    public int Height { get; private set; }
    public WebGLTexture WebGLTexture => _buffer;

    private IWebGL2RenderingContext _gl;
    private ITextureDescriptor _descriptor;
    private IRenderingAPI _renderAPI;

    private WebGLTexture _buffer;

    public WebTextureResource(IWebGL2RenderingContext gl, IRenderingAPI renderAPI, ITextureDescriptor descriptor)
    {
        _gl = gl;
        _descriptor = descriptor;
        _renderAPI = renderAPI;

        Width = _descriptor.Width;
        Height = _descriptor.Height;

        _renderAPI.Submit(ctx =>
        {
            _buffer = gl.CreateTexture();
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

                _gl.TexImage2D(WebGLTextureTarget.TEXTURE_2D, 0, formatInfo.InternalFormat, width, height, formatInfo.PixelFormat, formatInfo.PixelType);

                _gl.TexParameter(WebGLTextureTarget.TEXTURE_2D, WebGLTexParamName.TEXTURE_WRAP_S, WrapModeToGLType(_descriptor.WrapS));
                _gl.TexParameter(WebGLTextureTarget.TEXTURE_2D, WebGLTexParamName.TEXTURE_WRAP_T, WrapModeToGLType(_descriptor.WrapT));

                _gl.TexParameter(WebGLTextureTarget.TEXTURE_2D, WebGLTexParamName.TEXTURE_MIN_FILTER, TextureFilterToGLType(_descriptor.FilterMin));
                _gl.TexParameter(WebGLTextureTarget.TEXTURE_2D, WebGLTexParamName.TEXTURE_MAG_FILTER, TextureFilterToGLType(_descriptor.FilterMag));

                if (_descriptor.GenerateMipmaps)
                {
                    _gl.GenerateMipmap(WebGLTextureTarget.TEXTURE_2D);
                }
            }

        });
    }

    public void SubmitData(Color[] data)
    {
        _renderAPI.Submit(ctx =>
        {
            GLFormatInfo formatInfo = _descriptor.Format.ToGLFormat();
            _gl.TexSubImage2D(WebGLTextureTarget.TEXTURE_2D, 0, 0, 0, Width, Height, formatInfo.PixelFormat, formatInfo.PixelType, data);
        });
    }

    public void SubmitData(System.Drawing.Rectangle bounds, byte[] data)
    {
        _renderAPI.Submit(ctx =>
        {
            GLFormatInfo formatInfo = _descriptor.Format.ToGLFormat();

            Bind();
            _gl.TexSubImage2D(
                target: WebGLTextureTarget.TEXTURE_2D,
                level: 0,
                xoffset: bounds.Left,
                yoffset: bounds.Top,
                width: bounds.Width,
                height: bounds.Height,
                format: formatInfo.PixelFormat,
                type: formatInfo.PixelType,
                pixels: data
            );
        });
    }

    public void SubmitData(System.Drawing.Rectangle bounds, Color[] data)
    {
        _renderAPI.Submit(ctx =>
        {
            GLFormatInfo formatInfo = _descriptor.Format.ToGLFormat();

            Bind();

            _gl.TexSubImage2D(
                target: WebGLTextureTarget.TEXTURE_2D,
                level: 0,
                xoffset: bounds.Left,
                yoffset: bounds.Top,
                width: bounds.Width,
                height: bounds.Height,
                format: formatInfo.PixelFormat,
                type: formatInfo.PixelType,
                pixels: data
            );
        });
    }

    public void Bind(WebGLTextureUnit textureSlot = WebGLTextureUnit.TEXTURE0)
    {
        _gl.ActiveTexture(textureSlot);
        _gl.BindTexture(WebGLTextureTarget.TEXTURE_2D, _buffer);
    }

    public void Dispose()
    {
        _renderAPI.Submit(ctx =>
        {
            _buffer.Dispose();
        });
    }

    private static WebGLTexParam WrapModeToGLType(TextureWrapMode wrapMode)
    {
        switch (wrapMode)
        {
            case TextureWrapMode.Repeat:
                return WebGLTexParam.REPEAT;
            case TextureWrapMode.MirroredRepeat:
                return WebGLTexParam.MIRRORED_REPEAT;
            case TextureWrapMode.ClampToEdge:
                return WebGLTexParam.CLAMP_TO_EDGE;
            case TextureWrapMode.ClampToBorder:
                throw new NotImplementedException("Not implemented");
            default:
                throw new Exception("Invalid wrap mode");
        }
    }

    private static WebGLTexParam TextureFilterToGLType(TextureFilter textureFilter)
    {
        switch (textureFilter)
        {
            case TextureFilter.Linear:
                return WebGLTexParam.LINEAR;
            case TextureFilter.LinearMipmapLinear:
                return WebGLTexParam.LINEAR_MIPMAP_LINEAR;
            case TextureFilter.NearestMipmapLinear:
                return WebGLTexParam.NEAREST_MIPMAP_NEAREST;
            case TextureFilter.Nearest:
                return WebGLTexParam.NEAREST;
            default:
                throw new Exception("Invalid wrap mode");
        }
    }
}

public readonly struct GLFormatInfo
{
    public readonly WebGLInternalFormat InternalFormat;
    public readonly WebGLFormat PixelFormat;
    public readonly WebGLTexelType PixelType;

    public GLFormatInfo(WebGLInternalFormat internalFormat, WebGLFormat pixelFormat, WebGLTexelType pixelType)
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
            TextureFormat.R8 => new GLFormatInfo(
                WebGLInternalFormat.R8,
                WebGLFormat.RED,
                WebGLTexelType.UNSIGNED_BYTE),

            TextureFormat.RG8 => new GLFormatInfo(
                WebGLInternalFormat.RG8,
                WebGLFormat.RG,
                WebGLTexelType.UNSIGNED_BYTE),

            TextureFormat.RGB8 => new GLFormatInfo(
                WebGLInternalFormat.RGB8,
                WebGLFormat.RGB,
                WebGLTexelType.UNSIGNED_BYTE),

            TextureFormat.RGBA8 => new GLFormatInfo(
                WebGLInternalFormat.RGBA8,
                WebGLFormat.RGBA,
                WebGLTexelType.UNSIGNED_BYTE),

            TextureFormat.R16F => new GLFormatInfo(
                WebGLInternalFormat.R16F,
                WebGLFormat.RED,
                WebGLTexelType.HALF_FLOAT),

            TextureFormat.RG16F => new GLFormatInfo(
                WebGLInternalFormat.RG16F,
                WebGLFormat.RG,
                WebGLTexelType.HALF_FLOAT),

            TextureFormat.RGBA16F => new GLFormatInfo(
                WebGLInternalFormat.RGBA16F,
                WebGLFormat.RGBA,
                WebGLTexelType.HALF_FLOAT),

            TextureFormat.R32F => new GLFormatInfo(
                WebGLInternalFormat.R32F,
                WebGLFormat.RED,
                WebGLTexelType.FLOAT),

            TextureFormat.RG32F => new GLFormatInfo(
                WebGLInternalFormat.RG32F,
                WebGLFormat.RG,
                WebGLTexelType.FLOAT),

            TextureFormat.RGBA32F => new GLFormatInfo(
                WebGLInternalFormat.RGBA32F,
                WebGLFormat.RGBA,
                WebGLTexelType.FLOAT),

            // Requires WEBGL_compressed_texture_s3tc
            //TextureFormat.DXT1 => ...
            //TextureFormat.DXT3 => ...
            //TextureFormat.DXT5 => ...

            // Requires EXT_texture_compression_bptc
            //TextureFormat.BC7 => ...

            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported texture format")
        };
    }
}