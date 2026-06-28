using nkast.Wasm.Canvas.WebGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalignEngine;

public class WebPipelineResource : IPipelineResource
{
    public bool DepthTest { get; private set; }
    public bool StencilTest { get; private set; }
    public BlendingMode BlendingMode { get; private set; }
    public CullMode CullMode { get; private set; }

    private IWebGL2RenderingContext _gl;

    public WebPipelineResource(IWebGL2RenderingContext  gl, IPipelineResourceDescriptor descriptor)
    {
        _gl = gl;

        DepthTest = descriptor.DepthTest;
        StencilTest = descriptor.StencilTest;
        BlendingMode = descriptor.BlendingMode;
        CullMode = descriptor.CullMode;
    }

    public void Bind()
    {
        if (BlendingMode != BlendingMode.None)
        {
            _gl.Enable(WebGLCapability.BLEND);

            /*
            if (BlendingMode == BlendingMode.AlphaBlend)
            {
                _gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
            }
            else if (BlendingMode == BlendingMode.Additive)
            {
                _gl.BlendFunc(GLEnum.One, GLEnum.One);
            }
            */
        }
        else
        {
            _gl.Disable(WebGLCapability.BLEND);
        }

        if (DepthTest)
        {
            _gl.Enable(WebGLCapability.DEPTH_TEST);
        }
        else
        {
            _gl.Disable(WebGLCapability.DEPTH_TEST);
        }

        if (CullMode != CullMode.None)
        {
            _gl.Enable(WebGLCapability.CULL_FACE);

            if (CullMode == CullMode.Front)
            {
                _gl.CullFace(WebGLCullFaceMode.FRONT);
            }
            else if (CullMode == CullMode.Back)
            {
                _gl.CullFace(WebGLCullFaceMode.BACK);
            }
        }
        else
        {
            _gl.Disable(WebGLCapability.CULL_FACE);
        }
    }

    public void Dispose()
    {

    }
}
