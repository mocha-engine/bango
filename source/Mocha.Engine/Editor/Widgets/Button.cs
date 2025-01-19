namespace Bango.Engine.Editor;

internal class Button : Widget
{
	protected Label label;
	public Action OnClick;
	public Vector2 TextAnchor = new Vector2( 0.5f, 0.5f );
	private Vector2 Padding => new Vector2( 32, 12 );

	public string Text
	{
		get => label.Text;
		set => label.Text = value;
	}

	public Button( string text, Action? onClick = null ) : base()
	{
		label = new( text, 11 );
		label.Parent = this;

		if ( onClick != null )
			OnClick += onClick;
	}

	internal override void Render()
	{
		(Color strokeStart, Color strokeEnd) = ("#dcdee0", "#c7d2d9");
		(Color fillStart, Color fillEnd) = ("#f6f8fa", "#dee9f1");

		Paint.Clear();

		Paint.SetShadowSize( 4.0f );
		Paint.SetShadowColor( new Color( 0, 0, 0, 8 ) );
		Paint.SetShadowOffset( new Vector2( 0, 1 ) );

		if ( IsMouseDown )
		{
			// Swap
			(fillStart, fillEnd) = (fillEnd, fillStart);
		}
		else if ( IsMouseOver )
		{
			// Lighten
			(fillStart, fillEnd) = (fillStart.Lighten( 0.1f ), fillEnd.Lighten( 0.1f ));
			Paint.SetShadowSize( 6.0f );
		}

		Paint.SetStrokeWidth( 1.0f );
		Paint.SetStrokeLinearGradient( strokeStart, strokeEnd );
		Paint.SetFillLinearGradient( fillStart, fillEnd );

		Paint.DrawRect( Bounds, new( 10 ) );

		UpdateLabel();
	}

	protected void UpdateLabel()
	{
		var labelBounds = label.Bounds;
		labelBounds.X = Bounds.X + ((Bounds.Width - (Padding.X * 2.0f) - label.MeasureText( label.Text, label.FontSize ).X) * TextAnchor.X);
		labelBounds.X += Padding.X;
		labelBounds.Y = Bounds.Y + (Padding.Y) - 7;
		label.Bounds = labelBounds;
		label.Color = "#303335";
	}

	internal override Vector2 GetDesiredSize()
	{
		var size = new Vector2( (label.MeasureText( label.Text, label.FontSize ).X + (Padding.X * 2)).Clamp( 75f, float.MaxValue ), Padding.Y * 2 );
		return size;
	}

	internal override void OnDelete()
	{
		base.OnDelete();

		label.Delete();
	}
}
