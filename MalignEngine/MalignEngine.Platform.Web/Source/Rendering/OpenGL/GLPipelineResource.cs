using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalignEngine;

public class GLPipelineResource : IPipelineResource
{
    public bool DepthTest { get; private set; }
    public bool StencilTest { get; private set; }
    public BlendingMode BlendingMode { get; private set; }
    public CullMode CullMode { get; private set; }

    private GL _gl;

    public GLPipelineResource(GL gl, IPipelineResourceDescriptor descriptor)
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
            _gl.Enable(GLEnum.Blend);

            if (BlendingMode == BlendingMode.AlphaBlend)
            {
                _gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
            }
            else if (BlendingMode == BlendingMode.Additive)
            {
                _gl.BlendFunc(GLEnum.One, GLEnum.One);
            }
        }
        else
        {
            _gl.Disable(GLEnum.Blend);
        }

        if (DepthTest)
        {
            _gl.Enable(GLEnum.DepthTest);
        }
        else
        {
            _gl.Disable(GLEnum.DepthTest);
        }

        if (CullMode != CullMode.None)
        {
            _gl.Enable(GLEnum.CullFace);

            if (CullMode == CullMode.Front)
            {
                _gl.CullFace(GLEnum.Front);
            }
            else if (CullMode == CullMode.Back)
            {
                _gl.CullFace(GLEnum.Back);
            }
        }
        else
        {
            _gl.Disable(GLEnum.CullFace);
        }
    }

    public void Dispose()
    {

    }
}
