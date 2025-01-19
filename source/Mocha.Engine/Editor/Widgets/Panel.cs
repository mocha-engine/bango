namespace Mocha.Engine.Editor;

[Flags]
public enum PanelInputFlags
{
	None = 0,
	MouseOver = 1,
	MouseDown = 2
}

internal class Panel : Widget
{
	public Color Color { get; set; } = Color.White;
	public Vector4 Rounding { get; set; } = Vector4.Zero;

	internal Panel( Vector2 size ) : base()
	{
		Bounds = new Rectangle( 0, size );
	}

	internal override void Render()
	{
		Graphics.DrawRect( Bounds, Color, Rounding );
	}
}
