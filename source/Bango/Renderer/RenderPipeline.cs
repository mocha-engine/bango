using Veldrid;

namespace Bango.Renderer;

public struct RenderPipeline
{
	public static PipelineFactory Factory => new();

	public ResourceLayout[] ResourceLayouts;
	public Pipeline VeldridPipeline;

	public RenderPipeline( Pipeline pipeline, params ResourceLayout[] resourceSets )
	{
		this.ResourceLayouts = resourceSets;
		this.VeldridPipeline = pipeline;
	}

	public void Delete()
	{
		VeldridPipeline?.Dispose();
		ResourceLayouts?.ToList().ForEach( x => x.Dispose() );
	}
}
