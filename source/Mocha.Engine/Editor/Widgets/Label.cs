﻿namespace Bango.Engine.Editor;

internal class Label : Widget
{
	private string calculatedText = "";
	private string text;

	internal Font Font { get; set; }
	internal Font IconFont { get; set; }

	public string Text
	{
		get => text; set
		{
			text = value;
			CalculateText();
		}
	}

	public Color Color { get; set; } = Color.Black;
	public float FontSize { get; set; } = 13;

	private Font.Data GetFontDataForGlyph( char c )
	{
		if ( c > FontAwesome.IconMin )
			return IconFont.FontData;

		return Font.FontData;
	}

	private Texture GetTextureForGlyph( char c )
	{
		if ( c > FontAwesome.IconMin )
			return IconFont.FontTexture;

		return Font.FontTexture;
	}

	public Vector2 MeasureText( string text, float fontSize )
	{
		float x = 0;

		foreach ( var c in text )
		{
			if ( !GetFontDataForGlyph( c ).Glyphs.Any( x => x.Unicode == c ) )
			{
				x += fontSize;
				continue;
			}

			var glyph = GetFontDataForGlyph( c ).Glyphs.First( x => x.Unicode == c );
			x += (float)glyph.Advance * fontSize;
		}

		// ????
		return new Vector2( x, fontSize * 1.25f );
	}

	internal Label( string text, float fontSize = 13, string fontFamily = "inter" ) : base()
	{
		FontSize = fontSize;
		Text = text;

		SetFont( fontFamily );
	}

	private Rectangle FontBoundsToAtlasRect( Font.Glyph glyph, Font.Bounds bounds )
	{
		Vector2 min = new Vector2( glyph.AtlasBounds.Left,
								  glyph.AtlasBounds.Top );

		Vector2 max = new Vector2( glyph.AtlasBounds.Right,
								  glyph.AtlasBounds.Bottom );

		Vector2 mins = min;
		Vector2 maxs = (max - min) * new Vector2( 1, -1 );

		var glyphRect = new Rectangle( mins, maxs );

		return glyphRect;
	}

	internal override void Render()
	{
		float x = 0;

		if ( Font.FontData == null )
			return;

		if ( Font.FontTexture == null )
			return;

		foreach ( var c in calculatedText )
		{
			if ( !GetFontDataForGlyph( c ).Glyphs.Any( x => x.Unicode == c ) )
			{
				x += FontSize;
				continue;
			}

			var glyph = GetFontDataForGlyph( c ).Glyphs.First( x => x.Unicode == (int)c );

			if ( glyph.AtlasBounds != null )
			{
				var glyphRect = FontBoundsToAtlasRect( glyph, glyph.AtlasBounds );

				var glyphSize = new Vector2( glyphRect.Width, glyphRect.Height );
				glyphSize *= FontSize / Font.FontData.Atlas.Size;

				var glyphPos = new Rectangle( new Vector2( Bounds.X + x, Bounds.Y + FontSize ), glyphSize );
				glyphPos.X += (float)glyph.PlaneBounds.Left * FontSize;
				glyphPos.Y -= (float)glyph.PlaneBounds.Top * FontSize;

				float screenPxRange = Font.FontData.Atlas.DistanceRange * (glyphPos.Size / Font.FontData.Atlas.Size).Length;
				screenPxRange *= 1.5f;

				if ( glyphPos.X > Bounds.X + Bounds.Width && Bounds.Width > 0 )
					return;

				Graphics.DrawCharacter(
					glyphPos,
					GetTextureForGlyph( c ),
					glyphRect,
					Color
				);
			}

			x += (float)glyph.Advance * FontSize;
		}
	}

	public void SetFont( string fontName )
	{
		Font = Font.Load( $"{fontName}" );
		IconFont = Font.Load( "fa-solid-900" );
	}

	internal override Vector2 GetDesiredSize()
	{
		return MeasureText( Text, FontSize );
	}

	internal override void OnBoundsChanged()
	{
		CalculateText();
	}

	private void CalculateText()
	{
		var text = Text;
		calculatedText = text;
	}
}
