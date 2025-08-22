namespace MalignEngine;

public interface IPipelineResourceDescriptor
{
    public BlendingMode BlendingMode { get; }
    public CullMode CullMode { get; }
    public bool DepthTest { get; }
    public bool StencilTest { get; }
}

public class PipelineResourceDescriptor : IPipelineResourceDescriptor
{
    public BlendingMode BlendingMode { get; set; }
    public CullMode CullMode { get; set; }
    public bool DepthTest { get; set; }
    public bool StencilTest { get; set; }

    public PipelineResourceDescriptor()
    {

    }
}
