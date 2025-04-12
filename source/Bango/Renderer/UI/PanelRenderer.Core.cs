using Veldrid;
using System.Runtime.InteropServices;

namespace Bango;partial class PanelRenderer
{
	private ResourceSet objectResourceSet;
	private uint indexCount;
	private bool isDirty = false;

	private DeviceBuffer vertexBuffer;
	private DeviceBuffer indexBuffer;
	private Shader shader;
	private RenderPipeline pipeline;

	private void UpdateIndexBuffer( uint[] indices )
	{
		indexCount = (uint)indices.Length;
		var targetSize = indexCount * sizeof( uint );

		if ( indexBuffer == null || indexBuffer.SizeInBytes != targetSize )
		{
			indexBuffer?.Dispose();

			indexBuffer = Device.ResourceFactory.CreateBuffer(
				new Veldrid.BufferDescription( targetSize, Veldrid.BufferUsage.IndexBuffer )
			);
		}

		Device.UpdateBuffer( indexBuffer, 0, indices );
	}

	private void UpdateVertexBuffer( UIVertex[] vertices )
	{
		var vertexStructSize = (uint)Marshal.SizeOf( typeof( UIVertex ) );
		var targetSize = (uint)vertices.Length * vertexStructSize;

		if ( vertexBuffer == null || vertexBuffer.SizeInBytes != targetSize )
		{
			vertexBuffer?.Dispose();

			vertexBuffer = Device.ResourceFactory.CreateBuffer(
				new Veldrid.BufferDescription( targetSize, Veldrid.BufferUsage.VertexBuffer )
			);
		}

		Device.UpdateBuffer( vertexBuffer, 0, vertices );
	}

	private void CreateResources()
	{
		pipeline = RenderPipeline.Factory
			.AddObjectResource( "g_tAtlas", ResourceKind.TextureReadOnly, ShaderStages.Fragment )
			.AddObjectResource( "g_sSampler", ResourceKind.Sampler, ShaderStages.Fragment )
			.AddObjectResource( "g_vScreenSize", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment )
			.WithVertexElementDescriptions( UIVertex.VertexElementDescriptions )
			.WithFramebuffer( RendererInstance.Current.MultisampledFramebuffer )
			.WithShader( shader )
			.Build();

		var ub = Device.ResourceFactory.CreateBuffer( new BufferDescription( 16, BufferUsage.UniformBuffer ) );
		Device.UpdateBuffer( ub, 0, new float[] { Screen.Size.X, Screen.Size.Y } );

		var objectResourceSetDescription = new ResourceSetDescription(
			pipeline.ResourceLayouts[0],
			AtlasBuilder.Texture.VeldridTexture,
			Device.Aniso4xSampler,
			ub
		);

		objectResourceSet = Device.ResourceFactory.CreateResourceSet( objectResourceSetDescription );
	}

	[Event.Window.Resized]
	public void OnWindowResized( Point2 newSize )
	{
		CreateResources();
	}

	private void UpdateBuffers()
	{
		UpdateVertexBuffer( Vertices.ToArray() );
		var generatedIndices = new List<uint>();

		for ( int i = 0; i < RectCount; ++i )
		{
			generatedIndices.AddRange( RectIndices.Select( x => (uint)(x + i * 4) ).ToArray() );
		}

		UpdateIndexBuffer( generatedIndices.ToArray() );
		isDirty = false;
	}

	public void Draw( CommandList commandList )
	{
		if ( isDirty )
			UpdateBuffers();

		if ( vertexBuffer == null || indexBuffer == null )
			return;

		if ( objectResourceSet == null )
			return;

		commandList.SetVertexBuffer( 0, vertexBuffer );
		commandList.SetPipeline( pipeline.VeldridPipeline );

		commandList.SetGraphicsResourceSet( 0, objectResourceSet );

		commandList.SetIndexBuffer( indexBuffer, IndexFormat.UInt32 );

		commandList.DrawIndexed(
			indexCount: indexCount,
			instanceCount: 1,
			indexStart: 0,
			vertexOffset: 0,
			instanceStart: 0
		);
	}
}
