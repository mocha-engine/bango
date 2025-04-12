namespace Bango.Renderer.UI;

public partial class PanelRenderer
{
	public AtlasBuilder AtlasBuilder { get; set; }

	public PanelRenderer()
	{
		AtlasBuilder = new();

		shader = ShaderBuilder.Default.FromPath( "core/shaders/ui/ui.mshdr" )
			.WithFramebuffer( RendererInstance.Current.MultisampledFramebuffer )
			.Build();

		AtlasBuilder.OnBuild += CreateResources;
		shader.OnRecompile += CreateResources;

		Event.Register( this );
	}

	public void NewFrame()
	{
		Vertices.Clear();
		RectCount = 0;
	}

	public void AddRectangle( Common.Rectangle rect, RectangleInfo info )
	{
		var colors = (info.FillMode ?? new FillMode()).GetColors();
		var textureCoordinates = info.TextureCoordinates ?? new Common.Rectangle( 0, 0, 1, 1 );
		var flags = info.Flags;
		var rounding = info.Rounding ?? new Vector4( 0, 0, 0, 0 );
		var unitRange = info.UnitRange ?? new Vector2( 0.0f );

		var dpi = Screen.DpiScale;
		var renderScale = Screen.RenderScale;
		var dpiRect = rect;
		dpiRect.X *= dpi * renderScale;
		dpiRect.Y *= dpi * renderScale;
		dpiRect.Width *= dpi * renderScale;
		dpiRect.Height *= dpi * renderScale;

		var ndcRect = dpiRect / (Screen.RawSize * Screen.RenderScale);
		
		var vertices = new UIVertex[4];
		for ( int i = 0; i < 4; i++ )
		{
			var x = RectVertices[i];
			var position = x.Position;
			position.X = (x.Position.X * ndcRect.Size.X) + ndcRect.Position.X;
			position.Y = (x.Position.Y * ndcRect.Size.Y) + ndcRect.Position.Y;

			var texCoords = x.TexCoords;
			texCoords.X = (x.TexCoords.X * textureCoordinates.Size.X) + textureCoordinates.Position.X;
			texCoords.Y = (x.TexCoords.Y * textureCoordinates.Size.Y) + textureCoordinates.Position.Y;

			var tx = x;
			position *= 2.0f;
			position.X -= 1.0f;
			position.Y = 1.0f - position.Y;

			tx.Position = position;
			tx.TexCoords = texCoords;
			tx.PanelPos *= dpiRect.Size;
			tx.PanelSize = dpiRect.Size;
			tx.Color = i switch
			{
				0 => colors.a,
				1 => colors.d,
				2 => colors.b,
				3 => colors.c,
				_ => Vector4.Zero,
			};
			tx.Flags = (short)flags;
			tx.Rounding = rounding * dpi * renderScale;
			tx.UnitRange = unitRange;

			vertices[i] = tx;
		}

		Vertices.AddRange( vertices );
		RectCount++;
		isDirty = true;
	}
}
