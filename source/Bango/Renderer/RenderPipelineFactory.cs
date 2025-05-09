﻿using Veldrid;

namespace Bango;
public class PipelineFactory
{
	private VertexElementDescription[]? vertexElementDescriptions;
	private FaceCullMode faceCullMode = FaceCullMode.Back;
	private Shader? shader;
	private Framebuffer? framebuffer;
	private List<ResourceLayoutElementDescription> objectResources = new();
	private List<ResourceLayoutElementDescription> lightingResources = new();

	public PipelineFactory() { }

	public PipelineFactory WithVertexElementDescriptions( VertexElementDescription[] vertexElementDescriptions )
	{
		this.vertexElementDescriptions = vertexElementDescriptions;
		return this;
	}

	public PipelineFactory WithFaceCullMode( FaceCullMode faceCullMode )
	{
		this.faceCullMode = faceCullMode;

		return this;
	}

	public PipelineFactory WithShader( Shader shader )
	{
		this.shader = shader;

		return this;
	}

	public PipelineFactory WithFramebuffer( Framebuffer framebuffer )
	{
		this.framebuffer = framebuffer;

		return this;
	}

	public PipelineFactory AddObjectResource( string name, ResourceKind kind, ShaderStages shaderStages )
	{
		objectResources.Add( new ResourceLayoutElementDescription( name, kind, shaderStages ) );
		return this;
	}

	public PipelineFactory AddLightingResource( string name, ResourceKind kind, ShaderStages shaderStages )
	{
		lightingResources.Add( new ResourceLayoutElementDescription( name, kind, shaderStages ) );
		return this;
	}

	public RenderPipeline Build()
	{
		if ( framebuffer == null )
			throw new Exception( "Pipeline has no framebuffer" );

		if ( shader == null )
			throw new Exception( "Pipeline has no shader" );

		var blendState = new BlendStateDescription()
		{
			AttachmentStates = new BlendAttachmentDescription[framebuffer.ColorTargets.Count],
			BlendFactor = new RgbaFloat( 0, 0, 0, 0 )
		};

		for ( int i = 0; i < framebuffer.ColorTargets.Count; i++ )
		{
			blendState.AttachmentStates[i] = BlendAttachmentDescription.AlphaBlend;
		}

		var vertexLayoutDescription = new VertexLayoutDescription( vertexElementDescriptions );
		var objectResourceLayoutDescription = new ResourceLayoutDescription()
		{
			Elements = objectResources.ToArray()
		};

		var objectResourceLayout = Device.ResourceFactory.CreateResourceLayout( objectResourceLayoutDescription );
		var lightingResourceLayoutDescription = new ResourceLayoutDescription()
		{
			Elements = lightingResources.ToArray()
		};

		var lightingResourceLayout = Device.ResourceFactory.CreateResourceLayout( lightingResourceLayoutDescription );
		var pipelineDescription = new GraphicsPipelineDescription()
		{
			BlendState = blendState,

			DepthStencilState = new DepthStencilStateDescription(
				true,
				true,
				ComparisonKind.LessEqual ),

			RasterizerState = new RasterizerStateDescription(
				faceCullMode,
				PolygonFillMode.Solid,
				FrontFace.Clockwise,
				true,
				false ),

			PrimitiveTopology = PrimitiveTopology.TriangleList,
			ResourceLayouts = new[] { objectResourceLayout, lightingResourceLayout },
			ShaderSet = new ShaderSetDescription( new[] { vertexLayoutDescription }, shader.ShaderProgram ),
			Outputs = framebuffer.OutputDescription
		};

		var pipeline = Device.ResourceFactory.CreateGraphicsPipeline( pipelineDescription );
		return new RenderPipeline( pipeline, objectResourceLayout, lightingResourceLayout );
	}
}
