namespace Bango;
public static class ImDraw
{
	private record CursorState
	{
		public string FontFamily = "segoeui";
		public float FontSize = 13.0f;
		public Vector2 Padding = new( 8, 8 );
		public Vector2 Position = new();

		public CursorState()
		{
			Position = Padding;
		}

		public Font Font => Font.Load( FontFamily );

		public void AddSpacing( float size = 8.0f )
		{
			Position.Y += size;
		}

		private Rectangle _lastElementBounds = new();
		public void Advance( Rectangle bounds )
		{
			_lastElementBounds = bounds;

			Position.X = Padding.X;
			Position.Y = _lastElementBounds.Y + _lastElementBounds.Height;
		}

		public void SameLine()
		{
			Position.X = _lastElementBounds.X + _lastElementBounds.Width + 8.0f;
			Position.Y = _lastElementBounds.Y;
		}

		private Stack<Vector2> _savedPositions = new();
		public void PushPosition()
		{
			_savedPositions.Push( Position );
		}

		public void PopPosition()
		{
			Position = _savedPositions.Pop();
		}

		public void BumpPosition( Vector2 bump )
		{
			Position += bump;
		}

		public Rectangle LastBounds => _lastElementBounds;
	}

	private static CursorState Cursor = null!;

	public static void NewFrame()
	{
		Cursor = new();
	}

	public static void Spacing( float size = 8.0f )
	{
		Cursor.AddSpacing( size );
	}

	public static void FillTo( float height )
	{
		var diff = Screen.Size.Y - Cursor.Position.Y;
		Cursor.AddSpacing( diff - height );
	}

	public static void TitleBar()
	{
		var bounds = new Rectangle( new Vector2( 0, 0 ), new Vector2( Screen.Size.X, 30 ) );

		Paint.Clear();

		var tex = Texture.Builder.FromPath( "core/logo.png" ).Build();
		Graphics.DrawTexture( new Rectangle( 8, 8, 16, 16 ), tex );

		Cursor.BumpPosition( new Vector2( 24, 0 ) );
		SetFont( "segoeui", 12.0f );
		Text( SdlWindow.Current.Title, Color.White ); Inline();
		var titleWidth = Graphics.MeasureText( SdlWindow.Current.Title, Cursor.Font, Cursor.FontSize ).X;
		Cursor.BumpPosition( new Vector2( Screen.Size.X - titleWidth - 175, 0 ) );

		void DrawButton( char character, Action onClick )
		{
			var buttonBounds = new Rectangle( Cursor.Position, new Vector2( 45.0f, 30.0f ) );

			Color foregroundColor = Color.White;
			Color backgroundColor = Theme.Default50;

			if ( buttonBounds.Contains( Input.MousePosition ) )
			{
				if ( character == (char)0xE8BB )
				{
					foregroundColor = Color.White;
					backgroundColor = "#c42b1c";

					if ( Input.MouseLeftDown )
						backgroundColor = backgroundColor.Darken( 0.1f );
				}
				else
				{
					backgroundColor = Theme.Default800;

					if ( Input.MouseLeftDown )
						backgroundColor = Theme.Default700;
				}

				if ( Input.MouseLeftPressed )
				{
					onClick?.Invoke();
				}

				Paint.SetFillSolid( backgroundColor );
				Paint.DrawRect( buttonBounds );
			}

			var glyph = Cursor.Font.FontData.Glyphs.First( x => x.Unicode == (char)character );
			var glyphRect = FontBoundsToAtlasRect( glyph, glyph.AtlasBounds );

			var glyphSize = new Vector2( glyphRect.Width, glyphRect.Height );
			glyphSize *= 10.0f / Cursor.Font.FontData.Atlas.Size;

			var glyphPos = new Rectangle( new Vector2( buttonBounds.X + 17.0f, buttonBounds.Y + 21.0f ), glyphSize );
			glyphPos.X += (float)glyph.PlaneBounds.Left * 11.0f;
			glyphPos.Y -= (float)glyph.PlaneBounds.Top * 11.0f;

			Graphics.DrawCharacter(
				glyphPos,
				Cursor.Font.FontTexture,
				glyphRect,
				foregroundColor
			);

			Cursor.Advance( buttonBounds );
		}

		SetFont( "segoe", 10f );
		Cursor.BumpPosition( new Vector2( 0, -8 ) );
		DrawButton( (char)0xE921, () => SdlWindow.Current.Minimize() );
		Inline(); Cursor.BumpPosition( new Vector2( -8, 0 ) );
		DrawButton( (char)0xE922, () => SdlWindow.Current.Maximize() );
		Inline(); Cursor.BumpPosition( new Vector2( -8, 0 ) );
		DrawButton( (char)0xE8BB, () => SdlWindow.Current.Close() );
		SetFontDefault();

		bounds.Height += Cursor.Padding.Y;
		Cursor.Advance( bounds );

		Paint.Clear();
	}

	private static Rectangle FontBoundsToAtlasRect( Font.Glyph glyph, Font.Bounds bounds )
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

	public static void Text( string text, Color? color = default )
	{
		var font = Cursor.Font;
		var fontSize = Cursor.FontSize;
		var start = Cursor.Position;

		foreach ( var character in text )
		{
			var glyph = font.FontData.Glyphs.FirstOrDefault( x => x.Unicode == (int)character );

			if ( glyph == null )
				continue;

			if ( glyph.AtlasBounds != null )
			{
				var glyphRect = FontBoundsToAtlasRect( glyph, glyph.AtlasBounds );

				var glyphSize = new Vector2( glyphRect.Width, glyphRect.Height );
				glyphSize *= fontSize / font.FontData.Atlas.Size;

				var glyphPos = new Rectangle( new Vector2( Cursor.Position.X, Cursor.Position.Y + fontSize ), glyphSize );
				glyphPos.X += (float)glyph.PlaneBounds.Left * fontSize;
				glyphPos.Y -= (float)glyph.PlaneBounds.Top * fontSize;

				Graphics.DrawCharacter(
					glyphPos,
					font.FontTexture,
					glyphRect,
					color ?? Theme.Default50
				);
			}

			Cursor.BumpPosition( new Vector2( (float)glyph.Advance * fontSize, 0 ) );
		}

		// yuck
		Cursor.Advance( new Rectangle( start, Graphics.MeasureText( text, font, fontSize ) ) );
	}

	public static void SetFontDefault( float fontSize = 13 )
	{
		Cursor.FontFamily = "segoeui";
		Cursor.FontSize = fontSize;
	}

	public static void SetFont( string fontFamily, float fontSize = 13 )
	{
		Cursor.FontFamily = fontFamily;
		Cursor.FontSize = fontSize;
	}

	private static bool ButtonInternal( string text, Color strokeStart, Color strokeEnd, Color fillStart, Color fillEnd, Color mouseDownFillStart, Color mouseDownFillEnd, Color textColor )
	{
		var isClicked = false;

		var font = Cursor.Font;
		var fontSize = Cursor.FontSize;

		var padding = new Vector2( 24, 16 );
		var textSize = Graphics.MeasureText( text, font, fontSize );
		var bounds = new Rectangle( Cursor.Position, textSize + padding );
		bounds.Width = bounds.Width.Clamp( 100, 10000 );

		Paint.Clear();

		if ( bounds.Contains( Input.MousePosition ) )
		{
			// Save off state
			if ( Input.MouseLeftPressed )
				isClicked = true;

			if ( Input.MouseLeftDown )
			{
				(fillStart, fillEnd) = (fillEnd, fillStart);
			}
			else
			{
				(fillStart, fillEnd) = (mouseDownFillStart, mouseDownFillEnd);
				Paint.SetShadowSize( 6.0f );
			}
		}

		Paint.SetStrokeWidth( 1.0f );
		Paint.SetStrokeLinearGradient( strokeStart, strokeEnd );
		Paint.SetFillLinearGradient( fillStart, fillEnd );

		Paint.DrawRect( bounds, new( 6 ) );

		Cursor.PushPosition();

		// Apply padding
		Cursor.BumpPosition( new Vector2( (bounds.Width - textSize.X) / 2f, padding.Y / 2f ) );

		Text( text, textColor );

		Cursor.PopPosition();

		Cursor.Advance( bounds );
		Cursor.AddSpacing();

		return isClicked;
	}

	public static bool Button( string text )
	{
		(Color strokeStart, Color strokeEnd) = (Theme.Default700, Theme.Default700);
		(Color fillStart, Color fillEnd) = (Theme.Default800, Theme.Default800);
		(Color mouseDownFillStart, Color mouseDownFillEnd) = (Theme.Default600, Theme.Default600);

		return ButtonInternal( text, strokeStart, strokeEnd, fillStart, fillEnd, mouseDownFillStart, mouseDownFillEnd, Theme.Default50 );
	}

	public static bool ButtonLight( string text )
	{
		(Color strokeStart, Color strokeEnd) = (Theme.Default200, Theme.Default200);
		(Color fillStart, Color fillEnd) = (Theme.Default100, Theme.Default100);
		(Color mouseDownFillStart, Color mouseDownFillEnd) = (Theme.Default200, Theme.Default200);

		return ButtonInternal( text, strokeStart, strokeEnd, fillStart, fillEnd, mouseDownFillStart, mouseDownFillEnd, Theme.Default950 );
	}

	public static bool ButtonDelete( string text )
	{
		(Color strokeStart, Color strokeEnd) = ("#b91c1c", "#b91c1c");
		(Color fillStart, Color fillEnd) = ("#991b1b", "#991b1b");
		(Color mouseDownFillStart, Color mouseDownFillEnd) = ("#dc2626", "#dc2626");

		return ButtonInternal( text, strokeStart, strokeEnd, fillStart, fillEnd, mouseDownFillStart, mouseDownFillEnd, "#fecaca" );
	}

	public static bool ButtonDisabled( string text )
	{
		(Color strokeStart, Color strokeEnd) = (Theme.Default800, Theme.Default800);
		(Color fillStart, Color fillEnd) = (Theme.Default900, Theme.Default900);
		(Color mouseDownFillStart, Color mouseDownFillEnd) = (Theme.Default900, Theme.Default900);

		ButtonInternal( text, strokeStart, strokeEnd, fillStart, fillEnd, mouseDownFillStart, mouseDownFillEnd, Theme.Default600 );
		return false;
	}

	public static bool Checkbox( string text, ref bool value )
	{
		var isChanged = false;

		var font = Cursor.Font;
		var fontSize = Cursor.FontSize;

		var padding = new Vector2( 24, 16 );
		var textSize = Graphics.MeasureText( text, font, fontSize );
		var bounds = new Rectangle( Cursor.Position, textSize + padding );
		bounds.Width = bounds.Width.Clamp( 100, 10000 );

		if ( bounds.Contains( Input.MousePosition ) && Input.MouseLeftPressed )
		{
			value = !value;
			isChanged = true;
		}

		Paint.Clear();

		// Check box
		{
			var r = new Rectangle( Cursor.Position, 16 );
			Paint.SetStrokeWidth( 1.0f );

			if ( value )
			{
				Paint.SetStrokeSolid( Theme.Default950 );
				Paint.SetFillSolid( Theme.Default950 );
			}
			else
			{
				Paint.SetStrokeSolid( Theme.Default500 );
				Paint.SetFillSolid( Theme.Default200 );
			}

			Paint.DrawRect( r, new( 6 ) );
		}

		if ( value )
		{
			Cursor.PushPosition();

			Cursor.BumpPosition( new Vector2( 1, 1 ) );
			SetFont( "segoe", 14f );
			Text( new string( (char)0xE73E, 1 ), Theme.Default50 );
			SetFontDefault();

			Cursor.PopPosition();
		}

		Cursor.PushPosition();

		Cursor.BumpPosition( new Vector2( 22, 0 ) );
		Text( text );

		Cursor.PopPosition();

		Cursor.Advance( bounds );
		Cursor.AddSpacing();

		// Changed?
		return isChanged;
	}

	public static void Inline()
	{
		Cursor.SameLine();
	}

	public static void Separator()
	{
		var bounds = new Rectangle( Cursor.Position, new Vector2( Screen.Size.X, 32 ) );

		Cursor.BumpPosition( new Vector2( 0, 16.0f ) );

		Paint.Clear();
		Paint.SetFillSolid( Theme.Default50.WithAlpha( 0.005f ) );
		Paint.DrawRect( new Rectangle( Cursor.Position, new Vector2( Screen.Size.X - (Cursor.Padding.X * 2.0f), 2.0f ) ) );

		Cursor.Advance( bounds );
	}
}

