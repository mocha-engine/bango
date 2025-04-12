using Veldrid;
using System.Runtime.InteropServices;

namespace Bango.Renderer.UI;
partial class PanelRenderer
{
	private UIVertex[] RectVertices => new UIVertex[] {
		new UIVertex { Position = new ( 0, 0, 0 ), TexCoords = new( 0, 0 ), PanelPos = new( 0, 0 ) },
		new UIVertex { Position = new ( 0, 1, 0 ), TexCoords = new( 0, 1 ), PanelPos = new( 0, 1 ) },
		new UIVertex { Position = new ( 1, 0, 0 ), TexCoords = new( 1, 0 ), PanelPos = new( 1, 0 ) },
		new UIVertex { Position = new ( 1, 1, 0 ), TexCoords = new( 1, 1 ), PanelPos = new( 1, 1 ) },
	};

	private uint[] RectIndices => new uint[] {
		2, 1, 0,
		1, 2, 3
	};

	private int RectCount = 0;
	private List<UIVertex> Vertices = new();

	[StructLayout( LayoutKind.Sequential )]
	public struct UIVertex
	{
		public Vector3 Position { get; set; }
		public Vector2 TexCoords { get; set; }
		public Vector4 Color { get; set; }
		public Vector2 PanelPos { get; set; }
		public Vector2 PanelSize { get; set; }
		public short Flags { get; set; }
		public Vector4 Rounding { get; set; }
		public Vector2 UnitRange { get; set; }

		public static VertexElementDescription[] VertexElementDescriptions = new[]
		{
			new VertexElementDescription( "position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3 ),
			new VertexElementDescription( "texCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2 ),
			new VertexElementDescription( "color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4 ),
			new VertexElementDescription( "panelPos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2 ),
			new VertexElementDescription( "panelSize", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2 ),
			new VertexElementDescription( "flags", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Int1 ),
			new VertexElementDescription( "rounding", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4 ),
			new VertexElementDescription( "unitRange", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2 )
		};
	}
}
