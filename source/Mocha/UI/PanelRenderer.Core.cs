using System.Runtime.InteropServices;

namespace Mocha.Renderer.UI;
partial class PanelRenderer
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
		Log.Info( $"Updating PanelRenderer object resource set" );

		Log.Info( $"New atlas has size {AtlasBuilder.Texture.Size}" );

		var objectResourceSetDescription = new ResourceSetDescription(
			pipeline.ResourceLayouts[0],
			AtlasBuilder.Texture.VeldridTexture,
			Device.Aniso4xSampler
		);

		objectResourceSet = Device.ResourceFactory.CreateResourceSet( objectResourceSetDescription );
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
