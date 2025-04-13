namespace Bango;

public static class Paint
{
	internal record State
	{
		public FillMode FillMode = null!;
		public FillMode StrokeMode = null!;
		public Vector4 Stroke = Vector4.Zero;

		public float ShadowSize = 0.0f;
		public Vector2 ShadowOffset = Vector2.Zero;
		public FillMode ShadowMode = null!;

		public State()
		{
			Reset();
		}

		public void Reset()
		{
			FillMode = FillMode.Solid( Vector4.Zero );
			StrokeMode = FillMode.Solid( Vector4.Zero );
			Stroke = Vector4.Zero;

			ShadowSize = 0.0f;
			ShadowOffset = Vector2.Zero;
			ShadowMode = FillMode.Solid( Vector4.Zero );
		}
	}

	internal readonly static State CurrentState = new();

	public static void SetStrokeWidth( float strokeWidth )
	{
		CurrentState.Stroke = Vector4.One * strokeWidth;
	}

	public static void SetStrokeWidth( Vector4 strokeWidth )
	{
		CurrentState.Stroke = strokeWidth;
	}

	public static void SetStrokeLinearGradient( Color start, Color end )
	{
		CurrentState.StrokeMode = FillMode.LinearGradient( start, end );
	}

	public static void SetStrokeSolid( Color color )
	{
		CurrentState.StrokeMode = FillMode.Solid( color );
	}

	public static void SetFillLinearGradient( Color start, Color end )
	{
		CurrentState.FillMode = FillMode.LinearGradient( start, end );
	}

	public static void SetFillSolid( Color color )
	{
		CurrentState.FillMode = FillMode.Solid( color );
	}

	public static void SetShadowSize( float shadowSize )
	{
		CurrentState.ShadowSize = shadowSize;
	}

	public static void SetShadowOffset( Vector2 shadowOffset )
	{
		CurrentState.ShadowOffset = shadowOffset;
	}

	public static void SetShadowColor( Color color )
	{
		CurrentState.ShadowMode = FillMode.Solid( color );
	}

	public static void Clear()
	{
		CurrentState.Reset();
	}

	public static void DrawRect( Rectangle rectangle, Vector4? rounding = default )
	{
		//
		// Draw shadow
		//
		if ( CurrentState.ShadowSize > 0f )
		{
			var shadowBounds = rectangle + CurrentState.ShadowOffset;

			Graphics.DrawShadow( shadowBounds, CurrentState.ShadowSize, CurrentState.ShadowMode, rounding ?? Vector4.Zero );
		}

		//
		// Draw border
		//
		if ( CurrentState.Stroke.LengthSquared() > 0f )
		{
			var info = new RectangleInfo()
			{
				FillMode = CurrentState.StrokeMode,
				Rounding = rounding + CurrentState.Stroke
			};

			Graphics.PanelRenderer.AddRectangle( rectangle.Expand( CurrentState.Stroke ), info );
		}

		//
		// Draw background
		//
		{
			var info = new RectangleInfo()
			{
				FillMode = CurrentState.FillMode,
				Rounding = rounding
			};

			Graphics.PanelRenderer.AddRectangle( rectangle, info );
		}
	}
}
