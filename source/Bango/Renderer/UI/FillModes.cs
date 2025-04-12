namespace Bango;
public class FillMode
{
	internal FillMode() { }

	public virtual (Vector4 a, Vector4 b, Vector4 c, Vector4 d) GetColors()
	{
		return (Vector4.One, Vector4.One, Vector4.One, Vector4.One);
	}

	public static FillMode Solid( Vector4 color )
	{
		return new SolidFillMode() { Color = color };
	}

	public static FillMode LinearGradient( Vector4 start, Vector4 end )
	{
		return new LinearGradientFillMode() { Start = start, End = end };
	}
}

public class SolidFillMode : FillMode 
{
	public Vector4 Color;

	public override (Vector4 a, Vector4 b, Vector4 c, Vector4 d) GetColors()
	{
		return (Color, Color, Color, Color);
	}
}

public class LinearGradientFillMode : FillMode
{
	public Vector4 Start;
	public Vector4 End;

	public override (Vector4 a, Vector4 b, Vector4 c, Vector4 d) GetColors()
	{
		return (Start, Start, End, End);
	}
}
