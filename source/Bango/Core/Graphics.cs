using Bango.Renderer.UI;

namespace Bango.Engine.Editor;

[Flags]
public enum RoundingFlags
{
	None = 0,
	TopLeft = 1,
	TopRight = 2,
	BottomLeft = 4,
	BottomRight = 8,

	All = TopLeft | TopRight | BottomLeft | BottomRight,
	Bottom = BottomLeft | BottomRight,
	Top = TopLeft | TopRight,
	Left = TopLeft | BottomLeft,
	Right = TopRight | BottomRight
}

public static partial class Graphics
{
	internal static PanelRenderer PanelRenderer { get; set; }

	public static void DrawRectangle( Rectangle bounds, RectangleInfo info )
	{
		PanelRenderer.AddRectangle( bounds, info );
	}

	public static void DrawRect( Rectangle bounds, Vector4 colorTop, Vector4 colorBottom, Vector4 rounding )
	{
		var info = new RectangleInfo()
		{
			FillMode = FillMode.LinearGradient( colorTop, colorBottom ),
			Rounding = rounding
		};

		PanelRenderer.AddRectangle( bounds, info );
	}

	public static void DrawRect( Rectangle bounds, Vector4 color, Vector4 rounding )
	{
		var info = new RectangleInfo()
		{
			FillMode = FillMode.Solid( color ),
			Rounding = rounding
		};

		PanelRenderer.AddRectangle( bounds, info );
	}

	public static void DrawShadow( Rectangle bounds, float size, FillMode fillMode, Vector4 rounding )
	{
		bounds = bounds.Expand( size / 2.0f );
		bounds.Y += 1;

		var adjustedRounding = rounding + new Vector4( size );

		for ( float i = 0; i < size; ++i )
		{
			var currBounds = bounds.Shrink( i );

			var c = fillMode.GetColors().a;
			c.W = (1f / size) * c.W;

			var rnd = new Vector4(
				(adjustedRounding.X - i).Clamp( 0, 1000 ),
				(adjustedRounding.Y - i).Clamp( 0, 1000 ),
				(adjustedRounding.Z - i).Clamp( 0, 1000 ),
				(adjustedRounding.W - i).Clamp( 0, 1000 )
			);

			var info = new RectangleInfo()
			{
				FillMode = FillMode.Solid( c ),
				Rounding = rnd
			};

			PanelRenderer.AddRectangle( currBounds, info );
		}
	}

	public static void DrawCharacter( Rectangle bounds, Texture texture, Rectangle atlasBounds, Vector4 color )
	{
		var flags = RenderMode.UseSdf;

		var texturePos = PanelRenderer.AtlasBuilder.AddOrGetTexture( texture );
		var textureSize = PanelRenderer.AtlasBuilder.Texture.Size;

		var texBounds = new Rectangle( (Vector2)texturePos, textureSize );

		// Move to top left of texture inside atlas
		atlasBounds.Y += textureSize.Y - texture.Height;
		atlasBounds.X += texBounds.X;

		// Convert to [0..1] normalized space
		atlasBounds /= textureSize;

		// Flip y axis
		atlasBounds.Y = 1.0f - atlasBounds.Y;

		// Find unit range
		var unitRange = new Vector2( 6.0f ) / texture.Size;

		var info = new RectangleInfo()
		{
			FillMode = FillMode.Solid( color ),
			TextureCoordinates = atlasBounds,
			Flags = flags,
			UnitRange = unitRange
		};

		PanelRenderer.AddRectangle( bounds, info );
	}

	public static void DrawTexture( Rectangle bounds, Texture texture )
	{
		var texturePos = PanelRenderer.AtlasBuilder.AddOrGetTexture( texture );
		var textureSize = PanelRenderer.AtlasBuilder.Texture.Size;

		var texBounds = new Rectangle( (Vector2)texturePos, texture.Size );

		// Convert to [0..1] normalized space
		texBounds /= textureSize;

		var info = new RectangleInfo()
		{
			Flags = RenderMode.UseRawImage,
			TextureCoordinates = texBounds
		};

		PanelRenderer.AddRectangle( bounds, info );
	}

	public static Vector2 MeasureText( string text, Font font, float fontSize )
	{
		float x = 0;

		foreach ( var c in text )
		{
			var glyph = font.FontData.Glyphs.First( x => x.Unicode == (int)c );

			x += glyph.Advance * fontSize;
		}

		// ????
		return new Vector2( x, fontSize * 1.25f );
	}
}
